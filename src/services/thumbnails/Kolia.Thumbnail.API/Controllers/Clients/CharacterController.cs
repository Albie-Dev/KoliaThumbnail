using Kolia.Thumbnail.API.DTOs.Characters;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Services.Characters;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// Controller quản lý nhân vật và ảnh biểu cảm toàn cục (Global Character - dùng chung giữa các dự án).
    /// </summary>
    [ApiController]
    [Route("api/v1/characters")]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách nhân vật toàn cục kèm thông tin ảnh biểu cảm chính (Primary Image).
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Danh sách CharacterSummaryDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CharacterSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<CharacterSummaryDto>>> GetAll(CancellationToken ct = default)
        {
            var list = await _characterService.GetAllAsync(ct);
            var dtos = list.Select(c => new CharacterSummaryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Images.Where(i => i.IsPrimary).Select(i => new CharacterImageDto(
                    i.Id,
                    i.ImageUrl,
                    i.ExpressionLabel,
                    i.AngleLabel,
                    i.IsPrimary
                )).FirstOrDefault()
            )).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Lấy chi tiết thông tin của 1 nhân vật kèm toàn bộ danh sách ảnh biểu cảm và góc máy.
        /// </summary>
        /// <param name="id">Id nhân vật</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>CharacterDto chi tiết nhân vật</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy nhân vật</exception>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CharacterDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var c = await _characterService.GetByIdAsync(id, ct);
            if (c == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân vật với ID: {id}");
            }

            var dto = new CharacterDto(
                c.Id,
                c.Name,
                c.Description,
                c.Images.Select(i => new CharacterImageDto(
                    i.Id,
                    i.ImageUrl,
                    i.ExpressionLabel,
                    i.AngleLabel,
                    i.IsPrimary
                )).ToList()
            );

            return Ok(dto);
        }

        /// <summary>
        /// Tạo một nhân vật mới trong hệ thống.
        /// </summary>
        /// <param name="request">Thông tin tên và mô tả nhân vật</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Thông tin định danh nhân vật vừa tạo</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CharacterDto>> Create([FromBody] CreateCharacterRequest request, CancellationToken ct = default)
        {
            var c = await _characterService.CreateAsync(request.Name, request.Description, ct);
            return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { id = c.Id });
        }

        /// <summary>
        /// Thêm một ảnh biểu cảm hoặc tư thế mới cho nhân vật.
        /// </summary>
        /// <param name="characterId">Id nhân vật</param>
        /// <param name="request">URL ảnh và thông tin tư thế/biểu cảm/góc máy</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>CharacterImageDto ảnh biểu cảm vừa thêm</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy nhân vật</exception>
        [HttpPost("{characterId:guid}/images")]
        [ProducesResponseType(typeof(CharacterImageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CharacterImageDto>> AddImage(Guid characterId, [FromBody] AddCharacterImageRequest request, CancellationToken ct = default)
        {
            var i = await _characterService.AddImageAsync(characterId, request.ImageUrl, request.ExpressionLabel, request.AngleLabel, request.IsPrimary, ct);
            var dto = new CharacterImageDto(
                i.Id,
                i.ImageUrl,
                i.ExpressionLabel,
                i.AngleLabel,
                i.IsPrimary
            );
            return Ok(dto);
        }

        /// <summary>
        /// Xóa mềm một nhân vật khỏi hệ thống.
        /// </summary>
        /// <param name="id">Id nhân vật</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>NoContent</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy nhân vật</exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await _characterService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
