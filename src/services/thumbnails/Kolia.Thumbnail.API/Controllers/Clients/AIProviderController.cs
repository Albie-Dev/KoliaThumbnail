using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý các nhà cung cấp AI, cung cấp các endpoint để thực hiện các thao tác CRUD (Create, Read, Update, Delete) trên các nhà cung cấp AI.
    /// </summary>
    [ApiController]
    [Route("api/v1/ai-providers")]
    public class AIProviderController : ControllerBase
    {
        private readonly IAIProviderService _aiProviderService;
        private readonly ILogger<AIProviderController> _logger;

        public AIProviderController(
            IAIProviderService aiProviderService,
            ILogger<AIProviderController> logger)
        {
            _aiProviderService = aiProviderService;
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
            var result = await _aiProviderService.GetWithPagingAsync(request, includeDeleted, deletedOnly, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException.
        /// </summary>
        /// <param name="aiProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{aiProviderId}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid aiProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.GetByIdAsync(aiProviderId, asNoTracking: true, includeDetails: false, cancellationToken: cancellationToken);
            
            if(result == null)
            {
                throw new NotFoundException($"Không tìm thấy nhà cung cấp AI với ID: {aiProviderId}");
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
            [FromBody]AIProviderCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.CreateAsync(request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp AI dựa trên ID. Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException;
        /// nếu tên đã tồn tại bởi nhà cung cấp khác, ném ra BusinessException.
        /// </summary>
        /// <param name="aiProviderId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{aiProviderId}")]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] Guid aiProviderId,
            [FromBody] AIProviderUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.UpdateAsync(aiProviderId, request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp AI dựa trên ID.
        /// Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException.
        /// </summary>
        /// <param name="aiProviderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{aiProviderId}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid aiProviderId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.DeleteAsync(aiProviderId, cancellationToken);

            return Ok(result);
        }
    }
}