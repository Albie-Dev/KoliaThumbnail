using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ThumbnailGeneration
{
    public class ThumbnailGenerationRequestEntityConfiguration : IEntityTypeConfiguration<ThumbnailGenerationRequestEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailGenerationRequestEntity> builder)
        {
            builder.ToTable("ThumbnailGenerationRequests");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ChangesRequestText).IsRequired();
            builder.Property(x => x.LockedElementsJson);
            builder.Property(x => x.ChangeableElementsJson);
            builder.Property(x => x.GeneratedPromptText);

            builder.Property(x => x.Ratio)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Resolution)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.RequestedImageCount)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ThumbnailGenerationRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Character)
                .WithMany()
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.GeneratedSets)
                .WithOne(x => x.ThumbnailGenerationRequest)
                .HasForeignKey(x => x.ThumbnailGenerationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
