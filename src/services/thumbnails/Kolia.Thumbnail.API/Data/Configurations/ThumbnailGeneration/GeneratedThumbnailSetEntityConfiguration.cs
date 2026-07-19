using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ThumbnailGeneration
{
    public class GeneratedThumbnailSetEntityConfiguration : IEntityTypeConfiguration<GeneratedThumbnailSetEntity>
    {
        public void Configure(EntityTypeBuilder<GeneratedThumbnailSetEntity> builder)
        {
            builder.ToTable("GeneratedThumbnailSets");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SetIndex).IsRequired();

            builder.HasOne(x => x.ThumbnailGenerationRequest)
                .WithMany(x => x.GeneratedSets)
                .HasForeignKey(x => x.ThumbnailGenerationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Variants)
                .WithOne(x => x.GeneratedThumbnailSet)
                .HasForeignKey(x => x.GeneratedThumbnailSetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
