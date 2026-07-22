using System.Collections.Concurrent;
using Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine crawl tin tức RSS thật — đọc nguồn từ DB (KHÔNG hard-code).
    /// Sử dụng 4-tier fallback pipeline với per-domain rate limiting và circuit breaker.
    /// Implements IRssNewsSourceEngine để giữ nguyên chữ ký interface (không sửa tầng trên).
    /// </summary>
    public class RealRssNewsSourceEngine : IRssNewsSourceEngine
    {
        private readonly HttpClient _httpClient;
        private readonly NewsSourceRegistry _sourceRegistry;
        private readonly SourceFetchPipeline _pipeline;
        private readonly CircuitBreakerRegistry _circuitBreaker;
        private readonly ResponseCacheStore _cache;
        private readonly ILogger<RealRssNewsSourceEngine> _logger;

        public RealRssNewsSourceEngine(
            HttpClient httpClient,
            NewsSourceRegistry sourceRegistry,
            SourceFetchPipeline pipeline,
            CircuitBreakerRegistry circuitBreaker,
            ResponseCacheStore cache,
            ILogger<RealRssNewsSourceEngine> logger)
        {
            _httpClient = httpClient;
            _sourceRegistry = sourceRegistry;
            _pipeline = pipeline;
            _circuitBreaker = circuitBreaker;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords, CMarketScope marketScope,
            int timeRangeDays, int maxCount, CancellationToken ct = default)
        {
            var keywordList = keywords.ToList();
            if (keywordList.Count == 0) return [];

            // 1. Load sources from DB via registry (cached 5 min)
            var sources = await _sourceRegistry.GetActiveSourcesAsync(marketScope, ct);
            if (sources.Count == 0)
            {
                _logger.LogWarning("CrawlAsync: no active sources found for scope={Scope}.", marketScope);
                return [];
            }

            var cutoffTime = DateTimeOffset.UtcNow.AddDays(-timeRangeDays);
            var results = new ConcurrentBag<CrawledNewsItem>();

            // 2. Parallel fetch with global concurrency cap = 6, per-domain=1 via DomainRateLimiterRegistry
            await Parallel.ForEachAsync(sources, new ParallelOptions
            {
                MaxDegreeOfParallelism = 6,
                CancellationToken = ct
            },
            async (source, token) =>
            {
                // Circuit breaker fast-path: skip Open domains, serve stale cache
                if (_circuitBreaker.IsOpen(source.Domain))
                {
                    var cached = await _cache.TryGetStaleAsync(source.Domain, token);
                    if (cached is { Count: > 0 })
                    {
                        foreach (var c in cached) results.Add(c);
                        _logger.LogDebug(
                            "CrawlAsync: circuit open for {Domain} — served {Count} cached items.",
                            source.Domain, cached.Count);
                    }
                    return;
                }

                var items = await _pipeline.FetchWithFallbackAsync(
                    source, keywordList, cutoffTime, maxCount, token);

                if (items.Count > 0)
                {
                    _circuitBreaker.RecordSuccess(source.Domain);
                    await _cache.SetAsync(source.Domain, keywordList, items, token);
                    foreach (var i in items) results.Add(i);
                }
                else
                {
                    // Only count as failure when ALL tiers + cache returned nothing
                    _circuitBreaker.RecordFailure(source.Domain);
                    _logger.LogDebug("CrawlAsync: all tiers failed for {Domain}.", source.Domain);
                }
            });

            // 3. Return deduplicated results ordered by publication time
            return results
                .DistinctBy(i => i.SourceUrl)
                .OrderByDescending(i => i.PublishedTime)
                .Take(maxCount)
                .ToList();
        }

        /// <summary>
        /// Fetch and parse the content of a single article URL (used for manual import).
        /// Kept close to original implementation — only the HttpClient reference changes.
        /// </summary>
        public async Task<CrawledNewsItem?> FetchSingleAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync(ct);

                var title = ExtractTitleFromHtml(html) ?? "Bài viết từ: " + url;
                var description = ExtractDescriptionFromHtml(html) ?? html[..Math.Min(500, html.Length)];

                return new CrawledNewsItem(
                    Title: title,
                    SourceName: "Báo ngoài",
                    SourceUrl: url,
                    MarketType: CMarketScope.Domestic,
                    PublishedTime: DateTimeOffset.UtcNow,
                    SummaryRaw: StripHtmlTags(description));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FetchSingleAsync failed for: {Url}", url);
                return null;
            }
        }

        // ── HTML helpers (unchanged from original) ────────────────────

        private static string ExtractTitleFromHtml(string html)
        {
            var titleStart = html.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
            if (titleStart < 0) return string.Empty;
            titleStart += 7;
            var titleEnd = html.IndexOf("</title>", titleStart, StringComparison.OrdinalIgnoreCase);
            if (titleEnd < 0) return string.Empty;
            return html[titleStart..titleEnd].Trim();
        }

        private static string ExtractDescriptionFromHtml(string html)
        {
            var patterns = new[]
            {
                "name=\"description\"", "name='description'",
                "property=\"og:description\"", "property='og:description'"
            };
            foreach (var pattern in patterns)
            {
                var idx = html.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) continue;
                var contentIdx = html.IndexOf("content=\"", idx, StringComparison.OrdinalIgnoreCase);
                if (contentIdx < 0) contentIdx = html.IndexOf("content='", idx, StringComparison.OrdinalIgnoreCase);
                if (contentIdx < 0) continue;
                contentIdx += 9;
                var endIdx = html.IndexOf('"', contentIdx);
                if (endIdx < 0) endIdx = html.IndexOf('\'', contentIdx);
                if (endIdx < 0) continue;
                return html[contentIdx..endIdx];
            }
            return string.Empty;
        }

        private static string StripHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = System.Text.RegularExpressions.Regex.Replace(input, "<[^>]*>", " ");
            return System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Trim();
        }
    }
}
