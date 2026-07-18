using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Models.Projects
{
    /// <summary>
    /// DTO dùng để tạo mới một dự án.
    /// Client chỉ cần gửi tên và mô tả; các trường còn lại (Code, Status, CreatedByUserId, …)
    /// được backend tự động sinh/default trong <c>ProjectService.CreateAsync</c>.
    /// </summary>
    public record ProjectCreateDto
    {
        /// <summary>
        /// Tên của dự án.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả về dự án.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một dự án hiện có. Nó bao gồm các thông tin cơ bản cần thiết để xác định và cập nhật một dự án trong hệ thống.
    /// </summary>
    public record ProjectUpdateDto : ProjectBaseDto
    {
        /// <summary>
        /// Id của dự án cần được cập nhật. Đây là một giá trị duy nhất để xác định dự án trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// DTO hủy projects
    /// </summary>
    public record CancelProjectDto
    {
        /// <summary>
        /// Lý do hủy
        /// </summary>
        public string? Reason { get; init; }
    }

    /// <summary>
    /// DTO dùng để trả về thông tin chi tiết của một dự án. Nó bao gồm các thông tin cơ bản cũng như Id duy nhất của dự án trong hệ thống.
    /// </summary>
    public record ProjectDetailDto : ProjectBaseDto
    {
        /// <summary>
        /// Id của dự án. Đây là một giá trị duy nhất để xác định dự án trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id của người dùng tạo dự án.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Tên người dùng tạo dự án.
        /// </summary>
        public string? CreatedByUserName { get; set; }

        /// <summary>
        /// Trạng thái xóa của dự án. Nếu giá trị là true, dự án đã bị xóa; nếu là false, dự án vẫn còn tồn tại trong hệ thống.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Thời gian tạo của dự án. Đây là thời điểm mà dự án được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }

        /// <summary>
        /// Thời gian sửa đổi cuối cùng của dự án. Đây là thời điểm mà thông tin của dự án được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }

        /// <summary>
        /// Thời gian xóa của dự án. Nếu dự án đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống. Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }

        /// <summary>
        /// Danh sách các bước của dự án.
        /// </summary>
        public List<ProjectStepDetailDto> Steps { get; set; } = new();
    }

    /// <summary>
    /// DTO dùng để đại diện cho các thuộc tính cơ bản của một dự án. Nó chứa các thông tin chung mà tất cả các DTO liên quan đến dự án đều có.
    /// </summary>
    public record ProjectBaseDto
    {
        /// <summary>
        /// Tên của dự án.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mã định danh của dự án.
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Mô tả về dự án.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu dự án.
        /// </summary>
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành dự án.
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// Thời gian thất bại của dự án.
        /// </summary>
        public DateTimeOffset? FailedAt { get; set; }

        /// <summary>
        /// Thông báo lỗi khi dự án thất bại.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Chi tiết lỗi khi dự án thất bại.
        /// </summary>
        public string? ErrorDetail { get; set; }

        /// <summary>
        /// Trạng thái của dự án.
        /// </summary>
        public CProjectStatus Status { get; set; }

        /// <summary>
        /// Tiến độ hoàn thành của dự án (tính bằng phần trăm).
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Tổng số bước của dự án.
        /// </summary>
        public int TotalSteps { get; set; }

        /// <summary>
        /// Số bước đã hoàn thành của dự án.
        /// </summary>
        public int CompletedSteps { get; set; }
    }
}