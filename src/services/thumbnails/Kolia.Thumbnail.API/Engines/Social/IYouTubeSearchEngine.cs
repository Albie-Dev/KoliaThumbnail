using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Social
{
    /// <summary>
    /// Kết quả 1 video YouTube tìm kiếm được.
    /// </summary>
    public record YouTubeVideoResult(
        string VideoId,
        string Title,
        string ChannelName,
        string ThumbnailImageUrl,
        string VideoUrl,
        DateTimeOffset? PublishedTime,
        long? ViewCount);

    /// <summary>
    /// Engine tìm kiếm YouTube (Phần 3).
    /// Hỗ trợ multi-key rotation — không gọi trực tiếp, đi qua SocialExecutorService.
    /// </summary>
    public interface IYouTubeSearchEngine
    {
        /// <summary>
        /// Tìm kiếm video theo keyword với bộ lọc thời gian và sắp xếp.
        /// Gọi ra ngoài API YouTube Data v3 — BỊ RATE-LIMIT — phải đi qua SocialExecutorService.
        /// </summary>
        Task<IReadOnlyList<YouTubeVideoResult>> SearchAsync(
            string keyword,
            CThumbnailTimeFilter timeFilter,
            CThumbnailSortFilter sortFilter,
            int maxResults,
            CancellationToken ct = default);

        /// <summary>
        /// Fetch thông tin 1 video theo URL (dùng cho import thủ công).
        /// </summary>
        Task<YouTubeVideoResult?> FetchByUrlAsync(string videoUrl, CancellationToken ct = default);
    }
}
