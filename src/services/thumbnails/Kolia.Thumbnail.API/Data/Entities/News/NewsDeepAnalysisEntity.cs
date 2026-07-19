using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.News
{
    /// <summary>
    /// Phân tích sâu 4 tầng cho một bản tin đã chọn.
    /// Quan hệ 1-1 với NewsItemEntity — chỉ tạo khi user bấm "Phân tích sâu".
    /// </summary>
    public class NewsDeepAnalysisEntity : BaseEntity
    {
        /// <summary>
        /// Id bản tin nguồn (unique — 1-1)
        /// </summary>
        public Guid NewsItemId { get; set; }

        /// <summary>
        /// Tầng 1: Tóm tắt sự kiện vĩ mô (danh sách các câu chốt) — JSON array of strings
        /// </summary>
        public string MacroEventSummaryJson { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 2: Phản ứng thị trường — JSON object phân tích diễn biến
        /// </summary>
        public string MarketReactionJson { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 3a: Kỳ vọng ngắn hạn (1-2 tuần tới). "Chưa rõ" nếu không có dữ liệu.
        /// </summary>
        public string ExpectationShortTerm { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 3b: Kỳ vọng dài hạn (1-3 tháng tới). "Chưa rõ" nếu không có dữ liệu.
        /// </summary>
        public string ExpectationLongTerm { get; set; } = string.Empty;

        /// <summary>
        /// Tầng 4: Tổng quan cảm xúc thị trường — JSON object
        /// </summary>
        public string SentimentOverviewJson { get; set; } = string.Empty;

        /// <summary>
        /// Tag cảm xúc có thể khai thác (Flags — có thể gộp nhiều cảm xúc)
        /// </summary>
        public CEmotionTag EmotionTags { get; set; } = CEmotionTag.None;

        /// <summary>
        /// Lý do chọn tag cảm xúc, mô tả cụ thể để guide tạo thumbnail
        /// </summary>
        public string EmotionReason { get; set; } = string.Empty;

        /// <summary>
        /// True nếu đây là nguồn quốc tế đã được dịch sang tiếng Việt
        /// </summary>
        public bool WasTranslatedFromForeign { get; set; } = false;

        /// <summary>
        /// Ghi chú về trường nào thiếu dữ liệu (field nào AI không có thông tin thì ghi "Chưa rõ" ở đây)
        /// </summary>
        public string? MissingDataNote { get; set; }

        // Navigation
        public virtual NewsItemEntity NewsItem { get; set; } = null!;
    }
}
