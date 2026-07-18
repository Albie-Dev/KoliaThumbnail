using Kolia.Thumbnail.API.Data.Entities.Socials;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.SocialMedias;

namespace Kolia.Thumbnail.API.Socials
{
    public interface ISocialMediaProviderService
    {
        /// <summary>
        /// Lấy danh sách các nhà cung cấp AI với phân trang dựa trên yêu cầu được cung cấp. Trả về một đối tượng PagedResponseDto chứa danh sách các nhà cung cấp AI và thông tin phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeDeleted">Bao gồm cả bản ghi đã xoá mềm.</param>
        /// <param name="deletedOnly">Chỉ lấy bản ghi đã xoá mềm.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PagedResponseDto<SocialMediaProviderDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo một nhà cung cấp AI mới trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="aIProviderCreateDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SocialMediaProviderDetailDto> CreateAsync(SocialMediaProviderCreateDto aIProviderCreateDto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI tồn tại, trả về thực thể nhà cung cấp AI; nếu không tồn tại, trả về null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SocialMediaProviderEntity?> GetByIdAsync(Guid id,
            bool asNoTracking = true,
            bool includeDeleted = false,
            bool includeDetails = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException;
        /// nếu tên đã tồn tại bởi nhà cung cấp khác, ném ra BusinessException.
        /// Trả về thực thể nhà cung cấp AI đã được cập nhật.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SocialMediaProviderDetailDto> UpdateAsync(Guid id,
            SocialMediaProviderUpdateDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp AI dựa trên ID.
        /// Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException.
        /// Trả về thực thể nhà cung cấp AI đã được đánh dấu xoá.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SocialMediaProviderDetailDto> DeleteAsync(Guid id,
            CancellationToken cancellationToken = default);
    }
}