using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
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
        private readonly IArticleContentFetcher _articleFetcher;

        public NewsController(INewsService newsService, IArticleContentFetcher articleFetcher)
        {
            _newsService = newsService;
            _articleFetcher = articleFetcher;
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
                request.SelectedSourceIds,
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
        /// Lấy danh sách NewsSource phân trang để client chọn khi search news.
        /// Hỗ trợ search theo tên, filter theo Region, sort theo Priority.
        /// </summary>
        /// <param name="projectId">Id dự án (để consistent routing)</param>
        /// <param name="request">Tham số phân trang, search</param>
        /// <param name="region">Filter theo Region (Domestic/International/Both) - optional</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách NewsSourceSelectDto phân trang</returns>
        [HttpGet("sources")]
        [ProducesResponseType(typeof(PagedResponseDto<NewsSourceSelectDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<NewsSourceSelectDto>>> GetNewsSources(
            Guid projectId,
            [FromQuery] PagedRequestDto request,
            [FromQuery] CMarketScope? region = null,
            CancellationToken ct = default)
        {
            var result = await _newsService.GetNewsSourcesPagingAsync(request, region, ct);
            return Ok(result);
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
        /// Endpoint dùng để test trích xuất full-text bài báo từ 1 URL bất kỳ (bao gồm Google News redirect URL).
        /// </summary>
        /// <param name="url">URL của bài báo hoặc link Google News RSS</param>
        /// <param name="ct">CancellationToken</param>
        [HttpGet("test-fetch-article")]
        [ProducesResponseType(typeof(ArticleContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ArticleContentResult>> TestFetchArticle(
            [FromQuery] string url,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new ErrorResponse
                {
                    Code = "BAD_REQUEST",
                    Message = "URL không được để trống."
                });
            }

            var result = await _articleFetcher.FetchFullTextAsync(url, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy kết quả phân tích sâu đã lưu của một bản tin (không gọi lại AI).
        /// </summary>
        [HttpGet("items/{newsItemId:guid}/deep-analyze")]
        [ProducesResponseType(typeof(NewsDeepAnalysisDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NewsDeepAnalysisDto>> GetDeepAnalysis(
            Guid newsItemId, CancellationToken ct = default)
        {
            var analysis = await _newsService.GetDeepAnalysisAsync(newsItemId, ct);
            if (analysis == null)
            {
                return NotFound(new ErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "Chưa có kết quả phân tích sâu cho bản tin này."
                });
            }

            return Ok(MapDeepAnalysisToDto(analysis));
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
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<NewsDeepAnalysisDto>> DeepAnalyze(
            Guid newsItemId,
            [FromQuery] Guid? operationId = null,
            CancellationToken ct = default)
        {
            var analysis = await _newsService.DeepAnalyzeAsync(newsItemId, operationId ?? Guid.Empty, ct);
            return Ok(MapDeepAnalysisToDto(analysis));
        }

        private static readonly System.Text.Json.JsonSerializerOptions JsonCaseInsensitiveOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static NewsDeepAnalysisDto MapDeepAnalysisToDto(NewsDeepAnalysisEntity analysis)
        {
            return new NewsDeepAnalysisDto(
                analysis.Id,
                analysis.NewsItemId,
                ParseMacroSummary(analysis.MacroEventSummaryJson),
                ParseMarketReaction(analysis.MarketReactionJson),
                analysis.ExpectationShortTerm,
                analysis.ExpectationLongTerm,
                ParseSentimentOverview(analysis.SentimentOverviewJson),
                analysis.EmotionTags,
                analysis.EmotionReason,
                analysis.WasTranslatedFromForeign,
                analysis.MissingDataNote,
                analysis.Status);
        }

        private static IReadOnlyList<MacroEventCategoryItem> ParseMacroSummary(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];

            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<MacroEventCategoryItem>>(json, JsonCaseInsensitiveOptions);
                if (items != null && items.Count > 0) return items;
            }
            catch
            {
                try
                {
                    var oldStrings = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, JsonCaseInsensitiveOptions);
                    if (oldStrings != null)
                    {
                        return oldStrings
                            .Select(s => new MacroEventCategoryItem("Sự kiện", s))
                            .ToList();
                    }
                }
                catch { }
            }

            return [];
        }

        private static IReadOnlyList<MarketReactionItem> ParseMarketReaction(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];

            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<MarketReactionItem>>(json, JsonCaseInsensitiveOptions);
                if (items != null && items.Count > 0) return items;
            }
            catch
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonCaseInsensitiveOptions);
                    if (dict != null)
                    {
                        return dict.Select(kv => new MarketReactionItem(kv.Key, kv.Value)).ToList();
                    }
                }
                catch
                {
                    try
                    {
                        var oldStrings = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, JsonCaseInsensitiveOptions);
                        if (oldStrings != null)
                        {
                            return oldStrings.Select(s => new MarketReactionItem("Thị trường", s)).ToList();
                        }
                    }
                    catch { }
                }
            }

            return [];
        }

        private static SentimentOverview ParseSentimentOverview(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new SentimentOverview(CMarketSentiment.Neutral, "Chưa rõ");

            try
            {
                var item = System.Text.Json.JsonSerializer.Deserialize<SentimentOverview>(json, JsonCaseInsensitiveOptions);
                if (item != null) return item;
            }
            catch { }

            return new SentimentOverview(CMarketSentiment.Neutral, json);
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
