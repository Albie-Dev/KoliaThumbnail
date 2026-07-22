using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả AI chấm điểm 1 bản tin (tổng 85 điểm).
    /// </summary>
    public record NewsScoringResult(
        int RelevanceToTopicScore,
        int ImportanceImpactScore,
        int EmotionPotentialScore,
        int NoveltyDataScore,
        int DataQualityScore,
        int TotalScore,
        CNewsRecommendation Recommendation,
        CRelevanceLevel RelevanceLevel,
        string SummaryOverview,
        string? SuggestedKeywordsForThumbnail,
        CEmotionTag EmotionTags);

    /// <summary>
    /// Engine AI chấm điểm và tóm tắt danh sách bản tin theo batch (Phần 2).
    /// Xử lý batch để tối ưu token — không gọi 1 tin/1 request.
    /// </summary>
    public interface INewsScoringEngine
    {
        /// <summary>
        /// Chấm điểm batch. key = NewsItemId, value = kết quả.
        /// Context: chủ đề từ ContentBrief để AI so sánh mức độ liên quan.
        /// </summary>
        Task<Dictionary<Guid, NewsScoringResult>> ScoreBatchAsync(
            IReadOnlyList<(Guid NewsItemId, string Title, string SourceName, string SummaryRaw)> items,
            string topicContext,
            CancellationToken ct = default);
    }
}
