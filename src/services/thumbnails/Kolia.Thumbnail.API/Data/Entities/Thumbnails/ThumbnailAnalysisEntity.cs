namespace Kolia.Thumbnail.API.Data.Entities.Thumbnails
{
    /// <summary>
    /// Phân tích sâu một thumbnail trong Library.
    /// Quan hệ 1-1 với ThumbnailLibraryItemEntity — chỉ tạo khi user tick "Phân tích sâu".
    /// </summary>
    public class ThumbnailAnalysisEntity : BaseEntity
    {
        /// <summary>
        /// Id item trong library (unique — 1-1)
        /// </summary>
        public Guid ThumbnailLibraryItemId { get; set; }

        /// <summary>
        /// Phân tích các yếu tố thumbnail: bố cục, màu sắc, composition, contrast...
        /// JSON object với các key phân tích cụ thể.
        /// </summary>
        public string ThumbnailFactorsJson { get; set; } = string.Empty;

        /// <summary>
        /// Phân tích text hiển thị trên thumbnail (Display Text style, font, vị trí...)
        /// </summary>
        public string TitleTextAnalysis { get; set; } = string.Empty;

        /// <summary>
        /// Phân tích tiêu đề video gốc (cách đặt title, từ khóa, hook...)
        /// </summary>
        public string VideoTitleAnalysis { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú phong cách chữ hiển thị để tham khảo cho Phần 4.1
        /// </summary>
        public string DisplayTextStyleNote { get; set; } = string.Empty;

        /// <summary>
        /// True khi user đã tick chọn thumbnail này làm mẫu để dùng ở Phần 4.2
        /// </summary>
        public bool IsChosenForGeneration { get; set; } = false;

        // Navigation
        public virtual ThumbnailLibraryItemEntity ThumbnailLibraryItem { get; set; } = null!;
    }
}
