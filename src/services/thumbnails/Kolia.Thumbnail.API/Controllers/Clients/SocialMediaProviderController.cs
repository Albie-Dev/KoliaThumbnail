using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.SocialMedias;
using Kolia.Thumbnail.API.Socials;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý các nhà cung cấp Social Media, cung cấp các endpoint để thực hiện các thao tác CRUD (Create, Read, Update, Delete).
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
        /// Lấy danh sách các nhà cung cấp Social Media với phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<SocialMediaProviderDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<SocialMediaProviderDetailDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.GetWithPagingAsync(request, includeDeleted, deletedOnly, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp Social Media dựa trên ID.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{socialMediaProviderId}")]
        [ProducesResponseType(typeof(SocialMediaProviderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SocialMediaProviderDetailDto>> GetByIdAsync(
            [FromRoute] Guid socialMediaProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.GetByIdAsync(socialMediaProviderId, asNoTracking: true, includeDetails: false, cancellationToken: cancellationToken);

            if (result == null)
            {
                throw new NotFoundException($"Không tìm thấy nhà cung cấp Social Media với ID: {socialMediaProviderId}");
            }

            return Ok(result.ToDetailDto());
        }

        /// <summary>
        /// Tạo một nhà cung cấp Social Media mới.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(SocialMediaProviderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SocialMediaProviderDetailDto>> CreateAsync(
            [FromBody] SocialMediaProviderCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.CreateAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp Social Media.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{socialMediaProviderId}")]
        [ProducesResponseType(typeof(SocialMediaProviderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SocialMediaProviderDetailDto>> UpdateAsync(
            [FromRoute] Guid socialMediaProviderId,
            [FromBody] SocialMediaProviderUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.UpdateAsync(socialMediaProviderId, request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp Social Media.
        /// </summary>
        /// <param name="socialMediaProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{socialMediaProviderId}")]
        [ProducesResponseType(typeof(SocialMediaProviderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SocialMediaProviderDetailDto>> DeleteAsync(
            [FromRoute] Guid socialMediaProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _socialMediaProvider.DeleteAsync(socialMediaProviderId, cancellationToken);
            return Ok(result);
        }
    }
}