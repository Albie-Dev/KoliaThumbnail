using System.Text.Json;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Interfaces.GoogleServices;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Security;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.GoogleServices
{
    public class ScheduledImportJobService : IScheduledImportJobService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IApiKeyProtector _protector;
        private readonly GoogleServiceAccountHelper _googleHelper;
        private readonly ILogger<ScheduledImportJobService> _logger;

        public ScheduledImportJobService(
            ThumbnailDbContext db,
            IApiKeyProtector protector,
            GoogleServiceAccountHelper googleHelper,
            ILogger<ScheduledImportJobService> logger)
        {
            _db = db;
            _protector = protector;
            _googleHelper = googleHelper;
            _logger = logger;
        }

        public async Task<PagedResponseDto<ScheduledJobSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            IQueryable<ScheduledImportJobEntity> query = _db.Set<ScheduledImportJobEntity>()
                .AsNoTracking()
                .Include(x => x.GoogleServiceAccount);

            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();
                if (deletedOnly == true)
                    query = query.Where(x => x.IsDeleted);
                else
                    query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<ScheduledImportJobEntity, ScheduledJobSummaryDto>(
                request,
                selector: e => new ScheduledJobSummaryDto(
                    e.Id,
                    e.Name,
                    e.SourceType,
                    e.SourceUrl,
                    e.GoogleServiceAccount.Name,
                    e.Status,
                    e.ErrorMessage,
                    e.CronExpression,
                    e.CronDescription,
                    e.ScheduledAt,
                    e.CreatedProjectId,
                    e.RetryCount,
                    e.CreationTime),
                cancellationToken: ct);
        }

        public async Task<ScheduledJobDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .AsNoTracking()
                .Include(x => x.GoogleServiceAccount)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<ScheduledJobDto> CreateAsync(
            CreateScheduledJobRequest request,
            CancellationToken ct = default)
        {
            // Validate service account tồn tại — dùng local variable tránh lỗi EF parameter eval
            var saId = request.GoogleServiceAccountId;
            var sa = await _db.Set<GoogleServiceAccountEntity>()
                .FirstOrDefaultAsync(x => x.Id == saId && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy service account với ID: {saId}");

            if (!sa.IsEnabled)
                throw new BusinessException($"Service account '{sa.Name}' đang bị vô hiệu hoá.");

            // Validate URL
            if (!IsValidGoogleUrl(request.SourceUrl, request.SourceType))
                throw new ValidationException($"URL không hợp lệ cho {request.SourceType}.");

            // Validate cron expression nếu có
            if (!string.IsNullOrWhiteSpace(request.CronExpression))
            {
                try
                {
                    Cronos.CronExpression.Parse(request.CronExpression);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"Cron expression không hợp lệ: {ex.Message}");
                }
            }

            var entity = new ScheduledImportJobEntity
            {
                Name = request.Name,
                Description = request.Description,
                SourceType = request.SourceType,
                SourceUrl = request.SourceUrl,
                GoogleServiceAccountId = request.GoogleServiceAccountId,
                ScheduledAt = request.CronExpression != null ? null : request.ScheduledAt,
                CronExpression = request.CronExpression,
                CronDescription = request.CronDescription,
                MaxRetries = request.MaxRetries,
                Status = CJobScheduleStatus.Pending
            };

            _db.Set<ScheduledImportJobEntity>().Add(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Created ScheduledImportJob: {Name} ({Type}) for {ScheduledAt}",
                entity.Name, entity.SourceType, entity.ScheduledAt);

            // Skeleton notification
            Notify("ScheduledImportJob", $"Job '{entity.Name}' đã được tạo thành công.");

            return MapToDto(entity);
        }

        public async Task<ScheduledJobDto> UpdateAsync(
            Guid id,
            UpdateScheduledJobRequest request,
            CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .Include(x => x.GoogleServiceAccount)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy job với ID: {id}");

            if (entity.Status != CJobScheduleStatus.Pending)
                throw new BusinessException("Chỉ có thể cập nhật job đang ở trạng thái Pending.");

            var sa = await _db.Set<GoogleServiceAccountEntity>()
                .FirstOrDefaultAsync(x => x.Id == request.GoogleServiceAccountId && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy service account với ID: {request.GoogleServiceAccountId}");

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.SourceUrl = request.SourceUrl;
            entity.GoogleServiceAccountId = request.GoogleServiceAccountId;
            entity.ScheduledAt = request.CronExpression != null ? null : request.ScheduledAt;
            entity.CronExpression = request.CronExpression;
            entity.CronDescription = request.CronDescription;
            entity.MaxRetries = request.MaxRetries;
            entity.LastModificationTime = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
            return MapToDto(entity);
        }

        public async Task CancelAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy job với ID: {id}");

            if (entity.Status is CJobScheduleStatus.Completed or CJobScheduleStatus.Cancelled)
                throw new BusinessException("Không thể huỷ job đã hoàn thành hoặc đã huỷ.");

            entity.Status = CJobScheduleStatus.Cancelled;
            entity.CompletedAt = DateTimeOffset.UtcNow;
            entity.LastModificationTime = DateTimeOffset.UtcNow;

            AppendLog(entity, LogEntry.Info($"Job đã bị huỷ bởi người dùng."));
            await _db.SaveChangesAsync(ct);

            Notify("ScheduledImportJob", $"Job '{entity.Name}' đã bị huỷ.");
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy job với ID: {id}");

            if (entity.Status is CJobScheduleStatus.Pending or CJobScheduleStatus.Running)
                throw new BusinessException("Không thể xoá job đang chờ hoặc đang chạy. Hãy huỷ trước.");

            entity.IsDeleted = true;
            entity.DeletionTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<CheckAccessResult> CheckAccessAsync(
            CheckAccessRequest request,
            CancellationToken ct = default)
        {
            var sa = await _db.Set<GoogleServiceAccountEntity>()
                .FirstOrDefaultAsync(x => x.Id == request.GoogleServiceAccountId && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy service account với ID: {request.GoogleServiceAccountId}");

            if (!sa.IsEnabled)
                return new CheckAccessResult(false, "Service account đang bị vô hiệu hoá.", null);

            if (!IsValidGoogleUrl(request.SourceUrl, request.SourceType))
                return new CheckAccessResult(false, "URL không hợp lệ.", null);

            // Kiểm tra có raw credential JSON để lấy access token không
            if (string.IsNullOrWhiteSpace(sa.RawCredentialJson))
                return new CheckAccessResult(false,
                    "Service account thiếu credential JSON. Vui lòng import lại file JSON.",
                    null);

            try
            {
                // Kiểm tra quyền dùng Google API chính thức (gọi metadata)
                var hasAccess = await _googleHelper.CheckAccessAsync(
                    request.SourceUrl, request.SourceType, sa, ct);

                if (hasAccess)
                {
                    _logger.LogInformation("CheckAccess thành công cho {Url} bằng {Email}",
                        request.SourceUrl, sa.ClientEmail);
                    return new CheckAccessResult(true, null, null);
                }

                // 403/401 — cần share quyền
                var instruction = request.SourceType switch
                {
                    CGoogleServiceType.GoogleSheets =>
                        $"Vui lòng share Google Sheet với email: {sa.ClientEmail}\n" +
                        "1. Mở Google Sheet\n" +
                        "2. Nhấn 'Share' ở góc trên bên phải\n" +
                        "3. Thêm email: " + sa.ClientEmail + "\n" +
                        "4. Chọn quyền 'Viewer' (hoặc 'Editor' nếu cần ghi)\n" +
                        "5. Nhấn 'Send'\n" +
                        "6. Sau đó thử lại.",

                    CGoogleServiceType.GoogleDocs =>
                        $"Vui lòng share Google Doc với email: {sa.ClientEmail}\n" +
                        "1. Mở Google Doc\n" +
                        "2. Nhấn 'Share' ở góc trên bên phải\n" +
                        "3. Thêm email: " + sa.ClientEmail + "\n" +
                        "4. Chọn quyền 'Viewer'\n" +
                        "5. Nhấn 'Send'\n" +
                        "6. Sau đó thử lại.",

                    _ => $"Vui lòng share tài liệu với email: {sa.ClientEmail}"
                };

                return new CheckAccessResult(false,
                    $"Service account '{sa.ClientEmail}' chưa có quyền truy cập tài liệu này.",
                    instruction);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực Google cho {Email}", sa.ClientEmail);
                return new CheckAccessResult(false,
                    $"Lỗi xác thực Google: {ex.Message}",
                    "Kiểm tra lại file JSON credential đã import.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối Google cho {Url}", request.SourceUrl);
                return new CheckAccessResult(false,
                    $"Lỗi kết nối tới Google: {ex.Message}",
                    null);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                return new CheckAccessResult(false,
                    "Kết nối tới Google bị timeout.",
                    null);
            }
        }

        public async Task<ScheduledJobDto> RetryAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .Include(x => x.GoogleServiceAccount)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy job với ID: {id}");

            if (entity.Status != CJobScheduleStatus.Failed)
                throw new BusinessException("Chỉ có thể chạy lại job đã thất bại.");

            entity.Status = CJobScheduleStatus.Pending;
            entity.ErrorMessage = null;
            entity.CompletedAt = null;
            entity.LastModificationTime = DateTimeOffset.UtcNow;

            AppendLog(entity, LogEntry.Info($"Job đã được yêu cầu chạy lại (lần {entity.RetryCount + 1})."));
            await _db.SaveChangesAsync(ct);

            return MapToDto(entity);
        }

        public async Task<IReadOnlyList<LogEntry>> GetLogsAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ScheduledImportJobEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy job với ID: {id}");

            if (string.IsNullOrWhiteSpace(entity.LogJson))
                return Array.Empty<LogEntry>();

            try
            {
                return (IReadOnlyList<LogEntry>)(JsonSerializer.Deserialize<List<LogEntry>>(entity.LogJson) ?? new List<LogEntry>());
            }
            catch
            {
                return Array.Empty<LogEntry>();
            }
        }

        // ── Helper methods ──────────────────────────────────────────────

        private static bool IsValidGoogleUrl(string url, CGoogleServiceType type)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return type switch
            {
                CGoogleServiceType.GoogleSheets =>
                    uri.Host.Contains("docs.google.com") && uri.AbsolutePath.Contains("/spreadsheets/"),
                CGoogleServiceType.GoogleDocs =>
                    uri.Host.Contains("docs.google.com") && uri.AbsolutePath.Contains("/document/"),
                _ => false
            };
        }

        /// <summary>
        /// Chuyển URL Google Sheet sang CSV export URL để kiểm tra quyền.
        /// </summary>
        private static string ConvertToCsvExportUrl(string sheetUrl)
        {
            var uri = new Uri(sheetUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var docIdIndex = Array.IndexOf(pathSegments, "d");
            if (docIdIndex < 0 || docIdIndex + 1 >= pathSegments.Length)
                throw new ArgumentException("URL Google Sheet không hợp lệ.");

            var docId = pathSegments[docIdIndex + 1];
            return $"https://docs.google.com/spreadsheets/d/{docId}/export?format=csv";
        }

        /// <summary>
        /// Chuyển URL Google Doc sang TXT export URL để kiểm tra quyền.
        /// </summary>
        private static string ConvertToDocsExportUrl(string docsUrl)
        {
            var uri = new Uri(docsUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var docIdIndex = Array.IndexOf(pathSegments, "d");
            if (docIdIndex < 0 || docIdIndex + 1 >= pathSegments.Length)
                throw new ArgumentException("URL Google Doc không hợp lệ.");

            var docId = pathSegments[docIdIndex + 1];
            return $"https://docs.google.com/document/d/{docId}/export?format=txt";
        }

        private static void AppendLog(ScheduledImportJobEntity entity, LogEntry entry)
        {
            var logs = string.IsNullOrWhiteSpace(entity.LogJson)
                ? new List<LogEntry>()
                : JsonSerializer.Deserialize<List<LogEntry>>(entity.LogJson) ?? new List<LogEntry>();

            logs.Add(entry);

            // Giữ tối đa 1000 entries
            if (logs.Count > 1000)
                logs.RemoveRange(0, logs.Count - 1000);

            entity.LogJson = JsonSerializer.Serialize(logs);
        }

        /// <summary>
        /// Skeleton notification method — hiện tại chỉ console log.
        /// Sau này có thể thay thế bằng SignalR, Email, Webhook, ...
        /// </summary>
        private static void Notify(string eventType, string message)
        {
            Console.WriteLine($"[NOTIFICATION] {DateTimeOffset.UtcNow:O} | {eventType} | {message}");
        }

        private static ScheduledJobDto MapToDto(ScheduledImportJobEntity e)
            => new(
                e.Id,
                e.Name,
                e.Description,
                e.SourceType,
                e.SourceUrl,
                e.GoogleServiceAccount?.Name,
                e.GoogleServiceAccount?.ClientEmail,
                e.Status,
                e.ErrorMessage,
                e.CreatedProjectId,
                e.CreatedBriefId,
                e.CronExpression,
                e.CronDescription,
                e.ScheduledAt,
                e.StartedAt,
                e.CompletedAt,
                e.RetryCount,
                e.MaxRetries,
                e.CreationTime,
                e.LastModificationTime);
    }
}
