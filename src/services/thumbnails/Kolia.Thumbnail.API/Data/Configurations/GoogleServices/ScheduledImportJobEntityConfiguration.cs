using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.GoogleServices
{
    public class ScheduledImportJobEntityConfiguration
        : BaseEntityConfiguration<ScheduledImportJobEntity>
    {
        public override void Configure(EntityTypeBuilder<ScheduledImportJobEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("ScheduledImportJobs");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.SourceUrl)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(4000);

            builder.Property(x => x.LogJson)
                .HasMaxLength(16000);

            builder.Property(x => x.ImportedContent)
                .HasMaxLength(50000);

            // Status mặc định được set qua entity initializer (CJobScheduleStatus.Pending)

            builder.Property(x => x.RetryCount)
                .HasDefaultValue(0);

            builder.Property(x => x.MaxRetries)
                .HasDefaultValue(3);

            builder.Property(x => x.CronExpression)
                .HasMaxLength(200);

            builder.Property(x => x.CronDescription)
                .HasMaxLength(500);

            builder.Property(x => x.TimeZone)
                .HasMaxLength(100)
                .HasDefaultValue("UTC");

            builder.HasOne(x => x.GoogleServiceAccount)
                .WithMany(x => x.ScheduledJobs)
                .HasForeignKey(x => x.GoogleServiceAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
