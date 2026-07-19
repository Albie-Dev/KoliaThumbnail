namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Công cụ chỉnh sửa được dùng khi sửa thumbnail đã generate (lưu lịch sử thao tác)
    /// </summary>
    public enum CThumbnailEditTool
    {
        /// <summary>
        /// Sửa ảnh tổng thể
        /// </summary>
        Image = 1,

        /// <summary>
        /// Sửa chữ hiển thị trên thumbnail
        /// </summary>
        Text = 2,

        /// <summary>
        /// Đổi phong cách (style)
        /// </summary>
        Style = 3,

        /// <summary>
        /// Đổi biểu cảm/cử chỉ nhân vật (theo ảnh avatar đã upload)
        /// </summary>
        Avatar = 4
    }
}
