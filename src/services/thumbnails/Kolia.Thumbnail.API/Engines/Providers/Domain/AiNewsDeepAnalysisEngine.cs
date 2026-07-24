using System.Text.Json;
using System.Text.Json.Serialization;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine phân tích sâu tin tức sử dụng AI thật qua AIExecutorService.
    /// Input là full-text bài báo (không phải meta description) để phân tích 4 tầng đầy đủ.
    /// </summary>
    public class AiNewsDeepAnalysisEngine : INewsDeepAnalysisEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiNewsDeepAnalysisEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";
        private const int DefaultMaxTokens = 500000;

        private const string SystemPrompt = @"Bạn là chuyên gia phân tích tin tức tài chính/kinh tế cho content creator.

YÊU CẦU BẮT BUỘC VỀ CHẤT LƯỢNG NỘI DUNG:
- Nội dung KHÔNG được quá ngắn hay chủ quan; mỗi mục phải là câu hoàn chỉnh, có căn cứ từ bài viết.
- PHẢI làm nổi bật mọi số liệu, mốc giá, tỷ lệ %, ngày tháng cụ thể được đề cập trong bài.
- Nếu bài viết là nguồn quốc tế (tiếng Anh hoặc ngôn ngữ khác), PHẢI tự dịch toàn bộ nội dung phân tích sang tiếng Việt — không được giữ nguyên tiếng nước ngoài.
- Nếu một hạng mục không có thông tin trong bài, PHẢI ghi chính xác chuỗi ""Chưa rõ"" — không bịa, không suy đoán số liệu.

Trả về CHÍNH XÁC JSON theo cấu trúc sau (không thêm text ngoài JSON, không markdown code fence):
{
  ""macroEventSummary"": [
    {""category"": ""Giá vàng"", ""content"": ""...""},
    {""category"": ""Địa chính trị"", ""content"": ""...""},
    {""category"": ""Ngoại hối"", ""content"": ""...""},
    {""category"": ""Năng lượng"", ""content"": ""...""},
    {""category"": ""Chính sách tiền tệ"", ""content"": ""...""},
    {""category"": ""Chứng khoán"", ""content"": ""...""},
    {""category"": ""Bất động sản"", ""content"": ""...""}
  ],
  ""marketReaction"": [
    {""marketOrTopic"": ""<tên thị trường liên quan trực tiếp đến bài, do bạn xác định>"", ""content"": ""...""},
    {""marketOrTopic"": ""Ý kiến nhà đầu tư / Chuyên gia"", ""content"": ""...""}
  ],
  ""expectationShortTerm"": ""Tác động ngắn hạn (1-3 tháng tới): ..."",
  ""expectationLongTerm"": ""Tác động dài hạn (6-12 tháng tới): ..."",
  ""sentimentOverview"": {""sentiment"": ""Optimistic"", ""rationale"": ""...""},
  ""emotionTags"": [""Fear"", ""Doubt""],
  ""emotionReason"": ""Giải thích ngắn gọn vì sao chọn các tag cảm xúc trên, gắn với tâm lý người xem."",
  ""wasTranslatedFromForeign"": true,
  ""missingDataNote"": null
}

QUAN TRỌNG:
- Mảng ""macroEventSummary"" PHẢI có đủ 7 hạng mục theo đúng thứ tự trên, kể cả khi content là ""Chưa rõ"". Có thể thêm hạng mục ""Khác"" ở cuối nếu bài có nội dung không thuộc 7 hạng mục trên.
- Mảng ""marketReaction"" mục cuối cùng LUÔN LUÔN là ""Ý kiến nhà đầu tư / Chuyên gia"" (ghi ""Chưa rõ"" nếu bài không trích dẫn ý kiến ai).
- ""sentiment"" chỉ chọn 1 trong 4 chuỗi: ""Optimistic"", ""Pessimistic"", ""Neutral"", ""Mixed"". KHÔNG tạo chỉ số số hoá (không có fear/greed index).
- emotionTags chọn từ: Fear, Doubt, Curiosity, Urgency, DecisionPressure, Surprise, Anger, Hope, FOMO.";

        public AiNewsDeepAnalysisEngine(IAIExecutorService aiExecutor, ILogger<AiNewsDeepAnalysisEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<NewsDeepAnalysisResult> AnalyzeAsync(
            string title, string sourceUrl, string sourceName,
            string fullArticleText, CMarketScope marketScope,
            CancellationToken ct = default)
        {
            var scopeHint = marketScope == CMarketScope.International
                ? "ĐÂY LÀ TIN QUỐC TẾ — bạn PHẢI dịch toàn bộ nội dung phân tích sang tiếng Việt."
                : "Đây là tin nội địa (tiếng Việt).";

            var userPrompt = $@"
## Tiêu đề
{title}

## Nguồn
{sourceName} ({sourceUrl})

## {scopeHint}

## Nội dung đầy đủ bài báo
{fullArticleText}

Phân tích theo đúng cấu trúc JSON đã định, tuân thủ nghiêm ngặt các yêu cầu bắt buộc ở trên.";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = SystemPrompt },
                    new() { Role = "user", Content = userPrompt }
                },
                Temperature = 0.3,
                MaxTokens = DefaultMaxTokens
            };

            var result = await _aiExecutor.ChatCompletionWithFunctionAsync(CAIFunctionType.NewsScoring, request, ct);

            DeepAnalysisResponse? parsed;
            try
            {
                var raw = StripMarkdownCodeFence(result.Content);

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

                parsed = JsonSerializer.Deserialize<DeepAnalysisResponse>(raw, jsonOptions)
                    ?? throw new JsonException("Null response");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse deep analysis AI response for {SourceUrl}", sourceUrl);
                throw new ExternalServiceException(
                    "AI trả về dữ liệu không đúng định dạng khi phân tích sâu. Vui lòng thử lại.", ex);
            }

            // Chuẩn hoá Tầng 1: đảm bảo đủ 7 hạng mục cố định
            var byCategory = (parsed.MacroEventSummary ?? [])
                .ToDictionary(m => m.Category, m => m.Content);
            var normalizedMacro = MacroEventCategories.Fixed
                .Select(cat => new MacroEventCategoryItem(cat, byCategory.GetValueOrDefault(cat, "Chưa rõ")))
                .Concat((parsed.MacroEventSummary ?? [])
                    .Where(m => !MacroEventCategories.Fixed.Contains(m.Category)))
                .ToList();

            // Chuẩn hoá Tầng 2: đảm bảo luôn có mục "Ý kiến nhà đầu tư / Chuyên gia" ở cuối
            var marketReaction = (parsed.MarketReaction ?? []).ToList();
            if (!marketReaction.Any(m => m.MarketOrTopic == "Ý kiến nhà đầu tư / Chuyên gia"))
                marketReaction.Add(new MarketReactionItem("Ý kiến nhà đầu tư / Chuyên gia", "Chưa rõ"));

            // Tầng 4: Robust sentiment parsing
            var sentimentOverview = ParseSentimentOverview(parsed.SentimentOverview);

            // Ép cứng cờ dịch thuật cho tin quốc tế
            var wasTranslated = parsed.WasTranslatedFromForeign;
            if (marketScope == CMarketScope.International && !wasTranslated)
            {
                _logger.LogWarning(
                    "AI báo wasTranslatedFromForeign=false cho tin quốc tế {SourceUrl} — ép về true.", sourceUrl);
                wasTranslated = true;
            }

            return new NewsDeepAnalysisResult(
                normalizedMacro,
                marketReaction,
                parsed.ExpectationShortTerm ?? "Chưa rõ",
                parsed.ExpectationLongTerm ?? "Chưa rõ",
                sentimentOverview,
                ParseEmotionTags(parsed.EmotionTags),
                parsed.EmotionReason ?? string.Empty,
                wasTranslated,
                parsed.MissingDataNote);
        }

        private static SentimentOverview ParseSentimentOverview(RawSentimentOverview? raw)
        {
            if (raw == null) return new SentimentOverview(CMarketSentiment.Neutral, "Chưa rõ");

            var rationale = raw.Rationale ?? "Chưa rõ";
            var sentiment = CMarketSentiment.Neutral;

            if (raw.Sentiment.ValueKind == JsonValueKind.String)
            {
                var str = raw.Sentiment.GetString()?.Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    if (Enum.TryParse<CMarketSentiment>(str, true, out var parsed))
                    {
                        sentiment = parsed;
                    }
                    else
                    {
                        var lower = str.ToLowerInvariant();
                        if (lower.Contains("lạc quan") || lower.Contains("optimistic") || lower.Contains("positive") || lower.Contains("tích cực"))
                            sentiment = CMarketSentiment.Optimistic;
                        else if (lower.Contains("bi quan") || lower.Contains("pessimistic") || lower.Contains("negative") || lower.Contains("tiêu cực"))
                            sentiment = CMarketSentiment.Pessimistic;
                        else if (lower.Contains("giằng co") || lower.Contains("mixed") || lower.Contains("trái chiều"))
                            sentiment = CMarketSentiment.Mixed;
                        else
                            sentiment = CMarketSentiment.Neutral;
                    }
                }
            }
            else if (raw.Sentiment.ValueKind == JsonValueKind.Number && raw.Sentiment.TryGetInt32(out var val))
            {
                if (Enum.IsDefined(typeof(CMarketSentiment), val))
                    sentiment = (CMarketSentiment)val;
            }

            return new SentimentOverview(sentiment, rationale);
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

        private sealed record RawSentimentOverview(
            JsonElement Sentiment,
            string? Rationale);

        private sealed record DeepAnalysisResponse(
            List<MacroEventCategoryItem>? MacroEventSummary,
            List<MarketReactionItem>? MarketReaction,
            string? ExpectationShortTerm,
            string? ExpectationLongTerm,
            RawSentimentOverview? SentimentOverview,
            List<string>? EmotionTags,
            string? EmotionReason,
            bool WasTranslatedFromForeign,
            string? MissingDataNote);
    }
}
