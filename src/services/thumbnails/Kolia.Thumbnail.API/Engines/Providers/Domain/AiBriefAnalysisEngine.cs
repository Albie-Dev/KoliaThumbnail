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
        public async Task<BriefAnalysisFromPasteResult> AnalyzeFromPastedTextAsync(
            string rawText,
            CancellationToken ct = default)
        {
            return await AnalyzeFromTextWithFilesAsync(rawText, null, ct);
        }

        /// <summary>
        /// Phân tích văn bản kết hợp với file đính kèm — file được gửi trực tiếp lên AI
        /// (inline_data) thay vì đọc text server-side, hỗ trợ PDF, ảnh, Word, ...
        /// </summary>
        public async Task<BriefAnalysisFromPasteResult> AnalyzeFromFilesAsync(
            List<ChatFileAttachment> files,
            CancellationToken ct = default)
        {
            return await AnalyzeFromTextWithFilesAsync(null, files, ct);
        }

        /// <summary>
        /// Internal: phân tích từ text hoặc files (hoặc cả 2), gửi trực tiếp lên AI.
        /// </summary>
        private async Task<BriefAnalysisFromPasteResult> AnalyzeFromTextWithFilesAsync(
            string? rawText, List<ChatFileAttachment>? files,
            CancellationToken ct = default)
        {
            var systemPrompt = @"Bạn là chuyên gia phân tích nội dung livestream/video.
Dưới đây là thông tin về nội dung livestream/video. Hãy phân tích và trả về JSON đúng cấu trúc:

{
  ""overview"": ""Tổng quan nội dung livestream/video (khoảng 2-3 câu, khái quát đầy đủ)"",
  ""viewpoint"": ""Quan điểm / lập trường muốn nhấn mạnh trong video"",
  ""keyData"": ""Dữ liệu / số liệu quan trọng cần chú ý"",
  ""topic"": ""Chủ đề chính của video (ngắn gọn, 1 dòng)"",
  ""mainMessage"": ""Thông điệp chính cần truyền tải đến người xem"",
  ""highlightData"": ""Các dữ liệu / số liệu nổi bật nhất cần nhấn mạnh"",
  ""suggestedKeywords"": [""từ khóa 1"", ""từ khóa 2"", ""từ khóa 3""]
}

Lưu ý:
- overview và viewpoint phải được VIẾT LẠI bằng GIỌNG VĂN của chuyên gia phân tích, KHÔNG copy nguyên văn từ văn bản đầu vào.
- keyData là danh sách các dữ liệu/số liệu được đề cập trong văn bản.
- topic là chủ đề chính, ngắn gọn súc tích.
- mainMessage là thông điệp chính rút ra từ văn bản.
- highlightData là dữ liệu nổi bật nhất (có thể giống hoặc khác keyData).
- suggestedKeywords: từ 3-5 từ khóa chính.";

            var userContent = !string.IsNullOrWhiteSpace(rawText)
                ? $@"## Văn bản đầu vào

{rawText}"
                : "Hãy phân tích nội dung file đính kèm bên dưới.";

            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = userContent,
                Files = files
            };

            var request = new ChatCompletionRequest
            {
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    userMessage
                },
                Temperature = 0.3,
                MaxTokens = 4096
            };

            var result = await _aiExecutor.ChatCompletionWithFunctionAsync(
                CAIFunctionType.ContentBriefAnalysis, request, ct);

            var json = result.Content;
            json = StripMarkdownCodeBlock(json);

            _logger.LogDebug("BriefAnalysisFromPaste AI response: {Json}", json);

            try
            {
                var parsed = JsonSerializer.Deserialize<BriefAnalysisFromPasteResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null)
                    throw new InvalidOperationException("AI returned null JSON.");

                // keyData và highlightData có thể là string hoặc array — xử lý linh hoạt
                var keyData = ConvertJsonNodeToString(parsed.KeyData);
                var highlightData = ConvertJsonNodeToString(parsed.HighlightData);

                return new BriefAnalysisFromPasteResult(
                    parsed.Overview ?? "Không xác định",
                    parsed.Viewpoint ?? "Không xác định",
                    keyData,
                    parsed.Topic ?? "Không xác định",
                    parsed.MainMessage ?? "Không xác định",
                    highlightData,
                    parsed.SuggestedKeywords ?? []);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse BriefAnalysisFromPaste AI response: {Json}", json);
                return new BriefAnalysisFromPasteResult(
                    "Tổng quan nội dung", "Quan điểm phân tích",
                    "Dữ liệu quan trọng", "Chủ đề livestream",
                    "Thông điệp chính", "Dữ liệu nổi bật",
                    new List<string>());
            }
        }

        /// <summary>
        /// Chuyển đổi JsonNode (có thể là string hoặc array) thành string.
        /// </summary>
        private static string ConvertJsonNodeToString(System.Text.Json.Nodes.JsonNode? node)
        {
            if (node == null)
                return "Không có dữ liệu";

            // Array: ["item1", "item2", ...] -> join bằng newline
            if (node is System.Text.Json.Nodes.JsonArray arr)
                return string.Join("\n", arr.Select(x => x?.GetValue<string>() ?? ""));

            // String: "some text"
            if (node is System.Text.Json.Nodes.JsonValue val && val.TryGetValue(out string? str))
                return str;

            // Fallback: ToString()
            return node.ToString();
        }

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

        /// <summary>
        /// Response từ AI cho AnalyzeFromPastedTextAsync.
        /// keyData và highlightData có thể là string hoặc array (AI đôi khi trả về mảng).
        /// Dùng System.Text.Json.Nodes.JsonNode để xử lý linh hoạt.
        /// </summary>
        private sealed record BriefAnalysisFromPasteResponse(
            string? Overview,
            string? Viewpoint,
            System.Text.Json.Nodes.JsonNode? KeyData,
            string? Topic,
            string? MainMessage,
            System.Text.Json.Nodes.JsonNode? HighlightData,
            List<string>? SuggestedKeywords);
    }
}
