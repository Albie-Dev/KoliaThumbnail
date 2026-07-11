using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.AIs
{
    public interface IAIConfigurationService
    {
        /// <summary>
        /// Lấy danh sách cấu hình AI có phân trang.
        /// </summary>
        Task<PagedResponseDto<AIConfigurationDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin cấu hình AI theo Id.
        /// </summary>
        Task<AIConfigurationEntity?> GetByIdAsync(
            Guid id,
            bool asNoTracking = true,
            bool includeDetails = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo mới cấu hình AI.
        /// </summary>
        Task<AIConfigurationDetailDto> CreateAsync(
            AIConfiurationCreateDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật cấu hình AI.
        /// </summary>
        Task<AIConfigurationDetailDto> UpdateAsync(
            Guid id,
            AIConfigurationUpdateDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa (soft delete) cấu hình AI.
        /// </summary>
        Task<AIConfigurationDetailDto> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}