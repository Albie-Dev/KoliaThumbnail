using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Services.News;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller tìm kiếm, quản lý và phân tích sâu tin tức (Phần 2).
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/news")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        /// <summary>
        /// Lấy danh sách tin tức phân trang, sắp xếp theo tổng điểm giảm dần.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Tham số phân trang, search, sort</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách NewsItemDto phân trang</returns>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<NewsItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<NewsItemDto>>> GetNewsPaging(
            Guid projectId,
            [FromQuery] PagedRequestDto request,
            CancellationToken ct = default)
        {
            var result = await _newsService.GetPagedByProjectAsync(projectId, request, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả tin tức liên kết với dự án, sắp xếp theo tổng điểm giảm dần.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách NewsItemDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<NewsItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<NewsItemDto>>> GetNews(Guid projectId, CancellationToken ct = default)
        {
            var items = await _newsService.GetByProjectAsync(projectId, ct);
            var dtos = items.Select(n => new NewsItemDto(
                n.Id,
                n.ProjectId,
                n.NewsSearchRequestId,
                n.SourceType,
                n.Title,
                n.SourceName,
                n.SourceUrl,
                n.MarketType,
                n.PublishedTime,
                n.ScannedTime,
                n.SummaryOverview,
                n.RelevanceToTopicScore,
                n.ImportanceImpactScore,
                n.EmotionPotentialScore,
                n.NoveltyDataScore,
                n.TotalScore,
                n.Recommendation,
                n.RelevanceLevel,
                n.IsSelectedByTeam,
                n.SuggestedKeywordsForThumbnail,
                n.DeepAnalysis != null,
                n.NewsSearchRequestId.HasValue ? "Batch" : "Manual"
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Thực hiện quét và tìm kiếm tin tức tự động dựa trên keywords, sau đó chấm điểm batch bằng AI.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Bộ lọc tìm kiếm tin tức</param>
        /// <param name="operationId">Id theo dõi tiến trình (optional)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NewsSearchResultDto chứa thông tin request và danh sách tin tức trả về</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(NewsSearchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NewsSearchResultDto>> Search(
            Guid projectId,
            [FromBody] NewsSearchRequest request,
            [FromQuery] Guid? operationId = null,
            CancellationToken ct = default)
        {
            var reqResult = await _newsService.SearchAsync(
                projectId, request.MarketScope, request.TimeRange, request.CountFilter,
                request.KeywordsRaw, request.SuggestedKeywordsSelected,
                operationId ?? Guid.Empty, ct);

            var dtos = reqResult.NewsItems.Select(n => new NewsItemDto(
                n.Id,
                n.ProjectId,
                n.NewsSearchRequestId,
                n.SourceType,
                n.Title,
                n.SourceName,
                n.SourceUrl,
                n.MarketType,
                n.PublishedTime,
                n.ScannedTime,
                n.SummaryOverview,
                n.RelevanceToTopicScore,
                n.ImportanceImpactScore,
                n.EmotionPotentialScore,
                n.NoveltyDataScore,
                n.TotalScore,
                n.Recommendation,
                n.RelevanceLevel,
                n.IsSelectedByTeam,
                n.SuggestedKeywordsForThumbnail,
                n.DeepAnalysis != null,
                n.NewsSearchRequestId.HasValue ? "Batch" : "Manual"
            )).ToList();

            var result = new NewsSearchResultDto(reqResult.Id, reqResult.MarketScope, reqResult.TimeRange, dtos);
            return Ok(result);
        }

        /// <summary>
        /// Import thủ công một bài báo qua đường link URL ngoài và chấm điểm AI.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Đường link bài báo</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NewsItemDto bài báo vừa tạo</returns>
        [HttpPost("import")]
        [ProducesResponseType(typeof(NewsItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NewsItemDto>> ImportManual(Guid projectId, [FromBody] NewsManualImportRequest request, CancellationToken ct = default)
        {
            var n = await _newsService.ImportManualLinkAsync(projectId, request.Url, ct);
            var dto = new NewsItemDto(
                n.Id,
                n.ProjectId,
                n.NewsSearchRequestId,
                n.SourceType,
                n.Title,
                n.SourceName,
                n.SourceUrl,
                n.MarketType,
                n.PublishedTime,
                n.ScannedTime,
                n.SummaryOverview,
                n.RelevanceToTopicScore,
                n.ImportanceImpactScore,
                n.EmotionPotentialScore,
                n.NoveltyDataScore,
                n.TotalScore,
                n.Recommendation,
                n.RelevanceLevel,
                n.IsSelectedByTeam,
                n.SuggestedKeywordsForThumbnail,
                n.DeepAnalysis != null,
                "Manual"
            );
            return Ok(dto);
        }

        /// <summary>
        /// Lấy danh sách các keyword gợi ý từ Content Brief của dự án.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách chuỗi keywords</returns>
        [HttpGet("suggested-keywords")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<string>>> GetSuggestedKeywords(Guid projectId, CancellationToken ct = default)
        {
            var keywords = await _newsService.GetSuggestedKeywordsAsync(projectId, ct);
            return Ok(keywords);
        }

        /// <summary>
        /// Chọn hoặc bỏ chọn một bản tin để dùng cho bước thiết kế Display Text và Video Title tiếp theo.
        /// </summary>
        /// <param name="newsItemId">Id bản tin</param>
        /// <param name="request">Trạng thái chọn</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy bản tin</exception>
        [HttpPut("items/{newsItemId:guid}/select")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SelectNewsItem(Guid newsItemId, [FromBody] NewsSelectionRequest request, CancellationToken ct = default)
        {
            await _newsService.SetSelectedAsync(newsItemId, request.IsSelected, ct);
            return NoContent();
        }

        /// <summary>
        /// Thực hiện phân tích sâu 4 tầng (vĩ mô, phản ứng thị trường, triển vọng, cảm xúc) cho bản tin được chỉ định.
        /// </summary>
        /// <param name="newsItemId">Id bản tin</param>
        /// <param name="operationId">Id theo dõi tiến trình (optional)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NewsDeepAnalysisDto chứa kết quả phân tích</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy bản tin</exception>
        [HttpPost("items/{newsItemId:guid}/deep-analyze")]
        [ProducesResponseType(typeof(NewsDeepAnalysisDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NewsDeepAnalysisDto>> DeepAnalyze(
            Guid newsItemId,
            [FromQuery] Guid? operationId = null,
            CancellationToken ct = default)
        {
            var analysis = await _newsService.DeepAnalyzeAsync(newsItemId, operationId ?? Guid.Empty, ct);
            var dto = new NewsDeepAnalysisDto(
                analysis.Id,
                analysis.NewsItemId,
                System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<string>>(analysis.MacroEventSummaryJson) ?? [],
                analysis.MarketReactionJson,
                analysis.ExpectationShortTerm,
                analysis.ExpectationLongTerm,
                analysis.SentimentOverviewJson,
                analysis.EmotionTags,
                analysis.EmotionReason,
                analysis.WasTranslatedFromForeign,
                analysis.MissingDataNote);

            return Ok(dto);
        }

        /// <summary>
        /// Xóa (xoá mềm) một bản tin khỏi dự án.
        /// </summary>
        /// <param name="newsItemId">Id bản tin</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy bản tin</exception>
        [HttpDelete("items/{newsItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNewsItem(Guid newsItemId, CancellationToken ct = default)
        {
            await _newsService.DeleteNewsItemAsync(newsItemId, ct);
            return NoContent();
        }
    }
}
