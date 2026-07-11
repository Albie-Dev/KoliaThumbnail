namespace Kolia.Thumbnail.API.Models.AIs
{
    /// <summary>
    /// DTO lưu trữ cấu hình kết nối đến một nhà cung cấp AI.
    /// </summary>
    public class AIConfigurationDetailDto : AIConfigurationBaseDto
    {
        /// <summary>
        /// Id của cấu hình AI. Đây là một giá trị duy nhất để xác định cấu hình AI trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
        #region AI Provider Info
        /// <summary>
        /// Tên rút gọn nhà cung cấp AI mà cấu hình thuộc về.
        /// </summary>
        public string AIProviderShortName { get; set; } = null!;
        /// <summary>
        /// Tên nhà cung cấp AI mà cấu hình thuộc về. Ví dụ: OpenAI, Google, Microsoft, v.v.
        /// </summary>
        public string AIProviderName { get; set; } = null!;
        /// <summary>
        /// Logo của nhà cung cấp AI mà cấu hình thuộc về. Đây là một URL hoặc đường dẫn đến hình ảnh logo của nhà cung cấp AI.
        /// Nếu không có logo, giá trị này có thể là null.
        /// </summary>
        public string? AIProviderLogo { get; set; } = null;
        #endregion
        /// <summary>
        /// Ngày giờ tạo của cấu hình AI. Đây là thời điểm mà cấu hình AI được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }
        /// <summary>
        /// Ngày giờ sửa đổi cuối cùng của cấu hình AI.
        /// Đây là thời điểm mà thông tin của cấu hình AI được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }
        /// <summary>
        /// Ngày giờ xóa của cấu hình AI. Nếu cấu hình AI đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống.
        /// Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }
        /// <summary>
        /// Người dùng đã xóa cấu hình AI. Nếu cấu hình AI đã bị xóa, đây là thông tin về người dùng đã thực hiện hành động xóa.
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO dùng để tạo mới một cấu hình AI. Nó chứa các thông tin cần thiết để thêm một cấu hình AI vào hệ thống.
    /// </summary>
    public class AIConfiurationCreateDto : AIConfigurationBaseDto
    {
        
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một cấu hình AI hiện có. Nó chứa các thông tin cần thiết để thay đổi các thuộc tính của cấu hình AI trong hệ thống.
    /// </summary>
    public class AIConfigurationUpdateDto : AIConfigurationBaseDto
    {
        
    }

    /// <summary>
    /// DTO cơ sở chứa các thuộc tính chung của cấu hình AI. Nó được sử dụng làm lớp cơ sở cho các DTO khác như AIConfigurationDetailDto, AIConfiurationCreateDto và AIConfigurationUpdateDto.
    /// </summary>
    public class AIConfigurationBaseDto
    {
        /// <summary>
        /// Tên hiển thị của cấu hình.
        /// Ví dụ: Production, Development, Staging.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả của cấu hình.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// API Key dùng để xác thực với nhà cung cấp AI.
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// Địa chỉ URL cơ sở của API.
        /// Ví dụ:
        /// https://api.openai.com/v1
        /// https://generativelanguage.googleapis.com
        /// </summary>
        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// Endpoint được sử dụng để ghi đè endpoint mặc định của provider.
        /// Để trống nếu sử dụng endpoint mặc định.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Phiên bản API.
        /// Chủ yếu sử dụng cho Azure OpenAI hoặc các nhà cung cấp yêu cầu version.
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Thời gian timeout tối đa cho mỗi request (giây).
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Số lần thử lại khi request thất bại.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Thứ tự ưu tiên của cấu hình.
        /// Giá trị càng nhỏ thì mức ưu tiên càng cao.
        /// Có thể dùng để failover hoặc load balancing.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Cho biết cấu hình có đang được kích hoạt hay không.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Cho biết đây có phải là cấu hình mặc định của nhà cung cấp AI hay không.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Thiết lập mở rộng dành riêng cho từng nhà cung cấp AI dưới dạng JSON.
        /// Ví dụ: Proxy, Headers, OrganizationId hoặc các tùy chọn khác.
        /// </summary>
        public string? ExtraSettingsJson { get; set; }

        /// <summary>
        /// Id của nhà cung cấp AI mà cấu hình này thuộc về.
        /// </summary>
        public Guid AIProviderId { get; set; }
    }
}