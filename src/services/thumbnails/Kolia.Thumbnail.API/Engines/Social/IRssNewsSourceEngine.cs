using Kolia.Thumbnail.API.Engines.Providers.Domain;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Social
{
    /// <summary>
    /// Kết quả 1 bản tin crawl được từ RSS/web.
    /// </summary>
    public record CrawledNewsItem(
        string Title,
        string SourceName,
        string SourceUrl,
        CMarketScope MarketType,
        DateTimeOffset? PublishedTime,
        string SummaryRaw);

    /// <summary>
    /// Engine crawl tin tức từ RSS/web (Phần 2).
    /// </summary>
    public interface IRssNewsSourceEngine
    {
        Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords,
            CMarketScope marketScope,
            int timeRangeDays,
            int maxCount,
            Action<NewsSourceSearchLog>? onSourceSearched = null,
            CancellationToken ct = default);

        Task<CrawledNewsItem?> FetchSingleAsync(string url, CancellationToken ct = default);
        Task<CMarketScope> DetectScopeForUrlAsync(string url, CancellationToken ct = default);
    }
}
