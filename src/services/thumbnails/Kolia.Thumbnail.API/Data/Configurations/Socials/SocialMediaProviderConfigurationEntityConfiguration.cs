using Kolia.Thumbnail.API.Data.Entities.Socials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Socials
{
    public class SocialMediaProviderConfigurationEntityConfiguration
        : IEntityTypeConfiguration<SocialMediaProviderConfigurationEntity>
    {
        public void Configure(EntityTypeBuilder<SocialMediaProviderConfigurationEntity> builder)
        {
            builder.ToTable("SocialMediaProviderConfigurations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ApiVersion)
                .HasMaxLength(50);

            builder.Property(x => x.ApiBaseUrl)
                .HasMaxLength(500);

            builder.Property(x => x.ApiKey)
                .HasMaxLength(500);

            builder.Property(x => x.ClientId)
                .HasMaxLength(255);

            builder.Property(x => x.ClientSecret)
                .HasMaxLength(500);

            builder.Property(x => x.AppId)
                .HasMaxLength(255);

            builder.Property(x => x.AppSecret)
                .HasMaxLength(500);

            builder.Property(x => x.BearerToken)
                .HasMaxLength(4000);

            builder.Property(x => x.RefreshToken)
                .HasMaxLength(4000);

            builder.Property(x => x.AccessToken)
                .HasMaxLength(4000);

            builder.Property(x => x.Scope)
                .HasMaxLength(1000);

            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(false);

            builder.HasIndex(x => new
            {
                x.SocialMediaProviderId,
                x.Name
            }).IsUnique();

            builder.HasOne(x => x.SocialMediaProvider)
                .WithMany(x => x.Configurations)
                .HasForeignKey(x => x.SocialMediaProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}