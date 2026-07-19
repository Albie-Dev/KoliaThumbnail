using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Models.AIs
{
    // ── Request DTOs ───────────────────────────────────────────────────

    /// <summary>
    /// DTO tạo mới cấu hình AI cho một chức năng.
    /// </summary>
    public class CreateAIFunctionConfigDto
    {
        /// <summary>Chức năng nghiệp vụ.</summary>
        public CAIFunctionType FunctionType { get; set; }

        /// <summary>Model mặc định (vd: "gemini-2.0-flash").</summary>
        public string? Model { get; set; }

        /// <summary>Temperature mặc định.</summary>
        public double? Temperature { get; set; }

        /// <summary>Max tokens mặc định.</summary>
        public int? MaxTokens { get; set; }

        /// <summary>Danh sách item (primary + fallback). Item đầu tiên (Priority=0) là primary.</summary>
        public List<CreateAIFunctionConfigItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO tạo mới một item trong cấu hình chức năng AI.
    /// </summary>
    public class CreateAIFunctionConfigItemDto
    {
        /// <summary>Thứ tự ưu tiên (0 = primary).</summary>
        public int Priority { get; set; }

        /// <summary>Id của AI provider.</summary>
        public Guid AIProviderId { get; set; }

        /// <summary>Id của configuration (API key).</summary>
        public Guid AIProviderConfigurationId { get; set; }

        /// <summary>Ghi đè model (null = dùng model mặc định của function).</summary>
        public string? Model { get; set; }

        /// <summary>Ghi đè temperature.</summary>
        public double? Temperature { get; set; }

        /// <summary>Ghi đè max tokens.</summary>
        public int? MaxTokens { get; set; }
    }

    /// <summary>
    /// DTO cập nhật cấu hình AI cho một chức năng.
    /// </summary>
    public class UpdateAIFunctionConfigDto
    {
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public List<UpdateAIFunctionConfigItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO cập nhật một item trong cấu hình chức năng AI.
    /// </summary>
    public class UpdateAIFunctionConfigItemDto
    {
        public Guid? Id { get; set; } // null = tạo mới
        public int Priority { get; set; }
        public Guid AIProviderId { get; set; }
        public Guid AIProviderConfigurationId { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    // ── Response DTOs ──────────────────────────────────────────────────

    /// <summary>
    /// DTO chi tiết cấu hình AI cho một chức năng (trả về từ API).
    /// </summary>
    public class AIFunctionConfigDetailDto
    {
        public Guid Id { get; set; }
        public CAIFunctionType FunctionType { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }

        /// <summary>Items sắp xếp theo Priority tăng dần.</summary>
        public List<AIFunctionConfigItemDetailDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO cho danh sách (paging) — nhẹ, không kèm items.
    /// </summary>
    public class AIFunctionConfigSummaryDto
    {
        public Guid Id { get; set; }
        public CAIFunctionType FunctionType { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public string? PrimaryProviderName { get; set; }
        public string? PrimaryConfigName { get; set; }
        public int FallbackCount { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }
    }

    /// <summary>
    /// DTO chi tiết một item trong cấu hình chức năng AI.
    /// </summary>
    public class AIFunctionConfigItemDetailDto
    {
        public Guid Id { get; set; }
        public int Priority { get; set; }
        public Guid AIProviderId { get; set; }
        public string AIProviderName { get; set; } = null!;
        public Guid AIProviderConfigurationId { get; set; }
        public string AIProviderConfigurationName { get; set; } = null!;
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public bool IsEnabled { get; set; }
    }
}
