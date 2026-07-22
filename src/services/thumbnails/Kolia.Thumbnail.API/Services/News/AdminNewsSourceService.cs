using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.News
{
    public sealed class AdminNewsSourceService : IAdminNewsSourceService
    {
        private readonly ThumbnailDbContext _db;
        private readonly NewsSourceRegistry _registry;
        private readonly ResponseCacheStore _responseCache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminNewsSourceService> _logger;

        public AdminNewsSourceService(
            ThumbnailDbContext db,
            NewsSourceRegistry registry,
            ResponseCacheStore responseCache,
            HttpClient httpClient,
            ILogger<AdminNewsSourceService> logger)
        {
            _db = db;
            _registry = registry;
            _responseCache = responseCache;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<NewsSourceListItemDto>> ListAsync(
            CNewsSourceGroup? group, CMarketScope? region, bool? isTrusted, CancellationToken ct)
        {
            var query = _db.NewsSources.AsQueryable();

            if (group.HasValue) query = query.Where(s => s.SourceGroup == group.Value);
            if (region.HasValue) query = query.Where(s => s.Region == region.Value);
            if (isTrusted.HasValue) query = query.Where(s => s.IsTrusted == isTrusted.Value);

            var sources = await query
                .OrderBy(s => s.Priority)
                .ToListAsync(ct);

            return sources.Select(NewsSourceMapper.ToListItem).ToList();
        }

        public async Task<NewsSourceDetailDto> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var source = await _db.NewsSources.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"NewsSource {id} không tìm thấy.");
            return NewsSourceMapper.ToDetail(source);
        }

        public async Task<NewsSourceDetailDto> CreateAsync(NewsSourceCreateDto dto, CancellationToken ct)
        {
            // Validate URL is reachable before saving
            await ValidateUrlReachableAsync(dto.RssOrFeedUrl, ct);

            var entity = new NewsSourceEntity
            {
                Name = dto.Name,
                RssOrFeedUrl = dto.RssOrFeedUrl,
                Region = dto.Region,
                IsTrusted = dto.IsTrusted,
                Priority = dto.Priority,
                SourceGroup = dto.SourceGroup,
                FetchMode = dto.FetchMode,
                Domain = dto.Domain,
                ConsecutiveFailureCount = 0
            };

            _db.NewsSources.Add(entity);
            await _db.SaveChangesAsync(ct);

            _registry.InvalidateCache();
            _logger.LogInformation("Admin created NewsSource {Id} ({Domain}).", entity.Id, entity.Domain);

            return NewsSourceMapper.ToDetail(entity);
        }

        public async Task<NewsSourceDetailDto> UpdateAsync(Guid id, NewsSourceUpdateDto dto, CancellationToken ct)
        {
            var entity = await _db.NewsSources.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"NewsSource {id} không tìm thấy.");

            // If URL changed, validate reachability first
            if (entity.RssOrFeedUrl != dto.RssOrFeedUrl)
                await ValidateUrlReachableAsync(dto.RssOrFeedUrl, ct);

            entity.Name = dto.Name;
            entity.RssOrFeedUrl = dto.RssOrFeedUrl;
            entity.Region = dto.Region;
            entity.IsTrusted = dto.IsTrusted;
            entity.Priority = dto.Priority;
            entity.SourceGroup = dto.SourceGroup;
            entity.FetchMode = dto.FetchMode;
            entity.Domain = dto.Domain;
            entity.LastModificationTime = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

            // Invalidate registry cache AND response cache for this domain
            _registry.InvalidateCache();
            _responseCache.Invalidate(entity.Domain);
            _logger.LogInformation("Admin updated NewsSource {Id} ({Domain}).", entity.Id, entity.Domain);

            return NewsSourceMapper.ToDetail(entity);
        }

        public async Task<NewsSourceDetailDto> ToggleAsync(Guid id, CancellationToken ct)
        {
            var entity = await _db.NewsSources.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"NewsSource {id} không tìm thấy.");

            entity.IsTrusted = !entity.IsTrusted;
            entity.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            _registry.InvalidateCache();
            _logger.LogInformation("Admin toggled IsTrusted={Value} for NewsSource {Id}.", entity.IsTrusted, id);

            return NewsSourceMapper.ToDetail(entity);
        }

        public async Task<NewsSourceTestFetchResultDto> TestFetchAsync(
            Guid id, List<string> keywords, CancellationToken ct)
        {
            var entity = await _db.NewsSources.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"NewsSource {id} không tìm thấy.");

            // Use a simple direct RSS fetch for test — does NOT touch circuit breaker stats
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, entity.RssOrFeedUrl);
                var response = await _httpClient.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new NewsSourceTestFetchResultDto(
                        Success: false,
                        TierUsed: "Tier1-RSS",
                        ItemCount: 0,
                        Items: [],
                        ErrorMessage: $"HTTP {(int)response.StatusCode} {response.StatusCode}");
                }

                var xml = await response.Content.ReadAsStringAsync(ct);
                var previews = PreviewParseRss(xml, keywords, 5);

                return new NewsSourceTestFetchResultDto(
                    Success: true,
                    TierUsed: "Tier1-RSS",
                    ItemCount: previews.Count,
                    Items: previews,
                    ErrorMessage: null);
            }
            catch (Exception ex)
            {
                return new NewsSourceTestFetchResultDto(
                    Success: false,
                    TierUsed: "Tier1-RSS",
                    ItemCount: 0,
                    Items: [],
                    ErrorMessage: ex.Message);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var entity = await _db.NewsSources.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"NewsSource {id} không tìm thấy.");

            entity.IsDeleted = true;
            entity.DeletionTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            _registry.InvalidateCache();
            _responseCache.Invalidate(entity.Domain);
            _logger.LogInformation("Admin soft-deleted NewsSource {Id} ({Domain}).", id, entity.Domain);
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task ValidateUrlReachableAsync(string url, CancellationToken ct)
        {
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await _httpClient.SendAsync(headRequest, ct);
                if (!response.IsSuccessStatusCode)
                {
                    // Some feeds don't support HEAD — try GET with a short timeout
                    using var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    var getResponse = await _httpClient.SendAsync(getRequest, ct);
                    if (!getResponse.IsSuccessStatusCode)
                        throw new InvalidOperationException(
                            $"URL '{url}' trả về HTTP {(int)getResponse.StatusCode}. " +
                            "Vui lòng kiểm tra lại URL trước khi lưu.");
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Không thể kết nối tới URL '{url}': {ex.Message}. " +
                    "Vui lòng kiểm tra URL và thử lại.", ex);
            }
        }

        private static List<NewsSourcePreviewItemDto> PreviewParseRss(
            string xml, List<string> keywords, int maxCount)
        {
            var results = new List<NewsSourcePreviewItemDto>();
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                var root = doc.Root;
                if (root == null) return results;

                bool isAtom = root.Name.LocalName == "feed";
                var entries = isAtom
                    ? doc.Descendants(root.Name.Namespace + "entry").ToList()
                    : doc.Root?.Element("channel")?.Elements("item").ToList() ?? [];

                foreach (var entry in entries)
                {
                    if (results.Count >= maxCount) break;

                    var title = isAtom
                        ? entry.Element(root.Name.Namespace + "title")?.Value ?? string.Empty
                        : entry.Element("title")?.Value ?? string.Empty;

                    string link;
                    if (isAtom)
                    {
                        var linkEl = entry.Element(root.Name.Namespace + "link");
                        link = linkEl?.Attribute("href")?.Value ?? linkEl?.Value ?? string.Empty;
                    }
                    else link = entry.Element("link")?.Value ?? string.Empty;

                    var summary = isAtom
                        ? entry.Element(root.Name.Namespace + "content")?.Value ?? string.Empty
                        : entry.Element("description")?.Value ?? string.Empty;

                    var pubDateStr = isAtom
                        ? entry.Element(root.Name.Namespace + "published")?.Value
                        : entry.Element("pubDate")?.Value;
                    DateTimeOffset.TryParse(pubDateStr, out var pubDate);

                    results.Add(new NewsSourcePreviewItemDto(
                        Title: title,
                        SourceUrl: link,
                        PublishedTime: pubDate == default ? null : pubDate,
                        SummaryRaw: summary.Length > 300 ? summary[..300] + "…" : summary));
                }
            }
            catch { /* Malformed XML — return what we have */ }
            return results;
        }
    }
}
