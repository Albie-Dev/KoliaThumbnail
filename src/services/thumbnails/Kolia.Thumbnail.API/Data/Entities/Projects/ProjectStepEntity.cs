using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    // Instance thực tế của 1 StepDefinition trong 1 Project — nơi lưu trạng thái + nội dung
    public class ProjectStepEntity : BaseEntity
    {
        /// <summary>
        /// Id dự án
        /// </summary>
        public Guid ProjectId { get; set; }
        /// <summary>
        /// Dự án
        /// </summary>
        public ProjectEntity Project { get; set; } = null!;
        /// <summary>
        /// Id bước thực hiện
        /// </summary>
        public Guid StepDefinitionId { get; set; }
        /// <summary>
        /// Định nghĩa bước thực hiện
        /// </summary>
        public virtual StepDefinitionEntity StepDefinition { get; set; } = null!;
        /// <summary>
        /// Trạng thái bước thực hiện của dự án
        /// </summary>
        public CProjectStepStatus Status { get; set; } = CProjectStepStatus.NotStarted;

        // Nội dung linh hoạt cho từng loại bước (mỗi bước có shape dữ liệu khác nhau)
        // Postgres: dùng jsonb (Npgsql.EntityFrameworkCore.PostgreSQL). SQL Server: nvarchar(max) + HasConversion.
        public string? ContentJson { get; set; }
        /// <summary>
        /// Thời gian bắt đầu làm bước này.
        /// </summary>
        public DateTimeOffset? StartedAt { get; set; }
        /// <summary>
        /// Thời gian kết thúc bước
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }
        /// <summary>
        /// Chi tiết lỗi xảy ra ở bước này.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}