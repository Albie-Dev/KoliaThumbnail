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
                        // Cron job: kiểm tra cron expression
                        if (!string.IsNullOrWhiteSpace(job.CronExpression))
                        {
                            try
                            {
                                var cron = Cronos.CronExpression.Parse(job.CronExpression);
                            var lastRun = job.StartedAt?.UtcDateTime ?? job.CreationTime.UtcDateTime;
                            var next = cron.GetNextOccurrence(lastRun);
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

                    foreach (var job in dueJobs)
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

            // Bước 2: Tạo Project tự động với tên không trùng
            var timestamp = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
            var projectName = $"Job Automatic projects {timestamp}";

            // Đảm bảo không trùng tên
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

            // Bước 3: Tạo Content Brief và gửi lên AI Agent phân tích
            // Tương tự flow ImportAndAnalyzeFromPasteAsync
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
                    LastModificationTime = DateTimeOffset.UtcNow
                };
                db.Set<Kolia.Thumbnail.API.Data.Entities.Briefs.ContentBriefEntity>().Add(brief);
                await db.SaveChangesAsync(ct);
            }

            job.CreatedBriefId = brief.Id;
            AppendLog(job, LogEntry.Info($"Đã tạo Content Brief (ID: {brief.Id}). Đang gọi AI phân tích..."));

            // Bước 4: Gọi AI Agent phân tích nội dung
            try
            {
                var textForAnalysis = content.Length > 100000 ? content[..100000] : content;
                var result = await briefAnalysisEngine.AnalyzeFromPastedTextAsync(textForAnalysis, ct);

                brief.OverviewInput = result.OverviewInput;
                brief.ViewpointInput = result.ViewpointInput;
                brief.KeyDataInput = result.KeyDataInput;
                brief.TopicOutput = result.Topic;
                brief.MainMessageOutput = result.MainMessage;
                brief.HighlightDataOutput = result.HighlightData;
                brief.SuggestedKeywordsJson = JsonSerializer.Serialize(result.SuggestedKeywords);
                brief.LastModificationTime = DateTimeOffset.UtcNow;

                await db.SaveChangesAsync(ct);

                AppendLog(job, LogEntry.Info("AI đã phân tích thành công 6 trường nội dung."));
                Notify("ScheduledImportJob", $"Job '{job.Name}' hoàn thành. Đã phân tích nội dung từ {job.SourceUrl}");

                // Bước 5: Kết thúc thành công
                job.Status = CJobScheduleStatus.Completed;
                job.CompletedAt = DateTimeOffset.UtcNow;
                AppendLog(job, LogEntry.Info("Job hoàn thành thành công."));
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI phân tích thất bại cho job {JobId}", job.Id);
                job.Status = CJobScheduleStatus.Failed;
                job.ErrorMessage = $"AI phân tích thất bại: {ex.Message}";
                job.CompletedAt = DateTimeOffset.UtcNow;
                AppendLog(job, LogEntry.Error($"AI phân tích thất bại: {ex.Message}"));
                await db.SaveChangesAsync(ct);

                Notify("ScheduledImportJob",
                    $"Job '{job.Name}' thất bại khi phân tích AI: {ex.Message}");
            }
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
    }
}
