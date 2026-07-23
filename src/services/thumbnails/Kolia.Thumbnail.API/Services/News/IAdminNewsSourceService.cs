using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.Services.News
{
    public interface IAdminNewsSourceService
    {
        Task<PagedResponseDto<NewsSourceListItemDto>> ListAsync(
            PagedRequestDto request,
            CNewsSourceGroup? group,
            CMarketScope? region,
            bool? isTrusted,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default);

        Task<NewsSourceDetailDto> GetByIdAsync(Guid id, CancellationToken ct);

        Task<NewsSourceDetailDto> CreateAsync(NewsSourceCreateDto dto, CancellationToken ct);

        Task<NewsSourceDetailDto> UpdateAsync(Guid id, NewsSourceUpdateDto dto, CancellationToken ct);

        /// <summary>Toggles IsTrusted for a source without changing other fields.</summary>
        Task<NewsSourceDetailDto> ToggleAsync(Guid id, CancellationToken ct);

        /// <summary>Bulk set IsTrusted for multiple sources (bật/tắt hàng loạt).</summary>
        Task BulkSetTrustAsync(List<Guid> ids, bool isTrusted, CancellationToken ct);

        /// <summary>
        /// Performs a live test-fetch for the source (using the real pipeline)
        /// WITHOUT affecting circuit-breaker or operational stats.
        /// </summary>
        Task<NewsSourceTestFetchResultDto> TestFetchAsync(Guid id, List<string> keywords, CancellationToken ct);

        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
