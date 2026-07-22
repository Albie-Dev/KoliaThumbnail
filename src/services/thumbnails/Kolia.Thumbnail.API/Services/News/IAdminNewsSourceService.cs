using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.News
{
    public interface IAdminNewsSourceService
    {
        Task<IReadOnlyList<NewsSourceListItemDto>> ListAsync(
            CNewsSourceGroup? group, CMarketScope? region, bool? isTrusted, CancellationToken ct);

        Task<NewsSourceDetailDto> GetByIdAsync(Guid id, CancellationToken ct);

        Task<NewsSourceDetailDto> CreateAsync(NewsSourceCreateDto dto, CancellationToken ct);

        Task<NewsSourceDetailDto> UpdateAsync(Guid id, NewsSourceUpdateDto dto, CancellationToken ct);

        /// <summary>Toggles IsTrusted for a source without changing other fields.</summary>
        Task<NewsSourceDetailDto> ToggleAsync(Guid id, CancellationToken ct);

        /// <summary>
        /// Performs a live test-fetch for the source (using the real pipeline)
        /// WITHOUT affecting circuit-breaker or operational stats.
        /// </summary>
        Task<NewsSourceTestFetchResultDto> TestFetchAsync(Guid id, List<string> keywords, CancellationToken ct);

        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
