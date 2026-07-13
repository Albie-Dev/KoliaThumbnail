using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.SocialMedias;
using Kolia.Thumbnail.API.Socials;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý các nhà cung cấp AI, cung cấp các endpoint để thực hiện các thao tác CRUD (Create, Read, Update, Delete) trên các nhà cung cấp AI.
    /// </summary>
    [ApiController]
    [Route("api/v1/social-media-providers")]
    public class SocialMediaProviderController : ControllerBase
    {
        private readonly ISocialMediaProviderService _socialMediaProvider;
        private readonly ILogger<SocialMediaProviderController> _logger;

        public SocialMediaProviderController(
            ISocialMediaProviderService socialMediaProvider,
            ILogger<SocialMediaProviderController> logger)
        {
            _socialMediaProvider = socialMediaProvider;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các nhà cung cấp AI với phân trang dựa trên yêu cầu được cung cấp.
        /// Trả về một đối tượng PagedResponseDto chứa danh sách các nhà cung cấp AI và thông tin phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paging")]
        public async Task<IActionResult> GetPagingAsync(
            [FromQuery]PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.GetWithPagingAsync(request, includeDeleted, deletedOnly, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{socialMediaProviderId}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid socialMediaProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.GetByIdAsync(socialMediaProviderId, asNoTracking: true, includeDetails: false, cancellationToken: cancellationToken);
            
            if(result == null)
            {
                throw new NotFoundException($"Không tìm thấy nhà cung cấp AI với ID: {socialMediaProviderId}");
            }

            return Ok(result.ToDetailDto());
        }

        /// <summary>
        /// Tạo một nhà cung cấp AI mới.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody]SocialMediaProviderCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.CreateAsync(request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException;
        /// nếu tên đã tồn tại bởi nhà cung cấp khác, ném ra BusinessException.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{socialMediaProviderId}")]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] Guid socialMediaProviderId,
            [FromBody] SocialMediaProviderUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.UpdateAsync(socialMediaProviderId, request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp AI dựa trên ID.
        /// Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{socialMediaProviderId}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid socialMediaProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.DeleteAsync(socialMediaProviderId, cancellationToken);

            return Ok(result);
        }
    }
}