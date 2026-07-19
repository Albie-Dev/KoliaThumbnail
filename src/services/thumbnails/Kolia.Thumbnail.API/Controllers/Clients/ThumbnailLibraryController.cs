using Kolia.Thumbnail.API.DTOs.Thumbnails;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.Thumbnails;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller tìm kiếm, quản lý và phân tích sâu các thumbnail tham khảo từ YouTube (Phần 3).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/thumbnail-library")]
    public class ThumbnailLibraryController : ControllerBase
    {
        private readonly IThumbnailLibraryService _libraryService;

        public ThumbnailLibraryController(IThumbnailLibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        /// <summary>
        /// Lấy danh sách thumbnail tham khảo trong kho thư viện của dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="excludeIrrelevant">Loại bỏ các ảnh bị đánh dấu không phù hợp</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách ThumbnailLibraryItemDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ThumbnailLibraryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ThumbnailLibraryItemDto>>> GetLibrary(Guid projectId, [FromQuery] bool excludeIrrelevant = true, CancellationToken ct = default)
        {
            var items = await _libraryService.GetLibraryAsync(projectId, excludeIrrelevant, ct);
            var dtos = items.Select(t => new ThumbnailLibraryItemDto(
                t.Id,
                t.ProjectId,
                t.ThumbnailSearchRequestId,
                t.SourceType,
                t.Platform,
                t.VideoTitle,
                t.VideoUrl,
                t.ChannelName,
                t.ThumbnailImageUrl,
                t.MarketType,
                t.PublishedTime,
                t.ViewCount,
                t.KeywordBatchTag,
                t.UserStatus,
                t.IsFilteredIrrelevant,
                t.Analysis != null
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Tìm kiếm video và thumbnail trên YouTube theo keyword, crawl thông tin và lưu vào thư viện.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Bộ lọc tìm kiếm thumbnail</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ThumbnailSearchResultDto chứa kết quả tìm kiếm</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(ThumbnailSearchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThumbnailSearchResultDto>> Search(Guid projectId, [FromBody] ThumbnailSearchRequest request, CancellationToken ct = default)
        {
            var reqResult = await _libraryService.SearchAsync(
                projectId, request.Keyword, request.TimeFilter, request.SortFilter,
                request.WasSuggestedFromNews, ct);

            var dtos = reqResult.LibraryItems.Select(t => new ThumbnailLibraryItemDto(
                t.Id,
                t.ProjectId,
                t.ThumbnailSearchRequestId,
                t.SourceType,
                t.Platform,
                t.VideoTitle,
                t.VideoUrl,
                t.ChannelName,
                t.ThumbnailImageUrl,
                t.MarketType,
                t.PublishedTime,
                t.ViewCount,
                t.KeywordBatchTag,
                t.UserStatus,
                t.IsFilteredIrrelevant,
                t.Analysis != null
            )).ToList();

            var result = new ThumbnailSearchResultDto(reqResult.Id, reqResult.Keyword, request.TimeFilter, request.SortFilter, dtos);
            return Ok(result);
        }

        /// <summary>
        /// Nhập thủ công một thumbnail tham khảo thông qua đường link video YouTube.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">URL video YouTube</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ThumbnailLibraryItemDto bài viết vừa import</returns>
        [HttpPost("import")]
        [ProducesResponseType(typeof(ThumbnailLibraryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThumbnailLibraryItemDto>> ImportManual(Guid projectId, [FromBody] ThumbnailManualImportRequest request, CancellationToken ct = default)
        {
            var t = await _libraryService.ImportManualLinkAsync(projectId, request.VideoUrl, ct);
            var dto = new ThumbnailLibraryItemDto(
                t.Id,
                t.ProjectId,
                t.ThumbnailSearchRequestId,
                t.SourceType,
                t.Platform,
                t.VideoTitle,
                t.VideoUrl,
                t.ChannelName,
                t.ThumbnailImageUrl,
                t.MarketType,
                t.PublishedTime,
                t.ViewCount,
                t.KeywordBatchTag,
                t.UserStatus,
                t.IsFilteredIrrelevant,
                t.Analysis != null
            );
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật trạng thái duyệt của user đối với một thumbnail tham khảo (Pending, Approved, Rejected).
        /// </summary>
        /// <param name="itemId">Id thumbnail tham khảo</param>
        /// <param name="request">Trạng thái duyệt mới</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy item</exception>
        [HttpPut("items/{itemId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid itemId, [FromBody] ThumbnailUserStatusRequest request, CancellationToken ct = default)
        {
            await _libraryService.SetUserStatusAsync(itemId, request.Status, ct);
            return NoContent();
        }

        /// <summary>
        /// Chọn hoặc bỏ chọn thumbnail tham khảo này làm mẫu (Reference Item) để generate ảnh AI ở Phần 4.2.
        /// </summary>
        /// <param name="itemId">Id thumbnail tham khảo</param>
        /// <param name="request">Trạng thái lựa chọn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy item</exception>
        [HttpPut("items/{itemId:guid}/choose-for-generation")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChooseForGeneration(Guid itemId, [FromBody] ThumbnailChosenForGenerationRequest request, CancellationToken ct = default)
        {
            await _libraryService.SetChosenForGenerationAsync(itemId, request.IsChosen, ct);
            return NoContent();
        }

        /// <summary>
        /// Thực hiện phân tích sâu (text trên ảnh, bố cục, phong cách) đối với thumbnail tham khảo được chọn.
        /// </summary>
        /// <param name="itemId">Id thumbnail tham khảo</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ThumbnailAnalysisDto kết quả phân tích</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy item</exception>
        [HttpPost("items/{itemId:guid}/deep-analyze")]
        [ProducesResponseType(typeof(ThumbnailAnalysisDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ThumbnailAnalysisDto>> DeepAnalyze(Guid itemId, CancellationToken ct = default)
        {
            var analysis = await _libraryService.DeepAnalyzeAsync(itemId, ct);
            var dto = new ThumbnailAnalysisDto(
                analysis.Id,
                analysis.ThumbnailLibraryItemId,
                analysis.ThumbnailFactorsJson,
                analysis.TitleTextAnalysis,
                analysis.VideoTitleAnalysis,
                analysis.DisplayTextStyleNote,
                analysis.IsChosenForGeneration);

            return Ok(dto);
        }
    }
}
