namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Trạng thái các bước của dự án
    /// </summary>
    public enum CProjectStepStatus
    {
        /// <summary>
        /// Chưa bắt đầu
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Đang làm
        /// </summary>
        InProgress = 1,
        /// <summary>
        /// Đã hoàn thành
        /// </summary>
        Completed = 2,
        /// <summary>
        /// Lỗi
        /// </summary>
        Failed = 3,
        /// <summary>
        /// Bỏ qua
        /// </summary>
        Skipped = 4
    }
}