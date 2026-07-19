using Kolia.Thumbnail.API.Data.Entities.ExternalRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ExternalRequests
{
    public class ExternalRequestUsageLogEntityConfiguration : IEntityTypeConfiguration<ExternalRequestUsageLogEntity>
    {
        public void Configure(EntityTypeBuilder<ExternalRequestUsageLogEntity> builder)
        {
            builder.ToTable("ExternalRequestUsageLogs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestCount).IsRequired();
            builder.Property(x => x.RecordedDate).IsRequired();

            builder.Property(x => x.EstimatedCostUsd)
                .HasPrecision(18, 4);

            // Index by date for cost report queries
            builder.HasIndex(x => x.RecordedDate);
        }
    }
}
