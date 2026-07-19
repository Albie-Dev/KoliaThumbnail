using System.Xml.Linq;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine crawl tin tức RSS thật — đọc RSS feeds từ các nguồn tin.
    /// Hỗ trợ tìm kiếm theo keyword qua Google News RSS và các nguồn RSS khác.
    /// </summary>
    public class RealRssNewsSourceEngine : IRssNewsSourceEngine
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RealRssNewsSourceEngine> _logger;

        // Cấu hình nguồn RSS theo thị trường
        private static readonly Dictionary<CMarketScope, string[]> RssFeeds = new()
        {
            [CMarketScope.Domestic] = new[]
            {
                "https://vnexpress.net/rss/tin-moi-nhat.rss",
                "https://tuoitre.vn/rss/tin-moi-nhat.rss",
                "https://thanhnien.vn/rss/home.rss"
            },
            [CMarketScope.International] = new[]
            {
                "https://feeds.bbci.co.uk/news/rss.xml",
                "https://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml",
                "https://feeds.reuters.com/reuters/topNews"
            },
            [CMarketScope.Both] = new[]
            {
                "https://vnexpress.net/rss/tin-moi-nhat.rss",
                "https://feeds.bbci.co.uk/news/rss.xml",
                "https://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml"
            }
        };

        public RealRssNewsSourceEngine(HttpClient httpClient, ILogger<RealRssNewsSourceEngine> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords, CMarketScope marketScope,
            int timeRangeDays, int maxCount, CancellationToken ct = default)
        {
            var keywordList = keywords.ToList();
            if (keywordList.Count == 0) return [];

            var cutoffTime = DateTimeOffset.UtcNow.AddDays(-timeRangeDays);
            var allItems = new List<CrawledNewsItem>();

            // Lấy RSS feeds theo market scope
            var feeds = RssFeeds.GetValueOrDefault(marketScope, RssFeeds[CMarketScope.Domestic]);

            foreach (var feedUrl in feeds)
            {
                if (allItems.Count >= maxCount) break;

                try
                {
                    var items = await FetchFeedAsync(feedUrl, keywordList, cutoffTime, maxCount - allItems.Count, ct);
                    allItems.AddRange(items);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch RSS feed: {Url}", feedUrl);
                }
            }

            // Nếu không có kết quả từ RSS, fallback sang Google News RSS search
            if (allItems.Count == 0 && keywordList.Count > 0)
            {
                foreach (var keyword in keywordList.Take(3))
                {
                    try
                    {
                        var googleNewsUrl = $"https://news.google.com/rss/search?q={Uri.EscapeDataString(keyword)}&hl=vi&gl=VN";
                        var items = await FetchFeedAsync(googleNewsUrl, keywordList, cutoffTime, maxCount - allItems.Count, ct);
                        allItems.AddRange(items);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch Google News RSS for: {Keyword}", keyword);
                    }
                }
            }

            return allItems
                .DistinctBy(i => i.SourceUrl)
                .Take(maxCount)
                .ToList();
        }

        public async Task<CrawledNewsItem?> FetchSingleAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync(ct);

                // Trích xuất tiêu đề từ HTML (đơn giản)
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
                _logger.LogWarning(ex, "Failed to fetch single article: {Url}", url);
                return null;
            }
        }

        private async Task<List<CrawledNewsItem>> FetchFeedAsync(
            string feedUrl, List<string> keywords, DateTimeOffset cutoffTime,
            int maxItems, CancellationToken ct)
        {
            var items = new List<CrawledNewsItem>();

            var response = await _httpClient.GetAsync(feedUrl, ct);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync(ct);
            var doc = XDocument.Parse(xml);

            // Hỗ trợ cả RSS 2.0 và Atom
            var root = doc.Root;
            if (root == null) return items;

            XName entryName;
            XName titleName;
            XName linkName;
            XName summaryName;
            XName pubDateName;
            bool isAtom;

            if (root.Name.LocalName == "feed")
            {
                // Atom format
                isAtom = true;
                entryName = root.Name.Namespace + "entry";
                titleName = root.Name.Namespace + "title";
                linkName = root.Name.Namespace + "link";
                summaryName = root.Name.Namespace + "content";
                pubDateName = root.Name.Namespace + "published";
            }
            else
            {
                // RSS 2.0 format
                isAtom = false;
                var channel = root.Element("channel");
                if (channel == null) return items;
                entryName = "item";
                titleName = "title";
                linkName = "link";
                summaryName = "description";
                pubDateName = "pubDate";
            }

            var feedTitle = doc.Root?.Element("channel")?.Element("title")?.Value ?? "RSS Feed";
            var entries = isAtom
                ? doc.Descendants(entryName).ToList()
                : doc.Root?.Element("channel")?.Elements(entryName).ToList() ?? [];

            foreach (var entry in entries)
            {
                if (items.Count >= maxItems) break;

                var title = entry.Element(titleName)?.Value ?? string.Empty;
                var summary = entry.Element(summaryName)?.Value ?? string.Empty;

                // Link handling
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

                // Date parsing
                var dateStr = entry.Element(pubDateName)?.Value;
                DateTimeOffset publishDate;
                if (!DateTimeOffset.TryParse(dateStr, out publishDate))
                    publishDate = DateTimeOffset.UtcNow;

                // Lọc theo keyword
                if (keywords.Count > 0 && !keywords.Any(k =>
                    title.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    summary.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Lọc theo thời gian
                if (publishDate < cutoffTime) continue;

                items.Add(new CrawledNewsItem(
                    Title: title,
                    SourceName: feedTitle,
                    SourceUrl: link,
                    MarketType: CMarketScope.Domestic,
                    PublishedTime: publishDate,
                    SummaryRaw: StripHtmlTags(summary)));
            }

            return items;
        }

        private static string ExtractTitleFromHtml(string html)
        {
            // Tìm <title> tag
            var titleStart = html.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
            if (titleStart < 0) return string.Empty;
            titleStart += 7;
            var titleEnd = html.IndexOf("</title>", titleStart, StringComparison.OrdinalIgnoreCase);
            if (titleEnd < 0) return string.Empty;
            return html[titleStart..titleEnd].Trim();
        }

        private static string ExtractDescriptionFromHtml(string html)
        {
            // Tìm <meta name="description"> tag
            var patterns = new[] { "name=\"description\"", "name='description'", "property=\"og:description\"", "property='og:description'" };
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
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            return result.Trim();
        }
    }
}
