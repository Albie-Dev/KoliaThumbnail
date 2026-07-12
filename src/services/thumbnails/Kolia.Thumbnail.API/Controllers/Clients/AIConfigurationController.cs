using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý các cấu hình AI, cung cấp các endpoint để thực hiện
    /// các thao tác CRUD (Create, Read, Update, Delete) trên các cấu hình AI.
    /// </summary>
    [ApiController]
    [Route("api/v1/ai-configurations")]
    public class AIConfigurationController : ControllerBase
    {
        private readonly IAIConfigurationService _aiConfigurationService;
        private readonly AIConfigurationMapper _mapper;
        private readonly ILogger<AIConfigurationController> _logger;

        public AIConfigurationController(
            IAIConfigurationService aiConfigurationService,
            AIConfigurationMapper mapper,
            ILogger<AIConfigurationController> logger)
        {
            _aiConfigurationService = aiConfigurationService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các cấu hình AI có phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="deletedOnly"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paging")]
        public async Task<IActionResult> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.GetWithPagingAsync(
                request,
                includeDeleted,
                deletedOnly,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một cấu hình AI theo Id.
        /// </summary>
        /// <param name="aiConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{aiConfigurationId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid aiConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.GetByIdAsync(
                aiConfigurationId,
                asNoTracking: true,
                includeDetails: true,
                cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI configuration với Id '{aiConfigurationId}'.",
                    code: "AI_CONFIGURATION_NOT_FOUND");
            }

            return Ok(_mapper.ToDetailDto(result));
        }

        /// <summary>
        /// Tạo mới một cấu hình AI.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] AIConfiurationCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.CreateAsync(
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin một cấu hình AI.
        /// </summary>
        /// <param name="aiConfigurationId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{aiConfigurationId:guid}")]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] Guid aiConfigurationId,
            [FromBody] AIConfigurationUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.UpdateAsync(
                aiConfigurationId,
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Dặt một cấu hình AI làm mặc định.
        /// </summary>
        /// <param name="aiConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{aiConfigurationId:guid}/set-default")]
        public async Task<IActionResult> SetDefaultAsync(
            [FromRoute] Guid aiConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.SetDefaultAsync(
                aiConfigurationId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Xóa (soft delete) một cấu hình AI.
        /// </summary>
        /// <param name="aiConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{aiConfigurationId:guid}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid aiConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiConfigurationService.DeleteAsync(
                aiConfigurationId,
                cancellationToken);

            return Ok(result);
        }
    }
}