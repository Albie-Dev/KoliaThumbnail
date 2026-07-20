using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.Interfaces.GoogleServices
{
    /// <summary>
    /// Service quản lý Google Service Account credentials.
    /// </summary>
    public interface IGoogleServiceAccountService
    {
        /// <summary>Lấy danh sách service account với phân trang</summary>
        Task<PagedResponseDto<GoogleServiceAccountSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default);

        /// <summary>Lấy chi tiết 1 service account</summary>
        Task<GoogleServiceAccountDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>Tạo mới service account từ JSON credential</summary>
        Task<GoogleServiceAccountDto> CreateAsync(
            CreateGoogleServiceAccountRequest request,
            CancellationToken ct = default);

        /// <summary>Cập nhật service account</summary>
        Task<GoogleServiceAccountDto> UpdateAsync(
            Guid id,
            UpdateGoogleServiceAccountRequest request,
            CancellationToken ct = default);

        /// <summary>Xoá mềm service account</summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
