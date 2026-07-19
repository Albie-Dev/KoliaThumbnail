using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Thumbnails
{
    /// <summary>
    /// Yêu cầu tìm kiếm thumbnail tham khảo (Phần 3). Mỗi lần search keyword = 1 bản ghi.
    /// </summary>
    public class ThumbnailSearchRequestEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Keyword tìm kiếm, vd "giá vàng sụt", "gold price crash"
        /// </summary>
        public string Keyword { get; set; } = null!;

        /// <summary>
        /// Bộ lọc thời gian (dùng chung enum với bộ lọc view-reference — A.3 fix)
        /// </summary>
        public CThumbnailTimeFilter TimeFilter { get; set; }

        /// <summary>
        /// Bộ lọc sắp xếp kết quả
        /// </summary>
        public CThumbnailSortFilter SortFilter { get; set; }

        /// <summary>
        /// True nếu keyword này lấy từ gợi ý của Phần 2, false nếu tự nhập
        /// </summary>
        public bool WasSuggestedFromNews { get; set; } = false;

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual ICollection<ThumbnailLibraryItemEntity> LibraryItems { get; set; } = new List<ThumbnailLibraryItemEntity>();
    }
}
