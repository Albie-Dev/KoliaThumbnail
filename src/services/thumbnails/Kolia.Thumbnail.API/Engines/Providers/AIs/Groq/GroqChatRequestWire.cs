using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kolia.Thumbnail.API.Engines.Providers
{
    // ============================================================
    // Các DTO "wire" dưới đây map 1-1 với JSON body thực tế của
    // Groq API (OpenAI-compatible). Chúng chỉ dùng nội bộ trong
    // GroqEngine để serialize/deserialize, không lộ ra ngoài -
    // các method public của GroqEngine luôn trả về DTO chung của
    // hệ thống (ChatCompletionResult, AIModelInfo...).
    // ============================================================

    #region Chat Completion

    internal sealed class GroqChatRequestWire
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("messages")]
        public List<GroqMessageWire> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("stream_options")]
        public GroqStreamOptionsWire? StreamOptions { get; set; }

        [JsonPropertyName("tools")]
        public List<GroqToolWire>? Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        public string? ToolChoice { get; set; }

        [JsonPropertyName("response_format")]
        public GroqResponseFormatWire? ResponseFormat { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }
    }

    internal sealed class GroqStreamOptionsWire
    {
        [JsonPropertyName("include_usage")]
        public bool IncludeUsage { get; set; }
    }

    internal sealed class GroqResponseFormatWire
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;
    }

    internal sealed class GroqMessageWire
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        /// <summary>
        /// Có thể là string đơn giản hoặc List&lt;object&gt; (content parts) khi có ảnh đính kèm.
        /// </summary>
        [JsonPropertyName("content")]
        public object? Content { get; set; }

        [JsonPropertyName("tool_call_id")]
        public string? ToolCallId { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GroqToolCallWire>? ToolCalls { get; set; }
    }

    internal sealed class GroqToolWire
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public GroqFunctionDefWire Function { get; set; } = default!;
    }

    internal sealed class GroqFunctionDefWire
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public JsonElement Parameters { get; set; }
    }

    internal sealed class GroqToolCallWire
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public GroqToolCallFunctionWire Function { get; set; } = default!;
    }

    internal sealed class GroqToolCallFunctionWire
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = default!;
    }

    internal sealed class GroqChatResponseWire
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("choices")]
        public List<GroqChoiceWire> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public GroqUsageWire? Usage { get; set; }
    }

    internal sealed class GroqChoiceWire
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public GroqResponseMessageWire Message { get; set; } = default!;

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    internal sealed class GroqResponseMessageWire
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GroqToolCallWire>? ToolCalls { get; set; }
    }

    internal sealed class GroqUsageWire
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    // -------- Streaming (SSE chunk) --------

    internal sealed class GroqStreamChunkWire
    {
        [JsonPropertyName("choices")]
        public List<GroqStreamChoiceWire>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public GroqUsageWire? Usage { get; set; }
    }

    internal sealed class GroqStreamChoiceWire
    {
        [JsonPropertyName("delta")]
        public GroqStreamDeltaWire? Delta { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    internal sealed class GroqStreamDeltaWire
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<GroqStreamToolCallDeltaWire>? ToolCalls { get; set; }
    }

    internal sealed class GroqStreamToolCallDeltaWire
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("function")]
        public GroqStreamFunctionDeltaWire? Function { get; set; }
    }

    internal sealed class GroqStreamFunctionDeltaWire
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }

    #endregion

    #region Models

    internal sealed class GroqModelListWire
    {
        [JsonPropertyName("data")]
        public List<GroqModelWire> Data { get; set; } = new();
    }

    internal sealed class GroqModelWire
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("owned_by")]
        public string? OwnedBy { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("context_window")]
        public int? ContextWindow { get; set; }

        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }
    }

    #endregion

    #region Audio - Speech to Text (Whisper)

    internal sealed class GroqTranscriptionResponseWire
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("duration")]
        public double? Duration { get; set; }

        [JsonPropertyName("segments")]
        public List<GroqSegmentWire>? Segments { get; set; }

        [JsonPropertyName("words")]
        public List<GroqWordWire>? Words { get; set; }
    }

    internal sealed class GroqSegmentWire
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }

    internal sealed class GroqWordWire
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = default!;

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }
    }

    #endregion

    #region Audio - Text to Speech

    internal sealed class GroqSpeechRequestWire
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("input")]
        public string Input { get; set; } = default!;

        [JsonPropertyName("voice")]
        public string Voice { get; set; } = default!;

        [JsonPropertyName("response_format")]
        public string ResponseFormat { get; set; } = "mp3";

        [JsonPropertyName("sample_rate")]
        public int SampleRate { get; set; } = 48000;

        [JsonPropertyName("speed")]
        public double Speed { get; set; } = 1.0;
    }

    #endregion
}