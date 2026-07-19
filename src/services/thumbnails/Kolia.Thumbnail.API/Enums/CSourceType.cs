namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Loại nguồn dữ liệu — crawl tự động hay nhập link thủ công
    /// </summary>
    public enum CSourceType
    {
        /// <summary>
        /// Dữ liệu được crawl tự động theo keyword
        /// </summary>
        Crawled = 1,

        /// <summary>
        /// Dữ liệu được nhập thủ công qua link ngoài
        /// </summary>
        ManualLink = 2
    }
}
