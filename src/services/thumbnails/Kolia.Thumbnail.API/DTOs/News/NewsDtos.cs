using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.News
{
    // ── Requests ──────────────────────────────────────────────────

    public record NewsSearchRequest(
        CMarketScope MarketScope,
        CNewsTimeRange TimeRange,
        CNewsCountFilter CountFilter,
        string KeywordsRaw,
        IEnumerable<string>? SuggestedKeywordsSelected,
        IEnumerable<Guid>? SelectedSourceIds = null);

    public record NewsManualImportRequest(string Url);

    public record NewsSelectionRequest(bool IsSelected);

    // ── Response DTOs ─────────────────────────────────────────────

    public record NewsItemDto(
        Guid Id,
        Guid ProjectId,
        Guid? NewsSearchRequestId,
        CSourceType SourceType,
        string Title,
        string SourceName,
        string SourceUrl,
        CMarketScope MarketType,
        DateTimeOffset? PublishedTime,
        DateTimeOffset ScannedTime,
        string SummaryOverview,
        int RelevanceToTopicScore,
        int ImportanceImpactScore,
        int EmotionPotentialScore,
        int NoveltyDataScore,
        int TotalScore,
        CNewsRecommendation Recommendation,
        CRelevanceLevel RelevanceLevel,
        bool IsSelectedByTeam,
        string? SuggestedKeywordsForThumbnail,
        bool HasDeepAnalysis,
        string KeywordBatchGroup,
        CEmotionTag? EmotionTags = null);

    public record NewsDeepAnalysisDto(
        Guid Id,
        Guid NewsItemId,
        IReadOnlyList<MacroEventCategoryItem> MacroEventSummary,
        IReadOnlyList<MarketReactionItem> MarketReaction,
        string ExpectationShortTerm,
        string ExpectationLongTerm,
        SentimentOverview SentimentOverview,
        CEmotionTag EmotionTags,
        string EmotionReason,
        bool WasTranslatedFromForeign,
        string? MissingDataNote,
        CDeepAnalysisStatus Status);

    public record NewsSearchResultDto(
        Guid SearchRequestId,
        CMarketScope MarketScope,
        CNewsTimeRange TimeRange,
        IReadOnlyList<NewsItemDto> Items);

    // ── News Source Selection DTOs ────────────────────────────────────

    /// <summary>
    /// DTO đơn giản để client select nguồn tin khi search news.
    /// Chỉ chứa thông tin cần thiết: Id, Name, Region, Priority.
    /// </summary>
    public record NewsSourceSelectDto(
        Guid Id,
        string Name,
        CMarketScope Region,
        int Priority);
}
