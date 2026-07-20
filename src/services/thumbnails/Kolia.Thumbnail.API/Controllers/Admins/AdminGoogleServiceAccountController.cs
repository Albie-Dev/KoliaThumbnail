using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Interfaces.GoogleServices;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Admins
{
    /// <summary>
    /// Controller quản lý Google Service Account (Admin).
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/google-service-accounts")]
    public class AdminGoogleServiceAccountController : ControllerBase
    {
        private readonly IGoogleServiceAccountService _service;
        private readonly ILogger<AdminGoogleServiceAccountController> _logger;

        public AdminGoogleServiceAccountController(
            IGoogleServiceAccountService service,
            ILogger<AdminGoogleServiceAccountController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách service account với phân trang.
        /// </summary>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(PagedResponseDto<GoogleServiceAccountSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<GoogleServiceAccountSummaryDto>>> GetPagingAsync(
            [FromQuery] PagedRequestDto request,
            [FromQuery] bool? includeDeleted = null,
            [FromQuery] bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            var result = await _service.GetWithPagingAsync(request, includeDeleted, deletedOnly, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết 1 service account.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GoogleServiceAccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GoogleServiceAccountDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            if (dto == null)
                throw new NotFoundException($"Không tìm thấy service account với ID: {id}");
            return Ok(dto);
        }

        /// <summary>
        /// Tạo mới service account từ file JSON credential.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GoogleServiceAccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GoogleServiceAccountDto>> Create(
            [FromBody] CreateGoogleServiceAccountRequest request,
            CancellationToken ct = default)
        {
            var dto = await _service.CreateAsync(request, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật service account.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(GoogleServiceAccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GoogleServiceAccountDto>> Update(
            Guid id,
            [FromBody] UpdateGoogleServiceAccountRequest request,
            CancellationToken ct = default)
        {
            var dto = await _service.UpdateAsync(id, request, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Xoá mềm service account.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Import file JSON credential và tự động tạo Service Account.
        /// Hỗ trợ .json — đọc file, parse, map các trường.
        /// </summary>
        /// <param name="name">Tên hiển thị của service account</param>
        /// <param name="description">Mô tả (tuỳ chọn)</param>
        /// <param name="scopes">Scopes (tuỳ chọn, cách nhau bằng dấu phẩy)</param>
        /// <param name="file">File .json credential tải từ Google Cloud Console</param>
        /// <param name="ct">CancellationToken</param>
        [HttpPost("import-file")]
        [ProducesResponseType(typeof(GoogleServiceAccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GoogleServiceAccountDto>> ImportFile(
            [FromForm] string name,
            [FromForm] string? description,
            [FromForm] string? scopes,
            IFormFile file,
            CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException("Vui lòng chọn file JSON credential.");

            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Chỉ hỗ trợ file .json.");

            if (file.Length > 5 * 1024 * 1024) // 5MB
                throw new ValidationException("File JSON không được vượt quá 5MB.");

            string jsonContent;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                jsonContent = await reader.ReadToEndAsync(ct);
            }

            if (string.IsNullOrWhiteSpace(jsonContent))
                throw new ValidationException("File JSON rỗng.");

            var request = new CreateGoogleServiceAccountRequest(
                Name: name,
                Description: description,
                CredentialJson: jsonContent,
                Scopes: scopes);

            var dto = await _service.CreateAsync(request, ct);
            _logger.LogInformation("Created GoogleServiceAccount from file: {Name} ({File})", dto.Name, file.FileName);
            return Ok(dto);
        }
    }
}
