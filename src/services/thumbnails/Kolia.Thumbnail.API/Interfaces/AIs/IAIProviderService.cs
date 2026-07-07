using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.AIs
{
    public interface IAIProviderService
    {
        /// <summary>
        /// Lấy danh sách các nhà cung cấp AI với phân trang dựa trên yêu cầu được cung cấp. Trả về một đối tượng PagedResponseDto chứa danh sách các nhà cung cấp AI và thông tin phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PagedResponseDto<AIProviderDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo một nhà cung cấp AI mới trong cơ sở dữ liệu. Nếu quá trình tạo thành công, trả về thực thể nhà cung cấp AI đã được tạo; nếu có lỗi xảy ra, trả về null.
        /// </summary>
        /// <param name="aIProviderCreateDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AIProviderEntity?> CreateAIProviderAsync(AIProviderCreateDto aIProviderCreateDto,
            CancellationToken cancellationToken = default);
    }
}