namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    public record NewsSourceSearchLog(
    string SourceName,
    string Keywords,
    int ResultCount,
    bool Success,
    string? ErrorMessage = null,
    bool ServedFromCache = false);
}