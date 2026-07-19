namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Bộ lọc số lượng tin tức trả về
    /// </summary>
    public enum CNewsCountFilter
    {
        /// <summary>
        /// Top 10 tin nổi bật nhất
        /// </summary>
        Top10 = 1,

        /// <summary>
        /// Top 20 tin nổi bật nhất
        /// </summary>
        Top20 = 2,

        /// <summary>
        /// Top 30 tin nổi bật nhất
        /// </summary>
        Top30 = 3,

        /// <summary>
        /// Lấy tất cả tin (không giới hạn số lượng)
        /// </summary>
        All = 4
    }
}
