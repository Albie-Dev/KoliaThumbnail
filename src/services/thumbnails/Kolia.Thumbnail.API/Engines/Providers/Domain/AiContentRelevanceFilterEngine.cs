using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine phân loại nội dung không liên quan sử dụng AI thật qua AIExecutorService.
    /// Phân tích tiêu đề và kênh để xác định video có phải MV/quảng cáo/giải trí không liên quan.
    /// </summary>
    public class AiContentRelevanceFilterEngine : IContentRelevanceFilterEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiContentRelevanceFilterEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        // Cache để tránh gọi AI liên tục cho cùng 1 title
        private readonly Dictionary<string, RelevanceFilterResult> _cache = new();

        public AiContentRelevanceFilterEngine(IAIExecutorService aiExecutor, ILogger<AiContentRelevanceFilterEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<RelevanceFilterResult> ClassifyAsync(string videoTitle, string channelName, CancellationToken ct = default)
        {
            var cacheKey = $"{channelName}|{videoTitle}".ToLowerInvariant();
            lock (_cache)
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                    return cached;
            }

            var systemPrompt = @"Bạn là bộ lọc nội dung cho thumbnail YouTube library.
Phân loại video dựa trên tiêu đề và tên kênh.

Trả về JSON:
{
  ""isIrrelevant"": true/false,
  ""reason"": ""Lý do nếu không liên quan, null nếu liên quan"",
  ""inferredMarketType"": ""Domestic"" hoặc ""International"" hoặc null
}

Đánh dấu isIrrelevant = true nếu video thuộc:
- MV nhạc, lyrics, audio
- Quảng cáo, sponsored, PR
- Giải trí thuần tuý (meme, hài kịch)
- Kênh vùng miền/ngôn ngữ không liên quan đến chủ đề finance/crypto/news
- Shorts mang tính giải trí

inferredMarketType: null nếu không xác định được.";

            var userPrompt = $@"Phân loại video:
Tiêu đề: ""{videoTitle}""
Kênh: ""{channelName}""";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = userPrompt }
                },
                Temperature = 0.1,
                MaxTokens = 256
            };

            try
            {
                var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

                var parsed = JsonSerializer.Deserialize<FilterResponse>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed == null) return CacheAndReturn(cacheKey, new RelevanceFilterResult(false, null, null));

                CMarketScope? marketType = parsed.InferredMarketType?.ToLower() switch
                {
                    "domestic" => CMarketScope.Domestic,
                    "international" => CMarketScope.International,
                    _ => null
                };

                var resultObj = new RelevanceFilterResult(
                    parsed.IsIrrelevant,
                    parsed.IsIrrelevant ? parsed.Reason : null,
                    marketType);

                return CacheAndReturn(cacheKey, resultObj);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ContentRelevanceFilter AI call failed, defaulting to relevant");
                return CacheAndReturn(cacheKey, new RelevanceFilterResult(false, null, null));
            }
        }

        private RelevanceFilterResult CacheAndReturn(string key, RelevanceFilterResult result)
        {
            lock (_cache)
            {
                _cache[key] = result;
            }
            return result;
        }

        private sealed record FilterResponse(bool IsIrrelevant, string? Reason, string? InferredMarketType);
    }
}
