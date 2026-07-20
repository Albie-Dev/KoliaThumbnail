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

        #region DbSets - Providers (AI & Social)
        public DbSet<Entities.AIs.AIProviderEntity> AIProviders { get; set; } = null!;
        public DbSet<Entities.AIs.AIProviderConfigurationEntity> AIProviderConfigurations { get; set; } = null!;
        public DbSet<Entities.AIs.AIFunctionConfigEntity> AIFunctionConfigs { get; set; } = null!;
        public DbSet<Entities.AIs.AIFunctionConfigItemEntity> AIFunctionConfigItems { get; set; } = null!;
        public DbSet<Entities.Socials.SocialMediaProviderEntity> SocialMediaProviders { get; set; } = null!;
        public DbSet<Entities.Socials.SocialMediaProviderConfigurationEntity> SocialMediaProviderConfigurations { get; set; } = null!;
        #endregion

        #region DbSets - Projects
        public DbSet<Entities.Projects.ProjectEntity> Projects { get; set; } = null!;
        public DbSet<Entities.Projects.ProjectStepEntity> ProjectSteps { get; set; } = null!;
        #endregion

        #region DbSets - Briefs
        public DbSet<Entities.Briefs.ContentBriefEntity> ContentBriefs { get; set; } = null!;
        #endregion

        #region DbSets - News
        public DbSet<Entities.News.NewsSourceEntity> NewsSources { get; set; } = null!;
        public DbSet<Entities.News.NewsSearchRequestEntity> NewsSearchRequests { get; set; } = null!;
        public DbSet<Entities.News.NewsItemEntity> NewsItems { get; set; } = null!;
        public DbSet<Entities.News.NewsDeepAnalysisEntity> NewsDeepAnalyses { get; set; } = null!;
        #endregion

        #region DbSets - Thumbnails
        public DbSet<Entities.Thumbnails.ThumbnailSearchRequestEntity> ThumbnailSearchRequests { get; set; } = null!;
        public DbSet<Entities.Thumbnails.ThumbnailLibraryItemEntity> ThumbnailLibraryItems { get; set; } = null!;
        public DbSet<Entities.Thumbnails.ThumbnailAnalysisEntity> ThumbnailAnalyses { get; set; } = null!;
        #endregion

        #region DbSets - Characters
        public DbSet<Entities.Characters.CharacterEntity> Characters { get; set; } = null!;
        public DbSet<Entities.Characters.CharacterImageEntity> CharacterImages { get; set; } = null!;
        #endregion

        #region DbSets - DisplayTexts
        public DbSet<Entities.DisplayTexts.DisplayTextRequestEntity> DisplayTextRequests { get; set; } = null!;
        public DbSet<Entities.DisplayTexts.DisplayTextRequestNewsItemEntity> DisplayTextRequestNewsItems { get; set; } = null!;
        public DbSet<Entities.DisplayTexts.DisplayTextOptionEntity> DisplayTextOptions { get; set; } = null!;
        #endregion

        #region DbSets - ThumbnailGeneration
        public DbSet<Entities.ThumbnailGeneration.ThumbnailGenerationRequestEntity> ThumbnailGenerationRequests { get; set; } = null!;
        public DbSet<Entities.ThumbnailGeneration.ThumbnailGenerationRequestDisplayTextEntity> ThumbnailGenerationRequestDisplayTexts { get; set; } = null!;
        public DbSet<Entities.ThumbnailGeneration.ThumbnailGenerationRequestReferenceEntity> ThumbnailGenerationRequestReferences { get; set; } = null!;
        public DbSet<Entities.ThumbnailGeneration.GeneratedThumbnailSetEntity> GeneratedThumbnailSets { get; set; } = null!;
        public DbSet<Entities.ThumbnailGeneration.GeneratedThumbnailEntity> GeneratedThumbnails { get; set; } = null!;
        #endregion

        #region DbSets - VideoTitles
        public DbSet<Entities.VideoTitles.VideoTitleRequestEntity> VideoTitleRequests { get; set; } = null!;
        public DbSet<Entities.VideoTitles.VideoTitleRequestThumbnailEntity> VideoTitleRequestThumbnails { get; set; } = null!;
        public DbSet<Entities.VideoTitles.VideoTitleRequestNewsItemEntity> VideoTitleRequestNewsItems { get; set; } = null!;
        public DbSet<Entities.VideoTitles.VideoTitleOptionEntity> VideoTitleOptions { get; set; } = null!;
        public DbSet<Entities.VideoTitles.VideoTitleFeedbackEntity> VideoTitleFeedbacks { get; set; } = null!;
        #endregion

        #region DbSets - CompletePackages
        public DbSet<Entities.CompletePackages.CompletePackageEntity> CompletePackages { get; set; } = null!;
        public DbSet<Entities.CompletePackages.CompletePackageTitleEntity> CompletePackageTitles { get; set; } = null!;
        #endregion

        #region DbSets - ExternalRequests
        public DbSet<Entities.ExternalRequests.ExternalRequestQueueEntity> ExternalRequestQueues { get; set; } = null!;
        public DbSet<Entities.ExternalRequests.ExternalRequestUsageLogEntity> ExternalRequestUsageLogs { get; set; } = null!;
        #endregion

        #region DbSets - Google Services
        public DbSet<Entities.GoogleServices.GoogleServiceAccountEntity> GoogleServiceAccounts { get; set; } = null!;
        public DbSet<Entities.GoogleServices.ScheduledImportJobEntity> ScheduledImportJobs { get; set; } = null!;
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