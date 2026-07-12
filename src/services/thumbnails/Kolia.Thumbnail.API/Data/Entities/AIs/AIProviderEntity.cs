using System.Collections.ObjectModel;
using Kolia.Thumbnail.API.Attributes;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.AIs
{
    /// <summary>
    /// Entity lưu trữ thông tin về nhà cung cấp AI
    /// Ví dụ: OpenAI, Gemini, Deepseek, v.v.
    /// </summary>
    public class AIProviderEntity : BaseEntity
    {
        /// <summary>
        /// Khởi tạo một nhà cung cấp AI mới
        /// </summary>
        public AIProviderEntity()
        {
            
        }

        /// <summary>
        /// Tên đầy đủ của nhà cung cấp AI
        /// </summary>
        [Queryable(
            Searchable = true,
            Sortable = true
        )]
        public string Name { get; set; } = null!;
        /// <summary>
        /// Tên viết tắt của nhà cung cấp AI
        /// </summary>
        [Queryable(
            Searchable = true,
            Sortable = true
        )]
        public string ShortName { get; set; } = null!;
        /// <summary>
        /// Loại nhà cung cấp AI
        /// </summary>
        [Queryable(
            Sortable = true,
            Filterable = true
        )]
        public CAIProviderType ProviderType { get; set; }
        /// <summary>
        /// URL của hình ảnh đại diện cho nhà cung cấp AI
        /// </summary>
        public string? ImageUrl { get; set; } = null;
        /// <summary>
        /// URL cơ sở (Base URL) của nhà cung cấp AI.
        /// Đây là địa chỉ chính mà các yêu cầu API sẽ được gửi đến khi tương tác với nhà cung cấp AI.
        /// Ví dụ: "https://api.openai.com/v1".
        /// </summary>
        public string BaseUrl { get; set; } = null!;
        /// <summary>
        /// Danh sách các cấu hình AI liên quan đến nhà cung cấp này
        /// </summary>
        public virtual ICollection<AIConfigurationEntity> Configurations { get; set; } = new Collection<AIConfigurationEntity>();
    }
}