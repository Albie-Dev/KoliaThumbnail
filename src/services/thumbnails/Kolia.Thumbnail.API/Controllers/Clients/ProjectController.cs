using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.Projects;
using Kolia.Thumbnail.API.Projects;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý các dự án (Project), cung cấp các endpoint để thực hiện
    /// các thao tác CRUD, quản lý từng bước (step) trong pipeline, chuyển trạng thái
    /// dự án (state machine) và lấy số liệu thống kê cho Dashboard.
    /// </summary>
    [ApiController]
    [Route("api/v1/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        // ───────────────────────── CRUD ─────────────────────────

        /// <summary>
        /// Lấy danh sách các dự án có phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="deletedOnly"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<ProjectDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<ProjectDetailDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetWithPagingAsync(
                request,
                includeDeleted,
                deletedOnly,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một dự án theo Id.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="includeSteps"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{projectId:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> GetByIdAsync(
            [FromRoute] Guid projectId,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] bool includeSteps = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetByIdAsync(
                projectId,
                asNoTracking: true,
                includeDeleted: includeDeleted,
                includeSteps: includeSteps,
                cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy dự án với Id '{projectId}'.",
                    code: "PROJECT_NOT_FOUND");
            }

            return Ok(result.ToDetailDto());
        }

        /// <summary>
        /// Tạo mới một dự án. Toàn bộ các bước (step) trong pipeline sẽ được
        /// tự động khởi tạo theo cấu hình StepDefinition hiện có.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDetailDto>> CreateAsync(
            [FromBody] ProjectCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.CreateAsync(
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin cơ bản của một dự án (tên, mã, mô tả).
        /// Không dùng endpoint này để đổi trạng thái dự án — dùng các endpoint
        /// start/pause/resume/cancel bên dưới để đảm bảo state machine hợp lệ.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{projectId:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDetailDto>> UpdateAsync(
            [FromRoute] Guid projectId,
            [FromBody] ProjectUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.UpdateAsync(
                projectId,
                request,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Xóa (soft delete) một dự án.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{projectId:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> DeleteAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.DeleteAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        // ───────────────────────── Step management ─────────────────────────

        /// <summary>
        /// Lấy danh sách phẳng (flat list) các bước thực thi của một dự án, sắp xếp
        /// theo đúng thứ tự pipeline.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{projectId:guid}/steps")]
        [ProducesResponseType(typeof(IReadOnlyList<ProjectStepDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProjectStepDetailDto>>> GetStepsAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetStepsAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy cây các bước của một dự án (bao gồm cả bước nhóm như "Thumbnail tham khảo",
        /// "Thumbnail" cùng các bước con), dùng để render sidebar/tiến trình trên UI.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{projectId:guid}/steps/tree")]
        [ProducesResponseType(typeof(IReadOnlyList<ProjectStepTreeNodeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProjectStepTreeNodeDto>>> GetStepTreeAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetStepTreeAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật trạng thái và/hoặc nội dung của một bước cụ thể trong dự án.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="stepDefinitionId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{projectId:guid}/steps/{stepDefinitionId:guid}")]
        [ProducesResponseType(typeof(ProjectStepDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectStepDetailDto>> UpdateStepAsync(
            [FromRoute] Guid projectId,
            [FromRoute] Guid stepDefinitionId,
            [FromBody] ProjectStepUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.UpdateStepAsync(
                projectId,
                stepDefinitionId,
                request,
                cancellationToken);

            return Ok(result);
        }

        // ───────────────────────── Status transitions ─────────────────────────

        /// <summary>
        /// Bắt đầu chạy dự án (chuyển sang trạng thái Running).
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{projectId:guid}/start")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> StartAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.StartAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Tạm dừng dự án đang chạy.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{projectId:guid}/pause")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> PauseAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.PauseAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Tiếp tục chạy dự án đang tạm dừng.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{projectId:guid}/resume")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> ResumeAsync(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.ResumeAsync(
                projectId,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Hủy dự án.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{projectId:guid}/cancel")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> CancelAsync(
            [FromRoute] Guid projectId,
            [FromBody] CancelProjectDto? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.CancelAsync(
                projectId,
                request?.Reason,
                cancellationToken);

            return Ok(result);
        }

        // ───────────────────────── Dashboard ─────────────────────────

        /// <summary>
        /// Lấy số liệu thống kê tổng quan cho Dashboard: tổng số dự án theo trạng thái,
        /// tiến độ trung bình, xu hướng tạo dự án theo ngày, các bước hay gặp lỗi/tắc nghẽn
        /// và danh sách dự án gần đây.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("dashboard/statistics")]
        [ProducesResponseType(typeof(ProjectDashboardStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProjectDashboardStatisticsDto>> GetDashboardStatisticsAsync(
            [FromQuery] DashboardStatisticsRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetDashboardStatisticsAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
    }
}