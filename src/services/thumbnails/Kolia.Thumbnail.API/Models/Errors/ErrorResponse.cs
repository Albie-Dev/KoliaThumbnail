namespace Kolia.Thumbnail.API.Models
{
    /// <summary>
    /// Đại diện cho phản hồi lỗi trả về cho client.
    /// </summary>
    public sealed class ErrorResponse
    {
        /// <summary>
        /// Mã lỗi đại diện cho loại lỗi xảy ra.
        /// </summary>
        public required string Code { get; init; }
        /// <summary>
        /// Thông điệp lỗi mô tả chi tiết về lỗi xảy ra.
        /// </summary>

        public required string Message { get; init; }

        /// <summary>
        /// Mã định danh theo dõi (trace identifier) của yêu cầu, giúp xác định và theo dõi lỗi trong hệ thống.
        /// Thông tin này hữu ích cho việc gỡ lỗi và phân tích sự cố.
        /// </summary>
        public string? TraceId { get; init; }

        /// <summary>
        /// Danh sách các lỗi xác thực (validation errors) liên quan đến yêu cầu, nếu có.
        /// </summary>
        public IReadOnlyCollection<ValidationError>? Errors { get; init; }
    }
    

    /// <summary>
    /// Đại diện cho một lỗi xác thực (validation error) trong phản hồi lỗi trả về cho client.
    /// Mỗi lỗi xác thực bao gồm tên của trường (property) bị lỗi, thông điệp lỗi mô tả chi tiết về lỗi và mã lỗi đại diện cho loại lỗi xảy ra.
    /// </summary>
    public sealed class ValidationError
    {
        /// <summary>
        /// Tên của trường (property) bị lỗi.
        /// </summary>
        public required string Property { get; init; }
        /// <summary>
        /// Thông điệp lỗi mô tả chi tiết về lỗi xảy ra.
        /// </summary>
        public required string Message { get; init; }
        /// <summary>
        /// Mã lỗi đại diện cho loại lỗi xảy ra.
        /// </summary>
        public required string ErrorCode { get; init; }
    }
}