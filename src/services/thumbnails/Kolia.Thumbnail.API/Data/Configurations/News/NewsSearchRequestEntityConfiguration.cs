using Kolia.Thumbnail.API.Data.Entities.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.News
{
    public class NewsSearchRequestEntityConfiguration : IEntityTypeConfiguration<NewsSearchRequestEntity>
    {
        public void Configure(EntityTypeBuilder<NewsSearchRequestEntity> builder)
        {
            builder.ToTable("NewsSearchRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MarketScope).IsRequired();
            builder.Property(x => x.TimeRange).IsRequired();
            builder.Property(x => x.CountFilter).IsRequired();

            builder.Property(x => x.KeywordsRaw)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.SuggestedKeywordsUsedJson);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.NewsSearchRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.NewsItems)
                .WithOne(x => x.NewsSearchRequest)
                .HasForeignKey(x => x.NewsSearchRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
