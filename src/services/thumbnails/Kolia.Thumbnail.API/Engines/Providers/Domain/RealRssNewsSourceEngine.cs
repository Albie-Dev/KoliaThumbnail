using System.Collections.Concurrent;
using Kolia.Thumbnail.API.Engines.AI;
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
        private readonly IKeywordTranslationEngine _keywordTranslator;
        private readonly ILogger<RealRssNewsSourceEngine> _logger;

        public RealRssNewsSourceEngine(
            HttpClient httpClient,
            NewsSourceRegistry sourceRegistry,
            SourceFetchPipeline pipeline,
            CircuitBreakerRegistry circuitBreaker,
            ResponseCacheStore cache,
            IKeywordTranslationEngine keywordTranslator,
            ILogger<RealRssNewsSourceEngine> logger)
        {
            _httpClient = httpClient;
            _sourceRegistry = sourceRegistry;
            _pipeline = pipeline;
            _circuitBreaker = circuitBreaker;
            _cache = cache;
            _keywordTranslator = keywordTranslator;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords, CMarketScope marketScope,
            int timeRangeDays, int maxCount,
            Action<NewsSourceSearchLog>? onSourceSearched = null,
            CancellationToken ct = default)
        {
            var keywordList = keywords.ToList();
            if (keywordList.Count == 0) return [];

            var translatedKeywords = await _keywordTranslator.TranslateAndExpandAsync(keywordList, ct);

            var sources = await _sourceRegistry.GetActiveSourcesAsync(marketScope, ct);
            if (sources.Count == 0)
            {
                _logger.LogWarning("CrawlAsync: no active sources found for scope={Scope}.", marketScope);
                return [];
            }

            var cutoffTime = DateTimeOffset.UtcNow.AddDays(-timeRangeDays);
            var results = new ConcurrentBag<CrawledNewsItem>();

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

                        onSourceSearched?.Invoke(new NewsSourceSearchLog(
                            source.Name, "(cache — circuit open)", cached.Count,
                            Success: true, ServedFromCache: true));
                    }
                    else
                    {
                        onSourceSearched?.Invoke(new NewsSourceSearchLog(
                            source.Name, "(circuit open, no cache)", 0,
                            Success: false, ErrorMessage: "Circuit breaker đang mở, không có cache dự phòng"));
                    }
                    return;
                }

                var sourceKeywords = source.Region switch
                {
                    CMarketScope.International => translatedKeywords.EnglishKeywords.Concat(translatedKeywords.OriginalKeywords).Distinct().ToList(),
                    CMarketScope.Domestic => translatedKeywords.VietnameseKeywords.Concat(translatedKeywords.OriginalKeywords).Distinct().ToList(),
                    _ => translatedKeywords.CombinedKeywords.ToList()
                };
                if (sourceKeywords.Count == 0) sourceKeywords = keywordList;

                var keywordsJoined = string.Join(", ", sourceKeywords);

                try
                {
                    var items = await _pipeline.FetchWithFallbackAsync(
                        source, sourceKeywords, cutoffTime, maxCount, token);

                    if (items.Count > 0)
                    {
                        _circuitBreaker.RecordSuccess(source.Domain);
                        await _cache.SetAsync(source.Domain, keywordList, items, token);
                        foreach (var i in items) results.Add(i);

                        onSourceSearched?.Invoke(new NewsSourceSearchLog(
                            source.Name, keywordsJoined, items.Count, Success: true));
                    }
                    else
                    {
                        _circuitBreaker.RecordFailure(source.Domain);
                        _logger.LogDebug("CrawlAsync: all tiers failed for {Domain}.", source.Domain);

                        onSourceSearched?.Invoke(new NewsSourceSearchLog(
                            source.Name, keywordsJoined, 0,
                            Success: false, ErrorMessage: "Không có tin nào sau khi thử hết các tier fallback"));
                    }
                }
                catch (Exception ex)
                {
                    _circuitBreaker.RecordFailure(source.Domain);
                    _logger.LogWarning(ex, "CrawlAsync: exception fetching {Domain}.", source.Domain);

                    onSourceSearched?.Invoke(new NewsSourceSearchLog(
                        source.Name, keywordsJoined, 0,
                        Success: false, ErrorMessage: ex.Message));
                }
            });

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

        public async Task<CMarketScope> DetectScopeForUrlAsync(string url, CancellationToken ct = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return CMarketScope.Domestic; // không parse được url → an toàn mặc định trong nước

            var host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? uri.Host[4..]
                : uri.Host;

            // 1. Ưu tiên tra trong bảng NewsSource đã cấu hình sẵn region cho từng domain
            var allSources = await _sourceRegistry.GetActiveSourcesAsync(CMarketScope.International, ct);
            allSources = allSources.Concat(await _sourceRegistry.GetActiveSourcesAsync(CMarketScope.Domestic, ct)).ToList();

            var matched = allSources.FirstOrDefault(s =>
                host.Equals(s.Domain, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + s.Domain, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                _logger.LogInformation("DetectScopeForUrlAsync: {Host} khớp nguồn '{Source}' → {Scope}",
                    host, matched.Name, matched.Region);
                return matched.Region;
            }

            // 2. Fallback heuristic theo TLD khi domain chưa có trong danh sách cấu hình
            var isLikelyVietnamese = host.EndsWith(".vn", StringComparison.OrdinalIgnoreCase);

            var scope = isLikelyVietnamese ? CMarketScope.Domestic : CMarketScope.International;
            _logger.LogInformation(
                "DetectScopeForUrlAsync: {Host} không có trong registry — fallback theo TLD → {Scope}",
                host, scope);
            return scope;
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
