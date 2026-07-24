using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine dịch thuật và mở rộng keyword hai ngôn ngữ (Việt - Anh) sử dụng AIExecutorService.
    /// Giải quyết bug: keyword người dùng nhập/gợi ý bị lẫn lộn tiếng Anh và tiếng Việt,
    /// khi tìm trên nguồn quốc tế cần dùng tiếng Anh, nguồn nội địa cần dùng tiếng Việt.
    /// </summary>
    public class AiKeywordTranslationEngine : IKeywordTranslationEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiKeywordTranslationEngine> _logger;

        private const string SystemPrompt = @"Bạn là trợ lý dịch thuật và mở rộng từ khóa tìm kiếm tin tức tài chính / kinh tế song ngữ (Việt - Anh).
Nhiệm vụ: Nhận vào danh sách keyword (có thể gồm tiếng Việt, tiếng Anh hoặc lẫn lộn cả hai). Với mỗi keyword:
- Nếu là tiếng Việt: hãy dịch sang tiếng Anh tương ứng (ngắn gọn, chuẩn thuật ngữ tài chính).
- Nếu là tiếng Anh: hãy dịch sang tiếng Việt tương ứng.
- Giữ lại cả bản gốc.

Trả về duy nhất một JSON object theo đúng định dạng sau (không markdown code fence, không giải thích):
{
  ""vietnameseKeywords"": [""Fed hạ lãi suất"", ""Giá vàng"", ""Lạm phát""],
  ""englishKeywords"": [""Fed rate cut"", ""Gold price"", ""Inflation""]
}";

        public AiKeywordTranslationEngine(
            IAIExecutorService aiExecutor,
            ILogger<AiKeywordTranslationEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<TranslatedKeywordSet> TranslateAndExpandAsync(
            IEnumerable<string> keywords,
            CancellationToken ct = default)
        {
            var origList = keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct()
                .ToList();

            if (origList.Count == 0)
            {
                return new TranslatedKeywordSet([], [], [], []);
            }

            try
            {
                var userPrompt = $"Danh sách keyword đầu vào: {string.Join(", ", origList)}";

                var request = new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        new() { Role = "system", Content = SystemPrompt },
                        new() { Role = "user", Content = userPrompt }
                    },
                    Temperature = 0.1,
                    MaxTokens = 100000
                };

                var response = await _aiExecutor.ChatCompletionWithFunctionAsync(CAIFunctionType.NewsScoring, request, ct);
                var raw = StripMarkdownCodeFence(response.Content);

                var parsed = JsonSerializer.Deserialize<TranslationResponse>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed != null && (parsed.VietnameseKeywords?.Count > 0 || parsed.EnglishKeywords?.Count > 0))
                {
                    var vi = (parsed.VietnameseKeywords ?? []).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList();
                    var en = (parsed.EnglishKeywords ?? []).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList();
                    var combined = origList.Concat(vi).Concat(en).Distinct().ToList();

                    return new TranslatedKeywordSet(origList, vi, en, combined);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to translate keywords using AI. Falling back to original keywords.");
            }

            // Fallback: trả về nguyên gốc nếu AI lỗi hoặc timeout
            return new TranslatedKeywordSet(origList, origList, origList, origList);
        }

        private static string StripMarkdownCodeFence(string raw)
        {
            var jsonStart = raw.IndexOf("```", StringComparison.Ordinal);
            if (jsonStart < 0) return raw;

            var firstNewline = raw.IndexOf('\n', jsonStart);
            var contentStart = firstNewline >= 0 ? firstNewline + 1 : jsonStart + 3;
            var jsonEnd = raw.LastIndexOf("```", StringComparison.Ordinal);
            return jsonEnd > contentStart
                ? raw[contentStart..jsonEnd].Trim()
                : raw[contentStart..].Trim();
        }

        private sealed record TranslationResponse(
            List<string>? VietnameseKeywords,
            List<string>? EnglishKeywords);
    }
}
