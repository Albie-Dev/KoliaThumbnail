namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Mục đích của request bên ngoài trong hàng đợi quản lý
    /// </summary>
    public enum CExternalRequestPurpose
    {
        /// <summary>
        /// Crawl tin tức theo keyword
        /// </summary>
        NewsCrawl = 1,

        /// <summary>
        /// Crawl thumbnail theo keyword (YouTube search)
        /// </summary>
        ThumbnailCrawl = 2,

        /// <summary>
        /// Tìm kiếm video YouTube
        /// </summary>
        YoutubeVideoSearch = 3,

        /// <summary>
        /// Import thủ công qua link ngoài
        /// </summary>
        ManualLinkImport = 4
    }
}
