using Kolia.Thumbnail.API.DTOs.CompletePackages;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.CompletePackages;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý xác nhận và lưu trữ lịch sử các bộ sản phẩm hoàn chỉnh (Complete Package).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/complete-packages")]
    public class CompletePackageController : ControllerBase
    {
        private readonly ICompletePackageService _packageService;

        public CompletePackageController(ICompletePackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>
        /// Lấy toàn bộ các bộ sản phẩm hoàn chỉnh đã xác nhận của một dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách CompletePackageDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CompletePackageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<CompletePackageDto>>> GetPackages(Guid projectId, CancellationToken ct = default)
        {
            var pkgs = await _packageService.GetByProjectAsync(projectId, ct);
            var dtos = pkgs.Select(p => new CompletePackageDto(
                p.Id,
                p.ProjectId,
                p.SelectedThumbnailId,
                p.SelectedThumbnail.ImageUrl,
                p.DisplayTextSnapshot,
                p.ConfirmedAt,
                p.SelectedTitles.Select(t => new CompletePackageTitleDto(
                    t.VideoTitleOptionId,
                    t.VideoTitleOption.Content
                )).ToList()
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Xác nhận lưu trữ bộ sản phẩm hoàn chỉnh cuối cùng (gồm 1 ảnh thumbnail và các tiêu đề đã chọn).
        /// Tự động cập nhật trạng thái dự án sang Completed.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Thông tin ảnh thumbnail và tiêu đề được lựa chọn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>CompletePackageDto thông tin bộ sản phẩm vừa lưu</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy thumbnail hoặc tiêu đề được chỉ định</exception>
        [HttpPost]
        [ProducesResponseType(typeof(CompletePackageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CompletePackageDto>> Confirm(Guid projectId, [FromBody] ConfirmPackageRequest request, CancellationToken ct = default)
        {
            var p = await _packageService.ConfirmAsync(projectId, request.SelectedThumbnailId, request.SelectedTitleOptionIds, ct);
            
            var pkgs = await _packageService.GetByProjectAsync(projectId, ct);
            var freshPkg = pkgs.FirstOrDefault(k => k.Id == p.Id);
            if (freshPkg == null)
            {
                return Created("", new { id = p.Id });
            }

            var dto = new CompletePackageDto(
                freshPkg.Id,
                freshPkg.ProjectId,
                freshPkg.SelectedThumbnailId,
                freshPkg.SelectedThumbnail.ImageUrl,
                freshPkg.DisplayTextSnapshot,
                freshPkg.ConfirmedAt,
                freshPkg.SelectedTitles.Select(t => new CompletePackageTitleDto(
                    t.VideoTitleOptionId,
                    t.VideoTitleOption.Content
                )).ToList()
            );

            return Created("", dto);
        }
    }
}
