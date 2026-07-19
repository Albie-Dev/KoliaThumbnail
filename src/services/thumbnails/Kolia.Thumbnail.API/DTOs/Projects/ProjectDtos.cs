namespace Kolia.Thumbnail.API.DTOs.Projects
{
    public record CreateProjectRequest(string Name);

    public record RenameProjectRequest(string NewName);

    public record ProjectStepStatusUpdateRequest(
        Enums.CProjectStepStatus NewStatus,
        string? OutputSummaryText);

    public record ProjectSummaryDto(
        Guid Id,
        string Name,
        Enums.CProjectStatus Status,
        Enums.CProjectStepNumber CurrentStepNumber,
        string? ThumbnailCoverUrl,
        DateTimeOffset? LastActivityTime,
        DateTimeOffset CreationTime);

    public record ProjectDetailDto(
        Guid Id,
        string Name,
        Enums.CProjectStatus Status,
        Enums.CProjectStepNumber CurrentStepNumber,
        string? ThumbnailCoverUrl,
        DateTimeOffset? LastActivityTime,
        DateTimeOffset CreationTime,
        IReadOnlyList<ProjectStepDto> Steps);

    public record ProjectStepDto(
        Guid Id,
        Enums.CProjectStepNumber StepNumber,
        string StepName,
        Enums.CProjectStepStatus StepStatus,
        string? OutputSummaryText,
        bool NeedsApproval,
        DateTimeOffset? ApprovedAt);
}
