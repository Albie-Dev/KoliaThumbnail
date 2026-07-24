using HtmlAgilityPack;
using Kolia.Thumbnail.API.Engines.Social;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Triển khai IArticleContentFetcher dùng HtmlAgilityPack để trích xuất full-text bài báo.
    /// Chỉ được gọi on-demand (1 tin/lần) trong bước "Phân tích sâu" — không dùng trong batch crawl.
    /// </summary>
    public class ArticleContentFetcher : IArticleContentFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ArticleContentFetcher> _logger;

        /// <summary>Giới hạn độ dài ký tự để tránh prompt AI quá dài.</summary>
        private const int MaxArticleChars = 8000;

        /// <summary>Độ dài tối thiểu — ngắn hơn coi là thất bại trích xuất (SPA/chặn bot).</summary>
        private const int MinArticleChars = 200;

        /// <summary>Độ dài tối thiểu của 1 đoạn &lt;p&gt; để giữ lại (loại bỏ caption/quảng cáo/link).</summary>
        private const int MinParagraphChars = 40;

        public ArticleContentFetcher(HttpClient httpClient, ILogger<ArticleContentFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Tải và trích xuất full-text bài báo từ URL. Không bao giờ throw — luôn trả ArticleContentResult.
        /// </summary>
        public async Task<ArticleContentResult> FetchFullTextAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var targetUrl = url;

                // Xử lý Google News RSS redirect link (ví dụ: https://news.google.com/rss/articles/CBM...)
                if (url.Contains("news.google.com"))
                {
                    try
                    {
                        var decodedUrl = DecodeGoogleNewsUrl(url);
                        if (!string.IsNullOrEmpty(decodedUrl) && IsValidNewsTargetUrl(decodedUrl))
                        {
                            targetUrl = decodedUrl;
                            _logger.LogInformation("Đã decode Google News URL thành công -> {RealUrl}", targetUrl);
                        }
                        else
                        {
                            // Thử gửi HEAD/GET không follow redirect để lấy Location header hoặc URL trang đích
                            using var responseMsg = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                            if (responseMsg.RequestMessage?.RequestUri != null &&
                                !responseMsg.RequestMessage.RequestUri.ToString().Contains("news.google.com"))
                            {
                                targetUrl = responseMsg.RequestMessage.RequestUri.ToString();
                                _logger.LogInformation("Đã lấy được Redirect Target URL từ HttpClient -> {RealUrl}", targetUrl);
                            }
                            else
                            {
                                var googleHtml = await responseMsg.Content.ReadAsStringAsync(ct);
                                var realUrl = await ExtractRealUrlFromGoogleRedirectAsync(url, googleHtml, ct);
                                if (!string.IsNullOrEmpty(realUrl))
                                {
                                    targetUrl = realUrl;
                                    _logger.LogInformation("Đã trích xuất Google News redirect URL từ HTML -> {RealUrl}", realUrl);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không giải mã được Google News redirect URL cho {Url}", url);
                    }
                }

                _logger.LogInformation("Bắt đầu fetch URL đích: {TargetUrl}", targetUrl);
                var html = await _httpClient.GetStringAsync(targetUrl, ct);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Bước 1: Loại bỏ các node nhiễu không thuộc nội dung bài viết
                var nodesToRemove = new[] { "script", "style", "nav", "footer", "aside", "form", "iframe", "svg", "button" };
                foreach (var tag in nodesToRemove)
                {
                    var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
                    if (nodes != null)
                        foreach (var node in nodes.ToList())
                            node.Remove();
                }

                // Loại bỏ các block có class thường chứa tin liên quan, quảng cáo, bình luận... (Lưu ý: Không chặn chữ 'sidebar' vì VnExpress dùng 'sidebar-1' cho cột nội dung chính)
                var noiseNodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'related') or contains(@class, 'tin-lien-quan') or contains(@class, 'comment') or contains(@class, 'banner') or contains(@class, 'tag') or contains(@class, 'author') or contains(@class, 'more-news') or contains(@class, 'bottom-news') or contains(@class, 'suggest') or contains(@class, 'recommended') or contains(@class, 'new-item')]");
                if (noiseNodes != null)
                {
                    foreach (var node in noiseNodes.ToList())
                        node.Remove();
                }

                // Bước 2: Ưu tiên lấy các thẻ <p> trong article / container bài báo; fallback lấy toàn trang <p>
                var validParagraphs = new List<string>();
                var strictSelectors = new[]
                {
                    "//main[contains(@class,'article') or contains(@id,'article') or contains(@class,'detail')]//p",
                    "//article//p",
                    "//*[contains(@class,'article-body') or contains(@class,'article-content') or contains(@class,'post-body') or contains(@class,'detail-content') or contains(@class,'content-detail') or contains(@class,'fck_detail') or contains(@class,'singular-content') or contains(@id,'article-editor')]//p",
                    "//div[contains(@class,'content') or contains(@class,'detail')]//p",
                    "//p"
                };

                foreach (var selector in strictSelectors)
                {
                    var paragraphs = doc.DocumentNode.SelectNodes(selector);
                    if (paragraphs != null && paragraphs.Count > 0)
                    {
                        var tempValid = paragraphs
                            .Select(p => System.Net.WebUtility.HtmlDecode(p.InnerText).Trim())
                            .Where(t => t.Length >= MinParagraphChars)
                            .Distinct()
                            .ToList();
                        
                        var tempText = string.Join("\n\n", tempValid);
                        // Đủ nội dung bài báo thì dừng, tránh rớt xuống các thẻ <p> chung chung
                        if (tempText.Length >= MinArticleChars)
                        {
                            validParagraphs = tempValid;
                            break;
                        }
                    }
                }

                var fullText = string.Join("\n\n", validParagraphs);

                // Bước 3: Nếu <p> không có đủ văn bản (trang tin thiết kế dạng div/span hoặc JS heavy), thử lấy thêm từ meta description / og:description
                if (fullText.Length < MinArticleChars)
                {
                    var metaDesc = doc.DocumentNode.SelectSingleNode("//meta[@name='description' or @property='og:description' or @name='twitter:description']")
                        ?.GetAttributeValue("content", "")?.Trim();

                    if (!string.IsNullOrEmpty(metaDesc))
                        metaDesc = System.Net.WebUtility.HtmlDecode(metaDesc);

                    if (!string.IsNullOrEmpty(metaDesc) && metaDesc.Length > 50)
                    {
                        fullText = string.IsNullOrEmpty(fullText) ? metaDesc : $"{fullText}\n\n{metaDesc}";
                    }
                }

                // Bước 4: Kiểm tra độ dài tối thiểu
                if (fullText.Length < MinArticleChars)
                    return new ArticleContentResult(false, null, 0,
                        "Không trích xuất được nội dung — có thể trang chặn bot hoặc render bằng JS (SPA)");

                // Bước 5: Cắt tại ranh giới câu gần nhất nếu quá dài
                if (fullText.Length > MaxArticleChars)
                    fullText = TruncateAtSentenceBoundary(fullText, MaxArticleChars);

                return new ArticleContentResult(true, fullText, fullText.Length, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fetch full-text thất bại cho {Url}", url);
                return new ArticleContentResult(false, null, 0,
                    $"Lỗi khi tải trang: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Giải mã Base64 / Protobuf wrapper token trong URL Google News RSS
        /// </summary>
        private static string? DecodeGoogleNewsUrl(string googleUrl)
        {
            try
            {
                var uri = new Uri(googleUrl);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var token = pathSegments.LastOrDefault();
                if (string.IsNullOrEmpty(token) || token.Equals("articles", StringComparison.OrdinalIgnoreCase))
                    return null;

                // Base64Url decode standard
                string base64 = token.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] bytes = Convert.FromBase64String(base64);

                // Quét tìm chuỗi HTTP/HTTPS trong byte array của Google News Protobuf payload
                var asciiStr = System.Text.Encoding.ASCII.GetString(bytes);
                var match = System.Text.RegularExpressions.Regex.Match(asciiStr, @"https?://[a-zA-Z0-9\.\-_/%?=&#]+");
                if (match.Success && IsValidNewsTargetUrl(match.Value))
                {
                    return match.Value;
                }

                // Thử UTF8 nếu ASCII không khớp
                var utf8Str = System.Text.Encoding.UTF8.GetString(bytes);
                match = System.Text.RegularExpressions.Regex.Match(utf8Str, @"https?://[a-zA-Z0-9\.\-_/%?=&#]+");
                if (match.Success && IsValidNewsTargetUrl(match.Value))
                {
                    return match.Value;
                }
            }
            catch
            {
                // Bỏ qua lỗi decode base64
            }
            return null;
        }

        private static readonly string[] ExcludedDomains = [
            "google.com",
            "googleusercontent.com",
            "gstatic.com",
            "googleapis.com",
            "googlesyndication.com",
            "google-analytics.com",
            "googletagmanager.com",
            "doubleclick.net",
            "schema.org",
            "w3.org",
            "angular.dev",
            "angular.io",
            "polymer-project.org",
            "apache.org",
            "github.com",
            "twitter.com",
            "facebook.com",
            "instagram.com",
            "youtube.com"
        ];

        private static bool IsValidNewsTargetUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != "http" && uri.Scheme != "https")
                return false;

            var host = uri.Host.ToLowerInvariant();
            if (ExcludedDomains.Any(d => host.EndsWith(d, StringComparison.OrdinalIgnoreCase)))
                return false;

            var path = uri.AbsolutePath.ToLowerInvariant();
            if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") ||
                path.EndsWith(".gif") || path.EndsWith(".svg") || path.EndsWith(".css") ||
                path.EndsWith(".js") || path.EndsWith(".ico") || path.Contains("license") ||
                url.Contains("=w16") || url.Contains("=s"))
                return false;

            return true;
        }

        /// <summary>
        /// Trích xuất URL trang báo thật từ trang redirect HTML của Google News.
        /// </summary>
        private async Task<string?> ExtractRealUrlFromGoogleRedirectAsync(string url, string html, CancellationToken ct)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // API Batchexecute technique (mới nhất từ Google News)
                var cWizDiv = doc.DocumentNode.SelectSingleNode("//c-wiz/div[@jscontroller]");
                if (cWizDiv != null)
                {
                    var signature = cWizDiv.GetAttributeValue("data-n-a-sg", "");
                    var timestamp = cWizDiv.GetAttributeValue("data-n-a-ts", "");
                    var base64Str = new Uri(url).AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                    if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(timestamp) && !string.IsNullOrEmpty(base64Str))
                    {
                        try
                        {
                            var reqUrl = "https://news.google.com/_/DotsSplashUi/data/batchexecute";
                            var payload = $"[\"garturlreq\",[[\"X\",\"X\",[\"X\",\"X\"],null,null,1,1,\"US:en\",null,1,null,null,null,null,null,0,1],\"X\",\"X\",1,[1,1,1],1,1,null,0,0,null,0],\"{base64Str}\",{timestamp},\"{signature}\"]";
                            var fReq = $"[[[\"Fbv4je\",\"{payload.Replace("\"", "\\\"")}\",null,\"generic\"]]]";

                            using var request = new HttpRequestMessage(HttpMethod.Post, reqUrl);
                            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
                            request.Content = new FormUrlEncodedContent([
                                new KeyValuePair<string, string>("f.req", fReq)
                            ]);

                            var response = await _httpClient.SendAsync(request, ct);
                            if (response.IsSuccessStatusCode)
                            {
                                var responseText = await response.Content.ReadAsStringAsync(ct);
                                var startIndex = responseText.IndexOf('[');
                                if (startIndex >= 0)
                                {
                                    var jsonPayload = responseText.Substring(startIndex);
                                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonPayload);
                                    var rootArr = jsonDoc.RootElement[0];
                                    var innerStr = rootArr[2].GetString();
                                    if (!string.IsNullOrEmpty(innerStr))
                                    {
                                        using var innerDoc = System.Text.Json.JsonDocument.Parse(innerStr);
                                        var decodedUrl = innerDoc.RootElement[1].GetString();
                                        if (!string.IsNullOrEmpty(decodedUrl) && IsValidNewsTargetUrl(decodedUrl))
                                        {
                                            return decodedUrl;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Lỗi khi giải mã Google News Batchexecute cho {Url}", url);
                        }
                    }
                }

                // 1. Thử lấy link từ thẻ <a jsname="t-a"> hoặc thẻ <a> trong <c-wiz> (Google News UI)
                var cWizAnchors = doc.DocumentNode.SelectNodes("//c-wiz//a[@href] | //a[@jsname='t-a' and @href] | //a[@data-n-head]");
                if (cWizAnchors != null)
                {
                    foreach (var a in cWizAnchors)
                    {
                        var href = a.GetAttributeValue("href", "");
                        if (IsValidNewsTargetUrl(href)) return href;
                    }
                }

                // 2. Thử lấy link từ canonical, og:url hoặc data-url
                var canonical = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']")?.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(canonical) && IsValidNewsTargetUrl(canonical))
                    return canonical;

                var ogUrl = doc.DocumentNode.SelectSingleNode("//meta[@property='og:url']")?.GetAttributeValue("content", "");
                if (!string.IsNullOrEmpty(ogUrl) && IsValidNewsTargetUrl(ogUrl))
                    return ogUrl;

                // 3. Thử lấy link từ meta refresh
                var metaRefresh = doc.DocumentNode.SelectSingleNode("//meta[translate(@http-equiv, 'REFRESH', 'refresh')='refresh']");
                if (metaRefresh != null)
                {
                    var content = metaRefresh.GetAttributeValue("content", "");
                    var idx = content.IndexOf("url=", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        var extracted = content[(idx + 4)..].Trim('\'', '"', ' ');
                        if (IsValidNewsTargetUrl(extracted)) return extracted;
                    }
                }

                // 4. Fallback: Data attributes (data-url, data-href)
                var dataNodes = doc.DocumentNode.SelectNodes("//*[@data-url or @data-href]");
                if (dataNodes != null)
                {
                    foreach (var n in dataNodes)
                    {
                        var dUrl = n.GetAttributeValue("data-url", "") ?? n.GetAttributeValue("data-href", "");
                        if (!string.IsNullOrEmpty(dUrl) && IsValidNewsTargetUrl(dUrl)) return dUrl;
                    }
                }

                // 5. Fallback: Thẻ <a> trong <noscript> hoặc body
                var allAnchors = doc.DocumentNode.SelectNodes("//noscript//a[@href] | //a[@href]");
                if (allAnchors != null)
                {
                    foreach (var a in allAnchors)
                    {
                        var href = a.GetAttributeValue("href", "");
                        if (IsValidNewsTargetUrl(href)) return href;
                    }
                }

                // 5. Fallback Regex quét HTTP/HTTPS URL trong trang HTML
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    html, @"https?://[a-zA-Z0-9\.\-_/%?=&#]+");

                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    var foundUrl = m.Value;
                    if (IsValidNewsTargetUrl(foundUrl)) return foundUrl;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Cắt chuỗi tại ranh giới câu gần nhất (dấu . ! ?) để không cắt giữa từ.
        /// </summary>
        private static string TruncateAtSentenceBoundary(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;

            var slice = text[..maxLength];
            var sentenceEnd = -1;
            for (int i = slice.Length - 1; i >= 0; i--)
            {
                if (slice[i] == '.' || slice[i] == '!' || slice[i] == '?')
                {
                    sentenceEnd = i;
                    break;
                }
            }

            return sentenceEnd > 0 ? slice[..(sentenceEnd + 1)] : slice;
        }
    }
}
