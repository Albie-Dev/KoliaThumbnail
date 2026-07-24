namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Mức độ liên quan của tin tức hoặc phân tích sâu đến chủ đề video
    /// </summary>
    public enum CRelevanceLevel
    {
        /// <summary>
        /// Liên quan cao — trực tiếp ảnh hưởng đến chủ đề
        /// </summary>
        High = 1,

        /// <summary>
        /// Liên quan trung bình — gián tiếp hoặc một phần
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Liên quan thấp — ít ảnh hưởng đến chủ đề
        /// </summary>
        Low = 3,

        /// <summary>
        /// Chưa xác định
        /// </summary>
        None = 4
    }
}
