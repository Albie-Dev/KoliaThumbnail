using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    public class ProjectEntity : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset? PausedAt { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
        public DateTimeOffset? FailedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public string? CancelReason { get; set; }
        public CProjectStatus Status { get; set; } = CProjectStatus.Draft;
        public int Progress { get; set; }
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }

        public ICollection<ProjectStepEntity> Steps { get; set; } = new List<ProjectStepEntity>();

        /// <summary>
        /// Tính lại TotalSteps/CompletedSteps/Progress/Status dựa trên Steps hiện có.
        /// Gọi ngay sau mỗi lần đổi Status của 1 ProjectStepEntity trong cùng project.
        /// Yêu cầu Steps đã được load (Include) trước khi gọi.
        /// </summary>
        public void RecalculateProgress()
        {
            TotalSteps = Steps.Count;
            CompletedSteps = Steps.Count(s => s.Status == CProjectStepStatus.Completed);
            Progress = TotalSteps == 0 ? 0 : (int)Math.Round(CompletedSteps * 100.0 / TotalSteps);
        }
    }
}