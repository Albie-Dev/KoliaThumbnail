using Kolia.Thumbnail.API.Engines.Providers.Domain;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Social
{
    /// <summary>
    /// Executor trung gian xử lý rate-limit cho tất cả Social API calls (YouTube...).
    /// Triển khai: round-robin multi-key, cooldown sau 429/403, ghi ExternalRequestQueue khi cần retry.
    /// Không service nào được gọi IYouTubeSearchEngine trực tiếp — phải đi qua đây.
    /// </summary>
    public interface ISocialExecutorService
    {
        /// <summary>
        /// Tìm kiếm YouTube với key rotation tự động.
        /// Nếu tất cả keys bị rate-limit → ghi ExternalRequestQueueEntity và throw RateLimitException.
        /// </summary>
        Task<IReadOnlyList<YouTubeVideoResult>> YouTubeSearchAsync(
            string keyword,
            CThumbnailTimeFilter timeFilter,
            CThumbnailSortFilter sortFilter,
            int maxResults,
            Guid? projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Fetch 1 video YouTube by URL với key rotation.
        /// </summary>
        Task<YouTubeVideoResult?> YouTubeFetchByUrlAsync(string videoUrl, Guid? projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Crawl tin tức RSS — không cần key rotation nhưng cần ghi queue khi lỗi.
        /// </summary>
        Task<IReadOnlyList<CrawledNewsItem>> RssCrawlAsync(
            IEnumerable<string> keywords,
            CMarketScope marketScope,
            int timeRangeDays,
            int maxCount,
            Guid? projectId,
            Action<NewsSourceSearchLog>? onSourceSearched = null,
            CancellationToken ct = default);

        Task<CMarketScope> DetectMarketScopeAsync(string url, CancellationToken ct = default);
    }
}
