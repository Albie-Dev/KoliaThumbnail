using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý cấu hình AI cho từng chức năng nghiệp vụ.
    /// Cho phép cấu hình provider, config, model và fallback cho mỗi chức năng.
    /// </summary>
    [ApiController]
    [Route("api/v1/ai-function-configs")]
    public class AIFunctionConfigController : ControllerBase
    {
        private readonly IAIFunctionConfigService _service;

        public AIFunctionConfigController(IAIFunctionConfigService service)
        {
            _service = service;
        }

        /// <summary>Lấy danh sách function configs với phân trang.</summary>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<AIFunctionConfigSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<AIFunctionConfigSummaryDto>>> GetPaging(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            return Ok(await _service.GetWithPagingAsync(request, includeDeleted, deletedOnly, ct));
        }

        /// <summary>Lấy chi tiết function config theo Id (kèm items).</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AIFunctionConfigDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIFunctionConfigDetailDto>> GetById(Guid id, CancellationToken ct)
        {
            return Ok(await _service.GetByIdAsync(id, ct));
        }

        /// <summary>Lấy function config theo FunctionType.</summary>
        [HttpGet("by-function/{functionType}")]
        [ProducesResponseType(typeof(AIFunctionConfigDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIFunctionConfigDetailDto>> GetByFunctionType(CAIFunctionType functionType, CancellationToken ct)
        {
            return Ok(await _service.GetByFunctionTypeAsync(functionType, ct));
        }

        /// <summary>Tạo mới function config (kèm danh sách items).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(AIFunctionConfigDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AIFunctionConfigDetailDto>> Create([FromBody] CreateAIFunctionConfigDto dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Cập nhật function config (thay thế toàn bộ items).</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AIFunctionConfigDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AIFunctionConfigDetailDto>> Update(Guid id, [FromBody] UpdateAIFunctionConfigDto dto, CancellationToken ct)
        {
            return Ok(await _service.UpdateAsync(id, dto, ct));
        }

        /// <summary>Xoá mềm function config.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách models available từ provider, dùng API key từ configuration.
        /// </summary>
        [HttpGet("provider-models")]
        [ProducesResponseType(typeof(List<AIModelInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<AIModelInfo>>> GetProviderModels(
            [FromQuery] Guid providerId,
            [FromQuery] Guid configurationId,
            CancellationToken ct)
        {
            var models = await _service.GetProviderModelsAsync(providerId, configurationId, ct);
            return Ok(models);
        }
    }
}
