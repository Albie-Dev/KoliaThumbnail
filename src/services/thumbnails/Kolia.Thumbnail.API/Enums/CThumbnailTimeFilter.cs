namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Bộ lọc thời gian khi tìm kiếm thumbnail tham khảo.
    /// Phải đồng bộ với bộ lọc view-reference — dùng chung 1 enum này (A.3 fix).
    /// </summary>
    public enum CThumbnailTimeFilter
    {
        /// <summary>
        /// Tuần này
        /// </summary>
        ThisWeek = 1,

        /// <summary>
        /// 1 tháng gần nhất
        /// </summary>
        OneMonth = 2,

        /// <summary>
        /// 3 tháng gần nhất
        /// </summary>
        ThreeMonths = 3,

        /// <summary>
        /// 6 tháng gần nhất
        /// </summary>
        SixMonths = 4,

        /// <summary>
        /// 1 năm gần nhất
        /// </summary>
        OneYear = 5
    }
}
