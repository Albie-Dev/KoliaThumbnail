using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine phân tích sâu tin tức sử dụng AI thật qua AIExecutorService.
    /// </summary>
    public class AiNewsDeepAnalysisEngine : INewsDeepAnalysisEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiNewsDeepAnalysisEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        public AiNewsDeepAnalysisEngine(IAIExecutorService aiExecutor, ILogger<AiNewsDeepAnalysisEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<NewsDeepAnalysisResult> AnalyzeAsync(
            string title, string sourceUrl, string sourceName, string fullContentOrSummary,
            CancellationToken ct = default)
        {
            var systemPrompt = @"Bạn là chuyên gia phân tích tin tức tài chính/kinh tế.
Phân tích bài báo sau và trả về JSON:
{
  ""macroEventSummary"": [""Sự kiến vĩ mô 1"", ""Sự kiến vĩ mô 2""],
  ""marketReactionJson"": ""{\""reaction\"": \""Tích cực/Tiêu cực\"", \""volume\"": \""Tăng/Giảm X%\""}"",
  ""expectationShortTerm"": ""Kỳ vọng ngắn hạn"",
  ""expectationLongTerm"": ""Kỳ vọng dài hạn"",
  ""sentimentOverviewJson"": ""{\""sentiment\"": \""Tâm lý chung\"", \""fear_greed_index\"": 50}"",
  ""emotionTags"": [""Hope"", ""Surprise""],
  ""emotionReason"": ""Lý do cảm xúc"",
  ""wasTranslatedFromForeign"": false,
  ""missingDataNote"": null
}";

            var userPrompt = $@"
## Tiêu đề
{title}

## Nguồn
{sourceName} ({sourceUrl})

## Nội dung
{fullContentOrSummary}

Trả về JSON theo cấu trúc đã định.
Tag cảm xúc (emotionTags) chọn từ: Hope, Fear, Surprise, Anger, Trust, Anticipation, Sadness, Joy, Disgust.";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = userPrompt }
                },
                Temperature = 0.3,
                MaxTokens = 2048
            };

            var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

            try
            {
                var parsed = JsonSerializer.Deserialize<DeepAnalysisResponse>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed == null) throw new InvalidOperationException("Null response");

                return new NewsDeepAnalysisResult(
                    parsed.MacroEventSummary ?? [],
                    parsed.MarketReactionJson ?? "{}",
                    parsed.ExpectationShortTerm ?? string.Empty,
                    parsed.ExpectationLongTerm ?? string.Empty,
                    parsed.SentimentOverviewJson ?? "{}",
                    ParseEmotionTags(parsed.EmotionTags),
                    parsed.EmotionReason ?? string.Empty,
                    parsed.WasTranslatedFromForeign,
                    parsed.MissingDataNote);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse deep analysis AI response");
                return new NewsDeepAnalysisResult([], "{}", string.Empty, string.Empty, "{}",
                    CEmotionTag.None, string.Empty, false, "Lỗi parse AI response");
            }
        }

        private static CEmotionTag ParseEmotionTags(List<string>? tags)
        {
            if (tags == null) return CEmotionTag.None;
            var result = CEmotionTag.None;
            foreach (var tag in tags)
            {
                if (Enum.TryParse<CEmotionTag>(tag, true, out var parsed))
                    result |= parsed;
            }
            return result;
        }

        private sealed record DeepAnalysisResponse(
            List<string>? MacroEventSummary, string? MarketReactionJson,
            string? ExpectationShortTerm, string? ExpectationLongTerm,
            string? SentimentOverviewJson, List<string>? EmotionTags,
            string? EmotionReason, bool WasTranslatedFromForeign, string? MissingDataNote);
    }
}
