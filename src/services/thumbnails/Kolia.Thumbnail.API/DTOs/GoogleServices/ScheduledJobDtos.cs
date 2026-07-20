using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.GoogleServices
{
    /// <summary>
    /// Request tạo mới Scheduled Import Job.
    /// </summary>
    public record CreateScheduledJobRequest(
        string Name,
        string? Description,
        CGoogleServiceType SourceType,
        string SourceUrl,
        Guid GoogleServiceAccountId,
        DateTimeOffset? ScheduledAt,
        string? CronExpression,
        string? CronDescription,
        int MaxRetries = 3);

    /// <summary>
    /// Request cập nhật Scheduled Import Job (vd: đổi lịch, đổi account).
    /// Chỉ cho phép update khi job đang ở trạng thái Pending.
    /// </summary>
    public record UpdateScheduledJobRequest(
        string Name,
        string? Description,
        string SourceUrl,
        Guid GoogleServiceAccountId,
        DateTimeOffset? ScheduledAt,
        string? CronExpression,
        string? CronDescription,
        int MaxRetries);

    /// <summary>
    /// DTO chi tiết Scheduled Import Job.
    /// </summary>
    public record ScheduledJobDto(
        Guid Id,
        string Name,
        string? Description,
        CGoogleServiceType SourceType,
        string SourceUrl,
        string? ServiceAccountName,
        string? ServiceAccountEmail,
        CJobScheduleStatus Status,
        string? ErrorMessage,
        Guid? CreatedProjectId,
        Guid? CreatedBriefId,
        string? CronExpression,
        string? CronDescription,
        DateTimeOffset? ScheduledAt,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        int RetryCount,
        int MaxRetries,
        DateTimeOffset CreationTime,
        DateTimeOffset? LastModificationTime);

    /// <summary>
    /// DTO cho danh sách job (summary).
    /// </summary>
    public record ScheduledJobSummaryDto(
        Guid Id,
        string Name,
        CGoogleServiceType SourceType,
        string SourceUrl,
        string? ServiceAccountName,
        CJobScheduleStatus Status,
        string? ErrorMessage,
        string? CronExpression,
        string? CronDescription,
        DateTimeOffset? ScheduledAt,
        Guid? CreatedProjectId,
        int RetryCount,
        DateTimeOffset CreationTime);

    /// <summary>
    /// Request kiểm tra quyền truy cập của service account vào URL.
    /// </summary>
    public record CheckAccessRequest(
        string SourceUrl,
        CGoogleServiceType SourceType,
        Guid GoogleServiceAccountId);

    /// <summary>
    /// Kết quả kiểm tra quyền truy cập.
    /// </summary>
    public record CheckAccessResult(
        bool HasAccess,
        string? ErrorMessage,
        string? Instruction);
}
