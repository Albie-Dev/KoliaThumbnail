using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Thumbnails
{
    public class ThumbnailSearchRequestEntityConfiguration : IEntityTypeConfiguration<ThumbnailSearchRequestEntity>
    {
        public void Configure(EntityTypeBuilder<ThumbnailSearchRequestEntity> builder)
        {
            builder.ToTable("ThumbnailSearchRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Keyword)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.TimeFilter).IsRequired();
            builder.Property(x => x.SortFilter).IsRequired();

            builder.Property(x => x.WasSuggestedFromNews)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ThumbnailSearchRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.LibraryItems)
                .WithOne(x => x.ThumbnailSearchRequest)
                .HasForeignKey(x => x.ThumbnailSearchRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
