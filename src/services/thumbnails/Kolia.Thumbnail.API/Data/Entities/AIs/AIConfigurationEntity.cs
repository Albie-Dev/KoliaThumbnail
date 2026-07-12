namespace Kolia.Thumbnail.API.Data.Entities.AIs
{
    /// <summary>
    /// Entity lưu trữ cấu hình kết nối đến một nhà cung cấp AI.
    /// Cấu hình này chỉ chứa các thông tin kết nối và xác thực, không bao gồm
    /// các thiết lập suy luận như Model, Temperature hay Prompt.
    /// </summary>
    public class AIConfigurationEntity : BaseEntity
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
        /// Được mã hoá khi lưu vào DB và giải mã khi đọc lên.
        /// </summary>
        public string ApiKey { get; set; } = null!;

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
        /// Tổng số token đã sử dụng (tích luỹ) cho cấu hình này.
        /// Dùng để theo dõi chi phí và quota. Khi đổi ApiKey, giá trị này tự động reset về 0.
        /// </summary>
        public long TotalTokensUsed { get; set; }

        /// <summary>
        /// Băm của ApiKey hiện tại, dùng để phát hiện khi ApiKey bị thay đổi.
        /// Khi ApiKey thay đổi, TotalTokensUsed sẽ tự động reset.
        /// </summary>
        public string? ApiKeyHash { get; set; }

        /// <summary>
        /// Thời điểm TotalTokensUsed bị reset gần nhất (do đổi ApiKey).
        /// </summary>
        public DateTimeOffset? LastTokenResetTime { get; set; }

        /// <summary>
        /// Thiết lập mở rộng dành riêng cho từng nhà cung cấp AI dưới dạng JSON.
        /// Ví dụ: Proxy, Headers, OrganizationId hoặc các tùy chọn khác.
        /// </summary>
        public string? ExtraSettingsJson { get; set; }

        #region Foreign Keys

        /// <summary>
        /// Id của nhà cung cấp AI mà cấu hình này thuộc về.
        /// </summary>
        public Guid AIProviderId { get; set; }

        /// <summary>
        /// Nhà cung cấp AI sở hữu cấu hình này.
        /// </summary>
        public virtual AIProviderEntity AIProvider { get; set; } = null!;

        #endregion
    }
}