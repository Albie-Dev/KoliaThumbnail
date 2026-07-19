using Kolia.Thumbnail.API.Data.Entities.News;

namespace Kolia.Thumbnail.API.Data.Entities.DisplayTexts
{
    /// <summary>
    /// Bảng nối n-n giữa DisplayTextRequest và NewsItem (các tin đã chọn để generate Display Text).
    /// </summary>
    public class DisplayTextRequestNewsItemEntity
    {
        /// <summary>
        /// Id yêu cầu Display Text
        /// </summary>
        public Guid DisplayTextRequestId { get; set; }

        /// <summary>
        /// Id bản tin đã chọn
        /// </summary>
        public Guid NewsItemId { get; set; }

        // Navigation
        public virtual DisplayTextRequestEntity DisplayTextRequest { get; set; } = null!;
        public virtual NewsItemEntity NewsItem { get; set; } = null!;
    }
}
