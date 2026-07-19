using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.DisplayTexts
{
    public class DisplayTextRequestEntityConfiguration : IEntityTypeConfiguration<DisplayTextRequestEntity>
    {
        public void Configure(EntityTypeBuilder<DisplayTextRequestEntity> builder)
        {
            builder.ToTable("DisplayTextRequests");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.DisplayTextRequests)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
