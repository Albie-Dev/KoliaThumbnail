using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Models.Engines
{
    /// <summary>
    /// Ngữ cảnh thực thi cho một provider AI, bao gồm thông tin provider
    /// và danh sách cấu hình (API keys) được sắp xếp theo Priority.
    /// Được tạo ra từ <see cref="AIProviderEntity"/> và <see cref="AIProviderConfigurationEntity"/>.
    /// </summary>
    public sealed class ProviderExecutionContext
    {
        /// <summary>
        /// Id của provider trong DB.
        /// </summary>
        public Guid ProviderId { get; init; }

        /// <summary>
        /// Loại provider (OpenAI, Gemini, Groq...).
        /// </summary>
        public CAIProviderType ProviderType { get; init; }

        /// <summary>
        /// Tên hiển thị.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Base URL mặc định từ provider (có thể ghi đè bởi từng config).
        /// </summary>
        public string BaseUrl { get; init; } = string.Empty;

        /// <summary>
        /// Danh sách cấu hình (API keys) đang hoạt động, sắp xếp theo Priority tăng dần.
        /// </summary>
        public IReadOnlyList<ConfigurationContext> Configurations { get; init; } = Array.Empty<ConfigurationContext>();

        /// <summary>
        /// Config mặc định (IsDefault == true), hoặc null nếu không có.
        /// </summary>
        public ConfigurationContext? DefaultConfiguration =>
            Configurations.FirstOrDefault(c => c.IsDefault);
    }

    /// <summary>
    /// Thông tin một cấu hình (API key) của provider.
    /// </summary>
    public sealed class ConfigurationContext
    {
        /// <summary>
        /// Id của cấu hình trong DB.
        /// </summary>
        public Guid ConfigurationId { get; init; }

        /// <summary>
        /// API key dùng để xác thực.
        /// </summary>
        public string ApiKey { get; init; } = string.Empty;

        /// <summary>
        /// Base URL riêng (ghi đè BaseUrl của provider nếu có).
        /// </summary>
        public string? BaseUrl { get; init; }

        /// <summary>
        /// Endpoint riêng (ghi đè nếu có).
        /// </summary>
        public string? Endpoint { get; init; }

        /// <summary>
        /// Phiên bản API.
        /// </summary>
        public string? ApiVersion { get; init; }

        /// <summary>
        /// Thứ tự ưu tiên (thấp = ưu tiên cao hơn).
        /// </summary>
        public int Priority { get; init; }

        /// <summary>
        /// Là cấu hình mặc định.
        /// </summary>
        public bool IsDefault { get; init; }

        /// <summary>
        /// Timeout (giây).
        /// </summary>
        public int TimeoutSeconds { get; init; } = 120;

        /// <summary>
        /// Số lần retry.
        /// </summary>
        public int RetryCount { get; init; } = 3;

        /// <summary>
        /// Thiết lập mở rộng (JSON).
        /// </summary>
        public string? ExtraSettingsJson { get; init; }
    }
}
