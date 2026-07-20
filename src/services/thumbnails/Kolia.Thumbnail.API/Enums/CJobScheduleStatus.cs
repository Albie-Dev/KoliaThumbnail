namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Trạng thái của một Scheduled Import Job.
    /// </summary>
    public enum CJobScheduleStatus
    {
        /// <summary>Đang chờ đến lúc chạy</summary>
        Pending = 1,

        /// <summary>Đang chạy</summary>
        Running = 2,

        /// <summary>Đã hoàn thành thành công</summary>
        Completed = 3,

        /// <summary>Thất bại — có lỗi xảy ra</summary>
        Failed = 4,

        /// <summary>Đã bị huỷ</summary>
        Cancelled = 5
    }
}
