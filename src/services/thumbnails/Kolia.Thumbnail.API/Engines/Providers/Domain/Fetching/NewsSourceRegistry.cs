using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Reads the active NewsSourceEntity list from DB, cached in IMemoryCache for 5 minutes.
    /// Cache is immediately invalidated when AdminNewsSourceService modifies any source.
    /// </summary>
    public sealed class NewsSourceRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NewsSourceRegistry> _logger;

        private const string CacheKey = "NewsSourceRegistry:ActiveSources";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public NewsSourceRegistry(
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache,
            ILogger<NewsSourceRegistry> logger)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Returns all active (IsTrusted=true, not deleted) sources filtered by market scope.
        /// Results are cached for 5 minutes to avoid per-request DB queries.
        /// </summary>
        public async Task<List<NewsSourceEntity>> GetActiveSourcesAsync(
            CMarketScope marketScope, CancellationToken ct)
        {
            if (!_cache.TryGetValue(CacheKey, out List<NewsSourceEntity>? allSources) || allSources == null)
            {
                allSources = await LoadFromDbAsync(ct);
                _cache.Set(CacheKey, allSources,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
                _logger.LogDebug("NewsSourceRegistry: loaded {Count} sources from DB.", allSources.Count);
            }

            return marketScope switch
            {
                CMarketScope.Domestic => allSources
                    .Where(s => s.Region == CMarketScope.Domestic)
                    .OrderBy(s => s.Priority)
                    .ToList(),

                CMarketScope.International => allSources
                    .Where(s => s.Region == CMarketScope.International)
                    .OrderBy(s => s.Priority)
                    .ToList(),

                CMarketScope.Both => allSources
                    .OrderBy(s => s.Priority)
                    .ToList(),

                _ => allSources.OrderBy(s => s.Priority).ToList()
            };
        }

        /// <summary>
        /// Immediately invalidates the in-memory cache.
        /// Call this after any admin Create/Update/Delete/Toggle operation so the
        /// engine picks up the new data on the next research request.
        /// </summary>
        public void InvalidateCache()
        {
            _logger.LogInformation("NewsSourceRegistry: cache invalidated by admin operation.");
            _cache.Remove(CacheKey);
        }

        private async Task<List<NewsSourceEntity>> LoadFromDbAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();

            return await db.NewsSources
                .Where(s => s.IsTrusted && !s.IsDeleted)
                .OrderBy(s => s.Priority)
                .ToListAsync(ct);
        }
    }
}
