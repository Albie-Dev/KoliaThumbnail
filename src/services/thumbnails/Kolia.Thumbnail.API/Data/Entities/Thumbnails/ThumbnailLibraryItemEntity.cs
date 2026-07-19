using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Thumbnails
{
    /// <summary>
    /// Item trong Thumbnail Library (Phần 3b).
    /// Kho tổng hợp — không bị ghi đè giữa các lần search, lưu lại theo từng batch keyword.
    /// </summary>
    public class ThumbnailLibraryItemEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id yêu cầu tìm kiếm nguồn gốc. Null nếu ManualLink.
        /// </summary>
        public Guid? ThumbnailSearchRequestId { get; set; }

        /// <summary>
        /// Nguồn dữ liệu: crawl tự động hay nhập link tay
        /// </summary>
        public CSourceType SourceType { get; set; }

        /// <summary>
        /// Nền tảng nguồn: YouTube hay Faceless
        /// </summary>
        public CThumbnailPlatform Platform { get; set; }

        /// <summary>
        /// Tiêu đề video gốc
        /// </summary>
        public string VideoTitle { get; set; } = null!;

        /// <summary>
        /// URL video gốc
        /// </summary>
        public string VideoUrl { get; set; } = null!;

        /// <summary>
        /// Tên kênh
        /// </summary>
        public string? ChannelName { get; set; }

        /// <summary>
        /// URL ảnh thumbnail
        /// </summary>
        public string ThumbnailImageUrl { get; set; } = null!;

        /// <summary>
        /// Loại thị trường của video: nội địa hay quốc tế
        /// </summary>
        public CMarketScope? MarketType { get; set; }

        /// <summary>
        /// Thời gian xuất bản video
        /// </summary>
        public DateTimeOffset? PublishedTime { get; set; }

        /// <summary>
        /// Lượt xem
        /// </summary>
        public long? ViewCount { get; set; }

        /// <summary>
        /// Tag theo lần search, vd "giá vàng" hay "Bitcoin".
        /// Dùng để phân nhóm hiển thị trong Library theo từng batch.
        /// </summary>
        public string KeywordBatchTag { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái duyệt của người dùng: Pending/Approved/Rejected
        /// </summary>
        public CLibraryUserStatus UserStatus { get; set; } = CLibraryUserStatus.Pending;

        /// <summary>
        /// True nếu bộ lọc tự động phát hiện là nội dung không liên quan
        /// (MV nhạc, quảng cáo, giải trí, kênh vùng miền không liên quan...).
        /// Ẩn khỏi kết quả mặc định khi true.
        /// </summary>
        public bool IsFilteredIrrelevant { get; set; } = false;

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual ThumbnailSearchRequestEntity? ThumbnailSearchRequest { get; set; }
        public virtual ThumbnailAnalysisEntity? Analysis { get; set; }
    }
}
