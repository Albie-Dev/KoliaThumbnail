namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Request gọi chat completion.
    /// </summary>
    public class ChatCompletionRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public List<ChatMessage> Messages { get; set; } = new();

        /// <summary>Điều chỉnh độ ngẫu nhiên của output (0.0 - 2.0 tùy provider).</summary>
        public double? Temperature { get; set; }

        /// <summary>Giới hạn số token sinh ra tối đa.</summary>
        public int? MaxTokens { get; set; }

        /// <summary>Top-p sampling.</summary>
        public double? TopP { get; set; }

        /// <summary>Danh sách chuỗi dừng sinh sớm.</summary>
        public List<string>? StopSequences { get; set; }

        /// <summary>Danh sách tool/function khai báo cho model (function calling).</summary>
        public List<ChatToolDefinition>? Tools { get; set; }

        /// <summary>Bắt buộc model trả JSON hợp lệ theo schema (nếu provider hỗ trợ).</summary>
        public bool ResponseAsJson { get; set; }

        /// <summary>User id để provider theo dõi/giới hạn lạm dụng (theo khuyến nghị 1 số API).</summary>
        public string? UserIdentifier { get; set; }
    }

    /// <summary>
    /// 1 tin nhắn trong hội thoại.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>system, user, assistant, tool</summary>
        public string Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        /// <summary>
        /// Danh sách ảnh đính kèm (base64 hoặc url) - dùng cho model có vision.
        /// </summary>
        public List<ChatImageAttachment>? Images { get; set; }

        /// <summary>
        /// Danh sách file đính kèm (PDF, Word, text, ...) - dùng cho model đa phương thức
        /// như Gemini. File có thể được upload lên provider (fileUri) hoặc gửi dạng base64.
        /// </summary>
        public List<ChatFileAttachment>? Files { get; set; }

        /// <summary>
        /// Nếu Role = "tool": id của tool call tương ứng đang phản hồi.
        /// </summary>
        public string? ToolCallId { get; set; }

        /// <summary>
        /// Nếu Role = "assistant" và model gọi tool: danh sách tool được gọi.
        /// </summary>
        public List<ChatToolCall>? ToolCalls { get; set; }
    }

    public class ChatImageAttachment
    {
        /// <summary>base64 hoặc url</summary>
        public string Source { get; set; } = default!;

        public ChatImageSourceType SourceType { get; set; } = ChatImageSourceType.Url;

        public string? MimeType { get; set; }
    }

    /// <summary>
    /// File đính kèm trong chat message.
    /// </summary>
    public class ChatFileAttachment
    {
        /// <summary>Đường dẫn file trên server hoặc tên file để log.</summary>
        public string FileName { get; set; } = default!;

        /// <summary>Nội dung file dạng base64 (nếu gửi inline).</summary>
        public string? Base64Content { get; set; }

        /// <summary>MIME type của file (ví dụ: application/pdf, text/plain).</summary>
        public string MimeType { get; set; } = "application/octet-stream";

        /// <summary>
        /// URI của file sau khi upload lên provider (Gemini File API, ...).
        /// Nếu có, Base64Content sẽ được bỏ qua.
        /// </summary>
        public string? FileUri { get; set; }
    }

    public enum ChatImageSourceType
    {
        Url = 0,
        Base64 = 1
    }

    public class ChatToolDefinition
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        /// <summary>JSON schema mô tả tham số của tool (dạng chuỗi JSON).</summary>
        public string ParametersJsonSchema { get; set; } = default!;
    }

    public class ChatToolCall
    {
        public string Id { get; set; } = default!;
        public string ToolName { get; set; } = default!;

        /// <summary>Tham số model truyền vào, dạng chuỗi JSON.</summary>
        public string ArgumentsJson { get; set; } = default!;
    }

    /// <summary>
    /// Kết quả trả về (không streaming).
    /// </summary>
    public class ChatCompletionResult
    {
        public string Content { get; set; } = default!;

        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens => PromptTokens + CompletionTokens;

        /// <summary>stop, length, tool_calls, content_filter...</summary>
        public string? FinishReason { get; set; }

        /// <summary>Nếu model gọi tool thay vì trả lời trực tiếp.</summary>
        public List<ChatToolCall>? ToolCalls { get; set; }

        /// <summary>Model thực tế đã xử lý request (provider có thể tự route sang model khác).</summary>
        public string? ModelUsed { get; set; }
    }

    /// <summary>
    /// 1 chunk trả về khi streaming.
    /// </summary>
    public class ChatCompletionChunk
    {
        public string DeltaContent { get; set; } = default!;

        /// <summary>Tool call đang được stream dần (nếu có).</summary>
        public ChatToolCall? DeltaToolCall { get; set; }

        public bool IsFinal { get; set; }

        /// <summary>Chỉ có giá trị khi IsFinal = true.</summary>
        public string? FinishReason { get; set; }

        /// <summary>Chỉ có giá trị khi IsFinal = true (nhiều provider chỉ trả usage ở chunk cuối).</summary>
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
    }
}