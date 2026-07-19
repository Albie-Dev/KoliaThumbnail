using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine sinh tiêu đề video sử dụng AI thật qua AIExecutorService.
    /// </summary>
    public class AiVideoTitleGenerationEngine : IVideoTitleGenerationEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiVideoTitleGenerationEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        public AiVideoTitleGenerationEngine(IAIExecutorService aiExecutor, ILogger<AiVideoTitleGenerationEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<string> BuildPromptAsync(
            IEnumerable<string> thumbnailDisplayTexts, IEnumerable<string> newsSummaries,
            string topicContext, CancellationToken ct = default)
        {
            var texts = string.Join(", ", thumbnailDisplayTexts);
            var news = string.Join("\n", newsSummaries);

            return $@"Prompt tổng hợp cho chủ đề '{topicContext}'.
Chữ trên ảnh: [{texts}].
Tin liên quan: {news}.";
        }

        public async Task<VideoTitleGenerationResult> GenerateAsync(
            string builtPromptText, CTitleStyle style, int requestedCount, CancellationToken ct = default)
        {
            var styleDesc = style switch
            {
                CTitleStyle.WarningExpert => "cảnh báo từ chuyên gia (tông mạnh, chuyên nghiệp)",
                CTitleStyle.NeutralClear => "trung lập, rõ ràng (cung cấp thông tin thẳng thắn)",
                CTitleStyle.CuriosityClick => "kích thích tò mò (đặt câu hỏi hoặc gây tò mò)",
                _ => "trung lập, rõ ràng"
            };

            var systemPrompt = @$"Bạn là chuyên gia SEO YouTube, viết tiêu đề video hấp dẫn.
Viết {requestedCount} tiêu đề theo phong cách {styleDesc}.
Nguyên tắc:
- Dưới 70 ký tự
- Có từ khóa chính ở đầu
- Kích thích click
- Phù hợp văn hoá Việt Nam

Trả về JSON: {{ ""titles"": [""Tiêu đề 1"", ""Tiêu đề 2"", ...] }}";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = builtPromptText }
                },
                Temperature = 0.8,
                MaxTokens = 1024
            };

            var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

            try
            {
                var parsed = JsonSerializer.Deserialize<TitleResponse>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var titles = parsed?.Titles?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

                if (titles != null && titles.Count > 0)
                    return new VideoTitleGenerationResult(titles);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse video title AI response");
            }

            return FallbackTitles(style, requestedCount);
        }

        public async Task<VideoTitleGenerationResult> GenerateWithFeedbackAsync(
            string builtPromptText, CTitleStyle style, int requestedCount,
            string feedbackText, CancellationToken ct = default)
        {
            var enhancedPrompt = $"{builtPromptText}\n\nPhản hồi từ người dùng cần áp dụng: {feedbackText}";
            return await GenerateAsync(enhancedPrompt, style, requestedCount, ct);
        }

        private static VideoTitleGenerationResult FallbackTitles(CTitleStyle style, int count)
        {
            var titles = new List<string>();
            for (int i = 0; i < count; i++)
                titles.Add($"Tiêu đề gợi ý số {i + 1} theo phong cách {style}");
            return new VideoTitleGenerationResult(titles);
        }

        private sealed record TitleResponse(List<string>? Titles);
    }
}
