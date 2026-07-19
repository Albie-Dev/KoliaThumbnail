using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine phân tích thumbnail sử dụng AI vision (Gemini) qua AIExecutorService.
    /// Gửi ảnh thumbnail cho AI để phân tích yếu tố trực quan.
    /// </summary>
    public class AiThumbnailAnalysisEngine : IThumbnailAnalysisEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiThumbnailAnalysisEngine> _logger;

        private const CAIProviderType DefaultProvider = CAIProviderType.Gemini;
        private const string DefaultModel = "gemini-2.0-flash";

        public AiThumbnailAnalysisEngine(IAIExecutorService aiExecutor, ILogger<AiThumbnailAnalysisEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<ThumbnailDeepAnalysisResult> AnalyzeAsync(
            string thumbnailImageUrl, string videoTitle, CancellationToken ct = default)
        {
            var systemPrompt = @"Bạn là chuyên gia phân tích thumbnail YouTube.
Phân tích ảnh thumbnail và trả về JSON:
{
  ""thumbnailFactorsJson"": ""{\""background\"": \""Mô tả nền\"", \""person\"": \""Mô tả nhân vật/nổi bật\"", \""colorScheme\"": \""Bảng màu\"", \""composition\"": \""Bố cục\""}"",
  ""titleTextAnalysis"": ""Phân tích chữ trên thumbnail (font, màu, vị trí, kích thước)"",
  ""videoTitleAnalysis"": ""Phân tích tiêu đề video gốc (cách đặt tiêu đề, cảm xúc)"",
  ""displayTextStyleNote"": ""Gợi ý phong cách chữ cho thumbnail tương tự (font, màu, vị trí)""
}";

            var userPrompt = $@"
Phân tích ảnh thumbnail từ video: ""{videoTitle}""
URL ảnh: {thumbnailImageUrl}

Mô tả chi tiết:
1. Bố cục tổng thể (background, foreground, điểm nhấn)
2. Màu sắc chủ đạo và tương phản
3. Chữ trên ảnh (nếu có): font, màu, kích thước, vị trí
4. Nhân vật/KOL (nếu có): biểu cảm, góc chụp, vị trí
5. Yếu tố gây chú ý (CTAs, mũi tên, khung viền...)";

            var request = new ChatCompletionRequest
            {
                Model = DefaultModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new()
                    {
                        Role = "user",
                        Content = userPrompt,
                        Images = new List<ChatImageAttachment>
                        {
                            new()
                            {
                                Source = thumbnailImageUrl,
                                SourceType = ChatImageSourceType.Url
                            }
                        }
                    }
                },
                Temperature = 0.2,
                MaxTokens = 2048
            };

            var result = await _aiExecutor.ChatCompletionWithFallbackAsync(DefaultProvider, request, ct);

            try
            {
                var parsed = JsonSerializer.Deserialize<AnalysisResponse>(result.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed == null) throw new InvalidOperationException("Null response");

                return new ThumbnailDeepAnalysisResult(
                    parsed.ThumbnailFactorsJson ?? "{}",
                    parsed.TitleTextAnalysis ?? string.Empty,
                    parsed.VideoTitleAnalysis ?? string.Empty,
                    parsed.DisplayTextStyleNote ?? string.Empty);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse thumbnail analysis AI response");
                return new ThumbnailDeepAnalysisResult("{}", string.Empty, string.Empty, string.Empty);
            }
        }

        private sealed record AnalysisResponse(
            string? ThumbnailFactorsJson, string? TitleTextAnalysis,
            string? VideoTitleAnalysis, string? DisplayTextStyleNote);
    }
}
