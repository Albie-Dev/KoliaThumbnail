using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Services.News;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Admins
{
    /// <summary>
    /// Admin CRUD cho danh sách nguồn tin (NewsSourceEntity).
    /// Mọi thay đổi có hiệu lực ngay lần research tiếp theo (cache invalidation tức thì).
    /// </summary>
    [ApiController]
    [Route("admin/news-sources")]
    public sealed class AdminNewsSourceController : ControllerBase
    {
        private readonly IAdminNewsSourceService _service;
        private readonly ILogger<AdminNewsSourceController> _logger;

        public AdminNewsSourceController(
            IAdminNewsSourceService service,
            ILogger<AdminNewsSourceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Liệt kê nguồn tin phân trang, filter tùy chọn theo nhóm / region / trạng thái.
        /// </summary>
        /// <param name="request">Tham số phân trang, search, sort</param>
        /// <param name="group">Filter theo SourceGroup (optional)</param>
        /// <param name="region">Filter theo Region — Domestic=1, International=2, Both=3 (optional)</param>
        /// <param name="isTrusted">Filter theo IsTrusted (optional)</param>
        /// <param name="includeDeleted">Bao gồm cả bản ghi đã xoá mềm</param>
        /// <param name="deletedOnly">Chỉ lấy bản ghi đã xoá mềm</param>
        /// <param name="ct">Cancellation token</param>
        [HttpGet("paging", Name = "ListNewsSourcesPaging")]
        [ProducesResponseType(typeof(PagedResponseDto<NewsSourceListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(
            [FromQuery] PagedRequestDto request,
            [FromQuery] CNewsSourceGroup? group,
            [FromQuery] CMarketScope? region,
            [FromQuery] bool? isTrusted,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            var result = await _service.ListAsync(request, group, region, isTrusted, includeDeleted, deletedOnly, ct);
            return Ok(result);
        }

        /// <summary>Chi tiết 1 nguồn tin.</summary>
        [HttpGet("{id:guid}", Name = "GetNewsSourceById")]
        [ProducesResponseType(typeof(NewsSourceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var source = await _service.GetByIdAsync(id, ct);
                return Ok(source);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Thêm nguồn tin mới.
        /// Hệ thống sẽ tự kiểm tra URL có accessible không trước khi lưu.
        /// </summary>
        [HttpPost(Name = "CreateNewsSource")]
        [ProducesResponseType(typeof(NewsSourceDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] NewsSourceCreateDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtRoute("GetNewsSourceById", new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật nguồn tin (URL, FetchMode, Priority…).
        /// Thường dùng khi site đổi path RSS — không cần deploy lại code.
        /// </summary>
        [HttpPut("{id:guid}", Name = "UpdateNewsSource")]
        [ProducesResponseType(typeof(NewsSourceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id, [FromBody] NewsSourceUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Bật/tắt nhanh IsTrusted cho 1 nguồn.
        /// Dùng khi nguồn đang lỗi liên tục và muốn tắt tạm thời mà không xóa cấu hình.
        /// </summary>
        [HttpPatch("{id:guid}/toggle", Name = "ToggleNewsSource")]
        [ProducesResponseType(typeof(NewsSourceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Toggle([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _service.ToggleAsync(id, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Bật/tắt IsTrusted hàng loạt cho nhiều nguồn cùng lúc.
        /// Dùng khi muốn shutdown/bật lại nhiều nguồn một lần.
        /// </summary>
        [HttpPost("bulk/set-trust", Name = "BulkSetTrustNewsSources")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkSetTrust(
            [FromBody] BulkSetTrustRequestDto request, CancellationToken ct)
        {
            if (request.Ids.Count == 0)
                return BadRequest(new { error = "Danh sách Ids không được để trống." });

            await _service.BulkSetTrustAsync(request.Ids, request.IsTrusted, ct);
            return NoContent();
        }

        /// <summary>
        /// Test thử fetch ngay (preview kết quả thật, không ảnh hưởng circuit breaker vận hành).
        /// Dùng để xác nhận URL mới đúng trước khi để hệ thống dùng thật.
        /// </summary>
        [HttpPost("{id:guid}/test", Name = "TestFetchNewsSource")]
        [ProducesResponseType(typeof(NewsSourceTestFetchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TestFetch(
            [FromRoute] Guid id,
            [FromBody] List<string>? keywords,
            CancellationToken ct)
        {
            try
            {
                var result = await _service.TestFetchAsync(id, keywords ?? [], ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>Soft delete nguồn tin (dùng ISoftDelete convention của project).</summary>
        [HttpDelete("{id:guid}", Name = "DeleteNewsSource")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
