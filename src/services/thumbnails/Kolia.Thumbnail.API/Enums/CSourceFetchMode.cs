namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Phương thức fetch dữ liệu cho từng NewsSource.
    /// Quyết định tier nào được thử đầu tiên trong SourceFetchPipeline.
    /// </summary>
    public enum CSourceFetchMode
    {
        /// <summary>
        /// Fetch RSS/Atom feed trực tiếp từ URL nguồn.
        /// Tier 1 (chính), KHÔNG tự động fallback sang Google News hoặc Sitemap.
        /// </summary>
        RssDirect = 1,

        /// <summary>
        /// Fetch RSS trực tiếp trước (Tier 1), nếu thất bại thì tự động thử
        /// Google News RSS site-restricted (Tier 2).
        /// </summary>
        GoogleNewsFallback = 2,

        /// <summary>
        /// Bỏ qua RSS, sử dụng Google News RSS site-restricted (Tier 2) làm
        /// nguồn chính, nếu thất bại thì thử Sitemap.xml (Tier 3).
        /// Dùng cho các nguồn có paywall hoặc không có RSS công khai.
        /// </summary>
        GoogleNewsSiteRestricted = 3,

        /// <summary>
        /// Thử RSS trước, nếu thất bại thử Sitemap.xml (Tier 3).
        /// Bỏ qua Google News. Dùng cho các nguồn có sitemap ổn định.
        /// </summary>
        SitemapFallback = 4,

        /// <summary>
        /// Thử Sitemap hoặc Google News (Tier 2/3), không có RSS trực tiếp.
        /// Dùng cho nguồn không có RSS công khai lẫn feed ổn định.
        /// </summary>
        SitemapOrGoogleNews = 5,

        /// <summary>
        /// Custom scraper riêng (vd: giá vàng SJC/PNJ/DOJI).
        /// Cần implement IGoldPriceFetcher hoặc scraper tương tự.
        /// </summary>
        Custom = 6
    }
}
