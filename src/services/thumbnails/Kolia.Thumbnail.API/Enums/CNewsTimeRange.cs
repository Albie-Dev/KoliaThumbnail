namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Khoảng thời gian lọc tin tức khi tìm kiếm.
    /// Lưu ý: Last30Days rất nặng về tài nguyên, hệ thống sẽ cảnh báo khi chọn.
    /// </summary>
    public enum CNewsTimeRange
    {
        /// <summary>
        /// 24 giờ gần nhất
        /// </summary>
        Last24Hours = 1,

        /// <summary>
        /// 48 giờ gần nhất
        /// </summary>
        Last48Hours = 2,

        /// <summary>
        /// 72 giờ gần nhất
        /// </summary>
        Last72Hours = 3,

        /// <summary>
        /// 7 ngày gần nhất (khuyến nghị — hiệu quả nhất)
        /// </summary>
        Last7Days = 4,

        /// <summary>
        /// 30 ngày gần nhất (cảnh báo: rất nặng, khó khả thi)
        /// </summary>
        Last30Days = 5
    }
}
