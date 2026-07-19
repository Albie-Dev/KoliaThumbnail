using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.VideoTitles
{
    public class VideoTitleFeedbackEntityConfiguration : IEntityTypeConfiguration<VideoTitleFeedbackEntity>
    {
        public void Configure(EntityTypeBuilder<VideoTitleFeedbackEntity> builder)
        {
            builder.ToTable("VideoTitleFeedbacks");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FeedbackText).IsRequired();
            builder.Property(x => x.AppliedToRound).IsRequired().HasDefaultValue(0);

            builder.HasOne(x => x.VideoTitleRequest)
                .WithMany(x => x.Feedbacks)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
