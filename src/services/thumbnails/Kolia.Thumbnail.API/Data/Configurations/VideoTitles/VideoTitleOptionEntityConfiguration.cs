using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.VideoTitles
{
    public class VideoTitleOptionEntityConfiguration : IEntityTypeConfiguration<VideoTitleOptionEntity>
    {
        public void Configure(EntityTypeBuilder<VideoTitleOptionEntity> builder)
        {
            builder.ToTable("VideoTitleOptions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.GenerationRound).IsRequired();

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(300);

            // IsSelected có thể true trên nhiều dòng cùng lúc — không có unique constraint
            builder.Property(x => x.IsSelected).HasDefaultValue(false);

            builder.HasOne(x => x.VideoTitleRequest)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.VideoTitleRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
