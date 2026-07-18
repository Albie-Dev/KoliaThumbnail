using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Data.Seeding.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Projects
{
    public sealed class StepDefinitionEntityConfiguration
        : BaseEntityConfiguration<StepDefinitionEntity>
    {
        public override void Configure(EntityTypeBuilder<StepDefinitionEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("StepDefinitions");

            builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.DisplayCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.SortOrder).IsRequired();
            builder.Property(x => x.IsTrackable).HasDefaultValue(true);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.ParentId, x.SortOrder });

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(StepDefinitionSeedData.GetAll());
        }
    }
}