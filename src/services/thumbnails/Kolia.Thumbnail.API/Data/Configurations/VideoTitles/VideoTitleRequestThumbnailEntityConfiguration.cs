using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.VideoTitles
{
    public class VideoTitleRequestThumbnailEntityConfiguration
        : IEntityTypeConfiguration<VideoTitleRequestThumbnailEntity>
    {
        public void Configure(EntityTypeBuilder<VideoTitleRequestThumbnailEntity> builder)
        {
            builder.ToTable("VideoTitleRequestThumbnails");

            builder.HasKey(x => new { x.VideoTitleRequestId, x.GeneratedThumbnailId });

            builder.HasOne(x => x.VideoTitleRequest)
                .WithMany(x => x.SelectedThumbnails)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.GeneratedThumbnail)
                .WithMany()
                .HasForeignKey(x => x.GeneratedThumbnailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.GeneratedThumbnail.IsDeleted);
        }
    }
}
