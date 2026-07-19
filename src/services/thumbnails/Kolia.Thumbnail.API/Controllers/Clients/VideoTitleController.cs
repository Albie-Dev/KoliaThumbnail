using Kolia.Thumbnail.API.DTOs.VideoTitles;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.VideoTitles;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller tạo và quản lý Video Title (Tiêu đề video - Phần 5). Hỗ trợ nâng cấp tiêu đề dựa trên phản hồi.
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/video-titles")]
    public class VideoTitleController : ControllerBase
    {
        private readonly IVideoTitleService _titleService;

        public VideoTitleController(IVideoTitleService titleService)
        {
            _titleService = titleService;
        }

        /// <summary>
        /// Lấy toàn bộ các yêu cầu sinh tiêu đề (Requests) và danh sách phương án đề xuất kèm lịch sử phản hồi trong dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách VideoTitleRequestDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<VideoTitleRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<VideoTitleRequestDto>>> GetTitles(Guid projectId, CancellationToken ct = default)
        {
            var reqs = await _titleService.GetByProjectAsync(projectId, ct);
            var dtos = reqs.Select(r => new VideoTitleRequestDto(
                r.Id,
                r.ProjectId,
                r.RequestedTitleCount,
                r.Style,
                r.KeywordsRaw,
                r.BuiltPromptText,
                r.GenerationRound,
                r.CreationTime,
                r.Options.Select(o => new VideoTitleOptionDto(
                    o.Id,
                    o.VideoTitleRequestId,
                    o.GenerationRound,
                    o.Content,
                    o.IsSelected
                )).ToList(),
                r.Feedbacks.Select(f => new VideoTitleFeedbackDto(
                    f.Id,
                    f.FeedbackText,
                    f.AppliedToRound
                )).ToList()
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Tạo một yêu cầu sinh danh sách tiêu đề mới dựa trên ảnh thumbnail đã duyệt, các bản tin được chọn và phong cách viết.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Tham số tạo tiêu đề</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>VideoTitleRequestDto chứa kết quả vừa sinh</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(VideoTitleRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VideoTitleRequestDto>> Generate(Guid projectId, [FromBody] GenerateVideoTitleRequest request, CancellationToken ct = default)
        {
            var r = await _titleService.GenerateAsync(
                projectId, request.SelectedThumbnailIds, request.SelectedNewsItemIds,
                request.Style, request.KeywordsRaw, request.RequestedCount, ct);

            var dto = new VideoTitleRequestDto(
                r.Id,
                r.ProjectId,
                r.RequestedTitleCount,
                r.Style,
                r.KeywordsRaw,
                r.BuiltPromptText,
                r.GenerationRound,
                r.CreationTime,
                r.Options.Select(o => new VideoTitleOptionDto(
                    o.Id,
                    o.VideoTitleRequestId,
                    o.GenerationRound,
                    o.Content,
                    o.IsSelected
                )).ToList(),
                r.Feedbacks.Select(f => new VideoTitleFeedbackDto(
                    f.Id,
                    f.FeedbackText,
                    f.AppliedToRound
                )).ToList()
            );
            return Ok(dto);
        }

        /// <summary>
        /// Thực hiện sinh lại tiêu đề (Regenerate) không có phản hồi, tăng Generation Round của yêu cầu chỉ định.
        /// </summary>
        /// <param name="requestId">Id của yêu cầu sinh tiêu đề</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>VideoTitleRequestDto sau khi sinh lại</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy yêu cầu gốc</exception>
        [HttpPost("requests/{requestId:guid}/regenerate")]
        [ProducesResponseType(typeof(VideoTitleRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VideoTitleRequestDto>> Regenerate(Guid requestId, CancellationToken ct = default)
        {
            var r = await _titleService.RegenerateAsync(requestId, ct);
            var dto = new VideoTitleRequestDto(
                r.Id,
                r.ProjectId,
                r.RequestedTitleCount,
                r.Style,
                r.KeywordsRaw,
                r.BuiltPromptText,
                r.GenerationRound,
                r.CreationTime,
                r.Options.Select(o => new VideoTitleOptionDto(
                    o.Id,
                    o.VideoTitleRequestId,
                    o.GenerationRound,
                    o.Content,
                    o.IsSelected
                )).ToList(),
                r.Feedbacks.Select(f => new VideoTitleFeedbackDto(
                    f.Id,
                    f.FeedbackText,
                    f.AppliedToRound
                )).ToList()
            );
            return Ok(dto);
        }

        /// <summary>
        /// Sinh lại tiêu đề dựa trên phản hồi góp ý của người dùng (Feedback-Driven), đưa nội dung phản hồi vào prompt để AI tối ưu.
        /// </summary>
        /// <param name="requestId">Id của yêu cầu sinh tiêu đề</param>
        /// <param name="request">Nội dung phản hồi góp ý</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>VideoTitleRequestDto chứa kết quả cải tiến vừa sinh</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy yêu cầu gốc</exception>
        [HttpPost("requests/{requestId:guid}/feedback")]
        [ProducesResponseType(typeof(VideoTitleRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VideoTitleRequestDto>> RegenerateWithFeedback(Guid requestId, [FromBody] VideoTitleFeedbackRequest request, CancellationToken ct = default)
        {
            var r = await _titleService.RegenerateWithFeedbackAsync(requestId, request.FeedbackText, ct);
            var dto = new VideoTitleRequestDto(
                r.Id,
                r.ProjectId,
                r.RequestedTitleCount,
                r.Style,
                r.KeywordsRaw,
                r.BuiltPromptText,
                r.GenerationRound,
                r.CreationTime,
                r.Options.Select(o => new VideoTitleOptionDto(
                    o.Id,
                    o.VideoTitleRequestId,
                    o.GenerationRound,
                    o.Content,
                    o.IsSelected
                )).ToList(),
                r.Feedbacks.Select(f => new VideoTitleFeedbackDto(
                    f.Id,
                    f.FeedbackText,
                    f.AppliedToRound
                )).ToList()
            );
            return Ok(dto);
        }

        /// <summary>
        /// Lựa chọn hoặc bỏ lựa chọn một phương án tiêu đề. Cho phép lựa chọn nhiều tiêu đề cùng lúc.
        /// </summary>
        /// <param name="optionId">Id phương án tiêu đề</param>
        /// <param name="request">Trạng thái chọn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy phương án</exception>
        [HttpPut("options/{optionId:guid}/select")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SelectOption(Guid optionId, [FromBody] VideoTitleSelectionRequest request, CancellationToken ct = default)
        {
            await _titleService.SetSelectedAsync(optionId, request.IsSelected, ct);
            return NoContent();
        }
    }
}
