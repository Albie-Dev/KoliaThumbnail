namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Trạng thái duyệt của item trong Thumbnail Library do người dùng xác nhận
    /// </summary>
    public enum CLibraryUserStatus
    {
        /// <summary>
        /// Chờ xem xét (mặc định khi thêm vào library)
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã duyệt — dùng làm tham khảo
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Đã từ chối — không dùng
        /// </summary>
        Rejected = 2
    }
}
