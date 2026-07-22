using Kolia.Thumbnail.API.Data.Entities;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Data.Extensions;
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

            // ── New fields ──────────────────────────────────────────────

            builder.Property(x => x.SourceGroup)
                .IsRequired()
                .HasDefaultValue(CNewsSourceGroup.InternationalFinance)
                .HasSentinel(default(CNewsSourceGroup));

            builder.Property(x => x.FetchMode)
                .IsRequired()
                .HasDefaultValue(CSourceFetchMode.RssDirect)
                .HasSentinel(default(CSourceFetchMode));

            builder.Property(x => x.Domain)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.LastFailedAt)
                .IsRequired(false);

            builder.Property(x => x.ConsecutiveFailureCount)
                .HasDefaultValue(0);

            builder.Property(x => x.LastEtag)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.LastModifiedHeader)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.LastFetchedAt)
                .IsRequired(false);

            // Unique index on Domain to prevent duplicate sources per domain
            builder.HasIndex(x => x.Domain)
                .IsUnique()
                .HasDatabaseName("IX_NewsSources_Domain");

            // Seed 3 nguồn mẫu (giữ nguyên dữ liệu cũ, bổ sung field mới)
            var ts = SeedConstants.FixedSeedTimestamp;
            builder.HasData(
                new
                {
                    Id = Guid.Parse("11111111-0001-7000-8000-000000000001"),
                    Name = "VnExpress",
                    RssOrFeedUrl = "https://vnexpress.net/rss/kinh-doanh.rss",
                    Region = CMarketScope.Domestic,
                    IsTrusted = true,
                    Priority = 1,
                    SourceGroup = CNewsSourceGroup.VietnamFinance,
                    FetchMode = CSourceFetchMode.RssDirect,
                    Domain = "vnexpress.net",
                    LastFailedAt = (DateTimeOffset?)null,
                    ConsecutiveFailureCount = 0,
                    LastEtag = (string?)null,
                    LastModifiedHeader = (string?)null,
                    LastFetchedAt = (DateTimeOffset?)null,
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
                    SourceGroup = CNewsSourceGroup.InternationalFinance,
                    FetchMode = CSourceFetchMode.RssDirect,
                    Domain = "coindesk.com",
                    LastFailedAt = (DateTimeOffset?)null,
                    ConsecutiveFailureCount = 0,
                    LastEtag = (string?)null,
                    LastModifiedHeader = (string?)null,
                    LastFetchedAt = (DateTimeOffset?)null,
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
                    SourceGroup = CNewsSourceGroup.OfficialData,
                    FetchMode = CSourceFetchMode.RssDirect,
                    Domain = "federalreserve.gov",
                    LastFailedAt = (DateTimeOffset?)null,
                    ConsecutiveFailureCount = 0,
                    LastEtag = (string?)null,
                    LastModifiedHeader = (string?)null,
                    LastFetchedAt = (DateTimeOffset?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null
                }
            );

            // Bổ sung toàn bộ 30 nguồn còn lại theo spec khách hàng (6 nhóm)
            builder.HasData(NewsSourceSeedData.GetAllSeedObjects());
        }
    }
}
