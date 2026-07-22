using Kolia.Thumbnail.API.Engines.Social;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Orchestrates fetching from multiple news sources with 4-tier fallback pipeline.
    /// Implemented by RealRssNewsSourceEngine; extracted here so tests can mock it.
    /// </summary>
    public interface INewsSourceOrchestrator
    {
        /// <summary>
        /// Fetches news items from a single source using the 4-tier fallback pipeline:
        /// Tier 1 = RSS direct, Tier 2 = Google News site-restricted,
        /// Tier 3 = Sitemap.xml, Tier 4 = stale cache.
        /// Never throws — returns empty list when all tiers + cache fail.
        /// </summary>
        Task<List<CrawledNewsItem>> FetchWithFallbackAsync(
            Data.Entities.News.NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            CancellationToken ct);
    }
}
