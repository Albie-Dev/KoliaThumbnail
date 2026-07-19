namespace Kolia.Thumbnail.API.DTOs.Characters
{
    // ── Requests ──────────────────────────────────────────────────

    public record CreateCharacterRequest(string Name, string? Description);

    public record AddCharacterImageRequest(
        string ImageUrl,
        string? ExpressionLabel,
        string? AngleLabel,
        bool IsPrimary);

    // ── Response DTOs ─────────────────────────────────────────────

    public record CharacterImageDto(
        Guid Id,
        string ImageUrl,
        string? ExpressionLabel,
        string? AngleLabel,
        bool IsPrimary);

    public record CharacterDto(
        Guid Id,
        string Name,
        string? Description,
        IReadOnlyList<CharacterImageDto> Images);

    public record CharacterSummaryDto(
        Guid Id,
        string Name,
        string? Description,
        CharacterImageDto? PrimaryImage);
}
