using Kolia.Thumbnail.API.Data.Entities.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.News
{
    public class NewsDeepAnalysisEntityConfiguration : IEntityTypeConfiguration<NewsDeepAnalysisEntity>
    {
        public void Configure(EntityTypeBuilder<NewsDeepAnalysisEntity> builder)
        {
            builder.ToTable("NewsDeepAnalyses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MacroEventSummaryJson).IsRequired();
            builder.Property(x => x.MarketReactionJson).IsRequired();
            builder.Property(x => x.ExpectationShortTerm).IsRequired();
            builder.Property(x => x.ExpectationLongTerm).IsRequired();
            builder.Property(x => x.SentimentOverviewJson).IsRequired();
            builder.Property(x => x.EmotionTags).IsRequired();
            builder.Property(x => x.EmotionReason).IsRequired();

            builder.Property(x => x.WasTranslatedFromForeign)
                .HasDefaultValue(false);

            // Unique FK: 1-1 với NewsItem
            builder.HasIndex(x => x.NewsItemId).IsUnique();
        }
    }
}
