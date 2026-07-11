using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Interceptors;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Middlewares;
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
                    options.JsonSerializerOptions.Converters.Add(new LocalDateTimeOffsetJsonConverter());
                });
            services.AddDbContext<ThumbnailDbContext>(config =>
            {
                config.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"));

                config.AddInterceptors(services.BuildServiceProvider()
                    .GetRequiredService<AuditEntityInterceptor>());
            });
            services.AddSingleton<AuditEntityInterceptor>();


            services.AddScoped<IAIProviderService, AIProviderService>();

            return services;
        }

        public static IApplicationBuilder UseGlobalException(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}