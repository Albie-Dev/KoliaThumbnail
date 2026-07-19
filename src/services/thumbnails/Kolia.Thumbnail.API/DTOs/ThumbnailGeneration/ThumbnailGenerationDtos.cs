using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.ThumbnailGeneration
{
    // ── Requests ──────────────────────────────────────────────────

    public record GenerateThumbnailRequest(
        IReadOnlyList<Guid> DisplayTextOptionIds,
        IReadOnlyList<Guid> ReferenceLibraryItemIds,
        Guid? CharacterId,
        string ChangesRequestText,
        string Ratio,
        string Resolution,
        int RequestedCount,
        string? OverridePromptText = null);

    public record EditThumbnailRequest(
        CThumbnailEditTool EditTool,
        string EditRequestText,
        Guid? SecondaryReferenceLibraryItemId = null,
        Guid? SecondaryCharacterImageId = null);

    public record ExportThumbnailPromptRequest(
        IReadOnlyList<Guid> DisplayTextOptionIds,
        IReadOnlyList<Guid> ReferenceLibraryItemIds,
        Guid? CharacterId,
        string ChangesRequestText,
        string Ratio,
        string Resolution);

    // ── Response DTOs ─────────────────────────────────────────────

    public record GeneratedThumbnailDto(
        Guid Id,
        Guid GeneratedThumbnailSetId,
        int VariantIndex,
        Guid? ParentGeneratedThumbnailId,
        int VersionNumber,
        string ImageUrl,
        string DisplayTextSnapshot,
        string? CharacterSnapshotName,
        CThumbnailEditTool? LastEditTool,
        bool IsApproved,
        DateTimeOffset? ApprovedAt,
        bool WasDownloaded,
        bool IsPushedToTitleStep);

    public record GeneratedThumbnailSetDto(
        Guid Id,
        Guid ThumbnailGenerationRequestId,
        int SetIndex,
        DateTimeOffset CreationTime,
        IReadOnlyList<GeneratedThumbnailDto> Variants);

    public record ThumbnailGenerationRequestDto(
        Guid Id,
        Guid ProjectId,
        Guid? CharacterId,
        string ChangesRequestText,
        string? GeneratedPromptText,
        string Ratio,
        string Resolution,
        int RequestedImageCount,
        IReadOnlyList<GeneratedThumbnailSetDto> GeneratedSets);
}
