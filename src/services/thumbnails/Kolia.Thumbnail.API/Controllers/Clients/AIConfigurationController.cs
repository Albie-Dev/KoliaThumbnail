using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
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
    public class AIProviderConfigurationController : ControllerBase
    {
        private readonly IAIProviderConfigurationService _aiProviderConfigurationService;
        private readonly AIProviderConfigurationMapper _mapper;
        private readonly ILogger<AIProviderConfigurationController> _logger;

        public AIProviderConfigurationController(
            IAIProviderConfigurationService aiProviderConfigurationService,
            AIProviderConfigurationMapper mapper,
            ILogger<AIProviderConfigurationController> logger)
        {
            _aiProviderConfigurationService = aiProviderConfigurationService;
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
        [ProducesResponseType(typeof(PagedResponseDto<AIProviderConfigurationDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<AIProviderConfigurationDetailDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.GetWithPagingAsync(
                request,
                includeDeleted,
                deletedOnly,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một cấu hình AI theo Id.
        /// </summary>
        /// <param name="aiProviderConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{aiProviderConfigurationId:guid}")]
        [ProducesResponseType(typeof(AIProviderConfigurationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIProviderConfigurationDetailDto>> GetByIdAsync(
            [FromRoute] Guid aiProviderConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.GetByIdAsync(
                aiProviderConfigurationId,
                asNoTracking: true,
                includeDetails: true,
                cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI configuration với Id '{aiProviderConfigurationId}'.",
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
        [ProducesResponseType(typeof(AIProviderConfigurationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AIProviderConfigurationDetailDto>> CreateAsync(
            [FromBody] AIConfiurationCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.CreateAsync(
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin một cấu hình AI.
        /// </summary>
        /// <param name="aiProviderConfigurationId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{aiProviderConfigurationId:guid}")]
        [ProducesResponseType(typeof(AIProviderConfigurationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AIProviderConfigurationDetailDto>> UpdateAsync(
            [FromRoute] Guid aiProviderConfigurationId,
            [FromBody] AIProviderConfigurationUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.UpdateAsync(
                aiProviderConfigurationId,
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Dặt một cấu hình AI làm mặc định.
        /// </summary>
        /// <param name="aiProviderConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{aiProviderConfigurationId:guid}/set-default")]
        [ProducesResponseType(typeof(AIProviderConfigurationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIProviderConfigurationDetailDto>> SetDefaultAsync(
            [FromRoute] Guid aiProviderConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.SetDefaultAsync(
                aiProviderConfigurationId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Xóa (soft delete) một cấu hình AI.
        /// </summary>
        /// <param name="aiProviderConfigurationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{aiProviderConfigurationId:guid}")]
        [ProducesResponseType(typeof(AIProviderConfigurationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIProviderConfigurationDetailDto>> DeleteAsync(
            [FromRoute] Guid aiProviderConfigurationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderConfigurationService.DeleteAsync(
                aiProviderConfigurationId,
                cancellationToken);

            return Ok(result);
        }
    }
}