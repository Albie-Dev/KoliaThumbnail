namespace Kolia.Thumbnail.API.Models.Projects
{
    /// <summary>
    /// DTO dùng để tạo mới một định nghĩa bước thực hiện. Nó chứa các thông tin cơ bản cần thiết để định nghĩa một định nghĩa bước trong hệ thống.
    /// </summary>
    public record StepDefinitionCreateDto : StepDefinitionBaseDto
    {
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một định nghĩa bước hiện có. Nó bao gồm các thông tin cơ bản cần thiết để xác định và cập nhật một định nghĩa bước trong hệ thống.
    /// </summary>
    public record StepDefinitionUpdateDto : StepDefinitionBaseDto
    {
        /// <summary>
        /// Id của định nghĩa bước cần được cập nhật. Đây là một giá trị duy nhất để xác định định nghĩa bước trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// DTO dùng để trả về thông tin chi tiết của một định nghĩa bước. Nó bao gồm các thông tin cơ bản cũng như Id duy nhất của định nghĩa bước trong hệ thống.
    /// </summary>
    public record StepDefinitionDetailDto : StepDefinitionBaseDto
    {
        /// <summary>
        /// Id của định nghĩa bước. Đây là một giá trị duy nhất để xác định định nghĩa bước trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Trạng thái xóa của định nghĩa bước. Nếu giá trị là true, định nghĩa bước đã bị xóa; nếu là false, định nghĩa bước vẫn còn tồn tại trong hệ thống.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Thời gian tạo của định nghĩa bước. Đây là thời điểm mà định nghĩa bước được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }

        /// <summary>
        /// Thời gian sửa đổi cuối cùng của định nghĩa bước. Đây là thời điểm mà thông tin của định nghĩa bước được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }

        /// <summary>
        /// Thời gian xóa của định nghĩa bước. Nếu định nghĩa bước đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống. Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }

        /// <summary>
        /// Id của định nghĩa bước cha (nếu có).
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Danh sách các định nghĩa bước con.
        /// </summary>
        public List<StepDefinitionDetailDto> Children { get; set; } = new();
    }

    /// <summary>
    /// DTO dùng để đại diện cho các thuộc tính cơ bản của một định nghĩa bước. Nó chứa các thông tin chung mà tất cả các DTO liên quan đến định nghĩa bước đều có.
    /// </summary>
    public record StepDefinitionBaseDto
    {
        /// <summary>
        /// Mã định danh của định nghĩa bước (ví dụ: "video_content", "news", "thumbnail_reference").
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Tên hiển thị của định nghĩa bước (ví dụ: "Nội dung video", "Tin tức").
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Thứ tự hiển thị trong cùng một cấp (giữa các anh em), KHÔNG mang ý nghĩa cấp bậc.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Nhãn hiển thị đầy đủ (ví dụ: "1", "2", "3", "3.1", "4", "4.1", "4.2", "5", "6").
        /// </summary>
        public string DisplayCode { get; set; } = null!;

        /// <summary>
        /// True nếu bước này thực sự sinh ra 1 ProjectStep để track trạng thái/nội dung.
        /// False nếu chỉ là bước "nhóm" cho UI.
        /// </summary>
        public bool IsTrackable { get; set; }
    }
}