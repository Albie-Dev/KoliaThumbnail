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
        public DbSet<Entities.AIs.AIProviderConfigurationEntity> AIProviderConfigurations { get; set; } = null!;

        public DbSet<Entities.Socials.SocialMediaProviderEntity> SocialMediaProviders { get; set; } = null!;
        public DbSet<Entities.Socials.SocialMediaProviderConfigurationEntity> SocialMediaProviderConfigurations { get; set; } = null!;

        public DbSet<Entities.Projects.ProjectEntity> Projects { get; set; } = null!;
        public DbSet<Entities.Projects.ProjectStepEntity> ProjectSteps { get; set; } = null!;
        public DbSet<Entities.Projects.StepDefinitionEntity> StepDefinitions { get; set; } = null!;
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