using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.AspNetCore;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Interceptors;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.BackgroundJobs;
using Kolia.Thumbnail.API.Engines.Providers.Domain;
using Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Engines.Providers;
using Kolia.Thumbnail.API.Engines.Providers.Sheets;
using Kolia.Thumbnail.API.Engines.Providers.Socials;
using Kolia.Thumbnail.API.Engines.Providers.Socials.Youtube;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Middlewares;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Security;
using Kolia.Thumbnail.API.Services.AIs;
using Kolia.Thumbnail.API.Services.Projects;
using Kolia.Thumbnail.API.Services.Briefs;
using Kolia.Thumbnail.API.Services.News;
using Kolia.Thumbnail.API.Services.Thumbnails;
using Kolia.Thumbnail.API.Services.DisplayTexts;
using Kolia.Thumbnail.API.Services.ThumbnailGeneration;
using Kolia.Thumbnail.API.Services.VideoTitles;
using Kolia.Thumbnail.API.Services.CompletePackages;
using Kolia.Thumbnail.API.Services.Characters;
using Kolia.Thumbnail.API.Services.GoogleServices;
using Kolia.Thumbnail.API.Interfaces.GoogleServices;
using Kolia.Thumbnail.API.Socials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Kolia.Thumbnail.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddKoliaThumbnailApi(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new LocalDateTimeOffsetJsonConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });
            services.AddSingleton<AuditEntityInterceptor>();

            services.AddDbContext<ThumbnailDbContext>((sp, config) =>
            {
                config.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"));

                config.AddInterceptors(sp
                    .GetRequiredService<AuditEntityInterceptor>());
            });

            services.AddMemoryCache();

            services.AddScoped<IAIProviderService, AIProviderService>();
            services.AddScoped<IAIProviderConfigurationService, AIProviderConfigurationService>();
            services.AddScoped<IAIFunctionConfigService, AIFunctionConfigService>();

            services.AddScoped<ISocialMediaProviderService, SocialMediaProviderService>();

            // Security - ApiKey protection
            services.AddDataProtection();
            services.AddScoped<IApiKeyProtector, ApiKeyProtector>();
            services.AddScoped<AIProviderConfigurationMapper>();

            // AI Engines — đăng ký typed HttpClient kèm interface để ResolveEngine hoạt động
            services.AddHttpClient<GeminiEngine>();
            services.AddHttpClient<GroqEngine>();

            // Đăng ký engine theo interface để AIExecutorService có thể ResolveEngine
            services.AddScoped<IAIEngine>(sp => sp.GetRequiredService<GeminiEngine>());
            services.AddScoped<IChatCapableEngine>(sp => sp.GetRequiredService<GeminiEngine>());
            services.AddScoped<IImageGenerationCapableEngine>(sp => sp.GetRequiredService<GeminiEngine>());
            services.AddScoped<IEmbeddingCapableEngine>(sp => sp.GetRequiredService<GeminiEngine>());
            services.AddScoped<IAIEngine>(sp => sp.GetRequiredService<GroqEngine>());
            services.AddScoped<IChatCapableEngine>(sp => sp.GetRequiredService<GroqEngine>());
            services.AddScoped<ISpeechToTextCapableEngine>(sp => sp.GetRequiredService<GroqEngine>());
            services.AddScoped<ITextToSpeechCapableEngine>(sp => sp.GetRequiredService<GroqEngine>());

            services.AddScoped<IAIExecutorService, AIExecutorService>();

            // Social Media Engines — YoutubeEngine không cần typed HttpClient vì
            // Google.Apis.YouTube.v3 tự quản lý HttpClient nội bộ theo từng credentials
            // (mỗi request OAuth/ApiKey có thể dùng 1 bộ credentials khác nhau).
            services.AddScoped<YoutubeEngine>();
            services.AddScoped<ISocialEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<IChannelManagementCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<IVideoManagementCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<IPlaylistManagementCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<ICommentManagementCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<ILiveStreamingCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());
            services.AddScoped<ISubscriptionManagementCapableEngine>(sp => sp.GetRequiredService<YoutubeEngine>());

            // ── Register Real Domain Engines ──────────────────────────────
            // AI-powered engines sử dụng AIExecutorService với Gemini/Groq thật
            services.AddScoped<IBriefAnalysisEngine, AiBriefAnalysisEngine>();
            services.AddScoped<INewsScoringEngine, AiNewsScoringEngine>();
            services.AddScoped<INewsDeepAnalysisEngine, AiNewsDeepAnalysisEngine>();
            services.AddScoped<IThumbnailAnalysisEngine, AiThumbnailAnalysisEngine>();
            services.AddScoped<IDisplayTextGenerationEngine, AiDisplayTextGenerationEngine>();
            services.AddScoped<IThumbnailImageGenerationEngine, AiThumbnailImageGenerationEngine>();
            services.AddScoped<IVideoTitleGenerationEngine, AiVideoTitleGenerationEngine>();
            services.AddScoped<IContentRelevanceFilterEngine, AiContentRelevanceFilterEngine>();

            // Social engines — RSS with enterprise Polly resilience (retry + circuit breaker + timeout)
            services.AddHttpClient<RealRssNewsSourceEngine>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(12);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 " +
                    "KoliaNewsBot/1.0 (+https://kolia.example/bot)");
                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "application/rss+xml, application/xml, text/xml, */*");
            })
            .AddResilienceHandler("rss-fetch-pipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1),
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests
                        || args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError
                        || args.Outcome.Exception is HttpRequestException or TaskCanceledException)
                });
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(60),
                    MinimumThroughput = 4,
                    BreakDuration = TimeSpan.FromMinutes(5)
                });
                builder.AddTimeout(TimeSpan.FromSeconds(12));
            });
            services.AddScoped<IRssNewsSourceEngine, RealRssNewsSourceEngine>();

            // Named HttpClient for GoogleNewsFallbackFetcher (lightweight — no Polly needed,
            // Google News is reliable enough)
            services.AddHttpClient<GoogleNewsFallbackFetcher>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 KoliaNewsBot/1.0");
            });

            services.AddHttpClient<SitemapFallbackFetcher>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 KoliaNewsBot/1.0");
            });

            // Named HttpClient for AdminNewsSourceService URL validation + test-fetch
            // Uses named client (injected via IHttpClientFactory) to avoid typed-client+interface conflict
            services.AddHttpClient("admin-news-source", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 KoliaNewsBot/1.0 (admin-test)");
            });

            services.AddScoped<IAdminNewsSourceService>(sp =>
            {
                var db = sp.GetRequiredService<Data.Contexts.ThumbnailDbContext>();
                var registry = sp.GetRequiredService<NewsSourceRegistry>();
                var cacheStore = sp.GetRequiredService<ResponseCacheStore>();
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("admin-news-source");
                var logger = sp.GetRequiredService<ILogger<AdminNewsSourceService>>();
                return new AdminNewsSourceService(db, registry, cacheStore, httpClient, logger);
            });
            // Named HttpClient for NewsSourceHealthCheckJob
            services.AddHttpClient("HealthCheck", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(8);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 KoliaNewsBot/1.0 (health-check)");
            });

            // ── RSS Pipeline infrastructure (singletons — in-memory state shared across requests) ──
            services.AddSingleton<DomainRateLimiterRegistry>();
            services.AddSingleton<CircuitBreakerRegistry>();
            services.AddSingleton<ResponseCacheStore>();
            services.AddSingleton<NewsSourceRegistry>();

            // SourceFetchPipeline: Tier 1 RSS fetcher also needs Polly resilience
            services.AddHttpClient<SourceFetchPipeline>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(12);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 " +
                    "KoliaNewsBot/1.0 (+https://kolia.example/bot)");
                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "application/rss+xml, application/xml, text/xml, */*");
            })
            .AddResilienceHandler("rss-tier1-pipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1),
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests
                        || args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError
                        || args.Outcome.Exception is HttpRequestException or TaskCanceledException)
                });
                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

            // GoogleNewsFallbackFetcher & SitemapFallbackFetcher:
            // AddHttpClient already registers as transient — no extra AddScoped needed
            // Gold price fetcher (feature-flagged stub)
            services.AddScoped<IGoldPriceFetcher, DomesticGoldPriceFetcher>();
            services.AddHttpClient<RealYouTubeSearchEngine>();
            services.AddScoped<IYouTubeSearchEngine, RealYouTubeSearchEngine>();

            // Sheet import engine thật (Google Sheet CSV export)
            services.AddHttpClient<GoogleSheetImportEngine>();
            services.AddScoped<ISheetImportEngine, GoogleSheetImportEngine>();

            // ── Register Domain Services ──────────────────────────────────
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IContentBriefService, ContentBriefService>();
            services.AddSingleton<OperationProgressStore>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IThumbnailLibraryService, ThumbnailLibraryService>();
            services.AddScoped<IDisplayTextService, DisplayTextService>();
            services.AddScoped<IThumbnailGenerationService, ThumbnailGenerationService>();
            services.AddScoped<IVideoTitleService, VideoTitleService>();
            services.AddScoped<ICompletePackageService, CompletePackageService>();
            services.AddScoped<ICharacterService, CharacterService>();
            services.AddScoped<ISocialExecutorService, SocialExecutorService>();
            services.AddScoped<IProjectStepGuard, ProjectStepGuard>();
            // IAdminNewsSourceService is registered via factory lambda in the HttpClient section above

            // Google Services
            services.AddScoped<IGoogleServiceAccountService, GoogleServiceAccountService>();
            services.AddScoped<IScheduledImportJobService, ScheduledImportJobService>();
            services.AddScoped<GoogleServiceAccountHelper>();

            // Background Jobs
            services.AddHostedService<ExternalRequestRetryJob>();
            services.AddHostedService<ScheduledImportJobRunner>();
            services.AddHostedService<NewsSourceHealthCheckJob>(); // Phase 2: proactive health checks

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }

        public static IApplicationBuilder UseGlobalException(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}