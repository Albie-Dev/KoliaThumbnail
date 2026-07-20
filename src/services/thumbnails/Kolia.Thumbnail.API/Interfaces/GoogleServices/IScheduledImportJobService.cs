using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.Interfaces.GoogleServices
{
    /// <summary>
    /// Service quản lý Scheduled Import Jobs.
    /// </summary>
    public interface IScheduledImportJobService
    {
        /// <summary>Lấy danh sách job với phân trang</summary>
        Task<PagedResponseDto<ScheduledJobSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default);

        /// <summary>Lấy chi tiết 1 job</summary>
        Task<ScheduledJobDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>Tạo mới scheduled job</summary>
        Task<ScheduledJobDto> CreateAsync(
            CreateScheduledJobRequest request,
            CancellationToken ct = default);

        /// <summary>Cập nhật scheduled job (chỉ khi Pending)</summary>
        Task<ScheduledJobDto> UpdateAsync(
            Guid id,
            UpdateScheduledJobRequest request,
            CancellationToken ct = default);

        /// <summary>Huỷ job (chỉ khi Pending hoặc Failed)</summary>
        Task CancelAsync(Guid id, CancellationToken ct = default);

        /// <summary>Xoá mềm job</summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>Kiểm tra quyền truy cập của service account vào URL</summary>
        Task<CheckAccessResult> CheckAccessAsync(
            CheckAccessRequest request,
            CancellationToken ct = default);

        /// <summary>Chạy lại job đã thất bại</summary>
        Task<ScheduledJobDto> RetryAsync(Guid id, CancellationToken ct = default);

        /// <summary>Lấy log của job</summary>
        Task<IReadOnlyList<LogEntry>> GetLogsAsync(Guid id, CancellationToken ct = default);
    }
}
