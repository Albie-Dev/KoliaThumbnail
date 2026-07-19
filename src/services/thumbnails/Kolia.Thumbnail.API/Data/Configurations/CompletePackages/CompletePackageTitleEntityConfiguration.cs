using Kolia.Thumbnail.API.Data.Entities.CompletePackages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.CompletePackages
{
    public class CompletePackageTitleEntityConfiguration : IEntityTypeConfiguration<CompletePackageTitleEntity>
    {
        public void Configure(EntityTypeBuilder<CompletePackageTitleEntity> builder)
        {
            builder.ToTable("CompletePackageTitles");

            builder.HasKey(x => new { x.CompletePackageId, x.VideoTitleOptionId });

            builder.HasOne(x => x.CompletePackage)
                .WithMany(x => x.SelectedTitles)
                .HasForeignKey(x => x.CompletePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.VideoTitleOption)
                .WithMany()
                .HasForeignKey(x => x.VideoTitleOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.CompletePackage.IsDeleted);
        }
    }
}
