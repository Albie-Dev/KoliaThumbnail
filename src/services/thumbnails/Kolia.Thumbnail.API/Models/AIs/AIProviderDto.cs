using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Models.AIs
{
    /// <summary>
    /// DTO (Data Transfer Object) dùng để tạo mới một nhà cung cấp AI. Nó chứa các thông tin cơ bản cần thiết để định nghĩa một nhà cung cấp AI trong hệ thống.
    /// </summary>
    public class AIProviderCreateDto : AIProviderBaseDto
    {
    }

    /// <summary>
    /// DTO (Data Transfer Object) dùng để cập nhật thông tin của một nhà cung cấp AI hiện có. Nó bao gồm các thông tin cơ bản cần thiết để xác định và cập nhật một nhà cung cấp AI trong hệ thống.
    /// </summary>
    public class AIProviderUpdateDto : AIProviderBaseDto
    {
        /// <summary>
        /// Id của nhà cung cấp AI cần được cập nhật. Đây là một giá trị duy nhất để xác định nhà cung cấp AI trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// DTO (Data Transfer Object) dùng để trả về thông tin chi tiết của một nhà cung cấp AI. Nó bao gồm các thông tin cơ bản cũng như Id duy nhất của nhà cung cấp AI trong hệ thống.
    /// </summary>
    public class AIProviderDetailDto : AIProviderBaseDto
    {
        /// <summary>
        /// Id của nhà cung cấp AI. Đây là một giá trị duy nhất để xác định nhà cung cấp AI trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Trạng thái xóa của nhà cung cấp AI. Nếu giá trị là true, nhà cung cấp AI đã bị xóa; nếu là false, nhà cung cấp AI vẫn còn tồn tại trong hệ thống.
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Thời gian tạo của nhà cung cấp AI. Đây là thời điểm mà nhà cung cấp AI được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }
        /// <summary>
        /// Thời gian sửa đổi cuối cùng của nhà cung cấp AI. Đây là thời điểm mà thông tin của nhà cung cấp AI được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }
        /// <summary>
        /// Thời gian xóa của nhà cung cấp AI. Nếu nhà cung cấp AI đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống. Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }
    }

    /// <summary>
    /// DTO (Data Transfer Object) dùng để đại diện cho các thuộc tính cơ bản của một nhà cung cấp AI. Nó chứa các thông tin chung mà tất cả các DTO liên quan đến nhà cung cấp AI đều có.
    /// </summary>
    public class AIProviderBaseDto
    {
        /// <summary>
        /// Tên của nhà cung cấp AI. Ví dụ: OpenAI, Google, Microsoft, v.v.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Tên viết tắt hoặc mã định danh ngắn gọn của nhà cung cấp AI. Ví dụ: "openai", "google", "microsoft", v.v.
        /// </summary>
        public string ShortName { get; set; } = null!;
        /// <summary>
        /// Loại nhà cung cấp AI
        /// </summary>
        public CAIProviderType ProviderType { get; set; }
        /// <summary>
        /// URL của hình ảnh đại diện cho nhà cung cấp AI. Đây có thể là logo hoặc bất kỳ hình ảnh nào liên quan đến nhà cung cấp AI, giúp người dùng dễ dàng nhận diện.
        /// </summary>
        public string? ImageUrl { get; set; } = null;

        /// <summary>
        /// URL cơ sở (Base URL) của nhà cung cấp AI.
        /// Đây là địa chỉ chính mà các yêu cầu API sẽ được gửi đến khi tương tác với nhà cung cấp AI.
        /// Ví dụ: "https://api.openai.com".
        /// </summary>
        public string BaseUrl { get; set; } = null!;
    }
}