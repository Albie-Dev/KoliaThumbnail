using Kolia.Thumbnail.API.Data.Entities.ExternalRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ExternalRequests
{
    public class ExternalRequestQueueEntityConfiguration : IEntityTypeConfiguration<ExternalRequestQueueEntity>
    {
        public void Configure(EntityTypeBuilder<ExternalRequestQueueEntity> builder)
        {
            builder.ToTable("ExternalRequestQueues");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Purpose).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(Enums.CExternalRequestStatus.Pending);
            builder.Property(x => x.PayloadJson).IsRequired();
            builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);

            // Indexed for queue polling queries
            builder.HasIndex(x => new { x.Status, x.NextRetryAt });

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
