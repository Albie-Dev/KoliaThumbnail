using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;

namespace Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration
{
    /// <summary>
    /// Bảng nối n-n: ThumbnailGenerationRequest ↔ DisplayTextOption (các Display Text đã chọn dùng cho generation).
    /// </summary>
    public class ThumbnailGenerationRequestDisplayTextEntity
    {
        /// <summary>
        /// Id yêu cầu generation
        /// </summary>
        public Guid ThumbnailGenerationRequestId { get; set; }

        /// <summary>
        /// Id option Display Text đã chọn
        /// </summary>
        public Guid DisplayTextOptionId { get; set; }

        // Navigation
        public virtual ThumbnailGenerationRequestEntity ThumbnailGenerationRequest { get; set; } = null!;
        public virtual DisplayTextOptionEntity DisplayTextOption { get; set; } = null!;
    }
}
