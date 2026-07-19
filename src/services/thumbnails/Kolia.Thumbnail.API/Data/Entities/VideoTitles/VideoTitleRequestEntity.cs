using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.VideoTitles
{
    /// <summary>
    /// Yêu cầu tạo Video Title (Phần 5).
    /// Input: thumbnail đã duyệt + tin đã chọn ở Phần 2 + style + keyword.
    /// </summary>
    public class VideoTitleRequestEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Số title yêu cầu generate (chỉ nhận 3, 5, 7, 10)
        /// </summary>
        public int RequestedTitleCount { get; set; } = 5;

        /// <summary>
        /// Phong cách viết title
        /// </summary>
        public CTitleStyle Style { get; set; }

        /// <summary>
        /// Keyword SEO bổ sung, phân tách bằng ";"
        /// </summary>
        public string KeywordsRaw { get; set; } = string.Empty;

        /// <summary>
        /// Prompt tổng hợp từ thumbnail + tin tức — đọc-only để hiển thị trong UI.
        /// Được build tự động, không để user sửa trực tiếp.
        /// </summary>
        public string BuiltPromptText { get; set; } = string.Empty;

        /// <summary>
        /// Round generate (tăng dần mỗi lần bấm "Gen lại" — thường hoặc theo feedback)
        /// </summary>
        public int GenerationRound { get; set; } = 1;

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual ICollection<VideoTitleRequestThumbnailEntity> SelectedThumbnails { get; set; } = new List<VideoTitleRequestThumbnailEntity>();
        public virtual ICollection<VideoTitleRequestNewsItemEntity> SelectedNewsItems { get; set; } = new List<VideoTitleRequestNewsItemEntity>();
        public virtual ICollection<VideoTitleOptionEntity> Options { get; set; } = new List<VideoTitleOptionEntity>();
        public virtual ICollection<VideoTitleFeedbackEntity> Feedbacks { get; set; } = new List<VideoTitleFeedbackEntity>();
    }
}
