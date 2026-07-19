using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ThumbnailGeneration
{
    public class ThumbnailGenerationRequestReferenceEntityConfiguration
        : IEntityTypeConfiguration<ThumbnailGenerationRequestReferenceEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailGenerationRequestReferenceEntity> builder)
        {
            builder.ToTable("ThumbnailGenerationRequestReferences");

            builder.HasKey(x => new { x.ThumbnailGenerationRequestId, x.ThumbnailLibraryItemId });

            builder.HasOne(x => x.ThumbnailGenerationRequest)
                .WithMany(x => x.SelectedReferenceItems)
                .HasForeignKey(x => x.ThumbnailGenerationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ThumbnailLibraryItem)
                .WithMany()
                .HasForeignKey(x => x.ThumbnailLibraryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.ThumbnailGenerationRequest.IsDeleted);
        }
    }
}
