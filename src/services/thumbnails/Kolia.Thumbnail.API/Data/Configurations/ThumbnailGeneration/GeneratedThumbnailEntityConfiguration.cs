using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.ThumbnailGeneration
{
    public class GeneratedThumbnailEntityConfiguration : IEntityTypeConfiguration<GeneratedThumbnailEntity>
    {
        public void Configure(EntityTypeBuilder<GeneratedThumbnailEntity> builder)
        {
            builder.ToTable("GeneratedThumbnails");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.VariantIndex).IsRequired();
            builder.Property(x => x.VersionNumber).IsRequired().HasDefaultValue(1);

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.DisplayTextSnapshot)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.CharacterSnapshotName)
                .HasMaxLength(150);

            builder.Property(x => x.LastEditTool);
            builder.Property(x => x.LastEditRequestText);

            builder.Property(x => x.IsApproved).HasDefaultValue(false);
            builder.Property(x => x.WasDownloaded).HasDefaultValue(false);
            builder.Property(x => x.IsPushedToTitleStep).HasDefaultValue(false);

            // Relationship to parent set
            builder.HasOne(x => x.GeneratedThumbnailSet)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.GeneratedThumbnailSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-reference for version chain — Restrict to avoid cascade loop
            builder.HasOne(x => x.ParentGeneratedThumbnail)
                .WithMany(x => x.ChildVersions)
                .HasForeignKey(x => x.ParentGeneratedThumbnailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
