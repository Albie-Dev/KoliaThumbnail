using Kolia.Thumbnail.API.DTOs.Briefs;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.Briefs;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý Content Brief (Phần 1 - Nội dung livestream/video).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/brief")]
    public class ContentBriefController : ControllerBase
    {
        private readonly IContentBriefService _briefService;

        public ContentBriefController(IContentBriefService briefService)
        {
            _briefService = briefService;
        }

        /// <summary>
        /// Lấy hoặc tạo mới Content Brief cho một dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ContentBriefDto chứa chi tiết Content Brief</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ContentBriefDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ContentBriefDto>> GetBrief(Guid projectId, CancellationToken ct = default)
        {
            var brief = await _briefService.GetOrCreateAsync(projectId, ct);
            var dto = new ContentBriefDto(
                brief.Id,
                brief.ProjectId,
                brief.ImportSource,
                brief.ImportedExternalLink,
                brief.ExternalSheetUrl,
                brief.LastSheetSyncTime,
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.TopicOutput,
                brief.MainMessageOutput,
                brief.HighlightDataOutput,
                brief.IsConfirmed,
                brief.ConfirmedAt);

            return Ok(dto);
        }

        /// <summary>
        /// Lưu dữ liệu nhập thủ công cho brief (Overview, Viewpoint, KeyData).
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Nội dung brief nhập tay</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="InvalidOperationException">Ném ra khi brief đã được confirm khóa</exception>
        [HttpPut("manual")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveManual(Guid projectId, [FromBody] SaveManualBriefRequest request, CancellationToken ct = default)
        {
            await _briefService.SaveManualInputAsync(projectId, request.OverviewInput, request.ViewpointInput, request.KeyDataInput, ct);
            return NoContent();
        }

        /// <summary>
        /// ⚠️ ĐÃ THAY THẾ: Import thông tin brief từ các nguồn lực bên ngoài.
        /// Tính năng Import từ External Link (Google Sheet/Docs) đã được thay thế bởi
        /// <see cref="Controllers.Admins.AdminScheduledImportJobController"/> với Scheduled Import Jobs
        /// sử dụng Google Service Account, hỗ trợ đặt lịch chạy, kiểm tra quyền và tự động tạo project.
        ///
        /// Endpoint này chỉ giữ lại cho PasteText và File upload (vẫn hoạt động bình thường).
        /// Đối với External Link, vui lòng sử dụng Admin Scheduled Import Jobs.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Thông tin nguồn import</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="InvalidOperationException">Ném ra khi brief đã được confirm khóa</exception>
        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Import(Guid projectId, [FromBody] ImportBriefRequest request, CancellationToken ct = default)
        {
            await _briefService.ImportAsync(projectId, request.Source, request.RawText, request.FileUrl, request.ExternalLink, ct);
            return NoContent();
        }

        /// <summary>
        /// Import dữ liệu từ PasteText và tự động gọi AI Agent để phân tích,
        /// trích xuất toàn bộ 6 trường nội dung (overview, viewpoint, keyData,
        /// topic, mainMessage, highlightData) ngay trong một lần gọi.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Thông tin nguồn import (bắt buộc RawText)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ContentBriefDto sau khi AI đã phân tích đầy đủ 6 trường</returns>
        /// <exception cref="InvalidOperationException">Ném ra khi brief đã được confirm khóa</exception>
        [HttpPost("import-and-analyze")]
        [ProducesResponseType(typeof(ContentBriefDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContentBriefDto>> ImportAndAnalyze(Guid projectId,
            [FromBody] ImportBriefRequest request,
            CancellationToken ct = default)
        {
            if (request.Source != CImportContentSource.PasteText)
                throw new ArgumentException("import-and-analyze chỉ hỗ trợ nguồn PasteText.");

            var brief = await _briefService.ImportAndAnalyzeFromPasteAsync(projectId, request.RawText!, ct);
            var dto = new ContentBriefDto(
                brief.Id,
                brief.ProjectId,
                brief.ImportSource,
                brief.ImportedExternalLink,
                brief.ExternalSheetUrl,
                brief.LastSheetSyncTime,
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.TopicOutput,
                brief.MainMessageOutput,
                brief.HighlightDataOutput,
                brief.IsConfirmed,
                brief.ConfirmedAt);

            return Ok(dto);
        }

        /// <summary>
        /// Upload file text và tự động gọi AI Agent để phân tích,
        /// trích xuất toàn bộ 6 trường nội dung.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="file">File text cần upload (.txt, .csv, .md, .json, .xml, ...)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ContentBriefDto sau khi AI đã phân tích đầy đủ 6 trường</returns>
        [HttpPost("import-file")]
        [ProducesResponseType(typeof(ContentBriefDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContentBriefDto>> ImportFile(Guid projectId,
            IFormFile file, CancellationToken ct = default)
        {
            var brief = await _briefService.ImportFileAndAnalyzeAsync(projectId, file, ct);
            var dto = new ContentBriefDto(
                brief.Id,
                brief.ProjectId,
                brief.ImportSource,
                brief.ImportedExternalLink,
                brief.ExternalSheetUrl,
                brief.LastSheetSyncTime,
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.TopicOutput,
                brief.MainMessageOutput,
                brief.HighlightDataOutput,
                brief.IsConfirmed,
                brief.ConfirmedAt);

            return Ok(dto);
        }

        /// <summary>
        /// Gọi AI phân tích dữ liệu đầu vào của brief để sinh ra Topic, Thông điệp chính và Dữ liệu nổi bật.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Nếu có ManualPrompt, dùng prompt này thay vì tự động ghép từ input</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ContentBriefDto sau khi AI đã phân tích</returns>
        /// <exception cref="InvalidOperationException">Ném ra khi brief đã được confirm khóa</exception>
        [HttpPost("analyze")]
        [ProducesResponseType(typeof(ContentBriefDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContentBriefDto>> Analyze(Guid projectId,
            [FromBody] AnalyzeBriefRequest request,
            CancellationToken ct = default)
        {
            var brief = await _briefService.AnalyzeWithAIAsync(projectId, request.ManualPrompt, ct);
            var dto = new ContentBriefDto(
                brief.Id,
                brief.ProjectId,
                brief.ImportSource,
                brief.ImportedExternalLink,
                brief.ExternalSheetUrl,
                brief.LastSheetSyncTime,
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.TopicOutput,
                brief.MainMessageOutput,
                brief.HighlightDataOutput,
                brief.IsConfirmed,
                brief.ConfirmedAt);

            return Ok(dto);
        }

        /// <summary>
        /// Đồng bộ nội dung từ Google Sheet nội bộ (team phân tích) vào Content Brief.
        /// </summary>
        [HttpPost("sync-sheet")]
        [ProducesResponseType(typeof(ContentBriefDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContentBriefDto>> SyncSheet(Guid projectId, [FromBody] SyncSheetRequest request, CancellationToken ct = default)
        {
            var brief = await _briefService.SyncFromSheetAsync(projectId, request.SheetUrl, ct);
            var dto = new ContentBriefDto(
                brief.Id,
                brief.ProjectId,
                brief.ImportSource,
                brief.ImportedExternalLink,
                brief.ExternalSheetUrl,
                brief.LastSheetSyncTime,
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.TopicOutput,
                brief.MainMessageOutput,
                brief.HighlightDataOutput,
                brief.IsConfirmed,
                brief.ConfirmedAt);
            return Ok(dto);
        }

        /// <summary>
        /// Xác nhận (khóa) Content Brief. Sau khi xác nhận, brief sẽ không thể sửa đổi.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="InvalidOperationException">Ném ra khi chưa thực hiện phân tích AI</exception>
        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Confirm(Guid projectId, CancellationToken ct = default)
        {
            await _briefService.ConfirmAsync(projectId, ct);
            return NoContent();
        }
    }
}
