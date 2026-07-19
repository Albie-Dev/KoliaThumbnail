using Kolia.Thumbnail.API.Data.Entities.AIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kolia.Thumbnail.API.Data.Configurations.AIs
{
    public sealed class AIFunctionConfigEntityConfiguration
        : BaseEntityConfiguration<AIFunctionConfigEntity>
    {
        public override void Configure(EntityTypeBuilder<AIFunctionConfigEntity> builder)
        {
            base.Configure(builder);

            builder.ToTable("AIFunctionConfigs");

            builder.Property(x => x.FunctionType)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(x => x.FunctionType)
                .IsUnique();

            builder.Property(x => x.Model)
                .HasMaxLength(100);

            builder.Property(x => x.Temperature)
                .HasPrecision(3, 2);

            builder.Property(x => x.MaxTokens);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.FunctionConfig)
                .HasForeignKey(x => x.FunctionConfigId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Seed 6 function configs mặc định ──────────────────────────
            // Chỉ seed shell (FunctionType + Id cố định), không seed model/config
            // vì phụ thuộc vào provider + config + model người dùng tự chọn qua UI.
            var ts = Entities.SeedConstants.FixedSeedTimestamp;
            builder.HasData(
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0001-7000-8000-000000000001"),
                    FunctionType = Enums.CAIFunctionType.ContentBriefAnalysis,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                },
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0002-7000-8000-000000000002"),
                    FunctionType = Enums.CAIFunctionType.NewsScoring,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                },
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0003-7000-8000-000000000003"),
                    FunctionType = Enums.CAIFunctionType.ThumbnailGeneration,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                },
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0004-7000-8000-000000000004"),
                    FunctionType = Enums.CAIFunctionType.DisplayTextGeneration,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                },
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0005-7000-8000-000000000005"),
                    FunctionType = Enums.CAIFunctionType.VideoTitleGeneration,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                },
                new
                {
                    Id = Guid.Parse("AAAAAAAA-0006-7000-8000-000000000006"),
                    FunctionType = Enums.CAIFunctionType.CompletePackageGeneration,
                    Model = (string?)null,
                    Temperature = (double?)null,
                    MaxTokens = (int?)null,
                    CreationTime = ts,
                    LastModificationTime = (DateTimeOffset?)null,
                    IsDeleted = false,
                    DeletionTime = (DateTimeOffset?)null,
                }
            );
        }
    }
}
