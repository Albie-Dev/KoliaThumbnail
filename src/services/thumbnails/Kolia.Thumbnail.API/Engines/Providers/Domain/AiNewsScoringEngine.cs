using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine chấm điểm tin tức sử dụng AI thật qua AIExecutorService.
    /// Batch scoring: gửi tất cả tin cùng lúc trong 1 prompt để AI so sánh tương đối.
    /// Đây là bước TRIAGE nhanh (lọc từ nhiều tin xuống vài tin đáng chú ý) — 
    /// KHÔNG làm phân tích sâu 4 tầng, việc đó thuộc về NewsDeepAnalysisEntity/DeepAnalyzeAsync.
    /// </summary>
    public class AiNewsScoringEngine : INewsScoringEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiNewsScoringEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        private const int MaxRelevance = 30;
        private const int MaxImportance = 20;
        private const int MaxEmotion = 20;
        private const int MaxNovelty = 15;
        private const int MaxDataQuality = 15;
        private const int MaxTotal = MaxRelevance + MaxImportance + MaxEmotion + MaxNovelty + MaxDataQuality; // = 100

        public AiNewsScoringEngine(IAIExecutorService aiExecutor, ILogger<AiNewsScoringEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, NewsScoringResult>> ScoreBatchAsync(
            IReadOnlyList<(Guid NewsItemId, string Title, string SourceName, string SummaryRaw)> items,
            string topicContext, CancellationToken ct = default)
        {
            if (items.Count == 0) return new Dictionary<Guid, NewsScoringResult>();

            var systemPrompt = $@"Bạn là chuyên gia phân tích tin tức tài chính cho content creator.
                Đánh giá từng tin theo đúng thang điểm sau (KHÔNG được vượt quá điểm tối đa của từng tiêu chí):
                - relevanceToTopicScore: Độ liên quan với chủ đề video đã khóa (0-{MaxRelevance})
                - importanceImpactScore: Độ quan trọng/tác động thị trường (0-{MaxImportance})
                - emotionPotentialScore: Khả năng tạo cảm xúc, khiến người xem muốn click (0-{MaxEmotion})
                - noveltyDataScore: Độ mới của tin (0-{MaxNovelty})
                - dataQualityScore: Có dữ liệu/số liệu nổi bật hay không (0-{MaxDataQuality})
                - emotionTags: Danh sách cảm xúc có thể khai thác cho thumbnail/title, chọn 1 hoặc nhiều trong: FOMO, Fear, Curiosity, Doubt, DecisionPressure, Urgency
                - relevanceLevel: ""High"", ""Medium"", hoặc ""Low""
                - summaryOverview: Tóm tắt NGẮN GỌN 1-3 câu, đủ để người đọc quyết định có nên xem chi tiết tin này không. KHÔNG cần phân tầng chi tiết — phần phân tích sâu 4 tầng sẽ được thực hiện riêng ở bước ""Phân tích sâu"" sau khi tin được chọn.
                - suggestedKeywordsForThumbnail: Keyword gợi ý tìm reference YouTube, cả tiếng Việt lẫn tiếng Anh nếu phù hợp (cách nhau bằng ;)

                KHÔNG tự tính totalScore hay recommendation — hệ thống sẽ tự tính dựa trên các điểm thành phần bạn trả về.

                Trả về JSON array:
                [
                {{
                    ""newsItemId"": ""guid-1"",
                    ""relevanceToTopicScore"": 25,
                    ""importanceImpactScore"": 18,
                    ""emotionPotentialScore"": 16,
                    ""noveltyDataScore"": 12,
                    ""dataQualityScore"": 13,
                    ""emotionTags"": [""Fear"", ""DecisionPressure""],
                    ""relevanceLevel"": ""High"",
                    ""summaryOverview"": ""Tóm tắt ngắn 1-3 câu..."",
                    ""suggestedKeywordsForThumbnail"": ""Keyword1;Keyword2""
                }}
                ]";

            var itemList = string.Join("\n---\n", items.Select((item, idx) =>
                $"[{idx + 1}] ID: {item.NewsItemId}\nTiêu đề: {item.Title}\nNguồn: {item.SourceName}\nTóm tắt: {item.SummaryRaw}"));

            var userPrompt = $@"
                ## Chủ đề ngữ cảnh
                {topicContext}

                ## Danh sách tin cần đánh giá
                {itemList}

                Trả về JSON array đúng cấu trúc với tất cả {items.Count} items.";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = userPrompt }
                },
                Temperature = 0.2,
                MaxTokens = 500000
            };

            var result = await _aiExecutor.ChatCompletionWithFunctionAsync(CAIFunctionType.NewsScoring, request, ct);

            try
            {
                // AI (Gemini) thường trả về JSON trong markdown code block.
                // Strip ```json ... ``` hoặc ``` ... ``` nếu có trước khi parse.
                var raw = result.Content;
                var jsonStart = raw.IndexOf("```", StringComparison.Ordinal);
                if (jsonStart >= 0)
                {
                    var firstNewline = raw.IndexOf('\n', jsonStart);
                    var contentStart = firstNewline >= 0 ? firstNewline + 1 : jsonStart + 3;
                    var jsonEnd = raw.LastIndexOf("```", StringComparison.Ordinal);
                    raw = jsonEnd > contentStart
                        ? raw[contentStart..jsonEnd].Trim()
                        : raw[contentStart..].Trim();
                }

                var scoredList = JsonSerializer.Deserialize<List<ScoredItemResponse>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (scoredList == null) return FallbackScoring(items);

                var dict = new Dictionary<Guid, NewsScoringResult>();
                foreach (var s in scoredList)
                {
                    if (!Guid.TryParse(s.NewsItemId, out var id)) continue;

                    var relevance = Math.Clamp(s.RelevanceToTopicScore, 0, MaxRelevance);
                    var importance = Math.Clamp(s.ImportanceImpactScore, 0, MaxImportance);
                    var emotion = Math.Clamp(s.EmotionPotentialScore, 0, MaxEmotion);
                    var novelty = Math.Clamp(s.NoveltyDataScore, 0, MaxNovelty);
                    var dataQuality = Math.Clamp(s.DataQualityScore, 0, MaxDataQuality);

                    var total = relevance + importance + emotion + novelty + dataQuality;

                    dict[id] = new NewsScoringResult(
                        relevance, importance, emotion, novelty, dataQuality, total,
                        CalculateRecommendation(total),
                        ParseRelevanceLevel(s.RelevanceLevel),
                        s.SummaryOverview ?? "Không có tóm tắt",
                        s.SuggestedKeywordsForThumbnail,
                        ParseEmotionTags(s.EmotionTags));
                }

                foreach (var item in items)
                {
                    if (!dict.ContainsKey(item.NewsItemId))
                        dict[item.NewsItemId] = CreateDefaultScore(item.Title);
                }

                return dict;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI scoring response");
                return FallbackScoring(items);
            }
        }

        private static CNewsRecommendation CalculateRecommendation(int total) => total switch
        {
            >= 80 => CNewsRecommendation.ShouldSelect,
            >= 65 => CNewsRecommendation.CanSelect,
            _ => CNewsRecommendation.NotPriority
        };

        private static CEmotionTag ParseEmotionTags(List<string>? tags)
        {
            if (tags == null) return CEmotionTag.None;
            var result = CEmotionTag.None;
            foreach (var t in tags)
            {
                if (Enum.TryParse<CEmotionTag>(t, true, out var parsed))
                    result |= parsed;
            }
            return result;
        }

        private static Dictionary<Guid, NewsScoringResult> FallbackScoring(
            IReadOnlyList<(Guid NewsItemId, string Title, string SourceName, string SummaryRaw)> items)
        {
            return items.ToDictionary(item => item.NewsItemId, item => CreateDefaultScore(item.Title));
        }

        private static NewsScoringResult CreateDefaultScore(string title)
        {
            const int total = 51;
            return new NewsScoringResult(15, 10, 10, 8, 8, total,
                CalculateRecommendation(total), CRelevanceLevel.Medium,
                $"Tin: {title}", null, CEmotionTag.None);
        }

        private static CRelevanceLevel ParseRelevanceLevel(string? val) => val?.ToLower() switch
        {
            "high" => CRelevanceLevel.High,
            "low" => CRelevanceLevel.Low,
            _ => CRelevanceLevel.Medium
        };

        private sealed record ScoredItemResponse(
            string NewsItemId, int RelevanceToTopicScore, int ImportanceImpactScore,
            int EmotionPotentialScore, int NoveltyDataScore, int DataQualityScore,
            List<string>? EmotionTags, string? RelevanceLevel,
            string? SummaryOverview, string? SuggestedKeywordsForThumbnail);
    }
}