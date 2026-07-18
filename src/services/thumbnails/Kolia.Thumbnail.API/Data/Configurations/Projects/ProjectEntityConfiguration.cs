using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Projects
{
    public sealed class ProjectEntityConfiguration
        : BaseEntityConfiguration<ProjectEntity>
    {
        public override void Configure(EntityTypeBuilder<ProjectEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("Projects");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.CreatedByUserId).IsRequired();
            builder.Property(x => x.CreatedByUserName).HasMaxLength(255);
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
            builder.Property(x => x.Status).HasDefaultValue(CProjectStatus.Draft);
            builder.Property(x => x.Progress).HasDefaultValue(0);
            builder.Property(x => x.TotalSteps).HasDefaultValue(0);
            builder.Property(x => x.CompletedSteps).HasDefaultValue(0);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.Status);

            // Quan hệ 1 Project - N ProjectStep chỉ khai báo ở ProjectStepEntityConfiguration,
            // không lặp lại HasMany ở đây để tránh 2 nguồn khai báo cho cùng 1 quan hệ.
        }
    }
}