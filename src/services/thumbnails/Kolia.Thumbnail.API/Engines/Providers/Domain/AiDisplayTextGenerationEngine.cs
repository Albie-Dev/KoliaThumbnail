using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine sinh chữ hiển thị trên thumbnail sử dụng AI thật qua AIExecutorService.
    /// </summary>
    public class AiDisplayTextGenerationEngine : IDisplayTextGenerationEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiDisplayTextGenerationEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        public AiDisplayTextGenerationEngine(IAIExecutorService aiExecutor, ILogger<AiDisplayTextGenerationEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<DisplayTextGenerationResult> GenerateAsync(
            Dictionary<Guid, string> newsSummaries, string topicContext, CancellationToken ct = default)
        {
            var newsList = string.Join("\n---\n", newsSummaries.Select(kv =>
                $"[News {kv.Key}]: {kv.Value}"));

            var systemPrompt = @"Bạn là chuyên gia content creator, chuyên viết chữ hiển thị trên thumbnail YouTube.
Với mỗi tin tức, hãy đề xuất 2-3 câu chữ ngắn (dưới 40 ký tự) để đặt lên thumbnail.

Nguyên tắc:
- Ngắn gọn, dễ đọc, kích thích tò mò
- Dùng từ ngữ cảm xúc mạnh: ĐỪNG BỎ LỠ, SỐC, CỰC KỲ, NÓNG
- Có thể dùng chữ in hoa, dấu chấm than
- Phù hợp văn hoá Việt Nam
- Mỗi câu chữ phải đứng riêng biệt, không dùng dấu cách quá dài

Trả về JSON:
{
  ""options"": [
    { ""sourceNewsItemId"": ""guid"", ""content"": ""CHỮ TRÊN ẢNH"" }
  ]
}";

            var userPrompt = $@"
## Chủ đề
{topicContext}

## Danh sách tin tức đã chọn
{newsList}

Với mỗi tin, đề xuất 2-3 câu chữ thumbnail phù hợp.
Trả về JSON theo cấu trúc.";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = userPrompt }
                },
                Temperature = 0.7,
                MaxTokens = 2048
            };

            var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

            try
            {
                var parsed = JsonSerializer.Deserialize<DisplayTextResponse>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed?.Options == null || parsed.Options.Count == 0)
                    return FallbackOptions(newsSummaries);

                var options = new List<(Guid SourceNewsItemId, string Content)>();
                foreach (var opt in parsed.Options)
                {
                    if (Guid.TryParse(opt.SourceNewsItemId, out var newsId) && !string.IsNullOrWhiteSpace(opt.Content))
                    {
                        options.Add((newsId, opt.Content.Trim()));
                    }
                }

                return options.Count > 0
                    ? new DisplayTextGenerationResult(options)
                    : FallbackOptions(newsSummaries);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse display text AI response");
                return FallbackOptions(newsSummaries);
            }
        }

        private static DisplayTextGenerationResult FallbackOptions(Dictionary<Guid, string> newsSummaries)
        {
            var options = new List<(Guid, string)>();
            foreach (var kv in newsSummaries)
            {
                options.Add((kv.Key, "ĐỪNG BỎ LỠ!"));
                options.Add((kv.Key, "SẬP HAY BỨT PHÁ?"));
            }
            return new DisplayTextGenerationResult(options);
        }

        private sealed record TextOption(string SourceNewsItemId, string Content);
        private sealed record DisplayTextResponse(List<TextOption>? Options);
    }
}
