using System.Text.Json;
using System.Text.RegularExpressions;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    public class GoogleNewsUrlResolver : IGoogleNewsUrlResolver
    {
        private readonly HttpClient _http;
        private readonly ILogger<GoogleNewsUrlResolver> _logger;

        public GoogleNewsUrlResolver(HttpClient http, ILogger<GoogleNewsUrlResolver> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<string> ResolveAsync(string url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                !uri.Host.EndsWith("news.google.com", StringComparison.OrdinalIgnoreCase))
            {
                return url; // Không phải link Google News, giữ nguyên
            }

            try
            {
                // Trường hợp format cũ /articles/... đôi khi vẫn redirect HTTP 30x thẳng tới bài gốc
                using var head = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
                if (head.RequestMessage?.RequestUri != null &&
                    !head.RequestMessage.RequestUri.Host.EndsWith("news.google.com", StringComparison.OrdinalIgnoreCase))
                {
                    return head.RequestMessage.RequestUri.ToString();
                }

                // Format mới /rss/articles/CBMi... → phải giải mã qua batchexecute
                var html = await head.Content.ReadAsStringAsync(ct);

                var artId = uri.Segments.Last().TrimEnd('/');
                var sig = Regex.Match(html, "data-n-a-sg=\"([^\"]+)\"").Groups[1].Value;
                var ts = Regex.Match(html, "data-n-a-ts=\"([^\"]+)\"").Groups[1].Value;

                if (string.IsNullOrEmpty(sig) || string.IsNullOrEmpty(ts))
                {
                    _logger.LogWarning("Không tìm thấy signature/timestamp để giải mã Google News url: {Url}", url);
                    return url;
                }

                var innerPayload = JsonSerializer.Serialize(new object[]
                {
                    "garturlreq",
                    new object[]
                    {
                        new object[] { "X", "X", new object[] { "X", "X" }, null!, null!, 1, 1, "US:en", null!, 1, null!, null!, null!, null!, null!, 0, 1 },
                        "X", "X", 1, new object[] { 1, 1, 1 }, 1, 1, null!, 0, 0, null!, 0
                    },
                    artId, ts, sig
                });

                var fReq = JsonSerializer.Serialize(new object[] { new object[] { new object[] { "Fbv4je", innerPayload } } });

                var form = new Dictionary<string, string> { ["f.req"] = fReq };
                using var content = new FormUrlEncodedContent(form);
                using var resp = await _http.PostAsync(
                    "https://news.google.com/_/DotsSplashUi/data/batchexecute", content, ct);
                var text = await resp.Content.ReadAsStringAsync(ct);

                // Response có prefix ")]}'\n" và nhiều dòng — cần bỏ dòng đầu rồi parse JSON
                var jsonLine = text.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith("[[\"wrb.fr\""));
                if (jsonLine == null) return url;

                using var doc = JsonDocument.Parse(jsonLine);
                var inner = doc.RootElement[0][2].GetString();
                if (inner == null) return url;

                using var innerDoc = JsonDocument.Parse(inner);
                var decodedUrl = innerDoc.RootElement[1].GetString();

                return string.IsNullOrWhiteSpace(decodedUrl) ? url : decodedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Giải mã Google News url thất bại, giữ url gốc: {Url}", url);
                return url;
            }
        }
    }
}