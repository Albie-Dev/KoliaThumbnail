using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.ExternalRequests;
using Kolia.Thumbnail.API.Data.Entities.Socials;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            CancellationToken ct = default)
        {
            _logger.LogInformation("RssCrawlAsync: scope={Scope}, days={Days}", marketScope, timeRangeDays);
            try
            {
                return await _rssEngine.CrawlAsync(keywords, marketScope, timeRangeDays, maxCount, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                await EnqueueRetryAsync(CExternalRequestPurpose.NewsCrawl, string.Join(",", keywords), null, null, maxCount, projectId, ct);
                throw new ExternalServiceException("RSS crawl thất bại — yêu cầu đã được ghi vào hàng đợi.", ex);
            }
        }

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
