using Kolia.Thumbnail.API.Data.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Projects
{
    public class ProjectStepEntityConfiguration : IEntityTypeConfiguration<ProjectStepEntity>
    {
        public void Configure(EntityTypeBuilder<ProjectStepEntity> builder)
        {
            builder.ToTable("ProjectSteps");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StepNumber)
                .IsRequired();

            builder.Property(x => x.StepStatus)
                .IsRequired();

            builder.Property(x => x.StepName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.OutputSummaryText)
                .HasMaxLength(500);

            builder.Property(x => x.NeedsApproval)
                .HasDefaultValue(false);

            builder.Property(x => x.ApprovedByNote)
                .HasMaxLength(1000);

            // Unique: một project chỉ có 1 bản ghi cho mỗi StepNumber
            builder.HasIndex(x => new { x.ProjectId, x.StepNumber })
                .IsUnique();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
