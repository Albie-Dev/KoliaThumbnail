using Kolia.Thumbnail.API.Data.Entities.Briefs;
using Kolia.Thumbnail.API.Data.Entities.CompletePackages;
using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    /// <summary>
    /// Project — đơn vị làm việc chính trong Kho lưu trữ.
    /// Mỗi project đi qua 5 bước theo quy trình tuyến tính.
    /// </summary>
    public class ProjectEntity : BaseEntity
    {
        /// <summary>
        /// Tên project, ví dụ "Livestream vàng tuần này"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Trạng thái tổng của project
        /// </summary>
        public CProjectStatus Status { get; set; } = CProjectStatus.Draft;

        /// <summary>
        /// Phần đang xử lý hiện tại (P1–P5)
        /// </summary>
        public CProjectStepNumber CurrentStepNumber { get; set; } = CProjectStepNumber.ContentBrief;

        /// <summary>
        /// URL ảnh đại diện card Kho lưu trữ (lấy từ complete package mới nhất)
        /// </summary>
        public string? ThumbnailCoverUrl { get; set; }

        /// <summary>
        /// Thời gian hoạt động cuối để hiển thị trên card (vd "16/7/2026")
        /// </summary>
        public DateTimeOffset? LastActivityTime { get; set; }

        // Navigation properties
        public virtual ContentBriefEntity? ContentBrief { get; set; }
        public virtual ICollection<ProjectStepEntity> Steps { get; set; } = new List<ProjectStepEntity>();
        public virtual ICollection<NewsSearchRequestEntity> NewsSearchRequests { get; set; } = new List<NewsSearchRequestEntity>();
        public virtual ICollection<ThumbnailSearchRequestEntity> ThumbnailSearchRequests { get; set; } = new List<ThumbnailSearchRequestEntity>();
        public virtual ICollection<ThumbnailLibraryItemEntity> ThumbnailLibraryItems { get; set; } = new List<ThumbnailLibraryItemEntity>();
        public virtual ICollection<DisplayTextRequestEntity> DisplayTextRequests { get; set; } = new List<DisplayTextRequestEntity>();
        public virtual ICollection<ThumbnailGenerationRequestEntity> ThumbnailGenerationRequests { get; set; } = new List<ThumbnailGenerationRequestEntity>();
        public virtual ICollection<VideoTitleRequestEntity> VideoTitleRequests { get; set; } = new List<VideoTitleRequestEntity>();
        public virtual ICollection<CompletePackageEntity> CompletePackages { get; set; } = new List<CompletePackageEntity>();
    }
}
