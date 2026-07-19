using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.DisplayTexts
{
    public class DisplayTextRequestNewsItemEntityConfiguration : IEntityTypeConfiguration<DisplayTextRequestNewsItemEntity>
    {
        public void Configure(EntityTypeBuilder<DisplayTextRequestNewsItemEntity> builder)
        {
            builder.ToTable("DisplayTextRequestNewsItems");

            // Composite PK for n-n join table
            builder.HasKey(x => new { x.DisplayTextRequestId, x.NewsItemId });

            builder.HasOne(x => x.DisplayTextRequest)
                .WithMany(x => x.SelectedNewsItems)
                .HasForeignKey(x => x.DisplayTextRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NewsItem)
                .WithMany()
                .HasForeignKey(x => x.NewsItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DisplayTextRequest.IsDeleted);
        }
    }
}
