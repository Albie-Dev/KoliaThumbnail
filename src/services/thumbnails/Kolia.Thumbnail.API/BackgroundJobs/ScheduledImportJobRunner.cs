using System.Text.Json;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Services.GoogleServices;
using Kolia.Thumbnail.API.Services.Projects;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.BackgroundJobs
{
    /// <summary>
    /// Background service chạy các Scheduled Import Jobs đã đến hạn.
    /// Quét DB mỗi 30 giây để tìm job Pending và chạy chúng.
    /// </summary>
    public class ScheduledImportJobRunner : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledImportJobRunner> _logger;

        public ScheduledImportJobRunner(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledImportJobRunner> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledImportJobRunner started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
                    var briefAnalysisEngine = scope.ServiceProvider.GetRequiredService<IBriefAnalysisEngine>();
                    var googleHelper = scope.ServiceProvider.GetRequiredService<GoogleServiceAccountHelper>();

                    // Tìm các job đến hạn: Pending và thoả mãn 1 trong 2:
                    // 1. CronExpression != null && cron matches thời điểm hiện tại (trong vòng 30s)
                    // 2. ScheduledAt != null && ScheduledAt <= now
                    // 3. Cả 2 null → chạy ngay (chưa từng chạy)
                    var now = DateTimeOffset.UtcNow;
                    var dueJobs = await db.Set<ScheduledImportJobEntity>()
                        .Include(x => x.GoogleServiceAccount)
                        .Where(x => x.Status == CJobScheduleStatus.Pending
                                 && !x.IsDeleted)
                        .OrderBy(x => x.CreationTime)
                        .ToListAsync(stoppingToken);

                    // Filter cron-based và one-time jobs trong memory
                    var jobsToRun = dueJobs.Where(job =>
                    {
                        // Cron job: kiểm tra cron expression theo múi giờ của job
                        if (!string.IsNullOrWhiteSpace(job.CronExpression))
                        {
                            try
                            {
                                var cron = Cronos.CronExpression.Parse(job.CronExpression);
                                var tz = GetTimeZoneInfo(job.TimeZone);
                                var lastRun = job.StartedAt?.UtcDateTime ?? job.CreationTime.UtcDateTime;
                                // Cronos.GetNextOccurrence(fromUtc, tz) nhận fromUtc là UTC,
                                // tự động convert sang tz, tìm next occurrence trong tz,
                                // và trả về kết quả dạng UTC — so sánh với now (UTC) là chính xác
                                var next = cron.GetNextOccurrence(lastRun, tz);
                                return next.HasValue && next.Value <= now.UtcDateTime;
                            }
                            catch
                            {
                                return false;
                            }
                        }

                        // One-time schedule
                        if (job.ScheduledAt.HasValue)
                            return job.ScheduledAt.Value <= now;

                        // Chạy ngay (chưa từng chạy, không cron, không scheduledAt)
                        return job.StartedAt == null;
                    })
                    .Take(10)
                    .ToList();

                    foreach (var job in jobsToRun)
                    {
                        try
                        {
                            await ProcessJobAsync(job, db, projectService, briefAnalysisEngine, googleHelper, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Job {JobId} ({Name}) thất bại với exception.", job.Id, job.Name);
                            job.Status = CJobScheduleStatus.Failed;
                            job.ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                            job.CompletedAt = DateTimeOffset.UtcNow;
                            AppendLog(job, LogEntry.Error($"Exception: {ex.Message}"));
                            await db.SaveChangesAsync(stoppingToken);

                            Notify("ScheduledImportJob",
                                $"Job '{job.Name}' thất bại với lỗi: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi quét ScheduledImportJobs.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task ProcessJobAsync(
            ScheduledImportJobEntity job,
            ThumbnailDbContext db,
            IProjectService projectService,
            IBriefAnalysisEngine briefAnalysisEngine,
            GoogleServiceAccountHelper googleHelper,
            CancellationToken ct)
        {
            _logger.LogInformation("Bắt đầu chạy job: {JobId} - {Name}", job.Id, job.Name);

            job.Status = CJobScheduleStatus.Running;
            job.StartedAt = DateTimeOffset.UtcNow;
            AppendLog(job, LogEntry.Info($"Job bắt đầu chạy."));
            await db.SaveChangesAsync(ct);

            Notify("ScheduledImportJob", $"Job '{job.Name}' đang chạy...");

            // Bước 1: Fetch nội dung từ Google
            AppendLog(job, LogEntry.Info($"Đang fetch nội dung từ: {job.SourceUrl}"));
            var content = await FetchGoogleContentAsync(job, googleHelper, ct);
            job.ImportedContent = content;

            AppendLog(job, LogEntry.Info($"Đã fetch thành công {content.Length} ký tự."));

            // Bước 2: Gọi AI Agent phân tích nội dung TRƯỚC
            // Nếu AI fail → job chưa tạo project → retry an toàn, không orphan data
            AppendLog(job, LogEntry.Info($"Đang gọi AI phân tích nội dung..."));
            var textForAnalysis = content.Length > 100000 ? content[..100000] : content;
            var aiResult = await briefAnalysisEngine.AnalyzeFromPastedTextAsync(textForAnalysis, ct);

            AppendLog(job, LogEntry.Info($"AI phân tích thành công. Đang tạo project..."));

            // Bước 3: AI thành công → mới tạo Project
            var timestamp = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
            var projectName = $"Auto-Generated Project {timestamp}";

            var counter = 0;
            string uniqueName;
            do
            {
                uniqueName = counter == 0 ? projectName : $"{projectName} ({counter})";
                var exists = await db.Set<Kolia.Thumbnail.API.Data.Entities.Projects.ProjectEntity>()
                    .AnyAsync(x => x.Name == uniqueName && !x.IsDeleted, ct);
                if (!exists) break;
                counter++;
            } while (true);

            var project = await projectService.CreateAsync(uniqueName, ct);
            job.CreatedProjectId = project.Id;
            AppendLog(job, LogEntry.Info($"Đã tạo project: '{uniqueName}' (ID: {project.Id})"));

            Notify("ScheduledImportJob", $"Đã tạo project '{uniqueName}' cho job '{job.Name}'.");

            // Bước 4: Tạo Content Brief và lưu kết quả AI
            var brief = await db.Set<Kolia.Thumbnail.API.Data.Entities.Briefs.ContentBriefEntity>()
                .FirstOrDefaultAsync(b => b.ProjectId == project.Id, ct);

            if (brief == null)
            {
                brief = new Kolia.Thumbnail.API.Data.Entities.Briefs.ContentBriefEntity
                {
                    ProjectId = project.Id,
                    ImportSource = CImportContentSource.ExternalLink,
                    ImportedExternalLink = job.SourceUrl,
                    ImportedRawText = content.Length > 50000 ? content[..50000] : content,
                    OverviewInput = aiResult.OverviewInput,
                    ViewpointInput = aiResult.ViewpointInput,
                    KeyDataInput = aiResult.KeyDataInput,
                    TopicOutput = aiResult.Topic,
                    MainMessageOutput = aiResult.MainMessage,
                    HighlightDataOutput = aiResult.HighlightData,
                    SuggestedKeywordsJson = JsonSerializer.Serialize(aiResult.SuggestedKeywords),
                    LastModificationTime = DateTimeOffset.UtcNow
                };
                db.Set<Kolia.Thumbnail.API.Data.Entities.Briefs.ContentBriefEntity>().Add(brief);
            }
            else
            {
                brief.OverviewInput = aiResult.OverviewInput;
                brief.ViewpointInput = aiResult.ViewpointInput;
                brief.KeyDataInput = aiResult.KeyDataInput;
                brief.TopicOutput = aiResult.Topic;
                brief.MainMessageOutput = aiResult.MainMessage;
                brief.HighlightDataOutput = aiResult.HighlightData;
                brief.SuggestedKeywordsJson = JsonSerializer.Serialize(aiResult.SuggestedKeywords);
                brief.LastModificationTime = DateTimeOffset.UtcNow;
            }

            job.CreatedBriefId = brief.Id;
            AppendLog(job, LogEntry.Info($"Đã lưu Content Brief (ID: {brief.Id}) và kết quả AI."));

            // Bước 5: Kết thúc thành công
            // Cron job → giữ Pending để chạy tiếp; One-time → Completed
            if (string.IsNullOrWhiteSpace(job.CronExpression))
            {
                job.Status = CJobScheduleStatus.Completed;
            }
            else
            {
                job.Status = CJobScheduleStatus.Pending; // cron job: chờ lần chạy tiếp theo
            }
            job.CompletedAt = DateTimeOffset.UtcNow;
            AppendLog(job, LogEntry.Info("Job hoàn thành thành công."));
            await db.SaveChangesAsync(ct);

            Notify("ScheduledImportJob",
                $"Job '{job.Name}' hoàn thành. Project '{uniqueName}' đã được tạo từ {job.SourceUrl}");
        }

        /// <summary>
        /// Fetch nội dung từ Google Sheets hoặc Google Docs dùng Google API chính thức.
        /// </summary>
        private async Task<string> FetchGoogleContentAsync(
            ScheduledImportJobEntity job,
            GoogleServiceAccountHelper googleHelper,
            CancellationToken ct)
        {
            var sa = job.GoogleServiceAccount;
            if (sa == null)
                throw new InvalidOperationException("Service account không tồn tại.");

            if (string.IsNullOrWhiteSpace(sa.RawCredentialJson))
                throw new InvalidOperationException(
                    "Service account thiếu credential JSON. Vui lòng import lại file JSON.");

            if (job.SourceType == CGoogleServiceType.GoogleSheets)
            {
                return await googleHelper.FetchSheetContentAsync(job.SourceUrl, sa, ct);
            }
            else if (job.SourceType == CGoogleServiceType.GoogleDocs)
            {
                return await googleHelper.FetchDocContentAsync(job.SourceUrl, sa, ct);
            }
            else
            {
                throw new NotSupportedException($"Loại nguồn '{job.SourceType}' chưa được hỗ trợ.");
            }
        }

        private static void AppendLog(ScheduledImportJobEntity job, LogEntry entry)
        {
            var logs = string.IsNullOrWhiteSpace(job.LogJson)
                ? new List<LogEntry>()
                : JsonSerializer.Deserialize<List<LogEntry>>(job.LogJson) ?? new List<LogEntry>();

            logs.Add(entry);
            if (logs.Count > 1000)
                logs.RemoveRange(0, logs.Count - 1000);
            job.LogJson = JsonSerializer.Serialize(logs);
        }

        /// <summary>
        /// Skeleton notification — chỉ console log, sau này thay bằng SignalR/Email.
        /// </summary>
        private static void Notify(string eventType, string message)
        {
            Console.WriteLine($"[NOTIFICATION] {DateTimeOffset.UtcNow:O} | {eventType} | {message}");
        }

        /// <summary>
        /// Lấy TimeZoneInfo từ tên múi giờ (VD: "Asia/Ho_Chi_Minh", "UTC").
        /// Linux dùng IANA timezone names, Windows dùng Windows timezone IDs.
        /// Xử lý các alias phổ biến (Asia/Saigon → Asia/Ho_Chi_Minh).
        /// Nếu không tìm thấy, fallback về UTC.
        /// </summary>
        private static readonly Dictionary<string, string> TimeZoneAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "asia/saigon", "Asia/Ho_Chi_Minh" },
            { "asia/shanghai", "Asia/Shanghai" },
            { "us/eastern", "America/New_York" },
            { "us/central", "America/Chicago" },
            { "us/mountain", "America/Denver" },
            { "us/pacific", "America/Los_Angeles" },
        };

        private static TimeZoneInfo GetTimeZoneInfo(string? timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone) || timeZone == "UTC")
                return TimeZoneInfo.Utc;

            // Resolve alias
            if (TimeZoneAliases.TryGetValue(timeZone, out var mapped))
                timeZone = mapped;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: thử tìm trên Windows (chuyển IANA → Windows ID)
                try
                {
                    if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZone, out var winId) && winId != null)
                        return TimeZoneInfo.FindSystemTimeZoneById(winId);
                }
                catch
                {
                    // ignore
                }
            }

            return TimeZoneInfo.Utc;
        }
    }
}
