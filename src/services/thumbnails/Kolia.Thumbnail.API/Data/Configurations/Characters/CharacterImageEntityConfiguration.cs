using Kolia.Thumbnail.API.Data.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.Characters
{
    public class CharacterImageEntityConfiguration : IEntityTypeConfiguration<CharacterImageEntity>
    {
        public void Configure(EntityTypeBuilder<CharacterImageEntity> builder)
        {
            builder.ToTable("CharacterImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ExpressionLabel)
                .HasMaxLength(100);

            builder.Property(x => x.AngleLabel)
                .HasMaxLength(100);

            builder.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Character)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
