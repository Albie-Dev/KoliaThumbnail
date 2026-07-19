using Kolia.Thumbnail.API.Data.Entities.Projects;

namespace Kolia.Thumbnail.API.Data.Entities.DisplayTexts
{
    /// <summary>
    /// Yêu cầu tạo Display Text (Phần 4.1 — chữ hiển thị trên thumbnail).
    /// Liên kết với nhiều tin tức đã chọn ở Phần 2 qua bảng nối.
    /// </summary>
    public class DisplayTextRequestEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual ICollection<DisplayTextRequestNewsItemEntity> SelectedNewsItems { get; set; } = new List<DisplayTextRequestNewsItemEntity>();
        public virtual ICollection<DisplayTextOptionEntity> Options { get; set; } = new List<DisplayTextOptionEntity>();
    }
}
