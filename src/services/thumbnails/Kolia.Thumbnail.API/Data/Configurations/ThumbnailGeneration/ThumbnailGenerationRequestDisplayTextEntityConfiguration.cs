using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ThumbnailGeneration
{
    public class ThumbnailGenerationRequestDisplayTextEntityConfiguration
        : IEntityTypeConfiguration<ThumbnailGenerationRequestDisplayTextEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailGenerationRequestDisplayTextEntity> builder)
        {
            builder.ToTable("ThumbnailGenerationRequestDisplayTexts");

            builder.HasKey(x => new { x.ThumbnailGenerationRequestId, x.DisplayTextOptionId });

            builder.HasOne(x => x.ThumbnailGenerationRequest)
                .WithMany(x => x.SelectedDisplayTextOptions)
                .HasForeignKey(x => x.ThumbnailGenerationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DisplayTextOption)
                .WithMany()
                .HasForeignKey(x => x.DisplayTextOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DisplayTextOption.IsDeleted);
        }
    }
}
