using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.BackgroundJobs
{
    /// <summary>
    /// Background service xử lý hàng đợi retry cho các External Request bị rate-limit hoặc lỗi.
    /// Chạy kiểm tra mỗi 5 phút, xử lý tối đa 20 item mỗi lần.
    /// Backoff tăng dần: (RetryCount + 1) * 30 phút.
    /// </summary>
    public class ExternalRequestRetryJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExternalRequestRetryJob> _logger;

        public ExternalRequestRetryJob(
            IServiceScopeFactory scopeFactory,
            ILogger<ExternalRequestRetryJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExternalRequestRetryJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                    var executor = scope.ServiceProvider.GetRequiredService<ISocialExecutorService>();

                    var dueItems = await db.ExternalRequestQueues
                        .Where(q => q.Status == CExternalRequestStatus.Pending
                                 && q.NextRetryAt != null
                                 && q.NextRetryAt <= DateTimeOffset.UtcNow)
                        .OrderBy(q => q.NextRetryAt)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    foreach (var item in dueItems)
                    {
                        try
                        {
                            // Deserialize payload và retry theo từng loại purpose
                            var payload = System.Text.Json.JsonSerializer.Deserialize<RetryPayload>(item.PayloadJson);
                            if (payload == null)
                            {
                                item.Status = CExternalRequestStatus.Abandoned;
                                item.ErrorMessage = "Payload không thể deserialize.";
                                continue;
                            }

                            switch (item.Purpose)
                            {
                                case CExternalRequestPurpose.YoutubeVideoSearch:
                                case CExternalRequestPurpose.ThumbnailCrawl:
                                    await executor.YouTubeSearchAsync(
                                        payload.Keyword ?? string.Empty,
                                        payload.TimeFilter ?? CThumbnailTimeFilter.ThisWeek,
                                        payload.SortFilter ?? CThumbnailSortFilter.MostRelevant,
                                        payload.MaxResults ?? 15,
                                        item.ProjectId,
                                        stoppingToken);
                                    break;

                                case CExternalRequestPurpose.NewsCrawl:
                                    // RSS crawl — retry with persisted MarketScope (Bug fix: was hard-coded to Domestic)
                                    await executor.RssCrawlAsync(
                                        (payload.Keyword ?? string.Empty).Split(','),
                                        payload.MarketScope ?? CMarketScope.Domestic,
                                        payload.TimeRangeDays ?? 7,
                                        payload.MaxResults ?? 10,
                                        item.ProjectId,
                                        null,
                                        stoppingToken);
                                    break;

                                default:
                                    item.Status = CExternalRequestStatus.Abandoned;
                                    item.ErrorMessage = $"Purpose {item.Purpose} chưa hỗ trợ retry tự động.";
                                    continue;
                            }

                            item.Status = CExternalRequestStatus.Success;
                            item.CompletedAt = DateTimeOffset.UtcNow;
                            _logger.LogInformation("Retry thành công cho queue item {ItemId}", item.Id);
                        }
                        catch (Exception ex)
                        {
                            item.RetryCount++;
                            item.ErrorMessage = ex.Message;

                            if (item.RetryCount >= 5)
                            {
                                item.Status = CExternalRequestStatus.Abandoned;
                                item.CompletedAt = DateTimeOffset.UtcNow;
                                _logger.LogWarning("Queue item {ItemId} đã thử {RetryCount} lần, chuyển sang Abandoned.", item.Id, item.RetryCount);
                            }
                            else
                            {
                                // Backoff: (RetryCount + 1) * 30 phút
                                item.NextRetryAt = DateTimeOffset.UtcNow.AddMinutes((item.RetryCount + 1) * 30);
                                item.Status = CExternalRequestStatus.RetryScheduled;
                                _logger.LogWarning("Queue item {ItemId} thất bại lần {RetryCount}, retry sau {Minutes} phút.",
                                    item.Id, item.RetryCount, (item.RetryCount + 1) * 30);
                            }
                        }
                    }

                    if (dueItems.Count > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý ExternalRequestRetryJob.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("ExternalRequestRetryJob stopped.");
        }

        private sealed record RetryPayload(
            string? Keyword,
            CThumbnailTimeFilter? TimeFilter,
            CThumbnailSortFilter? SortFilter,
            int? MaxResults,
            int? TimeRangeDays,
            CMarketScope? MarketScope);
    }
}
