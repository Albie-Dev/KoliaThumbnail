using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.VideoTitles
{
    public class VideoTitleRequestNewsItemEntityConfiguration
        : IEntityTypeConfiguration<VideoTitleRequestNewsItemEntity>
    {
        public void Configure(EntityTypeBuilder<VideoTitleRequestNewsItemEntity> builder)
        {
            builder.ToTable("VideoTitleRequestNewsItems");

            builder.HasKey(x => new { x.VideoTitleRequestId, x.NewsItemId });

            builder.HasOne(x => x.VideoTitleRequest)
                .WithMany(x => x.SelectedNewsItems)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NewsItem)
                .WithMany()
                .HasForeignKey(x => x.NewsItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.NewsItem.IsDeleted);
        }
    }
}
