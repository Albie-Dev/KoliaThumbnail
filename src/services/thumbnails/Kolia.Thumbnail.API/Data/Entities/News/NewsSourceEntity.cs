using Kolia.Thumbnail.API.Attributes;
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
        [Queryable(Searchable = true, Sortable = true)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// URL RSS feed hoặc Sitemap để crawl.
        /// Với các nguồn không có RSS ổn định, lưu URL trang chủ/section để dùng làm base
        /// cho Tier 2 (Google News site-restricted) hoặc Tier 3 (sitemap).
        /// </summary>
        public string RssOrFeedUrl { get; set; } = null!;

        /// <summary>
        /// Phạm vi thị trường của nguồn tin (Domestic/International)
        /// </summary>
        [Queryable(Filterable = true)]
        public CMarketScope Region { get; set; }

        /// <summary>
        /// True nếu đây là nguồn uy tín đã được team kiểm chứng
        /// </summary>
        [Queryable(Filterable = true)]
        public bool IsTrusted { get; set; } = true;

        /// <summary>
        /// Thứ tự ưu tiên khi fetch (số nhỏ = ưu tiên cao hơn)
        /// </summary>
        [Queryable(Filterable = true, Sortable = true, RangeFilterable = true)]
        public int Priority { get; set; } = 0;

        // ── New fields for multi-source fallback pipeline ──────────────

        /// <summary>
        /// Nhóm nguồn theo spec khách hàng (1-5, xem enum CNewsSourceGroup)
        /// </summary>
        [Queryable(Filterable = true)]
        public CNewsSourceGroup SourceGroup { get; set; }

        /// <summary>
        /// Cách fetch: RssDirect, GoogleNewsFallback, SitemapFallback, Custom…
        /// Quyết định tier nào được thử đầu tiên trong SourceFetchPipeline.
        /// </summary>
        [Queryable(Filterable = true)]
        public CSourceFetchMode FetchMode { get; set; } = CSourceFetchMode.RssDirect;

        /// <summary>
        /// Domain gốc (vd "cafef.vn") — dùng để tra DomainRateLimiterRegistry
        /// </summary>
        [Queryable(Searchable = true, Sortable = true)]
        public string Domain { get; set; } = null!;

        /// <summary>
        /// Thời điểm domain này bị lỗi/429 gần nhất (cập nhật bởi SourceFetchPipeline)
        /// </summary>
        public DateTimeOffset? LastFailedAt { get; set; }

        /// <summary>
        /// Số lỗi liên tiếp — dùng cho circuit breaker cấp DB (bổ trợ cho in-memory).
        /// Reset về 0 khi fetch thành công.
        /// </summary>
        [Queryable(Filterable = true)]
        public int ConsecutiveFailureCount { get; set; } = 0;

        /// <summary>
        /// ETag của lần fetch gần nhất — dùng conditional GET (If-None-Match)
        /// </summary>
        public string? LastEtag { get; set; }

        /// <summary>
        /// Giá trị Last-Modified header của lần fetch gần nhất — dùng conditional GET
        /// </summary>
        public string? LastModifiedHeader { get; set; }

        /// <summary>
        /// Thời điểm fetch thành công gần nhất
        /// </summary>
        [Queryable(Sortable = true, RangeFilterable = true)]
        public DateTimeOffset? LastFetchedAt { get; set; }
    }
}

