namespace Kolia.Thumbnail.API.DTOs.CompletePackages
{
    // ── Requests ──────────────────────────────────────────────────

    public record ConfirmPackageRequest(
        Guid SelectedThumbnailId,
        IReadOnlyList<Guid> SelectedTitleOptionIds);

    // ── Response DTOs ─────────────────────────────────────────────

    public record CompletePackageTitleDto(Guid VideoTitleOptionId, string Content);

    public record CompletePackageDto(
        Guid Id,
        Guid ProjectId,
        Guid SelectedThumbnailId,
        string ThumbnailImageUrl,
        string DisplayTextSnapshot,
        DateTimeOffset ConfirmedAt,
        IReadOnlyList<CompletePackageTitleDto> SelectedTitles);
}
