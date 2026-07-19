using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.Thumbnails
{
    // ── Requests ──────────────────────────────────────────────────

    public record ThumbnailSearchRequest(
        string Keyword,
        CThumbnailTimeFilter TimeFilter,
        CThumbnailSortFilter SortFilter,
        bool WasSuggestedFromNews = false);

    public record ThumbnailManualImportRequest(string VideoUrl);

    public record ThumbnailUserStatusRequest(CLibraryUserStatus Status);

    public record ThumbnailChosenForGenerationRequest(bool IsChosen);

    // ── Response DTOs ─────────────────────────────────────────────

    public record ThumbnailLibraryItemDto(
        Guid Id,
        Guid ProjectId,
        Guid? ThumbnailSearchRequestId,
        CSourceType SourceType,
        CThumbnailPlatform Platform,
        string VideoTitle,
        string VideoUrl,
        string? ChannelName,
        string ThumbnailImageUrl,
        CMarketScope? MarketType,
        DateTimeOffset? PublishedTime,
        long? ViewCount,
        string KeywordBatchTag,
        CLibraryUserStatus UserStatus,
        bool IsFilteredIrrelevant,
        bool HasAnalysis);

    public record ThumbnailAnalysisDto(
        Guid Id,
        Guid ThumbnailLibraryItemId,
        string ThumbnailFactorsJson,
        string TitleTextAnalysis,
        string VideoTitleAnalysis,
        string DisplayTextStyleNote,
        bool IsChosenForGeneration);

    public record ThumbnailSearchResultDto(
        Guid SearchRequestId,
        string Keyword,
        CThumbnailTimeFilter TimeFilter,
        CThumbnailSortFilter SortFilter,
        IReadOnlyList<ThumbnailLibraryItemDto> Items);
}
