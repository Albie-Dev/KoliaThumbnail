using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.News
{
    /// <summary>
    /// Phân tích sâu 4 tầng cho một bản tin đã chọn.
    /// Quan hệ 1-1 với NewsItemEntity — chỉ tạo khi user bấm "Phân tích sâu".
    /// </summary>
    public class NewsDeepAnalysisEntity : BaseEntity
    {
        /// <summary>Id bản tin nguồn (unique — 1-1)</summary>
        public Guid NewsItemId { get; set; }

        /// <summary>
        /// Tầng 1: JSON array of <see cref="Kolia.Thumbnail.API.Engines.AI.MacroEventCategoryItem"/>.
        /// LUÔN đủ các hạng mục cố định trong MacroEventCategories.Fixed.
        /// </summary>
        public string MacroEventSummaryJson { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 2: JSON array of <see cref="Kolia.Thumbnail.API.Engines.AI.MarketReactionItem"/>.
        /// Mục cuối luôn là "Ý kiến nhà đầu tư / Chuyên gia".
        /// </summary>
        public string MarketReactionJson { get; set; } = string.Empty;

        /// <summary>Tầng 3a: Kỳ vọng ngắn hạn (1-3 THÁNG tới). "Chưa rõ" nếu không có dữ liệu.</summary>
        public string ExpectationShortTerm { get; set; } = string.Empty;

        /// <summary>Tầng 3b: Kỳ vọng dài hạn (6-12 THÁNG tới). "Chưa rõ" nếu không có dữ liệu.</summary>
        public string ExpectationLongTerm { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 4: JSON object <see cref="Kolia.Thumbnail.API.Engines.AI.SentimentOverview"/>.
        /// KHÔNG chứa chỉ số số hoá tự bịa (không fear/greed index).
        /// </summary>
        public string SentimentOverviewJson { get; set; } = string.Empty;

        /// <summary>Tag cảm xúc có thể khai thác (Flags — có thể gộp nhiều cảm xúc)</summary>
        public CEmotionTag EmotionTags { get; set; } = CEmotionTag.None;

        /// <summary>Lý do chọn tag cảm xúc, mô tả cụ thể để guide tạo thumbnail</summary>
        public string EmotionReason { get; set; } = string.Empty;

        /// <summary>True nếu đây là nguồn quốc tế đã được dịch sang tiếng Việt</summary>
        public bool WasTranslatedFromForeign { get; set; } = false;

        /// <summary>Ghi chú về trường nào thiếu dữ liệu</summary>
        public string? MissingDataNote { get; set; }

        /// <summary>
        /// Trạng thái xử lý — dùng để KHÔNG cache vĩnh viễn một lần phân tích lỗi.
        /// Failed → lần bấm "Phân tích sâu" tiếp theo sẽ chạy lại thay vì trả bản cũ.
        /// </summary>
        public CDeepAnalysisStatus Status { get; set; } = CDeepAnalysisStatus.Completed;

        // Navigation
        public virtual NewsItemEntity NewsItem { get; set; } = null!;
    }
}
