using Kolia.Thumbnail.API.Data.Entities.AIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.AIs
{
    public sealed class AIFunctionConfigItemEntityConfiguration
        : BaseEntityConfiguration<AIFunctionConfigItemEntity>
    {
        public override void Configure(EntityTypeBuilder<AIFunctionConfigItemEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("AIFunctionConfigItems");

            builder.Property(x => x.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Model)
                .HasMaxLength(100);

            builder.Property(x => x.Temperature)
                .HasPrecision(3, 2);

            builder.Property(x => x.MaxTokens);

            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(true);

            // Index: FunctionConfigId + Priority là unique (mỗi priority chỉ 1 item)
            builder.HasIndex(x => new { x.FunctionConfigId, x.Priority })
                .IsUnique();

            // Relationships
            builder.HasOne(x => x.FunctionConfig)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.FunctionConfigId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AIProvider)
                .WithMany()
                .HasForeignKey(x => x.AIProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AIProviderConfiguration)
                .WithMany()
                .HasForeignKey(x => x.AIProviderConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
