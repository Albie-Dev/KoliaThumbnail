using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.News
{
    /// <summary>
    /// Whitelist nguồn tin tài chính được duyệt.
    /// Admin quản lý danh sách này để RSS Engine ưu tiên fetch.
    /// </summary>
    public class NewsSourceEntity : BaseEntity
    {
        /// <summary>
        /// Tên nguồn tin, vd "VnExpress", "CoinDesk", "Federal Reserve"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// URL RSS feed hoặc Sitemap để crawl
        /// </summary>
        public string RssOrFeedUrl { get; set; } = null!;

        /// <summary>
        /// Phạm vi thị trường của nguồn tin (Domestic/International)
        /// </summary>
        public CMarketScope Region { get; set; }

        /// <summary>
        /// True nếu đây là nguồn uy tín đã được team kiểm chứng
        /// </summary>
        public bool IsTrusted { get; set; } = true;

        /// <summary>
        /// Thứ tự ưu tiên khi fetch (số nhỏ = ưu tiên cao hơn)
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}
