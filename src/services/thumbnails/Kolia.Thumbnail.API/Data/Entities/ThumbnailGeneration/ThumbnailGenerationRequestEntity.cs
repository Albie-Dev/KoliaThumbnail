using Kolia.Thumbnail.API.Data.Entities.Characters;
using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration
{
    /// <summary>
    /// Yêu cầu generate thumbnail (Phần 4.2).
    /// Liên kết với Display Text đã chọn, thumbnail tham khảo, và nhân vật.
    /// </summary>
    public class ThumbnailGenerationRequestEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id nhân vật dùng tham khảo phong cách (null nếu không dùng nhân vật)
        /// </summary>
        public Guid? CharacterId { get; set; }

        /// <summary>
        /// Mô tả thay đổi mong muốn so với ảnh mẫu tham khảo (user nhập)
        /// </summary>
        public string ChangesRequestText { get; set; } = string.Empty;

        /// <summary>
        /// JSON chứa các yếu tố AI xác định KHÔNG được thay đổi
        /// (nhận dạng nhân vật, bố cục chủ đạo...).
        /// Cấu trúc JSON tổng quát, chờ file prompt gốc của Nhi để chuẩn hoá.
        /// </summary>
        public string? LockedElementsJson { get; set; }

        /// <summary>
        /// JSON chứa các yếu tố ĐƯỢC PHÉP thay đổi
        /// (biểu cảm, màu sắc, ánh sáng, cử chỉ...).
        /// Cấu trúc JSON tổng quát, chờ file prompt gốc của Nhi để chuẩn hoá.
        /// </summary>
        public string? ChangeableElementsJson { get; set; }

        /// <summary>
        /// Prompt cuối cùng gửi cho AI — có thể user sửa tay đè lên prompt AI tạo
        /// </summary>
        public string? GeneratedPromptText { get; set; }

        /// <summary>
        /// Tỷ lệ ảnh, vd "16:9 YouTube", "1:1", "9:16"
        /// </summary>
        public string Ratio { get; set; } = "16:9";

        /// <summary>
        /// Độ phân giải, vd "1K", "2K", "4K"
        /// </summary>
        public string Resolution { get; set; } = "2K";

        /// <summary>
        /// Số ảnh yêu cầu generate (1–5).
        /// Service PHẢI tạo đúng số này, không ít hơn — fix lỗi UI mẫu A.3.
        /// </summary>
        public int RequestedImageCount { get; set; } = 1;

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual CharacterEntity? Character { get; set; }
        public virtual ICollection<ThumbnailGenerationRequestDisplayTextEntity> SelectedDisplayTextOptions { get; set; } = new List<ThumbnailGenerationRequestDisplayTextEntity>();
        public virtual ICollection<ThumbnailGenerationRequestReferenceEntity> SelectedReferenceItems { get; set; } = new List<ThumbnailGenerationRequestReferenceEntity>();
        public virtual ICollection<GeneratedThumbnailSetEntity> GeneratedSets { get; set; } = new List<GeneratedThumbnailSetEntity>();
    }
}
