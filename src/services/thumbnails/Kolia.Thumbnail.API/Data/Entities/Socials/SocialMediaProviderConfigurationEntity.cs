namespace Kolia.Thumbnail.API.Data.Entities.Socials
{
    /// <summary>
    /// Cấu hình chi tiết của các nhà cung cấp
    /// </summary>
    public class SocialMediaProviderConfigurationEntity : BaseEntity
    {
        /// <summary>
        /// Tên cấu hình.
        /// Ví dụ: Production, Sandbox...
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Có đang sử dụng hay không.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Có được set làm mặc định
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Thứ tự ưu tiên.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Phiên bản API.
        /// Ví dụ: v3, v23.0...
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Endpoint API.
        /// </summary>
        public string? ApiBaseUrl { get; set; }

        /// <summary>
        /// Số lần thử lại khi request failed
        /// </summary>
        public int RetryCount { get; set; } = 0;
        /// <summary>
        /// Thời gian timeoute của một reuqest.
        /// </summary>

        public int TimeoutSeconds { get; set; } = 0;

        /// <summary>
        /// API Key.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// API key hash
        /// </summary>
        public string? ApiKeyHash { get; set; }

        /// <summary>
        /// Client Id.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client Secret.
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// App Id.
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// App Secret.
        /// </summary>
        public string? AppSecret { get; set; }

        /// <summary>
        /// Bearer Token.
        /// </summary>
        public string? BearerToken { get; set; }

        /// <summary>
        /// Refresh Token.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Access Token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Scope OAuth.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Tổng số lượng request.
        /// </summary>
        public int TotalRequest { get; set; } = 0;

        public DateTimeOffset? LastRequestResetTime { get; set; } = null;

        /// <summary>
        /// Thời điểm bị rate-limit (429/403) gần nhất.
        /// Dùng để tính cooldown trong Social Executor round-robin (D.6).
        /// </summary>
        public DateTimeOffset? LastRateLimitedAt { get; set; } = null;

        /// <summary>
        /// Thời gian cooldown tính bằng phút sau khi bị rate-limit (mặc định 1440 = 24 giờ).
        /// </summary>
        public int RateLimitCooldownMinutes { get; set; } = 1440;

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string? Description { get; set; } = null;

        /// <summary>
        /// Id nhà cung cấp
        /// </summary>
        public Guid SocialMediaProviderId { get; set; }

        /// <summary>
        /// Nhà cung cấp
        /// </summary>
        public virtual SocialMediaProviderEntity SocialMediaProvider { get; set; } = null!;
    }
}