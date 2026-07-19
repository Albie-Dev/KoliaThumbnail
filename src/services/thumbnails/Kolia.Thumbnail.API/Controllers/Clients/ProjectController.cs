using Kolia.Thumbnail.API.DTOs.Projects;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Services.Projects;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý luồng dự án (Project), cung cấp các endpoint phân trang, quản lý trạng thái các bước (5 Steps).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Lấy danh sách project với phân trang dựa trên yêu cầu (search, filter, sort).
        /// </summary>
        /// <param name="request">Thông tin yêu cầu phân trang</param>
        /// <param name="includeDeleted">Bao gồm bản ghi đã xoá mềm</param>
        /// <param name="deletedOnly">Chỉ lấy bản ghi đã xoá mềm</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>PagedResponseDto chứa danh sách ProjectSummaryDto và thông tin phân trang</returns>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<ProjectSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<ProjectSummaryDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            var result = await _projectService.GetWithPagingAsync(request, includeDeleted, deletedOnly, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của 1 project kèm danh sách các step bên trong.
        /// </summary>
        /// <param name="id">Id của project</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ProjectDetailDto chứa thông tin chi tiết project và các bước</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy project</exception>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var project = await _projectService.GetByIdAsync(id, ct);
            if (project == null)
            {
                throw new NotFoundException($"Không tìm thấy project với ID: {id}");
            }

            var dto = new ProjectDetailDto(
                project.Id,
                project.Name,
                project.Status,
                project.CurrentStepNumber,
                project.ThumbnailCoverUrl,
                project.LastActivityTime,
                project.CreationTime,
                project.Steps.Select(s => new ProjectStepDto(
                    s.Id,
                    s.StepNumber,
                    s.StepName,
                    s.StepStatus,
                    s.OutputSummaryText,
                    s.NeedsApproval,
                    s.DeletionTime
                )).ToList());

            return Ok(dto);
        }

        /// <summary>
        /// Tạo một project mới và tự động seed 5 bước quy trình.
        /// </summary>
        /// <param name="request">Thông tin tên project mới</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Thông tin Project mới được tạo</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDetailDto>> Create([FromBody] CreateProjectRequest request, CancellationToken ct = default)
        {
            var project = await _projectService.CreateAsync(request.Name, ct);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { id = project.Id });
        }

        /// <summary>
        /// Đổi tên dự án.
        /// </summary>
        /// <param name="id">Id của project</param>
        /// <param name="request">Tên mới của project</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy project</exception>
        [HttpPut("{id:guid}/rename")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rename(Guid id, [FromBody] RenameProjectRequest request, CancellationToken ct = default)
        {
            await _projectService.RenameAsync(id, request.NewName, ct);
            return NoContent();
        }

        /// <summary>
        /// Xóa mềm một dự án.
        /// </summary>
        /// <param name="id">Id của project</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy project</exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await _projectService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Chuyển bước hiện tại của project lên bước tiếp theo.
        /// </summary>
        /// <param name="id">Id của project</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy project</exception>
        [HttpPost("{id:guid}/advance-step")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdvanceStep(Guid id, CancellationToken ct = default)
        {
            await _projectService.AdvanceStepAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Cập nhật trạng thái của một bước cụ thể trong dự án.
        /// </summary>
        /// <param name="id">Id của project</param>
        /// <param name="stepNumber">Mã bước cần cập nhật</param>
        /// <param name="request">Trạng thái mới và mô tả tổng quan kết quả</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy bước tương ứng của project</exception>
        [HttpPut("{id:guid}/steps/{stepNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStepStatus(Guid id, Enums.CProjectStepNumber stepNumber, [FromBody] ProjectStepStatusUpdateRequest request, CancellationToken ct = default)
        {
            await _projectService.UpdateStepStatusAsync(id, stepNumber, request.NewStatus, request.OutputSummaryText, ct);
            return NoContent();
        }
    }
}
