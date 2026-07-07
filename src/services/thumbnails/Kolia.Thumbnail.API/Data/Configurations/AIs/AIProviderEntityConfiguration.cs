using Kolia.Thumbnail.API.Data.Entities.AIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.AIs
{
    public sealed class AIProviderEntityConfiguration
        : BaseEntityConfiguration<AIProviderEntity>
    {
        public override void Configure(EntityTypeBuilder<AIProviderEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("AIProviders");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ShortName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasIndex(x => x.ShortName).IsUnique();

            builder.HasMany(x => x.Configurations)
                .WithOne(x => x.AIProvider)
                .HasForeignKey(x => x.AIProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}