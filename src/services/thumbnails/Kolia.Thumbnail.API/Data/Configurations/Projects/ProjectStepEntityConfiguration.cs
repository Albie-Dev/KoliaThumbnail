using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Projects
{
    public sealed class ProjectStepEntityConfiguration
        : BaseEntityConfiguration<ProjectStepEntity>
    {
        public override void Configure(EntityTypeBuilder<ProjectStepEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("ProjectSteps");

            builder.Property(x => x.Status)
                .HasDefaultValue(CProjectStepStatus.NotStarted);

            // JSON linh hoạt cho nội dung từng loại bước.
            // Postgres: đổi sang .HasColumnType("jsonb") nếu dùng Npgsql.
            builder.Property(x => x.ContentJson);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            // Mỗi project chỉ có đúng 1 instance cho mỗi StepDefinition
            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.StepDefinitionId
            }).IsUnique();

            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Không cascade delete ProjectStep khi xóa StepDefinition,
            // vì StepDefinition là bảng định nghĩa dùng chung cho mọi project.
            builder.HasOne(x => x.StepDefinition)
                .WithMany()
                .HasForeignKey(x => x.StepDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}