namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Trạng thái xử lý của request bên ngoài trong hàng đợi.
    /// Không auto-retry ngay khi bị chặn — chờ cooldown theo A.4 #3.
    /// </summary>
    public enum CExternalRequestStatus
    {
        /// <summary>
        /// Đang chờ xử lý (mặc định)
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Thành công
        /// </summary>
        Success = 1,

        /// <summary>
        /// Thất bại (lỗi không phải rate-limit)
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Bị giới hạn tốc độ (429/403) — chờ cooldown trước khi thử lại
        /// </summary>
        RateLimited = 3,

        /// <summary>
        /// Đã lên lịch thử lại (sau khoảng thời gian cooldown)
        /// </summary>
        RetryScheduled = 4,

        /// <summary>
        /// Đã bỏ qua (quá số lần thử hoặc admin huỷ thủ công)
        /// </summary>
        Abandoned = 5
    }
}
