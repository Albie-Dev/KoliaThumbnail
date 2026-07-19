using Kolia.Thumbnail.API.Data.Entities.Briefs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Briefs
{
    public class ContentBriefEntityConfiguration : IEntityTypeConfiguration<ContentBriefEntity>
    {
        public void Configure(EntityTypeBuilder<ContentBriefEntity> builder)
        {
            builder.ToTable("ContentBriefs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImportSource);

            builder.Property(x => x.ImportedRawText);

            builder.Property(x => x.ImportedFileUrl)
                .HasMaxLength(1000);

            builder.Property(x => x.ImportedExternalLink)
                .HasMaxLength(1000);

            builder.Property(x => x.ExternalSheetUrl)
                .HasMaxLength(1000);

            builder.Property(x => x.OverviewInput)
                .IsRequired();

            builder.Property(x => x.ViewpointInput)
                .IsRequired();

            builder.Property(x => x.KeyDataInput)
                .IsRequired();

            builder.Property(x => x.TopicOutput)
                .HasMaxLength(300);

            builder.Property(x => x.MainMessageOutput);

            builder.Property(x => x.HighlightDataOutput);

            builder.Property(x => x.IsConfirmed)
                .HasDefaultValue(false);

            // Unique FK: 1-1 với Project
            builder.HasIndex(x => x.ProjectId)
                .IsUnique();
        }
    }
}
