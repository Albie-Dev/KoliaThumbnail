using System.Text.Json;
using FluentValidation;
using FluentValidation.AspNetCore;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Interceptors;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Engines.Providers;
using Kolia.Thumbnail.API.Engines.Providers.Socials;
using Kolia.Thumbnail.API.Engines.Providers.Socials.Youtube;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Middlewares;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Projects;
using Kolia.Thumbnail.API.Security;
using Kolia.Thumbnail.API.Services.AIs;
using Kolia.Thumbnail.API.Socials;
using Microsoft.EntityFrameworkCore;

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


            services.AddScoped<IAIProviderService, AIProviderService>();
            services.AddScoped<IAIProviderConfigurationService, AIProviderConfigurationService>();
            services.AddScoped<IProjectService, ProjectService>();

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