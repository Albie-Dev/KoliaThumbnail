using Kolia.Thumbnail.API.Data.Entities.News;

namespace Kolia.Thumbnail.API.Data.Entities.DisplayTexts
{
    /// <summary>
    /// Một phương án Display Text do AI generate (chữ hiển thị trên thumbnail).
    /// Ví dụ: "ĐỪNG MUA VỘI", "BỨT PHÁ HAY SẬP BẪY?"
    /// </summary>
    public class DisplayTextOptionEntity : BaseEntity
    {
        /// <summary>
        /// Id yêu cầu Display Text cha
        /// </summary>
        public Guid DisplayTextRequestId { get; set; }

        /// <summary>
        /// Id bản tin nguồn sinh ra option này
        /// </summary>
        public Guid SourceNewsItemId { get; set; }

        /// <summary>
        /// Nội dung Display Text, vd "ĐỪNG MUA VỘI", "BỨT PHÁ HAY SẬP BẪY?"
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// True khi user đã tick chọn option này để dùng ở Phần 4.2
        /// </summary>
        public bool IsSelected { get; set; } = false;

        // Navigation
        public virtual DisplayTextRequestEntity DisplayTextRequest { get; set; } = null!;
        public virtual NewsItemEntity SourceNewsItem { get; set; } = null!;
    }
}
