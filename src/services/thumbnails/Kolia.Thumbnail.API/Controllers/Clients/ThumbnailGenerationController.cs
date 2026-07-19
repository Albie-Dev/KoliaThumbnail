using Kolia.Thumbnail.API.DTOs.ThumbnailGeneration;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.ThumbnailGeneration;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller sinh và quản lý thumbnail AI (Phần 4.2). Hỗ trợ sửa đổi qua Version Chains.
    /// </summary>
    [ApiController]
    [Route("api/v1/projects/{projectId:guid}/thumbnail-generation")]
    public class ThumbnailGenerationController : ControllerBase
    {
        private readonly IThumbnailGenerationService _generationService;

        public ThumbnailGenerationController(IThumbnailGenerationService generationService)
        {
            _generationService = generationService;
        }

        /// <summary>
        /// Lấy toàn bộ các tập hợp ảnh đã generate (Sets) cùng các biến thể (Variants/Versions) của chúng.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách GeneratedThumbnailSetDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<GeneratedThumbnailSetDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<GeneratedThumbnailSetDto>>> GetSets(Guid projectId, CancellationToken ct = default)
        {
            var sets = await _generationService.GetSetsAsync(projectId, ct);
            var dtos = sets.Select(s => new GeneratedThumbnailSetDto(
                s.Id,
                s.ThumbnailGenerationRequestId,
                s.SetIndex,
                s.CreationTime,
                s.Variants.Select(v => new GeneratedThumbnailDto(
                    v.Id,
                    v.GeneratedThumbnailSetId,
                    v.VariantIndex,
                    v.ParentGeneratedThumbnailId,
                    v.VersionNumber,
                    v.ImageUrl,
                    v.DisplayTextSnapshot,
                    v.CharacterSnapshotName,
                    v.LastEditTool,
                    v.IsApproved,
                    v.DeletionTime, // approved time holder
                    v.WasDownloaded,
                    v.IsPushedToTitleStep
                )).ToList()
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Tạo một yêu cầu generate ảnh mới từ các mẫu tham khảo, chữ trên ảnh, nhân vật và tỉ lệ/độ phân giải được chọn.
        /// </summary>
        /// <param name="projectId">Id dự án</param>
        /// <param name="request">Tham số sinh ảnh</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>GeneratedThumbnailSetDto chứa lô ảnh vừa tạo</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(GeneratedThumbnailSetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneratedThumbnailSetDto>> Generate(Guid projectId, [FromBody] GenerateThumbnailRequest request, CancellationToken ct = default)
        {
            var set = await _generationService.GenerateAsync(
                projectId, request.DisplayTextOptionIds, request.ReferenceLibraryItemIds,
                request.CharacterId, request.ChangesRequestText, request.Ratio, request.Resolution,
                request.RequestedCount, request.OverridePromptText, ct);

            var dto = new GeneratedThumbnailSetDto(
                set.Id,
                set.ThumbnailGenerationRequestId,
                set.SetIndex,
                set.CreationTime,
                set.Variants.Select(v => new GeneratedThumbnailDto(
                    v.Id,
                    v.GeneratedThumbnailSetId,
                    v.VariantIndex,
                    v.ParentGeneratedThumbnailId,
                    v.VersionNumber,
                    v.ImageUrl,
                    v.DisplayTextSnapshot,
                    v.CharacterSnapshotName,
                    v.LastEditTool,
                    v.IsApproved,
                    v.DeletionTime,
                    v.WasDownloaded,
                    v.IsPushedToTitleStep
                )).ToList()
            );
            return Ok(dto);
        }

        /// <summary>
        /// Xuất prompt text từ các tham số đầu vào (cho phép user sửa tay trước khi generate).
        /// </summary>
        [HttpPost("export-prompt")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> ExportPrompt(Guid projectId, [FromBody] ExportThumbnailPromptRequest request, CancellationToken ct = default)
        {
            var prompt = await _generationService.BuildPromptAsync(
                projectId, request.DisplayTextOptionIds, request.ReferenceLibraryItemIds,
                request.CharacterId, request.ChangesRequestText, request.Ratio, request.Resolution, ct);
            return Ok(prompt);
        }

        /// <summary>
        /// Chỉnh sửa một ảnh đã có, tạo ra phiên bản mới liên kết với ảnh cũ (Version Chain) thay vì ghi đè.
        /// </summary>
        /// <param name="thumbnailId">Id của ảnh gốc cần sửa</param>
        /// <param name="request">Công cụ sửa và yêu cầu text mô tả thay đổi</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>GeneratedThumbnailDto phiên bản ảnh mới vừa tạo</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy ảnh gốc</exception>
        [HttpPost("variants/{thumbnailId:guid}/edit")]
        [ProducesResponseType(typeof(GeneratedThumbnailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneratedThumbnailDto>> Edit(Guid thumbnailId, [FromBody] EditThumbnailRequest request, CancellationToken ct = default)
        {
            var v = await _generationService.EditAsync(
                thumbnailId, request.EditTool, request.EditRequestText,
                request.SecondaryReferenceLibraryItemId, request.SecondaryCharacterImageId, ct);
            var dto = new GeneratedThumbnailDto(
                v.Id,
                v.GeneratedThumbnailSetId,
                v.VariantIndex,
                v.ParentGeneratedThumbnailId,
                v.VersionNumber,
                v.ImageUrl,
                v.DisplayTextSnapshot,
                v.CharacterSnapshotName,
                v.LastEditTool,
                v.IsApproved,
                v.DeletionTime,
                v.WasDownloaded,
                v.IsPushedToTitleStep
            );
            return Ok(dto);
        }

        /// <summary>
        /// Duyệt một ảnh thumbnail.
        /// </summary>
        /// <param name="thumbnailId">Id ảnh</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy ảnh</exception>
        [HttpPost("variants/{thumbnailId:guid}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve(Guid thumbnailId, CancellationToken ct = default)
        {
            await _generationService.ApproveAsync(thumbnailId, ct);
            return NoContent();
        }

        /// <summary>
        /// Đẩy ảnh thumbnail đã duyệt sang bước 5 để làm ngữ cảnh sinh tiêu đề video.
        /// </summary>
        /// <param name="thumbnailId">Id ảnh đã duyệt</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy ảnh</exception>
        [HttpPost("variants/{thumbnailId:guid}/push-to-title")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PushToTitle(Guid thumbnailId, CancellationToken ct = default)
        {
            await _generationService.PushToTitleStepAsync(thumbnailId, ct);
            return NoContent();
        }

        /// <summary>
        /// Đánh dấu là người dùng đã thực hiện tải ảnh này xuống.
        /// </summary>
        /// <param name="thumbnailId">Id ảnh</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy ảnh</exception>
        [HttpPost("variants/{thumbnailId:guid}/mark-downloaded")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkDownloaded(Guid thumbnailId, CancellationToken ct = default)
        {
            await _generationService.MarkDownloadedAsync(thumbnailId, ct);
            return NoContent();
        }
    }
}
