using Kolia.Thumbnail.API.Data.Entities.Socials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Socials
{
    public class SocialMediaProviderEntityConfiguration
        : IEntityTypeConfiguration<SocialMediaProviderEntity>
    {
        public void Configure(EntityTypeBuilder<SocialMediaProviderEntity> builder)
        {
            builder.ToTable("SocialMediaProviders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ShortName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.BaseUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.ProviderType)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasIndex(x => x.ShortName)
                .IsUnique();

            builder.HasMany(x => x.Configurations)
                .WithOne(x => x.SocialMediaProvider)
                .HasForeignKey(x => x.SocialMediaProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}