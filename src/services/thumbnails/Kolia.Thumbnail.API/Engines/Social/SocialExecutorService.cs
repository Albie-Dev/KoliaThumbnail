using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.ExternalRequests;
using Kolia.Thumbnail.API.Engines.Providers.Domain;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Engines.Social
{
    /// <summary>
    /// Thực thi Social API calls với round-robin key rotation và cooldown sau rate-limit.
    /// Không service nào được gọi YouTube trực tiếp — bắt buộc đi qua đây.
    /// </summary>
    public class SocialExecutorService : ISocialExecutorService
    {
        private readonly IYouTubeSearchEngine _youTubeEngine;
        private readonly IRssNewsSourceEngine _rssEngine;
        private readonly ThumbnailDbContext _db;
        private readonly ILogger<SocialExecutorService> _logger;

        public SocialExecutorService(
            IYouTubeSearchEngine youTubeEngine,
            IRssNewsSourceEngine rssEngine,
            ThumbnailDbContext db,
            ILogger<SocialExecutorService> logger)
        {
            _youTubeEngine = youTubeEngine;
            _rssEngine = rssEngine;
            _db = db;
            _logger = logger;
        }

        public async Task<IReadOnlyList<YouTubeVideoResult>> YouTubeSearchAsync(
            string keyword,
            CThumbnailTimeFilter timeFilter,
            CThumbnailSortFilter sortFilter,
            int maxResults,
            Guid? projectId,
            CancellationToken ct = default)
        {
            var candidateConfigs = await _db.SocialMediaProviderConfigurations
                .Include(c => c.SocialMediaProvider)
                .Where(c => c.IsEnabled && c.SocialMediaProvider.ProviderType == CSocialMediaProviderType.Youtube)
                .OrderBy(c => c.Priority)
                .ToListAsync(ct);

            var now = DateTimeOffset.UtcNow;
            var usableConfigs = candidateConfigs
                .Where(c => c.LastRateLimitedAt == null
                         || now >= c.LastRateLimitedAt.Value.AddMinutes(c.RateLimitCooldownMinutes))
                .ToList();

            if (usableConfigs.Count == 0)
            {
                await EnqueueRetryAsync(CExternalRequestPurpose.YoutubeVideoSearch, keyword, timeFilter, sortFilter, maxResults, projectId, ct);
                throw new TooManyRequestsException("Tất cả API key YouTube đang bị rate-limit. Yêu cầu đã được ghi vào hàng đợi để thử lại sau.");
            }

            foreach (var config in usableConfigs)
            {
                try
                {
                    var results = await _youTubeEngine.SearchAsync(keyword, timeFilter, sortFilter, maxResults, ct);
                    await LogUsageAsync(config.SocialMediaProvider.ProviderType, ct);
                    return results;
                }
                catch (ExternalServiceException)
                {
                    config.LastRateLimitedAt = now;
                    await _db.SaveChangesAsync(ct);
                    await LogUsageAsync(config.SocialMediaProvider.ProviderType, ct);
                    _logger.LogWarning("YouTube key {ConfigId} bị rate-limit, thử key tiếp theo.", config.Id);
                }
            }

            await EnqueueRetryAsync(CExternalRequestPurpose.YoutubeVideoSearch, keyword, timeFilter, sortFilter, maxResults, projectId, ct);
            throw new TooManyRequestsException("Tất cả API key YouTube đã thử đều bị rate-limit. Yêu cầu đã được ghi vào hàng đợi.");
        }

        public async Task<YouTubeVideoResult?> YouTubeFetchByUrlAsync(string videoUrl,
            Guid? projectId, CancellationToken ct = default)
        {
            _logger.LogInformation("YouTubeFetchByUrlAsync: {Url}", videoUrl);
            return await _youTubeEngine.FetchByUrlAsync(videoUrl, ct);
        }

        public async Task<IReadOnlyList<CrawledNewsItem>> RssCrawlAsync(
            IEnumerable<string> keywords,
            CMarketScope marketScope,
            int timeRangeDays,
            int maxCount,
            Guid? projectId,
            Action<NewsSourceSearchLog>? onSourceSearched = null,
            CancellationToken ct = default)
        {
            var keywordList = keywords.ToList();
            _logger.LogInformation("RssCrawlAsync: scope={Scope}, days={Days}, keywords={Keywords}",
                marketScope, timeRangeDays, string.Join(",", keywordList));
            try
            {
                var items = await _rssEngine.CrawlAsync(
                    keywordList, marketScope, timeRangeDays, maxCount,
                    onSourceSearched: log =>
                    {
                        if (log.Success)
                            _logger.LogInformation("RSS source '{Source}' [{Keywords}] → {Count} tin{Cache}",
                                log.SourceName, log.Keywords, log.ResultCount,
                                log.ServedFromCache ? " (cache)" : "");
                        else
                            _logger.LogWarning("RSS source '{Source}' [{Keywords}] lỗi: {Error}",
                                log.SourceName, log.Keywords, log.ErrorMessage);

                        onSourceSearched?.Invoke(log);
                    },
                    ct);

                if (items.Count == 0)
                {
                    _logger.LogWarning(
                        "RssCrawlAsync: 0 results after full fallback pipeline for keywords={Keywords}",
                        string.Join(",", keywordList));
                    await EnqueueNewsCrawlRetryAsync(keywordList, marketScope, timeRangeDays, maxCount, projectId, ct);
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RssCrawlAsync: unexpected error outside pipeline fallback.");
                await EnqueueNewsCrawlRetryAsync(keywordList, marketScope, timeRangeDays, maxCount, projectId, ct);
                throw new ExternalServiceException(
                    "RSS crawl thất bại toàn bộ — yêu cầu đã được ghi vào hàng đợi.", ex);
            }
        }

        private async Task EnqueueNewsCrawlRetryAsync(
            List<string> keywords, CMarketScope marketScope, int timeRangeDays,
            int maxResults, Guid? projectId, CancellationToken ct)
        {
            _db.ExternalRequestQueues.Add(new ExternalRequestQueueEntity
            {
                Purpose = CExternalRequestPurpose.NewsCrawl,
                ProjectId = projectId,
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    keyword = string.Join(",", keywords),
                    marketScope,
                    timeRangeDays,
                    maxResults
                }),
                Status = CExternalRequestStatus.Pending,
                NextRetryAt = DateTimeOffset.UtcNow.AddMinutes(30),
                RetryCount = 0
            });
            await _db.SaveChangesAsync(ct);
        }

        public async Task<CMarketScope> DetectMarketScopeAsync(string url, CancellationToken ct = default)
            => await _rssEngine.DetectScopeForUrlAsync(url, ct);

        // ── Private Helpers ──────────────────────────────────────────

        private async Task EnqueueRetryAsync(
            CExternalRequestPurpose purpose, string keyword, CThumbnailTimeFilter? timeFilter,
            CThumbnailSortFilter? sortFilter, int maxResults, Guid? projectId, CancellationToken ct)
        {
            _db.ExternalRequestQueues.Add(new ExternalRequestQueueEntity
            {
                Purpose = purpose,
                ProjectId = projectId,
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { keyword, timeFilter, sortFilter, maxResults }),
                Status = CExternalRequestStatus.Pending,
                NextRetryAt = DateTimeOffset.UtcNow.AddMinutes(30),
                RetryCount = 0
            });
            await _db.SaveChangesAsync(ct);
        }

        private async Task LogUsageAsync(CSocialMediaProviderType providerType, CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
            var existingLog = await _db.ExternalRequestUsageLogs
                .FirstOrDefaultAsync(l => l.SocialMediaProviderType == providerType
                                       && l.RecordedDate == today, ct);

            if (existingLog != null)
            {
                existingLog.RequestCount++;
            }
            else
            {
                _db.ExternalRequestUsageLogs.Add(new ExternalRequestUsageLogEntity
                {
                    SocialMediaProviderType = providerType,
                    RequestCount = 1,
                    RecordedDate = today
                });
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}
