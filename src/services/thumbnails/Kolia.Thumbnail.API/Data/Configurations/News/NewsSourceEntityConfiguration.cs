using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.News
{
    public class NewsSourceEntityConfiguration : IEntityTypeConfiguration<NewsSourceEntity>
    {
        public void Configure(EntityTypeBuilder<NewsSourceEntity> builder)
        {
            builder.ToTable("NewsSources");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.RssOrFeedUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Region)
                .IsRequired();

            builder.Property(x => x.IsTrusted)
                .HasDefaultValue(true);

            builder.Property(x => x.Priority)
                .HasDefaultValue(0);

            // Seed 3 nguồn mẫu theo D.10 mục 10.
            // Dùng anonymous object để vượt qua protected set của BaseEntity.Id và CreationTime.
            var ts = Entities.SeedConstants.FixedSeedTimestamp;
            builder.HasData(
                new
                {
                    Id = Guid.Parse("11111111-0001-7000-8000-000000000001"),
                    Name = "VnExpress",
                    RssOrFeedUrl = "https://vnexpress.net/rss/kinh-doanh.rss",
                    Region = CMarketScope.Domestic,
                    IsTrusted = true,
                    Priority = 1,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null
                },
                new
                {
                    Id = Guid.Parse("11111111-0001-7000-8000-000000000002"),
                    Name = "CoinDesk",
                    RssOrFeedUrl = "https://www.coindesk.com/arc/outboundfeeds/rss/",
                    Region = CMarketScope.International,
                    IsTrusted = true,
                    Priority = 2,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null
                },
                new
                {
                    Id = Guid.Parse("11111111-0001-7000-8000-000000000003"),
                    Name = "Federal Reserve",
                    RssOrFeedUrl = "https://www.federalreserve.gov/feeds/press_all.xml",
                    Region = CMarketScope.International,
                    IsTrusted = true,
                    Priority = 3,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null
                }
            );
        }
    }
}
