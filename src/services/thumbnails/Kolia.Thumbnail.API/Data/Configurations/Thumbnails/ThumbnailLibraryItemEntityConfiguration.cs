using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Thumbnails
{
    public class ThumbnailLibraryItemEntityConfiguration : IEntityTypeConfiguration<ThumbnailLibraryItemEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailLibraryItemEntity> builder)
        {
            builder.ToTable("ThumbnailLibraryItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceType).IsRequired();
            builder.Property(x => x.Platform).IsRequired();
            builder.Property(x => x.UserStatus).IsRequired();

            builder.Property(x => x.VideoTitle)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.VideoUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ChannelName)
                .HasMaxLength(200);

            builder.Property(x => x.ThumbnailImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.KeywordBatchTag)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.IsFilteredIrrelevant)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ThumbnailLibraryItems)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ThumbnailSearchRequest)
                .WithMany(x => x.LibraryItems)
                .HasForeignKey(x => x.ThumbnailSearchRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-1 with Analysis
            builder.HasOne(x => x.Analysis)
                .WithOne(x => x.ThumbnailLibraryItem)
                .HasForeignKey<ThumbnailAnalysisEntity>(x => x.ThumbnailLibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
