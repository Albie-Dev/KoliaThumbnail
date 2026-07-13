using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.AIs
{
    public interface IAIProviderConfigurationService
    {
        /// <summary>
        /// Lấy danh sách cấu hình AI có phân trang.
        /// </summary>
        Task<PagedResponseDto<AIProviderConfigurationDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin cấu hình AI theo Id.
        /// </summary>
        Task<AIProviderConfigurationEntity?> GetByIdAsync(
            Guid id,
            bool asNoTracking = true,
            bool includeDetails = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo mới cấu hình AI.
        /// </summary>
        Task<AIProviderConfigurationDetailDto> CreateAsync(
            AIConfiurationCreateDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật cấu hình AI.
        /// </summary>
        Task<AIProviderConfigurationDetailDto> UpdateAsync(
            Guid id,
            AIProviderConfigurationUpdateDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đặt cấu hình AI mặc định.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AIProviderConfigurationDetailDto> SetDefaultAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa (soft delete) cấu hình AI.
        /// </summary>
        Task<AIProviderConfigurationDetailDto> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}