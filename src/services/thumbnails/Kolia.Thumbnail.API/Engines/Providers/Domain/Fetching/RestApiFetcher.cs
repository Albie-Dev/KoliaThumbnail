using System.Net.Http.Json;
using System.Text.Json;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Engines.Social;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Fetcher cho nguồn tin dạng REST API.
    /// Hỗ trợ: API key (header/X-Api-Key hoặc query param), JSON path để map response,
    /// request headers tuỳ chỉnh, query params template với placeholder,
    /// và phân trang dạng offset/page/cursor.
    /// </summary>
    public sealed class RestApiFetcher : IRestApiFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestApiFetcher> _logger;

        public RestApiFetcher(
            HttpClient httpClient,
            ILogger<RestApiFetcher> logger)
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
            if (string.IsNullOrWhiteSpace(source.ApiEndpoint))
            {
                _logger.LogWarning("RestApiFetcher: ApiEndpoint is null/empty for source {Id}.", source.Id);
                return [];
            }

            try
            {
                var items = new List<CrawledNewsItem>();

                // Build request URL with query params
                var url = BuildUrl(source, keywords, cutoff, maxCount, paginationParam: null);
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Apply custom headers (from ApiRequestHeaders JSON)
                ApplyCustomHeaders(request, source.ApiRequestHeaders);

                // Apply API key (default: X-Api-Key header)
                if (!string.IsNullOrWhiteSpace(source.ApiKey))
                {
                    request.Headers.TryAddWithoutValidation("X-Api-Key", source.ApiKey);
                }

                _logger.LogDebug("RestApiFetcher: GET {Url} for source {Id}", url, source.Id);

                var response = await _httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

                // Navigate to items container via JSON path
                var itemsContainer = NavigateJsonPath(json, source.ApiResponseJsonPath);

                if (itemsContainer is null)
                {
                    _logger.LogWarning(
                        "RestApiFetcher: JSON path '{Path}' not found for source {Id}.",
                        source.ApiResponseJsonPath, source.Id);
                    return [];
                }

                // Extract items: handle both array and object (dictionary) containers
                var rawItems = new List<JsonElement>();
                if (itemsContainer.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in itemsContainer.Value.EnumerateArray())
                        rawItems.Add(element);
                }
                else if (itemsContainer.Value.ValueKind == JsonValueKind.Object)
                {
                    // Object container: iterate over property values (e.g., World Bank "documents")
                    foreach (var prop in itemsContainer.Value.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Object)
                            rawItems.Add(prop.Value);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "RestApiFetcher: JSON path '{Path}' is neither array nor object for source {Id}.",
                        source.ApiResponseJsonPath, source.Id);
                    return [];
                }

                foreach (var element in rawItems)
                {
                    if (items.Count >= maxCount) break;

                    var crawled = MapToCrawledItem(element, source);
                    if (crawled == null) continue;

                    // Apply cutoff filter
                    if (crawled.PublishedTime.HasValue && crawled.PublishedTime < cutoff)
                        continue;

                    items.Add(crawled);
                }

                // Handle pagination if configured
                if (items.Count < maxCount && source.ApiPaginationType.HasValue
                    && source.ApiPaginationType != Enums.CApiPaginationType.None)
                {
                    items.AddRange(await FetchNextPagesAsync(
                        source, keywords, cutoff, maxCount - items.Count, items.Count, ct));
                }

                _logger.LogDebug(
                    "RestApiFetcher: Fetched {Count} items from {Endpoint} for source {Id}.",
                    items.Count, source.ApiEndpoint, source.Id);

                return items;
            }
            catch (OperationCanceledException)
            {
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "RestApiFetcher: Failed to fetch from {Endpoint} for source {Id}.",
                    source.ApiEndpoint, source.Id);
                return [];
            }
        }

        // ── Private helpers ──────────────────────────────────────────

        private string BuildUrl(
            NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int maxCount,
            int? paginationParam)
        {
            var url = source.ApiEndpoint!;

            // Replace placeholders in query params template
            if (!string.IsNullOrWhiteSpace(source.ApiQueryParamsTemplate))
            {
                var template = source.ApiQueryParamsTemplate;

                // Replace {keywords} with joined keywords
                var kw = keywords.Count > 0 ? string.Join(" ", keywords.Take(3)) : "";
                template = template.Replace("{keywords}", Uri.EscapeDataString(kw));
                template = template.Replace("{cutoff}", Uri.EscapeDataString(cutoff.ToString("O")));
                template = template.Replace("{maxCount}", maxCount.ToString());

                // Handle pagination params
                if (paginationParam.HasValue && source.ApiPaginationType.HasValue)
                {
                    template = source.ApiPaginationType switch
                    {
                        Enums.CApiPaginationType.Offset => template
                            .Replace("{offset}", paginationParam.Value.ToString()),
                        Enums.CApiPaginationType.Page => template
                            .Replace("{page}", (paginationParam.Value + 1).ToString()),
                        _ => template
                    };
                }

                url += (url.Contains('?') ? "&" : "?") + template;
            }

            return url;
        }

        private void ApplyCustomHeaders(HttpRequestMessage request, string? headersJson)
        {
            if (string.IsNullOrWhiteSpace(headersJson)) return;

            try
            {
                using var doc = JsonDocument.Parse(headersJson);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    request.Headers.TryAddWithoutValidation(prop.Name, prop.Value.GetString());
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "RestApiFetcher: Invalid ApiRequestHeaders JSON.");
            }
        }

        private static JsonElement? NavigateJsonPath(JsonElement root, string? jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                return root;

            var parts = jsonPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            foreach (var part in parts)
            {
                if (current.ValueKind != JsonValueKind.Object)
                    return null;

                if (!current.TryGetProperty(part, out current))
                    return null;
            }

            return current;
        }

        private static CrawledNewsItem? MapToCrawledItem(JsonElement item, NewsSourceEntity source)
        {
            try
            {
                var title = GetNestedProperty(item, "title") ?? GetNestedProperty(item, "headline")
                    ?? GetNestedProperty(item, "webTitle") ?? GetNestedProperty(item, "name") ?? "";
                var url = GetStringProperty(item, "url") ?? GetStringProperty(item, "webUrl")
                    ?? GetStringProperty(item, "link") ?? GetNestedProperty(item, "sourceUrl") ?? "";
                var description = GetNestedProperty(item, "description") ?? GetNestedProperty(item, "abstract")
                    ?? GetNestedProperty(item, "body") ?? GetNestedProperty(item, "bodyText")
                    ?? GetNestedProperty(item, "descr") ?? GetNestedProperty(item, "content_1000")
                    ?? GetNestedProperty(item, "content") ?? "";

                if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(url))
                    return null;

                // Parse published time from various formats
                DateTimeOffset? publishedTime = null;
                var pubStr = GetStringProperty(item, "publishedAt") ?? GetStringProperty(item, "pubDate")
                    ?? GetStringProperty(item, "date") ?? GetStringProperty(item, "webPublicationDate")
                    ?? GetStringProperty(item, "dateTime") ?? GetStringProperty(item, "lnchdt");
                if (!string.IsNullOrWhiteSpace(pubStr))
                {
                    if (DateTimeOffset.TryParse(pubStr, out var parsed))
                        publishedTime = parsed;
                }

                return new CrawledNewsItem(
                    Title: title,
                    SourceName: source.Name,
                    SourceUrl: url,
                    MarketType: source.Region,
                    PublishedTime: publishedTime,
                    SummaryRaw: (description.Length > 500 ? description[..500] + "…" : description));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a string property, supporting nested dotted paths like "title.cdata!"
        /// and "cdata!" suffix which extracts from {"cdata!": "value"} objects.
        /// </summary>
        private static string? GetNestedProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;

            var parts = propertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var current = element;

            foreach (var part in parts)
            {
                if (current.ValueKind != JsonValueKind.Object) return null;
                if (!current.TryGetProperty(part, out current)) return null;
            }

            // Handle "cdata!" wrapper: if the result is {"cdata!": "value"}, extract the string
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (current.TryGetProperty("cdata!", out var cdata) && cdata.ValueKind == JsonValueKind.String)
                    return cdata.GetString();
                // Or nested "content" with CDATA
                if (current.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                    return content.GetString();
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }

        private static string? GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
            return null;
        }

        private async Task<List<CrawledNewsItem>> FetchNextPagesAsync(
            NewsSourceEntity source,
            List<string> keywords,
            DateTimeOffset cutoff,
            int remaining,
            int currentCount,
            CancellationToken ct)
        {
            var results = new List<CrawledNewsItem>();
            if (remaining <= 0) return results;

            var maxPages = 3; // Prevent infinite pagination

            for (int page = 0; page < maxPages && remaining > 0; page++)
            {
                var paginationValue = source.ApiPaginationType switch
                {
                    Enums.CApiPaginationType.Offset => currentCount + results.Count,
                    Enums.CApiPaginationType.Page => page + 1,
                    _ => 0
                };

                var url = BuildUrl(source, keywords, cutoff, remaining, paginationValue);
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrWhiteSpace(source.ApiKey))
                    request.Headers.TryAddWithoutValidation("X-Api-Key", source.ApiKey);
                ApplyCustomHeaders(request, source.ApiRequestHeaders);

                try
                {
                    var response = await _httpClient.SendAsync(request, ct);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                    var itemsContainer = NavigateJsonPath(json, source.ApiResponseJsonPath);

                    if (itemsContainer is null) break;

                    // Handle both array and object containers
                    var rawItems = new List<JsonElement>();
                    if (itemsContainer.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in itemsContainer.Value.EnumerateArray())
                            rawItems.Add(el);
                    }
                    else if (itemsContainer.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in itemsContainer.Value.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Object)
                                rawItems.Add(prop.Value);
                        }
                    }

                    var pageItems = 0;
                    foreach (var element in rawItems)
                    {
                        if (results.Count >= remaining) break;
                        var crawled = MapToCrawledItem(element, source);
                        if (crawled == null) continue;
                        if (crawled.PublishedTime.HasValue && crawled.PublishedTime < cutoff) continue;
                        results.Add(crawled);
                        pageItems++;
                    }

                    if (pageItems == 0) break; // No more items on this page
                }
                catch
                {
                    break; // Stop pagination on error
                }
            }

            return results;
        }
    }
}
