using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using System.Xml.Linq;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Tier 2 fallback: fetches news from Google News RSS using a site-restricted query.
    /// URL pattern: https://news.google.com/rss/search?q=site:DOMAIN+KEYWORD&amp;hl=en&amp;gl=US
    /// Does NOT count as hitting the original domain — uses Google as intermediary.
    /// </summary>
    public sealed class GoogleNewsFallbackFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleNewsFallbackFetcher> _logger;

        public GoogleNewsFallbackFetcher(HttpClient httpClient, ILogger<GoogleNewsFallbackFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Fetches news items from Google News RSS filtered to <paramref name="domain"/>
        /// using site-restricted search queries for each keyword.
        /// </summary>
        public async Task<List<CrawledNewsItem>> FetchAsync(
            string domain,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            CMarketScope marketType,
            CancellationToken ct)
        {
            var results = new List<CrawledNewsItem>();

            foreach (var keyword in keywords.Take(3)) // take top 3 keywords
            {
                if (results.Count >= maxCount) break;
                try
                {
                    var q = Uri.EscapeDataString($"site:{domain} {keyword}");
                    var url = $"https://news.google.com/rss/search?q={q}&hl=en&gl=US&ceid=US:en";

                    var response = await _httpClient.GetAsync(url, ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("GoogleNews RSS returned {Status} for domain={Domain} keyword={Keyword}.",
                            response.StatusCode, domain, keyword);
                        continue;
                    }

                    var xml = await response.Content.ReadAsStringAsync(ct);
                    var items = ParseRss(xml, keywords, cutoff, maxCount - results.Count, marketType);
                    results.AddRange(items);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GoogleNewsFallbackFetcher failed for domain={Domain} keyword={Keyword}.",
                        domain, keyword);
                }
            }

            return results.DistinctBy(i => i.SourceUrl).ToList();
        }

        private static List<CrawledNewsItem> ParseRss(
            string xml, List<string> keywords, DateTimeOffset cutoff, int maxCount, CMarketScope marketType)
        {
            var items = new List<CrawledNewsItem>();
            try
            {
                var doc = XDocument.Parse(xml);
                var channel = doc.Root?.Element("channel");
                if (channel == null) return items;

                foreach (var item in channel.Elements("item"))
                {
                    if (items.Count >= maxCount) break;

                    var title = item.Element("title")?.Value ?? string.Empty;
                    var link = item.Element("link")?.Value ?? string.Empty;
                    var description = item.Element("description")?.Value ?? string.Empty;
                    var pubDateStr = item.Element("pubDate")?.Value;

                    if (!DateTimeOffset.TryParse(pubDateStr, out var pubDate))
                        pubDate = DateTimeOffset.UtcNow;

                    if (pubDate < cutoff) continue;

                    // Google News RSS uses the source name in <source> element
                    var sourceName = item.Element("source")?.Value ?? "Google News";

                    items.Add(new CrawledNewsItem(
                        Title: title,
                        SourceName: sourceName,
                        SourceUrl: link,
                        MarketType: marketType,
                        PublishedTime: pubDate,
                        SummaryRaw: StripHtml(description)));
                }
            }
            catch
            {
                // Malformed XML — return whatever we have
            }
            return items;
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = System.Text.RegularExpressions.Regex.Replace(input, "<[^>]*>", " ");
            return System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Trim();
        }
    }
}
