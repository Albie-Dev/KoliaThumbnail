using Kolia.Thumbnail.API.Attributes;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    public class ProjectEntity : BaseEntity
    {
        [Queryable(Searchable = true, Filterable = true, Sortable = true)]
        public string Name { get; set; } = null!;

        [Queryable(Searchable = true, Filterable = true, Sortable = true)]
        public string Code { get; set; } = null!;

        [Queryable(Searchable = true)]
        public string? Description { get; set; }

        [Queryable(Filterable = true, Sortable = true)]
        public Guid CreatedByUserId { get; set; }

        [Queryable(Searchable = true)]
        public string? CreatedByUserName { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public DateTimeOffset? StartedAt { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public DateTimeOffset? CompletedAt { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public DateTimeOffset? PausedAt { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public DateTimeOffset? CancelledAt { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public DateTimeOffset? FailedAt { get; set; }

        [Queryable(Searchable = true)]
        public string? ErrorMessage { get; set; }

        [Queryable(Searchable = true)]
        public string? ErrorDetail { get; set; }

        [Queryable(Searchable = true)]
        public string? CancelReason { get; set; }

        [Queryable(Filterable = true, Sortable = true)]
        public CProjectStatus Status { get; set; } = CProjectStatus.Draft;

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public int Progress { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
        public int TotalSteps { get; set; }

        [Queryable(Filterable = true, RangeFilterable = true, Sortable = true)]
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