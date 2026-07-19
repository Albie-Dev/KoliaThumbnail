using System.Text.Json;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain
{
    /// <summary>
    /// Engine phân tích Content Brief sử dụng AI thật (Gemini/Groq) qua AIExecutorService.
    /// Đọc provider từ DB, tự động fallback giữa các API key.
    /// </summary>
    public class AiBriefAnalysisEngine : IBriefAnalysisEngine
    {
        private readonly IAIExecutorService _aiExecutor;
        private readonly ILogger<AiBriefAnalysisEngine> _logger;

        public AiBriefAnalysisEngine(IAIExecutorService aiExecutor, ILogger<AiBriefAnalysisEngine> logger)
        {
            _aiExecutor = aiExecutor;
            _logger = logger;
        }

        public async Task<BriefAnalysisResult> AnalyzeAsync(
            string overview, string viewpoint, string keyData,
            string? importedRawText, string? externalSheetContent,
            string? manualPrompt = null,
            CancellationToken ct = default)
        {
            var systemPrompt = @"Bạn là chuyên gia phân tích nội dung livestream/video. 
Phân tích các thông tin đầu vào sau và trả về JSON đúng cấu trúc:
{
  ""topic"": ""Chủ đề chính của video"",
  ""mainMessage"": ""Thông điệp chính cần truyền tải"",
  ""highlightData"": ""Các dữ liệu/số liệu nổi bật cần nhấn mạnh"",
  ""suggestedKeywords"": [""từ khóa 1"", ""từ khóa 2"", ...]
}";

            var userPrompt = !string.IsNullOrWhiteSpace(manualPrompt)
                ? manualPrompt
                : BuildAutoPrompt(overview, viewpoint, keyData, importedRawText, externalSheetContent);

            var request = !string.IsNullOrWhiteSpace(manualPrompt)
                ? new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        new() { Role = "user", Content = manualPrompt }
                    },
                    Temperature = 0.3,
                    MaxTokens = 2048
                }
                : new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        new() { Role = "system", Content = systemPrompt },
                        new() { Role = "user", Content = userPrompt }
                    },
                    Temperature = 0.3,
                    MaxTokens = 2048
                };

            var result = await _aiExecutor.ChatCompletionWithFunctionAsync(
                CAIFunctionType.ContentBriefAnalysis, request, ct);

            var json = result.Content;

            // Xoá markdown code block nếu AI trả về ```json ... ```
            json = StripMarkdownCodeBlock(json);

            _logger.LogDebug("BriefAnalysis AI response: {Json}", json);

            try
            {
                var parsed = JsonSerializer.Deserialize<BriefAnalysisResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null) throw new InvalidOperationException("AI returned null JSON.");

                return new BriefAnalysisResult(
                    parsed.Topic ?? "Chủ đề không xác định",
                    parsed.MainMessage ?? "Thông điệp không xác định",
                    parsed.HighlightData ?? "Không có dữ liệu nổi bật",
                    parsed.SuggestedKeywords ?? []);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse BriefAnalysis AI response: {Json}", json);
                return new BriefAnalysisResult(
                    "Chủ đề livestream", "Thông điệp chính", "Dữ liệu nổi bật",
                    new List<string>());
            }
        }

        /// <summary>
        /// Xây dựng prompt tự động từ dữ liệu đầu vào của brief.
        /// Method này là public static để FE có thể tái tạo prompt tương tự
        /// cho tính năng "chỉnh sửa prompt thủ công".
        /// </summary>
        public static string BuildAutoPrompt(
            string overview, string viewpoint, string keyData,
            string? importedRawText = null, string? externalSheetContent = null)
        {
            var prompt = $@"
## Tổng quan
{overview}

## Quan điểm muốn nhấn mạnh
{viewpoint}

## Dữ liệu quan trọng
{keyData}
";

            if (!string.IsNullOrWhiteSpace(importedRawText))
                prompt += $"\n## Dữ liệu import (paste/file)\n{importedRawText}\n";

            if (!string.IsNullOrWhiteSpace(externalSheetContent))
                prompt += $"\n## Nội dung từ Google Sheet\n{externalSheetContent}\n";

            return prompt;
        }

        /// <summary>
        /// Strips markdown code block delimiters like ```json ... ``` from AI response.
        /// </summary>
        private static string StripMarkdownCodeBlock(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var trimmed = text.Trim();
            if (trimmed.StartsWith("```"))
            {
                // Find the first newline after the opening ```
                var start = trimmed.IndexOf('\n');
                if (start > 0)
                {
                    trimmed = trimmed[(start + 1)..];
                }
                // Remove trailing ```
                var end = trimmed.LastIndexOf("```");
                if (end >= 0)
                {
                    trimmed = trimmed[..end];
                }
            }

            return trimmed.Trim();
        }

        private sealed record BriefAnalysisResponse(
            string? Topic,
            string? MainMessage,
            string? HighlightData,
            List<string>? SuggestedKeywords);
    }
}
