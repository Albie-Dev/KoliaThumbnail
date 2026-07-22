using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.News
{
    /// <summary>
    /// Bản tin (Phần 2). Cả crawl tự động lẫn import link thủ công đều đổ vào bảng này.
    /// </summary>
    public class NewsItemEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id yêu cầu tìm kiếm nguồn gốc. Null nếu là ManualLink không qua search request.
        /// </summary>
        public Guid? NewsSearchRequestId { get; set; }

        /// <summary>
        /// Nguồn dữ liệu: crawl tự động hay nhập link tay
        /// </summary>
        public CSourceType SourceType { get; set; }

        /// <summary>
        /// Tiêu đề bài báo
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Tên nguồn, vd "VnExpress", "CoinDesk"
        /// </summary>
        public string SourceName { get; set; } = null!;

        /// <summary>
        /// URL bài báo gốc
        /// </summary>
        public string SourceUrl { get; set; } = null!;

        /// <summary>
        /// Loại thị trường của tin: nội địa hay quốc tế
        /// </summary>
        public CMarketScope MarketType { get; set; }

        /// <summary>
        /// Thời gian xuất bản bài báo
        /// </summary>
        public DateTimeOffset? PublishedTime { get; set; }

        /// <summary>
        /// Thời điểm hệ thống quét/import bài báo này
        /// </summary>
        public DateTimeOffset ScannedTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Tóm tắt 1 đoạn để hiển thị trong bảng tin
        /// </summary>
        public string SummaryOverview { get; set; } = string.Empty;

        // ----- Điểm AI chấm (tổng 85 điểm) -----

        /// <summary>
        /// Điểm liên quan đến chủ đề (0–30)
        /// </summary>
        public int RelevanceToTopicScore { get; set; }

        /// <summary>
        /// Điểm tầm quan trọng/tác động (0–20)
        /// </summary>
        public int ImportanceImpactScore { get; set; }

        /// <summary>
        /// Điểm tiềm năng cảm xúc (0–20)
        /// </summary>
        public int EmotionPotentialScore { get; set; }

        /// <summary>
        /// Điểm dữ liệu mới/độc đáo (0–15)
        /// </summary>
        public int NoveltyDataScore { get; set; }

        /// <summary>
        /// Tổng điểm (tính tại service = sum của 4 điểm thành phần)
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Nhãn đề xuất của AI: Nên chọn / Có thể chọn / Không ưu tiên
        /// </summary>
        public CNewsRecommendation Recommendation { get; set; }

        /// <summary>
        /// Mức độ liên quan: Cao / Trung bình / Thấp
        /// </summary>
        public CRelevanceLevel RelevanceLevel { get; set; }

        /// <summary>
        /// True khi team đã tick "Dùng cho Phần 4 và Phần 5"
        /// </summary>
        public bool IsSelectedByTeam { get; set; } = false;

        /// <summary>
        /// Keyword đề xuất để tìm thumbnail ở Phần 3, phân tách bằng ";"
        /// </summary>
        public string? SuggestedKeywordsForThumbnail { get; set; }

        /// <summary>
        /// Điểm có dữ liệu/số liệu nổi bật (0–15)
        /// </summary>
        public int DataQualityScore { get; set; }

        /// <summary>
        /// Cảm xúc có thể khai thác (bitmask: FOMO, Fear, Curiosity, Doubt, DecisionPressure, Urgency)
        /// </summary>
        public CEmotionTag EmotionTags { get; set; }

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual NewsSearchRequestEntity? NewsSearchRequest { get; set; }
        public virtual NewsDeepAnalysisEntity? DeepAnalysis { get; set; }
    }
}
