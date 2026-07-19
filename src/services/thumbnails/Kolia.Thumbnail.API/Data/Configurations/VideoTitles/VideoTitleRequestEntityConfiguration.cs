using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.VideoTitles
{
    public class VideoTitleRequestEntityConfiguration : IEntityTypeConfiguration<VideoTitleRequestEntity>
    {
        public void Configure(EntityTypeBuilder<VideoTitleRequestEntity> builder)
        {
            builder.ToTable("VideoTitleRequests");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestedTitleCount).IsRequired();
            builder.Property(x => x.Style).IsRequired();
            builder.Property(x => x.GenerationRound).IsRequired().HasDefaultValue(1);

            builder.Property(x => x.KeywordsRaw)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.BuiltPromptText).IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.VideoTitleRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Options)
                .WithOne(x => x.VideoTitleRequest)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Feedbacks)
                .WithOne(x => x.VideoTitleRequest)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
