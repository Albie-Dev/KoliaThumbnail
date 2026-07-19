using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Thumbnails
{
    public class ThumbnailAnalysisEntityConfiguration : IEntityTypeConfiguration<ThumbnailAnalysisEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailAnalysisEntity> builder)
        {
            builder.ToTable("ThumbnailAnalyses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ThumbnailFactorsJson).IsRequired();
            builder.Property(x => x.TitleTextAnalysis).IsRequired();
            builder.Property(x => x.VideoTitleAnalysis).IsRequired();
            builder.Property(x => x.DisplayTextStyleNote).IsRequired();

            builder.Property(x => x.IsChosenForGeneration)
                .HasDefaultValue(false);

            // Unique FK: 1-1 với ThumbnailLibraryItem
            builder.HasIndex(x => x.ThumbnailLibraryItemId).IsUnique();
        }
    }
}
