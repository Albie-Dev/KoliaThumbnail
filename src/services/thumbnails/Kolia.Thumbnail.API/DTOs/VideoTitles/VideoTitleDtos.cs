using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.VideoTitles
{
    // ── Requests ──────────────────────────────────────────────────

    public record GenerateVideoTitleRequest(
        IReadOnlyList<Guid> SelectedThumbnailIds,
        IReadOnlyList<Guid> SelectedNewsItemIds,
        CTitleStyle Style,
        string KeywordsRaw,
        int RequestedCount);

    public record VideoTitleFeedbackRequest(string FeedbackText);

    public record VideoTitleSelectionRequest(bool IsSelected);

    // ── Response DTOs ─────────────────────────────────────────────

    public record VideoTitleOptionDto(
        Guid Id,
        Guid VideoTitleRequestId,
        int GenerationRound,
        string Content,
        bool IsSelected);

    public record VideoTitleFeedbackDto(
        Guid Id,
        string FeedbackText,
        int AppliedToRound);

    public record VideoTitleRequestDto(
        Guid Id,
        Guid ProjectId,
        int RequestedTitleCount,
        CTitleStyle Style,
        string KeywordsRaw,
        string BuiltPromptText,
        int GenerationRound,
        DateTimeOffset CreationTime,
        IReadOnlyList<VideoTitleOptionDto> Options,
        IReadOnlyList<VideoTitleFeedbackDto> Feedbacks);
}
