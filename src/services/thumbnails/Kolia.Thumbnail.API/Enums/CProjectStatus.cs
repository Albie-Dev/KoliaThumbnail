namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Trạng thái dự án
    /// </summary>
    public enum CProjectStatus
    {
        /// <summary>
        /// Mới tạo
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Đang chờ xử lý
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Đang xử lý
        /// </summary>
        Running = 2,

        /// <summary>
        /// Tạm dừng
        /// </summary>
        Paused = 3,

        /// <summary>
        /// Hoàn thành
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Thất bại
        /// </summary>
        Failed = 5,

        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 6
    }
}