using Kolia.Thumbnail.API.Data.Entities.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.News
{
    public class NewsItemEntityConfiguration : IEntityTypeConfiguration<NewsItemEntity>
    {
        public void Configure(EntityTypeBuilder<NewsItemEntity> builder)
        {
            builder.ToTable("NewsItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceType).IsRequired();
            builder.Property(x => x.MarketType).IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.SourceName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SourceUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ScannedTime).IsRequired();
            builder.Property(x => x.SummaryOverview).IsRequired();

            builder.Property(x => x.RelevanceToTopicScore).IsRequired();
            builder.Property(x => x.ImportanceImpactScore).IsRequired();
            builder.Property(x => x.EmotionPotentialScore).IsRequired();
            builder.Property(x => x.NoveltyDataScore).IsRequired();
            builder.Property(x => x.TotalScore).IsRequired();

            builder.Property(x => x.Recommendation).IsRequired();
            builder.Property(x => x.RelevanceLevel).IsRequired();

            builder.Property(x => x.IsSelectedByTeam)
                .HasDefaultValue(false);

            builder.Property(x => x.SuggestedKeywordsForThumbnail)
                .HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NewsSearchRequest)
                .WithMany(x => x.NewsItems)
                .HasForeignKey(x => x.NewsSearchRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-1 with DeepAnalysis
            builder.HasOne(x => x.DeepAnalysis)
                .WithOne(x => x.NewsItem)
                .HasForeignKey<NewsDeepAnalysisEntity>(x => x.NewsItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
