using System.Text.Json;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine tìm kiếm YouTube thật — gọi YouTube Data API v3 Search endpoint qua HttpClient.
    /// API key được lấy từ config (mặc định) hoặc set qua property (SocialExecutorService sẽ set).
    /// </summary>
    public class RealYouTubeSearchEngine : IYouTubeSearchEngine
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RealYouTubeSearchEngine> _logger;

        private const string SearchUrl = "https://www.googleapis.com/youtube/v3/search";

        public RealYouTubeSearchEngine(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RealYouTubeSearchEngine> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// API key hiện tại đang dùng. SocialExecutorService có thể set key này
        /// trước khi gọi SearchAsync để thực hiện key rotation.
        /// </summary>
        public string? CurrentApiKey { get; set; }

        public async Task<IReadOnlyList<YouTubeVideoResult>> SearchAsync(
            string keyword, CThumbnailTimeFilter timeFilter, CThumbnailSortFilter sortFilter,
            int maxResults, CancellationToken ct = default)
        {
            var apiKey = CurrentApiKey ?? _configuration["YouTube:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("YouTube API key not configured, using mock fallback");
                return FallbackSearch(keyword, maxResults);
            }

            var order = sortFilter switch
            {
                CThumbnailSortFilter.MostViewed => "viewCount",
                CThumbnailSortFilter.Newest => "date",
                CThumbnailSortFilter.MostRelevant => "relevance",
                _ => "relevance"
            };

            // PublishedAfter theo time filter
            var publishedAfter = timeFilter switch
            {
                CThumbnailTimeFilter.ThisWeek => DateTimeOffset.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CThumbnailTimeFilter.OneMonth => DateTimeOffset.UtcNow.AddMonths(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CThumbnailTimeFilter.ThreeMonths => DateTimeOffset.UtcNow.AddMonths(-3).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CThumbnailTimeFilter.SixMonths => DateTimeOffset.UtcNow.AddMonths(-6).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CThumbnailTimeFilter.OneYear => DateTimeOffset.UtcNow.AddYears(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                _ => null
            };

            var url = $"{SearchUrl}?part=snippet&q={Uri.EscapeDataString(keyword)}&maxResults={maxResults}&order={order}&key={apiKey}&type=video";
            if (!string.IsNullOrEmpty(publishedAfter))
                url += $"&publishedAfter={Uri.EscapeDataString(publishedAfter)}";

            var response = await _httpClient.GetAsync(url, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube API returned {Status}: {Body}", response.StatusCode, body);
                throw new ExternalServiceException(
                    $"YouTube API error ({(int)response.StatusCode}): {body}");
            }

            var data = JsonSerializer.Deserialize<YouTubeSearchResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data?.Items == null || data.Items.Count == 0)
                return [];

            return data.Items
                .Where(i => i.Id?.Kind == "youtube#video")
                .Select(i => new YouTubeVideoResult(
                    VideoId: i.Id?.VideoId ?? string.Empty,
                    Title: i.Snippet?.Title ?? "Không có tiêu đề",
                    ChannelName: i.Snippet?.ChannelTitle ?? "Không rõ",
                    ThumbnailImageUrl: i.Snippet?.Thumbnails?.High?.Url
                        ?? i.Snippet?.Thumbnails?.Default?.Url
                        ?? $"https://img.youtube.com/vi/{i.Id?.VideoId}/hqdefault.jpg",
                    VideoUrl: $"https://youtube.com/watch?v={i.Id?.VideoId}",
                    PublishedTime: DateTimeOffset.TryParse(i.Snippet?.PublishedAt, out var dt) ? dt : null,
                    ViewCount: null // ViewCount không có trong search endpoint, cần gọi videos/list riêng
                ))
                .ToList();
        }

        public async Task<YouTubeVideoResult?> FetchByUrlAsync(string videoUrl, CancellationToken ct = default)
        {
            var videoId = ExtractVideoId(videoUrl);
            if (string.IsNullOrEmpty(videoId))
            {
                _logger.LogWarning("Invalid YouTube URL: {Url}", videoUrl);
                return null;
            }

            var apiKey = CurrentApiKey ?? _configuration["YouTube:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new YouTubeVideoResult(
                    VideoId: videoId, Title: "Video YouTube", ChannelName: "Không rõ",
                    ThumbnailImageUrl: $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg",
                    VideoUrl: videoUrl, PublishedTime: null, ViewCount: null);
            }

            var url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={videoId}&key={apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync(ct);

                var data = JsonSerializer.Deserialize<YouTubeVideoListResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var item = data?.Items?.FirstOrDefault();
                if (item == null) return null;

                return new YouTubeVideoResult(
                    VideoId: videoId,
                    Title: item.Snippet?.Title ?? "Video YouTube",
                    ChannelName: item.Snippet?.ChannelTitle ?? "Không rõ",
                    ThumbnailImageUrl: item.Snippet?.Thumbnails?.High?.Url
                        ?? item.Snippet?.Thumbnails?.Default?.Url
                        ?? $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg",
                    VideoUrl: videoUrl,
                    PublishedTime: DateTimeOffset.TryParse(item.Snippet?.PublishedAt, out var dt) ? dt : null,
                    ViewCount: item.Statistics?.ViewCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch YouTube video: {Url}", videoUrl);
                return null;
            }
        }

        private static IReadOnlyList<YouTubeVideoResult> FallbackSearch(string keyword, int maxResults)
        {
            var list = new List<YouTubeVideoResult>();
            for (int i = 0; i < maxResults; i++)
            {
                list.Add(new YouTubeVideoResult(
                    VideoId: $"vid-{Guid.NewGuid()}",
                    Title: $"Video mẫu về {keyword} số {i + 1}",
                    ChannelName: "Kênh Crypto Finance",
                    ThumbnailImageUrl: $"https://img.youtube.com/vi/mock-{i}/hqdefault.jpg",
                    VideoUrl: $"https://youtube.com/watch?v=mock-{i}",
                    PublishedTime: DateTimeOffset.UtcNow.AddDays(-i),
                    ViewCount: 150000 - (i * 10000)
                ));
            }
            return list;
        }

        private static string? ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];
                if (!string.IsNullOrEmpty(videoId)) return videoId;

                if (uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
                {
                    var seg = uri.AbsolutePath.TrimStart('/').Split('/');
                    if (seg.Length > 0 && !string.IsNullOrWhiteSpace(seg[0]))
                        return seg[0];
                }

                if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                    return uri.AbsolutePath["/embed/".Length..].Split('/').FirstOrDefault();
            }
            catch { }
            return null;
        }

        // ── Response DTOs for YouTube Data API ─────────────────────

        private sealed record YouTubeSearchResponse(
            List<SearchItem>? Items, string? NextPageToken);

        private sealed record SearchItem(SearchItemId? Id, SnippetData? Snippet);

        private sealed record SearchItemId(string? Kind, string? VideoId);

        private sealed record SnippetData(
            string? Title, string? ChannelTitle, string? PublishedAt,
            ThumbnailData? Thumbnails);

        private sealed record ThumbnailData(ThumbnailInfo? Default, ThumbnailInfo? Medium, ThumbnailInfo? High);

        private sealed record ThumbnailInfo(string? Url);

        private sealed record YouTubeVideoListResponse(List<VideoDetailItem>? Items);

        private sealed record VideoDetailItem(SnippetData? Snippet, VideoStatistics? Statistics);

        private sealed record VideoStatistics(long? ViewCount);
    }
}
