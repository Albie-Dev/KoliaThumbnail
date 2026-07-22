using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// IMemoryCache wrapper for crawled news items.
    /// Key = SHA256(domain + sorted-keywords).
    /// Fresh TTL: 10 minutes.  Stale entries are kept for up to 24 hours so that
    /// Tier 4 (cache fallback) can serve them even after the TTL expires.
    /// </summary>
    public sealed class ResponseCacheStore
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResponseCacheStore> _logger;

        private static readonly TimeSpan FreshTtl = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan StaleTtl = TimeSpan.FromHours(24);

        public ResponseCacheStore(IMemoryCache cache, ILogger<ResponseCacheStore> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Tries to get a fresh (within TTL) cache entry.
        /// Returns null when the entry is missing or stale.
        /// </summary>
        public Task<List<CrawledNewsItem>?> TryGetAsync(
            string domain, List<string> keywords, CancellationToken _)
        {
            var freshKey = FreshKey(domain, keywords);
            _cache.TryGetValue(freshKey, out List<CrawledNewsItem>? items);
            return Task.FromResult(items);
        }

        /// <summary>
        /// Tries to get a stale (beyond TTL but within 24 h) cache entry.
        /// Used as Tier 4 fallback when all live tiers fail.
        /// </summary>
        public Task<List<CrawledNewsItem>?> TryGetStaleAsync(string domain, CancellationToken _)
        {
            var staleKey = StaleKey(domain);
            _cache.TryGetValue(staleKey, out List<CrawledNewsItem>? items);
            if (items != null)
                _logger.LogInformation("ResponseCacheStore: serving stale cache for {Domain}.", domain);
            return Task.FromResult(items);
        }

        /// <summary>
        /// Stores fetched items under both a fresh key (10 min TTL) and a stale key (24 h TTL).
        /// The stale key is always updated on successful fetch so Tier 4 has the most recent data.
        /// </summary>
        public Task SetAsync(
            string domain, List<string> keywords, List<CrawledNewsItem> items, CancellationToken _)
        {
            _cache.Set(FreshKey(domain, keywords), items,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = FreshTtl });

            _cache.Set(StaleKey(domain), items,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = StaleTtl });

            return Task.CompletedTask;
        }

        /// <summary>Invalidates cached items for a domain (called when admin updates source URL).</summary>
        public void Invalidate(string domain)
        {
            _logger.LogInformation("ResponseCacheStore: invalidating cache for {Domain}.", domain);
            _cache.Remove(StaleKey(domain));
            // Fresh keys include keywords so we can't remove them individually without knowing the keywords;
            // they will expire naturally within FreshTtl (≤10 min). This is acceptable.
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static string FreshKey(string domain, List<string> keywords)
        {
            var sorted = string.Join("|", keywords.OrderBy(k => k, StringComparer.OrdinalIgnoreCase));
            var raw = $"rss:fresh:{domain}:{sorted}";
            return StableHash(raw);
        }

        private static string StaleKey(string domain) => $"rss:stale:{domain}";

        private static string StableHash(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToHexString(hash)[..16];
        }
    }
}
