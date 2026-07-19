using Kolia.Thumbnail.API.DTOs.DisplayTexts;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.DisplayTexts;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller tạo và quản lý Display Text (Chữ hiển thị trên thumbnail - Phần 4.1).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/display-texts")]
    public class DisplayTextController : ControllerBase
    {
        private readonly IDisplayTextService _displayTextService;

        public DisplayTextController(IDisplayTextService displayTextService)
        {
            _displayTextService = displayTextService;
        }

        /// <summary>
        /// Lấy toàn bộ các yêu cầu sinh Display Text và các tùy chọn tương ứng trong dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách DisplayTextRequestDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DisplayTextRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<DisplayTextRequestDto>>> GetDisplayTexts(Guid projectId, CancellationToken ct = default)
        {
            var requests = await _displayTextService.GetByProjectAsync(projectId, ct);
            var dtos = requests.Select(r => new DisplayTextRequestDto(
                r.Id,
                r.ProjectId,
                r.CreationTime,
                r.SelectedNewsItems.Select(n => n.NewsItemId).ToList(),
                r.Options.Select(o => new DisplayTextOptionDto(
                    o.Id,
                    o.DisplayTextRequestId,
                    o.SourceNewsItemId,
                    o.Content,
                    o.IsSelected
                )).ToList()
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Yêu cầu AI sinh danh sách các Display Text dựa trên các tin tức đã chọn ở Phần 2.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Danh sách Id tin tức nguồn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>DisplayTextRequestDto chứa thông tin kết quả vừa tạo</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(DisplayTextRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DisplayTextRequestDto>> Generate(Guid projectId, [FromBody] GenerateDisplayTextRequest request, CancellationToken ct = default)
        {
            var r = await _displayTextService.GenerateAsync(projectId, request.NewsItemIds, ct);
            var dto = new DisplayTextRequestDto(
                r.Id,
                r.ProjectId,
                r.CreationTime,
                r.SelectedNewsItems.Select(n => n.NewsItemId).ToList(),
                r.Options.Select(o => new DisplayTextOptionDto(
                    o.Id,
                    o.DisplayTextRequestId,
                    o.SourceNewsItemId,
                    o.Content,
                    o.IsSelected
                )).ToList()
            );
            return Ok(dto);
        }

        /// <summary>
        /// Tick chọn hoặc bỏ chọn một phương án Display Text để dùng khi sinh ảnh AI tiếp theo.
        /// </summary>
        /// <param name="optionId">Id của tùy chọn Display Text</param>
        /// <param name="request">Trạng thái chọn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy tùy chọn tương ứng</exception>
        [HttpPut("options/{optionId:guid}/select")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SelectOption(Guid optionId, [FromBody] DisplayTextSelectionRequest request, CancellationToken ct = default)
        {
            await _displayTextService.SetSelectedAsync(optionId, request.IsSelected, ct);
            return NoContent();
        }
    }
}
