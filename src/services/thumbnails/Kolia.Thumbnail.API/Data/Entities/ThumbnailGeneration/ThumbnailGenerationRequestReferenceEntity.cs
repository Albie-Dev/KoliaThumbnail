using Kolia.Thumbnail.API.Data.Entities.Thumbnails;

namespace Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration
{
    /// <summary>
    /// Bảng nối n-n: ThumbnailGenerationRequest ↔ ThumbnailLibraryItem (ảnh tham khảo đã chọn từ Library).
    /// </summary>
    public class ThumbnailGenerationRequestReferenceEntity
    {
        /// <summary>
        /// Id yêu cầu generation
        /// </summary>
        public Guid ThumbnailGenerationRequestId { get; set; }

        /// <summary>
        /// Id item tham khảo trong Library
        /// </summary>
        public Guid ThumbnailLibraryItemId { get; set; }

        // Navigation
        public virtual ThumbnailGenerationRequestEntity ThumbnailGenerationRequest { get; set; } = null!;
        public virtual ThumbnailLibraryItemEntity ThumbnailLibraryItem { get; set; } = null!;
    }
}
