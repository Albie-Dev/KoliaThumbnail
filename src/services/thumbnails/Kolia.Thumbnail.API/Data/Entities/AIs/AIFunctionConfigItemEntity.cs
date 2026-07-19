namespace Kolia.Thumbnail.API.Data.Entities.AIs
{
    /// <summary>
    /// Một mục trong danh sách cấu hình của chức năng AI.
    /// Priority = 0 là primary, Priority > 0 là fallback theo thứ tự tăng dần.
    /// </summary>
    public class AIFunctionConfigItemEntity : BaseEntity
    {
        /// <summary>
        /// FK → <see cref="AIFunctionConfigEntity"/>.
        /// </summary>
        public Guid FunctionConfigId { get; set; }

        /// <summary>
        /// Thứ tự ưu tiên (0 = primary, 1 = fallback 1, 2 = fallback 2, ...).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// FK → <see cref="AIProviderEntity"/>.
        /// </summary>
        public Guid AIProviderId { get; set; }

        /// <summary>
        /// FK → <see cref="AIProviderConfigurationEntity"/>.
        /// </summary>
        public Guid AIProviderConfigurationId { get; set; }

        /// <summary>
        /// Ghi đè model nếu khác model mặc định của function (để null nếu dùng model mặc định).
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Ghi đè temperature nếu khác mặc định.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Ghi đè max tokens nếu khác mặc định.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Cho phép mục này được sử dụng hay không.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        // ── Navigation properties ──

        public virtual AIFunctionConfigEntity FunctionConfig { get; set; } = null!;
        public virtual AIProviderEntity AIProvider { get; set; } = null!;
        public virtual AIProviderConfigurationEntity AIProviderConfiguration { get; set; } = null!;
    }
}
