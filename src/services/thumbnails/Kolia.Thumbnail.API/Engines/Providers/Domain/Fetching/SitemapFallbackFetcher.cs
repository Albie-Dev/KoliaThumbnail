using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using System.Xml.Linq;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Tier 3 fallback: discovers news URLs by parsing a site's sitemap.xml or
    /// news-sitemap.xml when no usable RSS feed is available.
    /// Strategy:
    ///   1. Try /sitemap.xml  (standard location)
    ///   2. Try /news-sitemap.xml  (Google News sitemap format)
    ///   3. Try /sitemap_news.xml  (common alternate name)
    /// Filters URLs by keyword match in the &lt;loc&gt; or &lt;news:title&gt; elements.
    /// </summary>
    public sealed class SitemapFallbackFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SitemapFallbackFetcher> _logger;

        private static readonly string[] SitemapPaths =
        [
            "/news-sitemap.xml",
            "/sitemap_news.xml",
            "/sitemap.xml"
        ];

        public SitemapFallbackFetcher(HttpClient httpClient, ILogger<SitemapFallbackFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CrawledNewsItem>> FetchAsync(
            NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            CancellationToken ct)
        {
            // Derive base URL from RssOrFeedUrl or Domain
            var baseUrl = DeriveBaseUrl(source);

            foreach (var path in SitemapPaths)
            {
                var sitemapUrl = baseUrl.TrimEnd('/') + path;
                try
                {
                    var response = await _httpClient.GetAsync(sitemapUrl, ct);
                    if (!response.IsSuccessStatusCode) continue;

                    var xml = await response.Content.ReadAsStringAsync(ct);
                    var items = ParseSitemap(xml, source, keywords, cutoff, maxCount);
                    if (items.Count > 0)
                    {
                        _logger.LogInformation(
                            "SitemapFallbackFetcher: found {Count} items from {Url}.",
                            items.Count, sitemapUrl);
                        return items;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "SitemapFallbackFetcher: {Url} failed.", sitemapUrl);
                }
            }

            return [];
        }

        private static List<CrawledNewsItem> ParseSitemap(
            string xml, NewsSourceEntity source, List<string> keywords,
            DateTimeOffset cutoff, int maxCount)
        {
            var items = new List<CrawledNewsItem>();
            try
            {
                var doc = XDocument.Parse(xml);
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                XNamespace newsNs = "http://www.google.com/schemas/sitemap-news/0.9";

                foreach (var urlEl in doc.Descendants(ns + "url"))
                {
                    if (items.Count >= maxCount) break;

                    var loc = urlEl.Element(ns + "loc")?.Value ?? string.Empty;
                    if (string.IsNullOrEmpty(loc)) continue;

                    // Try to get publication date from news sitemap or lastmod
                    DateTimeOffset pubDate = DateTimeOffset.UtcNow;
                    var lastmodStr = urlEl.Element(ns + "lastmod")?.Value
                        ?? urlEl.Descendants(newsNs + "publication_date").FirstOrDefault()?.Value;
                    if (lastmodStr != null && DateTimeOffset.TryParse(lastmodStr, out var parsedDate))
                        pubDate = parsedDate;

                    if (pubDate < cutoff) continue;

                    // Get title from news namespace if available
                    var title = urlEl.Descendants(newsNs + "title").FirstOrDefault()?.Value
                        ?? loc;  // fallback: use URL as title

                    // Keyword filter — match in URL or title
                    var combined = $"{title} {loc}";
                    if (keywords.Count > 0 &&
                        !keywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    items.Add(new CrawledNewsItem(
                        Title: title,
                        SourceName: source.Name,
                        SourceUrl: loc,
                        MarketType: source.Region,
                        PublishedTime: pubDate,
                        SummaryRaw: string.Empty));
                }
            }
            catch
            {
                // Malformed XML — return whatever was collected
            }
            return items;
        }

        private static string DeriveBaseUrl(NewsSourceEntity source)
        {
            // Prefer to derive from RssOrFeedUrl when it contains a full URL
            if (Uri.TryCreate(source.RssOrFeedUrl, UriKind.Absolute, out var feedUri))
                return $"{feedUri.Scheme}://{feedUri.Host}";

            return $"https://{source.Domain}";
        }
    }
}
