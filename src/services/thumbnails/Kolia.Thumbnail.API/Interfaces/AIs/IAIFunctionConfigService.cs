using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.AIs
{
    /// <summary>
    /// Service quản lý cấu hình AI cho từng chức năng nghiệp vụ.
    /// </summary>
    public interface IAIFunctionConfigService
    {
        /// <summary>Lấy danh sách function configs với phân trang.</summary>
        Task<PagedResponseDto<AIFunctionConfigSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default);

        /// <summary>Lấy chi tiết function config theo Id (kèm items).</summary>
        Task<AIFunctionConfigDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>Lấy function config theo FunctionType.</summary>
        Task<AIFunctionConfigDetailDto> GetByFunctionTypeAsync(CAIFunctionType functionType, CancellationToken ct = default);

        /// <summary>Tạo mới function config (kèm items).</summary>
        Task<AIFunctionConfigDetailDto> CreateAsync(CreateAIFunctionConfigDto dto, CancellationToken ct = default);

        /// <summary>Cập nhật function config (thay thế toàn bộ items).</summary>
        Task<AIFunctionConfigDetailDto> UpdateAsync(Guid id, UpdateAIFunctionConfigDto dto, CancellationToken ct = default);

        /// <summary>Xoá mềm function config.</summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>Lấy danh sách models từ provider.</summary>
        Task<List<AIModelInfo>> GetProviderModelsAsync(Guid providerId, Guid configurationId, CancellationToken ct = default);
    }
}
