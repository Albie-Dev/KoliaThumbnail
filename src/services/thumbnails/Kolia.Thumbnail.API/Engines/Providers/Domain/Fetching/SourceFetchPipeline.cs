using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using System.Net;
using System.Xml.Linq;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Implements the 4-tier fallback fetch pipeline for a single NewsSourceEntity:
    ///   Tier 1: RSS/Atom direct
    ///   Tier 2: Google News RSS site-restricted
    ///   Tier 3: Sitemap.xml / news-sitemap.xml
    ///   Tier 4: Stale cache (last known-good data)
    ///
    /// Rate-limiting and circuit breaking are applied BEFORE any network call.
    /// Never throws — returns empty list only when all tiers + cache fail.
    /// </summary>
    public sealed class SourceFetchPipeline : INewsSourceOrchestrator
    {
        private readonly HttpClient _httpClient;
        private readonly DomainRateLimiterRegistry _rateLimiter;
        private readonly GoogleNewsFallbackFetcher _googleNewsFetcher;
        private readonly SitemapFallbackFetcher _sitemapFetcher;
        private readonly IRestApiFetcher _restApiFetcher;
        private readonly ResponseCacheStore _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SourceFetchPipeline> _logger;

        public SourceFetchPipeline(
            HttpClient httpClient,
            DomainRateLimiterRegistry rateLimiter,
            GoogleNewsFallbackFetcher googleNewsFetcher,
            SitemapFallbackFetcher sitemapFetcher,
            IRestApiFetcher restApiFetcher,
            ResponseCacheStore cache,
            IServiceScopeFactory scopeFactory,
            ILogger<SourceFetchPipeline> logger)
        {
            _httpClient = httpClient;
            _rateLimiter = rateLimiter;
            _googleNewsFetcher = googleNewsFetcher;
            _sitemapFetcher = sitemapFetcher;
            _restApiFetcher = restApiFetcher;
            _cache = cache;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<List<CrawledNewsItem>> FetchWithFallbackAsync(
            NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            CancellationToken ct)
        {
            // Rate limiter: enforce per-domain concurrency=1 + inter-request delay
            using var rateLimitHandle = await _rateLimiter.AcquireAsync(source.Domain, ct);

            // TIER 0: REST API — standalone mode, skip all RSS/Google/Sitemap tiers
            if (source.FetchMode is CSourceFetchMode.RestApi)
            {
                var restResults = await _restApiFetcher.FetchAsync(source, keywords, cutoff, maxCount, ct);
                if (restResults.Count > 0)
                {
                    await MarkFetchedOkAsync(source);
                    return restResults;
                }

                // Fallback: stale cache
                var staleRest = await _cache.TryGetStaleAsync(source.Domain, ct);
                await MarkFetchFailedAsync(source);
                return staleRest ?? [];
            }

            // TIER 1: RSS/Atom direct
            if (source.FetchMode is CSourceFetchMode.RssDirect or CSourceFetchMode.GoogleNewsFallback
                or CSourceFetchMode.SitemapFallback)
            {
                var tier1 = await TryFetchRssAsync(source, keywords, cutoff, maxCount, ct);
                if (tier1.Count > 0)
                {
                    await MarkFetchedOkAsync(source);
                    return tier1;
                }
            }

            // TIER 2: Google News RSS site-restricted
            if (source.FetchMode is CSourceFetchMode.GoogleNewsFallback
                or CSourceFetchMode.GoogleNewsSiteRestricted
                or CSourceFetchMode.SitemapOrGoogleNews)
            {
                var tier2 = await _googleNewsFetcher.FetchAsync(
                    source.Domain, keywords, cutoff, maxCount, source.Region, ct);
                if (tier2.Count > 0)
                {
                    await MarkFetchedOkAsync(source);
                    return tier2;
                }
            }

            // TIER 3: Sitemap.xml / news-sitemap.xml
            if (source.FetchMode is CSourceFetchMode.SitemapFallback
                or CSourceFetchMode.SitemapOrGoogleNews
                or CSourceFetchMode.GoogleNewsSiteRestricted
                or CSourceFetchMode.Custom)
            {
                var tier3 = await _sitemapFetcher.FetchAsync(source, keywords, cutoff, maxCount, ct);
                if (tier3.Count > 0)
                {
                    await MarkFetchedOkAsync(source);
                    return tier3;
                }
            }

            // TIER 4: Stale cache (degrade gracefully — do NOT update failure count for cache hits)
            var tier4 = await _cache.TryGetStaleAsync(source.Domain, ct);
            await MarkFetchFailedAsync(source);
            return tier4 ?? [];
        }

        // ── Tier 1: RSS/Atom fetch ────────────────────────────────────

        private async Task<List<CrawledNewsItem>> TryFetchRssAsync(
            NewsSourceEntity source, List<string> keywords,
            DateTimeOffset cutoff, int maxCount, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(source.RssOrFeedUrl)) return [];

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, source.RssOrFeedUrl);

                // Conditional GET: respect ETag / Last-Modified to reduce bandwidth + 304s
                if (!string.IsNullOrEmpty(source.LastEtag))
                    request.Headers.TryAddWithoutValidation("If-None-Match", source.LastEtag);
                if (!string.IsNullOrEmpty(source.LastModifiedHeader))
                    request.Headers.TryAddWithoutValidation("If-Modified-Since", source.LastModifiedHeader);

                var response = await _httpClient.SendAsync(request, ct);

                // 304 Not Modified → feed unchanged, return empty (caller falls through to cache)
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    _logger.LogDebug("RSS 304 Not Modified for {Domain}.", source.Domain);
                    return [];
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    _rateLimiter.RecordRateLimited(source.Domain, retryAfter);
                    return [];
                }

                response.EnsureSuccessStatusCode();

                // Persist ETag / Last-Modified for next conditional GET (fire-and-forget, best effort)
                var etag = response.Headers.ETag?.Tag;
                var lastModified = response.Content.Headers.LastModified?.ToString("R");
                _ = UpdateCacheHeadersAsync(source.Id, etag, lastModified);

                var xml = await response.Content.ReadAsStringAsync(ct);
                return ParseFeed(xml, source, keywords, cutoff, maxCount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tier1 RSS fetch failed for {Domain}: {Message}",
                    source.Domain, ex.Message);
                return [];
            }
        }

        private static List<CrawledNewsItem> ParseFeed(
            string xml, NewsSourceEntity source, List<string> keywords,
            DateTimeOffset cutoff, int maxCount)
        {
            var items = new List<CrawledNewsItem>();
            try
            {
                var doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root == null) return items;

                bool isAtom = root.Name.LocalName == "feed";
                XName entryName, titleName, linkName, summaryName, pubDateName;

                if (isAtom)
                {
                    var ns = root.Name.Namespace;
                    entryName = ns + "entry";
                    titleName = ns + "title";
                    linkName = ns + "link";
                    summaryName = ns + "content";
                    pubDateName = ns + "published";
                }
                else
                {
                    entryName = "item";
                    titleName = "title";
                    linkName = "link";
                    summaryName = "description";
                    pubDateName = "pubDate";
                }

                var sourceName = isAtom
                    ? root.Elements(root.Name.Namespace + "title").FirstOrDefault()?.Value ?? source.Name
                    : doc.Root?.Element("channel")?.Element("title")?.Value ?? source.Name;

                var entries = isAtom
                    ? doc.Descendants(entryName).ToList()
                    : doc.Root?.Element("channel")?.Elements(entryName).ToList() ?? [];

                foreach (var entry in entries)
                {
                    if (items.Count >= maxCount) break;

                    var title = entry.Element(titleName)?.Value ?? string.Empty;
                    var summary = entry.Element(summaryName)?.Value ?? string.Empty;

                    string link;
                    if (isAtom)
                    {
                        var linkEl = entry.Element(linkName);
                        link = linkEl?.Attribute("href")?.Value ?? linkEl?.Value ?? string.Empty;
                    }
                    else
                    {
                        link = entry.Element(linkName)?.Value ?? string.Empty;
                    }

                    if (!DateTimeOffset.TryParse(entry.Element(pubDateName)?.Value, out var pubDate))
                        pubDate = DateTimeOffset.UtcNow;

                    if (pubDate < cutoff) continue;

                    // Keyword filter (same fuzzy 60% threshold as original code)
                    if (keywords.Count > 0 && !MatchesAnyKeyword(title, summary, keywords)) continue;

                    items.Add(new CrawledNewsItem(
                        Title: title,
                        SourceName: sourceName,
                        SourceUrl: link,
                        MarketType: source.Region,
                        PublishedTime: pubDate,
                        SummaryRaw: StripHtml(summary)));
                }
            }
            catch
            {
                // Malformed XML — return whatever we have
            }
            return items;
        }

        private static bool MatchesAnyKeyword(string title, string summary, List<string> keywords)
        {
            return keywords.Any(k => MatchesKeyword(title, summary, k));
        }

        private static bool MatchesKeyword(string title, string summary, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return false;
            var words = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= 1)
                return title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || summary.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            const double MatchThreshold = 0.6;
            var matchCount = words.Count(w =>
                title.Contains(w, StringComparison.OrdinalIgnoreCase) ||
                summary.Contains(w, StringComparison.OrdinalIgnoreCase));

            return matchCount >= (int)Math.Ceiling(words.Length * MatchThreshold);
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var r = System.Text.RegularExpressions.Regex.Replace(input, "<[^>]*>", " ");
            return System.Text.RegularExpressions.Regex.Replace(r, @"\s+", " ").Trim();
        }

        // ── DB update helpers (fire-and-forget) ───────────────────────

        private async Task UpdateCacheHeadersAsync(Guid sourceId, string? etag, string? lastModified)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                var entity = await db.NewsSources.FindAsync(sourceId);
                if (entity == null) return;
                entity.LastEtag = etag;
                entity.LastModifiedHeader = lastModified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to persist ETag/LastModified for source {Id}.", sourceId);
            }
        }

        private async Task MarkFetchedOkAsync(NewsSourceEntity source)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                var entity = await db.NewsSources.FindAsync(source.Id);
                if (entity == null) return;
                entity.LastFetchedAt = DateTimeOffset.UtcNow;
                entity.ConsecutiveFailureCount = 0;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to update LastFetchedAt for source {Id}.", source.Id);
            }
        }

        private async Task MarkFetchFailedAsync(NewsSourceEntity source)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                var entity = await db.NewsSources.FindAsync(source.Id);
                if (entity == null) return;
                entity.LastFailedAt = DateTimeOffset.UtcNow;
                entity.ConsecutiveFailureCount++;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to update ConsecutiveFailureCount for source {Id}.", source.Id);
            }
        }
    }
}
