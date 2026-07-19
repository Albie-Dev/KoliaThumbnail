using Kolia.Thumbnail.API.Data.Entities.CompletePackages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.CompletePackages
{
    public class CompletePackageEntityConfiguration : IEntityTypeConfiguration<CompletePackageEntity>
    {
        public void Configure(EntityTypeBuilder<CompletePackageEntity> builder)
        {
            builder.ToTable("CompletePackages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisplayTextSnapshot)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.ConfirmedAt).IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.CompletePackages)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SelectedThumbnail)
                .WithMany()
                .HasForeignKey(x => x.SelectedThumbnailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SelectedTitles)
                .WithOne(x => x.CompletePackage)
                .HasForeignKey(x => x.CompletePackageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
