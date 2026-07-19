using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;

namespace Kolia.Thumbnail.API.Data.Entities.VideoTitles
{
    /// <summary>
    /// Bảng nối n-n: VideoTitleRequest ↔ GeneratedThumbnail (ảnh đã đẩy từ Phần 4 để tạo title).
    /// </summary>
    public class VideoTitleRequestThumbnailEntity
    {
        /// <summary>
        /// Id yêu cầu Video Title
        /// </summary>
        public Guid VideoTitleRequestId { get; set; }

        /// <summary>
        /// Id ảnh thumbnail đã được chọn
        /// </summary>
        public Guid GeneratedThumbnailId { get; set; }

        // Navigation
        public virtual VideoTitleRequestEntity VideoTitleRequest { get; set; } = null!;
        public virtual GeneratedThumbnailEntity GeneratedThumbnail { get; set; } = null!;
    }
}
