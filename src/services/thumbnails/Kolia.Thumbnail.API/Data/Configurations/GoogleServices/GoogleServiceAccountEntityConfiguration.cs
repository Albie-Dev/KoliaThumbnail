using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.GoogleServices
{
    public class GoogleServiceAccountEntityConfiguration
        : BaseEntityConfiguration<GoogleServiceAccountEntity>
    {
        public override void Configure(EntityTypeBuilder<GoogleServiceAccountEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("GoogleServiceAccounts");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.ClientEmail)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.ClientId)
                .HasMaxLength(300);

            builder.Property(x => x.ProjectId)
                .HasMaxLength(200);

            builder.Property(x => x.TokenUri)
                .HasMaxLength(500);

            builder.Property(x => x.AuthUri)
                .HasMaxLength(500);

            builder.Property(x => x.AuthProviderX509CertUrl)
                .HasMaxLength(500);

            builder.Property(x => x.PrivateKeyIdHash)
                .HasMaxLength(512);

            builder.Property(x => x.PrivateKey)
                .IsRequired()
                .HasMaxLength(8000);

            builder.Property(x => x.RawCredentialJson)
                .HasMaxLength(16000);

            builder.Property(x => x.CredentialJsonHash)
                .HasMaxLength(512);

            builder.Property(x => x.Scopes)
                .HasMaxLength(2000);

            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.ClientEmail)
                .IsUnique();
        }
    }
}
