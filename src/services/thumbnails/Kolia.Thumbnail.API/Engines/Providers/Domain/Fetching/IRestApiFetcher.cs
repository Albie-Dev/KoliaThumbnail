using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Engines.Social;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Fetcher cho nguồn tin dạng REST API.
    /// Gọi HTTP GET đến ApiEndpoint với headers/params cấu hình,
    /// parse JSON response theo ApiResponseJsonPath,
    /// map vào CrawledNewsItem.
    /// </summary>
    public interface IRestApiFetcher
    {
        /// <summary>
        /// Fetch tin tức từ REST API.
        /// </summary>
        /// <param name="source">NewsSourceEntity với FetchMode = RestApi</param>
        /// <param name="keywords">Keywords để filter/queries</param>
        /// <param name="cutoff">Chỉ lấy items có PublishedTime >= cutoff</param>
        /// <param name="maxCount">Số lượng items tối đa</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách CrawledNewsItem (rỗng nếu lỗi hoặc không có kết quả)</returns>
        Task<List<CrawledNewsItem>> FetchAsync(
            NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            CancellationToken ct);
    }
}
