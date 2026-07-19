using Kolia.Thumbnail.API.Data.Entities.News;

namespace Kolia.Thumbnail.API.Data.Entities.VideoTitles
{
    /// <summary>
    /// Bảng nối n-n: VideoTitleRequest ↔ NewsItem (các tin đã chọn ở Phần 2 đưa vào prompt tạo title).
    /// </summary>
    public class VideoTitleRequestNewsItemEntity
    {
        /// <summary>
        /// Id yêu cầu Video Title
        /// </summary>
        public Guid VideoTitleRequestId { get; set; }

        /// <summary>
        /// Id bản tin đã chọn
        /// </summary>
        public Guid NewsItemId { get; set; }

        // Navigation
        public virtual VideoTitleRequestEntity VideoTitleRequest { get; set; } = null!;
        public virtual NewsItemEntity NewsItem { get; set; } = null!;
    }
}
