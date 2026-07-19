using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine chấm điểm tin tức sử dụng AI thật qua AIExecutorService.
    /// Batch scoring: gửi tất cả tin cùng lúc trong 1 prompt để AI so sánh tương đối.
    /// </summary>
    public class AiNewsScoringEngine : INewsScoringEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiNewsScoringEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

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

            var systemPrompt = @"Bạn là chuyên gia phân tích tin tức cho content creator.
Đánh giá từng tin theo các tiêu chí sau (thang 0-30 mỗi tiêu chí):
- relevanceToTopicScore: Mức độ liên quan đến chủ đề (0-30)
- importanceImpactScore: Tầm quan trọng/tác động (0-30)
- emotionPotentialScore: Tiềm năng gây cảm xúc cho người xem (0-30)
- noveltyDataScore: Tính mới/dữ liệu độc đáo (0-30)
- totalScore: Tổng điểm (0-120)
- recommendation: ""ShouldSelect"" hoặc ""CanSelect"" hoặc ""NotPriority""
- relevanceLevel: ""High"", ""Medium"", hoặc ""Low""
- summaryOverview: Tóm tắt ngắn gọn (1-2 câu)
- suggestedKeywordsForThumbnail: Keyword gợi ý cho thumbnail (cách nhau bằng ;)

Trả về JSON array:
[
  {
    ""newsItemId"": ""guid-1"",
    ""relevanceToTopicScore"": 25,
    ""importanceImpactScore"": 20,
    ""emotionPotentialScore"": 18,
    ""noveltyDataScore"": 15,
    ""totalScore"": 78,
    ""recommendation"": ""ShouldSelect"",
    ""relevanceLevel"": ""High"",
    ""summaryOverview"": ""Tóm tắt..."",
    ""suggestedKeywordsForThumbnail"": ""Keyword1;Keyword2""
  }
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
                MaxTokens = 4096
            };

            var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

            try
            {
                var scoredList = JsonSerializer.Deserialize<List<ScoredItemResponse>>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (scoredList == null) return FallbackScoring(items, topicContext);

                var dict = new Dictionary<Guid, NewsScoringResult>();
                foreach (var s in scoredList)
                {
                    if (Guid.TryParse(s.NewsItemId, out var id))
                    {
                        dict[id] = new NewsScoringResult(
                            s.RelevanceToTopicScore, s.ImportanceImpactScore,
                            s.EmotionPotentialScore, s.NoveltyDataScore, s.TotalScore,
                            ParseRecommendation(s.Recommendation),
                            ParseRelevanceLevel(s.RelevanceLevel),
                            s.SummaryOverview ?? "Không có tóm tắt",
                            s.SuggestedKeywordsForThumbnail);
                    }
                }

                // Fallback cho item nào AI không trả về
                foreach (var item in items)
                {
                    if (!dict.ContainsKey(item.NewsItemId))
                    {
                        dict[item.NewsItemId] = CreateDefaultScore(item.Title);
                    }
                }

                return dict;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI scoring response");
                return FallbackScoring(items, topicContext);
            }
        }

        private static Dictionary<Guid, NewsScoringResult> FallbackScoring(
            IReadOnlyList<(Guid NewsItemId, string Title, string SourceName, string SummaryRaw)> items, string topicContext)
        {
            return items.ToDictionary(item => item.NewsItemId, item => CreateDefaultScore(item.Title));
        }

        private static NewsScoringResult CreateDefaultScore(string title)
        {
            return new NewsScoringResult(15, 10, 10, 8, 43,
                CNewsRecommendation.CanSelect, CRelevanceLevel.Medium,
                $"Tin: {title}", null);
        }

        private static CNewsRecommendation ParseRecommendation(string? val) => val?.ToLower() switch
        {
            "shouldselect" => CNewsRecommendation.ShouldSelect,
            "notpriority" => CNewsRecommendation.NotPriority,
            _ => CNewsRecommendation.CanSelect
        };

        private static CRelevanceLevel ParseRelevanceLevel(string? val) => val?.ToLower() switch
        {
            "high" => CRelevanceLevel.High,
            "low" => CRelevanceLevel.Low,
            _ => CRelevanceLevel.Medium
        };

        private sealed record ScoredItemResponse(
            string NewsItemId, int RelevanceToTopicScore, int ImportanceImpactScore,
            int EmotionPotentialScore, int NoveltyDataScore, int TotalScore,
            string? Recommendation, string? RelevanceLevel,
            string? SummaryOverview, string? SuggestedKeywordsForThumbnail);
    }
}
