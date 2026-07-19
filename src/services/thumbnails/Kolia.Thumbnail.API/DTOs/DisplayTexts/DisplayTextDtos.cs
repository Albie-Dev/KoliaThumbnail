namespace Kolia.Thumbnail.API.DTOs.DisplayTexts
{
    // ── Requests ──────────────────────────────────────────────────

    public record GenerateDisplayTextRequest(IReadOnlyList<Guid> NewsItemIds);

    public record DisplayTextSelectionRequest(bool IsSelected);

    // ── Response DTOs ─────────────────────────────────────────────

    public record DisplayTextOptionDto(
        Guid Id,
        Guid DisplayTextRequestId,
        Guid SourceNewsItemId,
        string Content,
        bool IsSelected);

    public record DisplayTextRequestDto(
        Guid Id,
        Guid ProjectId,
        DateTimeOffset CreationTime,
        IReadOnlyList<Guid> SelectedNewsItemIds,
        IReadOnlyList<DisplayTextOptionDto> Options);
}
