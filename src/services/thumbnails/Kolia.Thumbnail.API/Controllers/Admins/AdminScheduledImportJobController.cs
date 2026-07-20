using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Interfaces.GoogleServices;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Admins
{
    /// <summary>
    /// Controller quản lý Scheduled Import Jobs (Admin).
    /// Cho phép tạo, cập nhật, huỷ, kiểm tra quyền, và xem log jobs.
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/scheduled-import-jobs")]
    public class AdminScheduledImportJobController : ControllerBase
    {
        private readonly IScheduledImportJobService _service;
        private readonly ILogger<AdminScheduledImportJobController> _logger;

        public AdminScheduledImportJobController(
            IScheduledImportJobService service,
            ILogger<AdminScheduledImportJobController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách jobs với phân trang.
        /// </summary>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<ScheduledJobSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<ScheduledJobSummaryDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            var result = await _service.GetWithPagingAsync(request, includeDeleted, deletedOnly, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết 1 job.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ScheduledJobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScheduledJobDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            if (dto == null)
                throw new NotFoundException($"Không tìm thấy job với ID: {id}");
            return Ok(dto);
        }

        /// <summary>
        /// Tạo mới scheduled import job.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ScheduledJobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ScheduledJobDto>> Create(
            [FromBody] CreateScheduledJobRequest request,
            CancellationToken ct = default)
        {
            var dto = await _service.CreateAsync(request, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật job (chỉ khi Pending).
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ScheduledJobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScheduledJobDto>> Update(
            Guid id,
            [FromBody] UpdateScheduledJobRequest request,
            CancellationToken ct = default)
        {
            var dto = await _service.UpdateAsync(id, request, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Huỷ job (chỉ khi Pending hoặc Failed).
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct = default)
        {
            await _service.CancelAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Xoá mềm job (chỉ khi Completed, Failed, hoặc Cancelled).
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Kiểm tra quyền truy cập của service account vào URL.
        /// </summary>
        [HttpPost("check-access")]
        [ProducesResponseType(typeof(CheckAccessResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CheckAccessResult>> CheckAccess(
            [FromBody] CheckAccessRequest request,
            CancellationToken ct = default)
        {
            var result = await _service.CheckAccessAsync(request, ct);
            return Ok(result);
        }

        /// <summary>
        /// Chạy lại job đã thất bại.
        /// </summary>
        [HttpPost("{id:guid}/retry")]
        [ProducesResponseType(typeof(ScheduledJobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScheduledJobDto>> Retry(Guid id, CancellationToken ct = default)
        {
            var dto = await _service.RetryAsync(id, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Lấy log chi tiết của job.
        /// </summary>
        [HttpGet("{id:guid}/logs")]
        [ProducesResponseType(typeof(IReadOnlyList<LogEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<LogEntry>>> GetLogs(Guid id, CancellationToken ct = default)
        {
            var logs = await _service.GetLogsAsync(id, ct);
            return Ok(logs);
        }
    }
}
