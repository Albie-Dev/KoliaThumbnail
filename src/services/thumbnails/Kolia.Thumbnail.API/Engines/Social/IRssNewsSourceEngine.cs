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
        /// <summary>
        /// Crawl tin tức từ danh sách nguồn ưu tiên + keyword.
        /// timeRangeDays: số ngày về trước (7/30 ngày).
        /// maxCount: giới hạn số tin trả về.
        /// </summary>
        Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords,
            CMarketScope marketScope,
            int timeRangeDays,
            int maxCount,
            CancellationToken ct = default);

        /// <summary>
        /// Fetch và parse nội dung 1 URL bài báo (dùng cho import thủ công).
        /// Trả null nếu không thể fetch.
        /// </summary>
        Task<CrawledNewsItem?> FetchSingleAsync(string url, CancellationToken ct = default);
    }
}
