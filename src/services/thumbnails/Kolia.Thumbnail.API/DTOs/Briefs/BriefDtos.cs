using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.Briefs
{
    public record SaveManualBriefRequest(
        string OverviewInput,
        string ViewpointInput,
        string KeyDataInput);

    public record ImportBriefRequest(
        CImportContentSource Source,
        string? RawText,
        string? FileUrl,
        string? ExternalLink);

    public record SyncSheetRequest(string SheetUrl);

    public record AnalyzeBriefRequest(string? ManualPrompt);

    public record ContentBriefDto(
        Guid Id,
        Guid ProjectId,
        CImportContentSource? ImportSource,
        string? ImportedExternalLink,
        string? ExternalSheetUrl,
        DateTimeOffset? LastSheetSyncTime,
        string OverviewInput,
        string ViewpointInput,
        string KeyDataInput,
        string? TopicOutput,
        string? MainMessageOutput,
        string? HighlightDataOutput,
        bool IsConfirmed,
        DateTimeOffset? ConfirmedAt);
}
