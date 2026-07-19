using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.News
{
    /// <summary>
    /// Yêu cầu tìm kiếm tin tức (Phần 2). Mỗi lần bấm "Tìm tin" = 1 bản ghi.
    /// </summary>
    public class NewsSearchRequestEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Phạm vi thị trường: nội địa / quốc tế / cả hai
        /// </summary>
        public CMarketScope MarketScope { get; set; }

        /// <summary>
        /// Khoảng thời gian lọc tin. Last30Days sẽ kích hoạt cảnh báo hiệu năng.
        /// </summary>
        public CNewsTimeRange TimeRange { get; set; }

        /// <summary>
        /// Số lượng tin tối đa trả về
        /// </summary>
        public CNewsCountFilter CountFilter { get; set; }

        /// <summary>
        /// Keyword nhập tay, phân tách bằng dấu ";"
        /// </summary>
        public string KeywordsRaw { get; set; } = null!;

        /// <summary>
        /// JSON chứa danh sách keyword gợi ý từ Brief mà user đã click chọn
        /// </summary>
        public string? SuggestedKeywordsUsedJson { get; set; }

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual ICollection<NewsItemEntity> NewsItems { get; set; } = new List<NewsItemEntity>();
    }
}
