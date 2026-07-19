using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.News
{
    // ── Requests ──────────────────────────────────────────────────

    public record NewsSearchRequest(
        CMarketScope MarketScope,
        CNewsTimeRange TimeRange,
        CNewsCountFilter CountFilter,
        string KeywordsRaw,
        IEnumerable<string>? SuggestedKeywordsSelected);

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
        string KeywordBatchGroup);

    public record NewsDeepAnalysisDto(
        Guid Id,
        Guid NewsItemId,
        IReadOnlyList<string> MacroEventSummary,
        string MarketReactionJson,
        string ExpectationShortTerm,
        string ExpectationLongTerm,
        string SentimentOverviewJson,
        CEmotionTag EmotionTags,
        string EmotionReason,
        bool WasTranslatedFromForeign,
        string? MissingDataNote);

    public record NewsSearchResultDto(
        Guid SearchRequestId,
        CMarketScope MarketScope,
        CNewsTimeRange TimeRange,
        IReadOnlyList<NewsItemDto> Items);
}
