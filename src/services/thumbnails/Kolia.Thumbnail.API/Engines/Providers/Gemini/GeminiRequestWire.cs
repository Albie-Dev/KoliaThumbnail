using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kolia.Thumbnail.API.Engines.Providers
{
    // ============================================================
    // Các DTO "wire" dưới đây map 1-1 với JSON body thực tế của Gemini API.
    // Gemini cung cấp 2 mặt bằng API cho cùng 1 backend:
    //   1) OpenAI-compatible layer (https://generativelanguage.googleapis.com/v1beta/openai)
    //      dùng cho chat/completions, embeddings, images/generations - giữ nguyên format
    //      request/response kiểu OpenAI nên field naming (snake_case) và cấu trúc giống hệt
    //      GroqChatRequestWire. Xem: https://ai.google.dev/gemini-api/docs/openai
    //   2) Native REST API (https://generativelanguage.googleapis.com/v1beta) dùng cho
    //      models.list và models.countTokens - 2 endpoint này có metadata phong phú hơn
    //      (input/output token limit thật, danh sách generation method thật) so với OpenAI-compat
    //      /models nên được ưu tiên dùng thay vì suy đoán/hardcode.
    // Các DTO này chỉ dùng nội bộ trong GeminiEngine, không lộ ra ngoài - các method public
    // của GeminiEngine luôn trả về DTO chung của hệ thống (ChatCompletionResult, AIModelInfo...).
    // ============================================================

    #region Chat Completion (OpenAI-compatible: /v1beta/openai/chat/completions)

    internal sealed class GeminiChatRequestWire
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("messages")]
        public List<GeminiMessageWire> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Gemini OpenAI-compat layer dùng "max_tokens" (không phải "max_completion_tokens"
        /// như OpenAI bản mới) - xác nhận qua ví dụ chính thức của Google.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("stream_options")]
        public GeminiStreamOptionsWire? StreamOptions { get; set; }

        [JsonPropertyName("tools")]
        public List<GeminiToolWire>? Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        public string? ToolChoice { get; set; }

        [JsonPropertyName("response_format")]
        public GeminiResponseFormatWire? ResponseFormat { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }
    }

    internal sealed class GeminiStreamOptionsWire
    {
        [JsonPropertyName("include_usage")]
        public bool IncludeUsage { get; set; }
    }

    internal sealed class GeminiResponseFormatWire
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;
    }

    internal sealed class GeminiMessageWire
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        /// <summary>
        /// Có thể là string đơn giản hoặc List&lt;object&gt; (content parts) khi có ảnh/audio đính kèm.
        /// </summary>
        [JsonPropertyName("content")]
        public object? Content { get; set; }

        [JsonPropertyName("tool_call_id")]
        public string? ToolCallId { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GeminiToolCallWire>? ToolCalls { get; set; }
    }

    internal sealed class GeminiToolWire
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public GeminiFunctionDefWire Function { get; set; } = default!;
    }

    internal sealed class GeminiFunctionDefWire
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public JsonElement Parameters { get; set; }
    }

    internal sealed class GeminiToolCallWire
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public GeminiToolCallFunctionWire Function { get; set; } = default!;
    }

    internal sealed class GeminiToolCallFunctionWire
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = default!;
    }

    internal sealed class GeminiChatResponseWire
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("choices")]
        public List<GeminiChoiceWire> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public GeminiUsageWire? Usage { get; set; }
    }

    internal sealed class GeminiChoiceWire
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public GeminiResponseMessageWire Message { get; set; } = default!;

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    internal sealed class GeminiResponseMessageWire
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GeminiToolCallWire>? ToolCalls { get; set; }
    }

    internal sealed class GeminiUsageWire
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    // -------- Streaming (SSE chunk) --------

    internal sealed class GeminiStreamChunkWire
    {
        [JsonPropertyName("choices")]
        public List<GeminiStreamChoiceWire>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public GeminiUsageWire? Usage { get; set; }
    }

    internal sealed class GeminiStreamChoiceWire
    {
        [JsonPropertyName("delta")]
        public GeminiStreamDeltaWire? Delta { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    internal sealed class GeminiStreamDeltaWire
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GeminiStreamToolCallDeltaWire>? ToolCalls { get; set; }
    }

    internal sealed class GeminiStreamToolCallDeltaWire
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("function")]
        public GeminiStreamFunctionDeltaWire? Function { get; set; }
    }

    internal sealed class GeminiStreamFunctionDeltaWire
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }

    #endregion

    #region Embeddings (OpenAI-compatible: /v1beta/openai/embeddings)

    internal sealed class GeminiEmbeddingRequestWire
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("input")]
        public List<string> Input { get; set; } = new();

        /// <summary>
        /// Số chiều vector mong muốn. Lưu ý: Google chỉ công bố hỗ trợ chính thức tham số
        /// này thông qua field native "output_dimensionality" (không phải endpoint OpenAI-compat) -
        /// gửi kèm "dimensions" ở đây theo tinh thần tương thích OpenAI, nhưng có thể bị Gemini
        /// bỏ qua nếu model không hỗ trợ rút gọn chiều.
        /// </summary>
        [JsonPropertyName("dimensions")]
        public int? Dimensions { get; set; }
    }

    internal sealed class GeminiEmbeddingResponseWire
    {
        [JsonPropertyName("data")]
        public List<GeminiEmbeddingDataWire> Data { get; set; } = new();

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("usage")]
        public GeminiEmbeddingUsageWire? Usage { get; set; }
    }

    internal sealed class GeminiEmbeddingDataWire
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; } = new();
    }

    internal sealed class GeminiEmbeddingUsageWire
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    #endregion

    #region Image Generation (OpenAI-compatible: /v1beta/openai/images/generations)

    internal sealed class GeminiImageGenerationRequestWire
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = default!;

        [JsonPropertyName("n")]
        public int N { get; set; } = 1;

        /// <summary>Định dạng "{width}x{height}", vd "1024x1024".</summary>
        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("response_format")]
        public string ResponseFormat { get; set; } = "b64_json";
    }

    internal sealed class GeminiImageGenerationResponseWire
    {
        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("data")]
        public List<GeminiImageDataWire> Data { get; set; } = new();
    }

    internal sealed class GeminiImageDataWire
    {
        [JsonPropertyName("b64_json")]
        public string? B64Json { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("revised_prompt")]
        public string? RevisedPrompt { get; set; }
    }

    #endregion

    #region Models (Native: /v1beta/models)

    internal sealed class GeminiModelListWire
    {
        [JsonPropertyName("models")]
        public List<GeminiModelWire> Models { get; set; } = new();

        [JsonPropertyName("nextPageToken")]
        public string? NextPageToken { get; set; }
    }

    internal sealed class GeminiModelWire
    {
        /// <summary>Dạng "models/gemini-2.5-flash" - cần strip prefix "models/" khi map sang ModelId.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("inputTokenLimit")]
        public int? InputTokenLimit { get; set; }

        [JsonPropertyName("outputTokenLimit")]
        public int? OutputTokenLimit { get; set; }

        /// <summary>Vd: ["generateContent", "countTokens"], ["embedContent"], ["predict"]...</summary>
        [JsonPropertyName("supportedGenerationMethods")]
        public List<string> SupportedGenerationMethods { get; set; } = new();
    }

    #endregion

    #region Count Tokens (Native: /v1beta/models/{model}:countTokens)

    internal sealed class GeminiCountTokensRequestWire
    {
        [JsonPropertyName("contents")]
        public List<GeminiCountTokensContentWire> Contents { get; set; } = new();
    }

    internal sealed class GeminiCountTokensContentWire
    {
        [JsonPropertyName("parts")]
        public List<GeminiCountTokensPartWire> Parts { get; set; } = new();
    }

    internal sealed class GeminiCountTokensPartWire
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }

    internal sealed class GeminiCountTokensResponseWire
    {
        [JsonPropertyName("totalTokens")]
        public int TotalTokens { get; set; }
    }

    #endregion
}
