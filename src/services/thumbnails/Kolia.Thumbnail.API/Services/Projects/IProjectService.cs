using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.DTOs.Projects;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.Services.Projects
{
    /// <summary>
    /// Quản lý project trong Kho lưu trữ.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Lấy danh sách project với phân trang (hỗ trợ search, filter, sort).
        /// </summary>
        Task<PagedResponseDto<ProjectSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy chi tiết 1 project kèm Steps.
        /// </summary>
        Task<ProjectEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Tạo project mới — tự động seed 5 ProjectSteps.
        /// </summary>
        Task<ProjectEntity> CreateAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// Đổi tên project.
        /// </summary>
        Task RenameAsync(Guid id, string newName, CancellationToken ct = default);

        /// <summary>
        /// Xóa mềm project.
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Chuyển step hiện tại lên bước tiếp theo (chỉ tiến, không lùi).
        /// </summary>
        Task AdvanceStepAsync(Guid projectId, CancellationToken ct = default);

        /// <summary>
        /// Cập nhật trạng thái 1 step cụ thể.
        /// </summary>
        Task UpdateStepStatusAsync(Guid projectId, Enums.CProjectStepNumber stepNumber,
            Enums.CProjectStepStatus newStatus, string? outputSummary = null,
            CancellationToken ct = default);
    }
}
