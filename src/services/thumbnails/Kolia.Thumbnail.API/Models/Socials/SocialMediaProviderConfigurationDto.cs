namespace Kolia.Thumbnail.API.Models.SocialMedias
{
    /// <summary>
    /// DTO lưu trữ cấu hình kết nối đến một nhà cung cấp SocialMedia.
    /// </summary>
    public class SocialMediaProviderConfigurationDetailDto : SocialMediaProviderConfigurationBaseDto
    {
        /// <summary>
        /// Id của cấu hình SocialMedia. Đây là một giá trị duy nhất để xác định cấu hình SocialMedia trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
        #region SocialMedia Provider Info
        /// <summary>
        /// Tên rút gọn nhà cung cấp SocialMedia mà cấu hình thuộc về.
        /// </summary>
        public string SocialMediaProviderShortName { get; set; } = null!;
        /// <summary>
        /// Tên nhà cung cấp SocialMedia mà cấu hình thuộc về. Ví dụ: OpenSocialMedia, Google, Microsoft, v.v.
        /// </summary>
        public string SocialMediaProviderName { get; set; } = null!;
        /// <summary>
        /// Logo của nhà cung cấp SocialMedia mà cấu hình thuộc về. Đây là một URL hoặc đường dẫn đến hình ảnh logo của nhà cung cấp SocialMedia.
        /// Nếu không có logo, giá trị này có thể là null.
        /// </summary>
        public string? SocialMediaProviderLogo { get; set; } = null;
        #endregion
        #region Masked
        /// <summary>
        /// 
        /// </summary>
        public string? AccessTokenMasked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? RefreshTokenMasked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? AppSecretMasked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? BearerTokenMasked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ClientSecretMasked { get; set; }
        #endregion
        /// <summary>
        /// API Key đã được che giấu (chỉ hiện 4 ký tự cuối), dùng để hiển thị ở FE.
        /// </summary>
        public string ApiKeyMasked { get; set; } = null!;
        /// <summary>
        /// Thời điểm TotalTokensUsed bị reset gần nhất (do đổi ApiKey).
        /// </summary>
        public DateTimeOffset? LastTokenResetTime { get; set; }
        /// <summary>
        /// Ngày giờ tạo của cấu hình SocialMedia. Đây là thời điểm mà cấu hình SocialMedia được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }
        /// <summary>
        /// Ngày giờ sửa đổi cuối cùng của cấu hình SocialMedia.
        /// Đây là thời điểm mà thông tin của cấu hình SocialMedia được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }
        /// <summary>
        /// Ngày giờ xóa của cấu hình SocialMedia. Nếu cấu hình SocialMedia đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống.
        /// Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }
        /// <summary>
        /// Người dùng đã xóa cấu hình SocialMedia. Nếu cấu hình SocialMedia đã bị xóa, đây là thông tin về người dùng đã thực hiện hành động xóa.
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO dùng để tạo mới một cấu hình SocialMedia. Nó chứa các thông tin cần thiết để thêm một cấu hình SocialMedia vào hệ thống.
    /// </summary>
    public class SocialMediaProviderConfigurationCreateDto : SocialMediaProviderConfigurationBaseDto
    {
        
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một cấu hình SocialMedia hiện có. Nó chứa các thông tin cần thiết để thay đổi các thuộc tính của cấu hình SocialMedia trong hệ thống.
    /// </summary>
    public class SocialMediaProviderConfigurationUpdateDto : SocialMediaProviderConfigurationBaseDto
    {
        
    }

    /// <summary>
    /// DTO cơ sở chứa các thuộc tính chung của cấu hình SocialMedia. Nó được sử dụng làm lớp cơ sở cho các DTO khác như SocialMediaProviderConfigurationDetSocialMedialDto, SocialMediaConfiurationCreateDto và SocialMediaProviderConfigurationUpdateDto.
    /// </summary>
    public class SocialMediaProviderConfigurationBaseDto
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
        /// Scope
        /// </summary>
        public string? Scope { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ApiBaseUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ClientSecret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? BearerToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? AppSecret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? AppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? RefreshToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// API Key dùng để xác thực với nhà cung cấp SocialMedia.
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// Phiên bản API.
        /// Chủ yếu sử dụng cho Azure OpenSocialMedia hoặc các nhà cung cấp yêu cầu version.
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
        /// Có thể dùng để fSocialMedialover hoặc load balancing.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Cho biết cấu hình có đang được kích hoạt hay không.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Cho biết đây có phải là cấu hình mặc định của nhà cung cấp SocialMedia hay không.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Tổng số lượt request
        /// </summary>
        public int TotalRequest { get; set; }

        /// <summary>
        /// Thời gian reset lại số lượt request.
        /// </summary>
        public DateTimeOffset? LastRequestResetTime { get; set; }

        /// <summary>
        /// Id của nhà cung cấp SocialMedia mà cấu hình này thuộc về.
        /// </summary>
        public Guid SocialMediaProviderId { get; set; }
    }
}