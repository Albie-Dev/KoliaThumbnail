using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.DisplayTexts
{
    public class DisplayTextOptionEntityConfiguration : IEntityTypeConfiguration<DisplayTextOptionEntity>
    {
        public void Configure(EntityTypeBuilder<DisplayTextOptionEntity> builder)
        {
            builder.ToTable("DisplayTextOptions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.IsSelected)
                .HasDefaultValue(false);

            builder.HasOne(x => x.DisplayTextRequest)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.DisplayTextRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SourceNewsItem)
                .WithMany()
                .HasForeignKey(x => x.SourceNewsItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
