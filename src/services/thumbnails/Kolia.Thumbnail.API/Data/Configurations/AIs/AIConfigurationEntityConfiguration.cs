using Kolia.Thumbnail.API.Data.Entities.AIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.AIs
{
    public sealed class AIConfigurationEntityConfiguration
        : BaseEntityConfiguration<AIConfigurationEntity>
    {
        public override void Configure(EntityTypeBuilder<AIConfigurationEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("AIConfigurations");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.ApiKey)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.ApiVersion)
                .HasMaxLength(50);

            builder.Property(x => x.TimeoutSeconds)
                .HasDefaultValue(120);

            builder.Property(x => x.RetryCount)
                .HasDefaultValue(3);

            builder.Property(x => x.Priority)
                .HasDefaultValue(0);

            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.Property(x => x.TotalTokensUsed)
                .HasDefaultValue(0L);

            builder.Property(x => x.ApiKeyHash)
                .HasMaxLength(512);

            builder.Property(x => x.LastTokenResetTime);

            builder.Property(x => x.ExtraSettingsJson);

            builder.HasIndex(x => new
            {
                x.AIProviderId,
                x.Name
            }).IsUnique();

            builder.HasOne(x => x.AIProvider)
                .WithMany(x => x.Configurations)
                .HasForeignKey(x => x.AIProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}