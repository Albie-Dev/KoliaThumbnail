using Kolia.Thumbnail.API.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Data.Contexts
{
    public class ThumbnailDbContext : DbContext
    {
        public ThumbnailDbContext(DbContextOptions<ThumbnailDbContext> options)
            : base(options)
        {
        }

        #region DbSets
        public DbSet<Entities.AIs.AIProviderEntity> AIProviders { get; set; } = null!;
        public DbSet<Entities.AIs.AIConfigurationEntity> AIConfigurations { get; set; } = null!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ThumbnailDbContext).Assembly);

            modelBuilder.ApplySoftDeleteQueryFilter();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}