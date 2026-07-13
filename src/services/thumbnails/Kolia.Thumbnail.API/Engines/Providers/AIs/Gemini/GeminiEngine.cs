using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers
{
    /// <summary>
    /// Engine cho Google Gemini (https://ai.google.dev/gemini-api/docs).
    /// Triển khai qua 2 mặt bằng API của cùng 1 backend Gemini:
    ///  - OpenAI-compatible layer (BaseUrlOpenAiCompat) dùng cho chat completion (kể cả
    ///    streaming, vision, function calling), embeddings và sinh ảnh - vì format request/response
    ///    giống hệt OpenAI/Groq nên tái dùng được cấu trúc GroqEngine gần như nguyên vẹn.
    ///    Xem: https://ai.google.dev/gemini-api/docs/openai
    ///  - Native REST API (BaseUrlNative) dùng cho models.list và models.countTokens vì 2
    ///    endpoint này của Gemini trả về metadata thật (input/output token limit, danh sách
    ///    generation method) chính xác hơn nhiều so với /models của lớp OpenAI-compat.
    ///    Xem: https://ai.google.dev/api/models, https://ai.google.dev/api/tokens
    ///
    /// Gemini hỗ trợ: chat completion (kèm vision + function calling), sinh ảnh (Nano Banana /
    /// Imagen thông qua images/generations), text embedding. Gemini KHÔNG có endpoint OpenAI-compatible
    /// công khai cho text-to-speech, speech-to-text (transcription độc lập) hay chỉnh sửa ảnh
    /// (image edit) - các khả năng này được xử lý rõ ràng bên dưới (throw/không implement interface)
    /// thay vì giả lập dữ liệu không có thật.
    /// </summary>
    public sealed class GeminiEngine :
        IChatCapableEngine,
        IImageGenerationCapableEngine,
        IEmbeddingCapableEngine
    {
        private readonly string _baseUrlOpenAiCompat;
        private readonly string _baseUrlNative;

        private readonly ILogger<GeminiEngine> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Khởi tạo GeminiEngine với BaseUrl động.
        /// Mặc định: "https://generativelanguage.googleapis.com/v1beta/openai" (OpenAI-compat)
        /// và "https://generativelanguage.googleapis.com/v1beta" (native).
        /// </summary>
        public GeminiEngine(HttpClient httpClient,
            ILogger<GeminiEngine> logger,
            string? baseUrl = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                var trimmed = baseUrl.TrimEnd('/');
                _baseUrlOpenAiCompat = trimmed + "/openai";
                _baseUrlNative = trimmed;
            }
            else
            {
                _baseUrlOpenAiCompat = "https://generativelanguage.googleapis.com/v1beta/openai";
                _baseUrlNative = "https://generativelanguage.googleapis.com/v1beta";
            }
        }

        public CAIProviderType ProviderType => CAIProviderType.Gemini;

        // ============================================================
        // IAIEngine - phần chung
        // ============================================================

        public AIProviderCapabilities GetCapabilities() => new()
        {
            SupportsChat = true,
            SupportsStreaming = true,
            SupportsVision = true, // Gemini là multimodal gốc - hầu hết model chat đều đọc được ảnh/audio/video
            SupportsFunctionCalling = true,
            SupportsImageGeneration = true, // Nano Banana / Imagen qua images/generations
            SupportsImageEditing = false, // lớp OpenAI-compat của Gemini chưa có endpoint images/edits
            SupportsTextToSpeech = false, // TTS của Gemini chỉ lộ qua native generateContent (responseModalities: AUDIO), không có trong lớp OpenAI-compat
            SupportsSpeechToText = false, // Gemini không có endpoint transcription độc lập kiểu Whisper - audio input chỉ được xử lý lồng trong chat completion
            SupportsVideoGeneration = false, // Veo là provider riêng (CAIProviderType.GoogleVeo)
            SupportsEmbedding = true
        };

        /// <summary>
        /// Gemini API hiện không công khai endpoint tra cứu số dư/quota.
        /// Trả về IsSuccess = false kèm lý do thay vì giả lập số liệu không có thật.
        /// </summary>
        public Task<AIBalanceInfo> GetAIBalanceInfoAsync(string apiKey)
        {
            return Task.FromResult(new AIBalanceInfo
            {
                ProviderType = CAIProviderType.Gemini,
                IsSuccess = false,
                ErrorMessage = "Gemini API hiện không cung cấp endpoint công khai để tra cứu số dư/quota. " +
                               "Vui lòng kiểm tra trực tiếp tại aistudio.google.com/apikey (free tier) " +
                               "hoặc Google Cloud Billing console (paid tier)."
            });
        }

        /// <summary>
        /// Dùng native models.list (không phải lớp OpenAI-compat) vì trả về đủ inputTokenLimit,
        /// outputTokenLimit, supportedGenerationMethods thật - tránh phải suy đoán/hardcode.
        /// Tự động phân trang bằng nextPageToken cho đến khi lấy hết danh sách.
        /// </summary>
        public async Task<List<AIModelInfo>> GetAIModelInfosAsync(string apiKey)
        {
            var result = new List<AIModelInfo>();
            string? pageToken = null;

            do
            {
                var path = $"/models?pageSize=1000{(pageToken is null ? string.Empty : $"&pageToken={Uri.EscapeDataString(pageToken)}")}";

                using var httpRequest = CreateNativeAuthorizedRequest(HttpMethod.Get, path, apiKey);
                using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                await EnsureSuccessAsync(response, nameof(GetAIModelInfosAsync)).ConfigureAwait(false);

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var wire = JsonSerializer.Deserialize<GeminiModelListWire>(json, JsonOptions) ?? new GeminiModelListWire();

                result.AddRange(wire.Models.Select(MapModelWireToInfo));
                pageToken = wire.NextPageToken;
            }
            while (!string.IsNullOrEmpty(pageToken));

            return result;
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                using var httpRequest = CreateNativeAuthorizedRequest(HttpMethod.Get, "/models?pageSize=1", apiKey);
                using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // IChatCapableEngine
        // ============================================================

        public async Task<ChatCompletionResult> ChatCompletionAsync(ChatCompletionRequest request)
        {
            var wireRequest = BuildChatRequestWire(request, stream: false);

            using var httpRequest = CreateOpenAiCompatJsonRequest(HttpMethod.Post, "/chat/completions", request.ApiKey, wireRequest);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(ChatCompletionAsync), request.Model).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GeminiChatResponseWire>(json, JsonOptions)
                ?? throw new InvalidOperationException("Gemini trả về phản hồi rỗng cho yêu cầu chat completion.");

            var choice = wire.Choices.FirstOrDefault()
                ?? throw new InvalidOperationException("Gemini không trả về choice nào cho yêu cầu chat completion.");

            return new ChatCompletionResult
            {
                Content = choice.Message.Content ?? string.Empty,
                PromptTokens = wire.Usage?.PromptTokens ?? 0,
                CompletionTokens = wire.Usage?.CompletionTokens ?? 0,
                FinishReason = choice.FinishReason,
                ModelUsed = wire.Model,
                ToolCalls = choice.Message.ToolCalls?.Select(tc => new ChatToolCall
                {
                    Id = tc.Id,
                    ToolName = tc.Function.Name,
                    ArgumentsJson = tc.Function.Arguments
                }).ToList()
            };
        }

        public async IAsyncEnumerable<ChatCompletionChunk> ChatCompletionStreamAsync(
            ChatCompletionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var wireRequest = BuildChatRequestWire(request, stream: true);

            using var httpRequest = CreateOpenAiCompatJsonRequest(
                HttpMethod.Post,
                "/chat/completions",
                request.ApiKey,
                wireRequest);

            using var response = await _httpClient
                .SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);

            await EnsureSuccessAsync(response, nameof(ChatCompletionStreamAsync), request.Model).ConfigureAwait(false);

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            using var reader = new StreamReader(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader
                    .ReadLineAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (line is null)
                {
                    yield break;
                }

                if (string.IsNullOrWhiteSpace(line) ||
                    !line.StartsWith("data:", StringComparison.Ordinal))
                {
                    continue;
                }

                var payload = line["data:".Length..].Trim();

                if (payload == "[DONE]")
                {
                    yield return new ChatCompletionChunk
                    {
                        DeltaContent = string.Empty,
                        IsFinal = true
                    };

                    yield break;
                }

                GeminiStreamChunkWire? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<GeminiStreamChunkWire>(payload, JsonOptions);
                }
                catch (JsonException)
                {
                    // Ignore malformed SSE lines instead of terminating the stream.
                    continue;
                }

                var choice = chunk?.Choices?.FirstOrDefault();
                if (choice is null)
                {
                    continue;
                }

                var isFinal = choice.FinishReason is not null;

                yield return new ChatCompletionChunk
                {
                    DeltaContent = choice.Delta?.Content ?? string.Empty,
                    DeltaToolCall = MapStreamToolCallDelta(choice.Delta?.ToolCalls?.FirstOrDefault()),
                    IsFinal = isFinal,
                    FinishReason = choice.FinishReason,
                    PromptTokens = isFinal ? chunk?.Usage?.PromptTokens : null,
                    CompletionTokens = isFinal ? chunk?.Usage?.CompletionTokens : null
                };
            }
        }

        /// <summary>
        /// Gemini có endpoint native models.{model}:countTokens trả về số token THẬT (xem
        /// GeminiCountTokensRequestWire/ResponseWire), nhưng endpoint này yêu cầu xác thực bằng
        /// API key trong khi chữ ký IChatCapableEngine.CountTokensAsync(model, text) của hệ thống
        /// KHÔNG có tham số apiKey - vì vậy không thể gọi HTTP tới Gemini một cách an toàn ở đây
        /// (sẽ luôn thất bại 401). Thay vì gọi API chắc chắn lỗi, dùng ước lượng gần đúng theo
        /// quy tắc phổ biến (~4 ký tự / token, giống cách GroqEngine xử lý khi provider không có
        /// endpoint đếm token công khai), KHÔNG chính xác tuyệt đối - chỉ dùng để ước tính sơ bộ.
        /// Nếu cần số liệu chính xác, nên bổ sung overload nhận apiKey vào IChatCapableEngine rồi
        /// gọi thẳng $"{BaseUrlNative}/models/{model}:countTokens" với header "x-goog-api-key".
        /// </summary>
        public Task<int> CountTokensAsync(string model, string text)
        {
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(0);

            var approxTokens = (int)Math.Ceiling(text.Length / 4.0);
            return Task.FromResult(approxTokens);
        }

        // ============================================================
        // IEmbeddingCapableEngine
        // ============================================================

        public async Task<EmbeddingResult> CreateEmbeddingAsync(EmbeddingRequest request)
        {
            // Lưu ý: request.InputType (Document/Query) tương ứng "task_type" ở native embedContent
            // API của Gemini, nhưng lớp OpenAI-compat KHÔNG công bố field OpenAI nào để truyền
            // task_type - vì vậy tham số này chưa được áp dụng ở đây thay vì âm thầm bỏ qua sai lệch.
            var wireRequest = new GeminiEmbeddingRequestWire
            {
                Model = request.Model,
                Input = request.Inputs,
                Dimensions = request.Dimensions
            };

            using var httpRequest = CreateOpenAiCompatJsonRequest(HttpMethod.Post, "/embeddings", request.ApiKey, wireRequest);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(CreateEmbeddingAsync), request.Model).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GeminiEmbeddingResponseWire>(json, JsonOptions)
                ?? throw new InvalidOperationException("Gemini trả về phản hồi rỗng cho yêu cầu embedding.");

            return new EmbeddingResult
            {
                Vectors = wire.Data
                    .OrderBy(d => d.Index)
                    .Select(d => d.Embedding.ToArray())
                    .ToList(),
                Dimensions = wire.Data.FirstOrDefault()?.Embedding.Count ?? 0,
                PromptTokens = wire.Usage?.PromptTokens ?? 0,
                ModelUsed = wire.Model ?? request.Model
            };
        }

        // ============================================================
        // IImageGenerationCapableEngine
        // ============================================================

        public async Task<ImageGenerationResult> GenerateImageAsync(ImageGenerationRequest request)
        {
            // Lưu ý: NegativePrompt, Seed, GuidanceScale, Steps, StylePreset của DTO chung KHÔNG có
            // field tương ứng được công bố cho endpoint images/generations của lớp OpenAI-compat
            // Gemini (chỉ hỗ trợ prompt, model, n, size, response_format) - các tham số này vì vậy
            // chưa được gửi đi thay vì gửi sai và gây hiểu nhầm là đã được áp dụng.
            var wireRequest = new GeminiImageGenerationRequestWire
            {
                Model = request.Model,
                Prompt = request.Prompt,
                N = request.NumberOfImages,
                Size = $"{request.Width}x{request.Height}",
                ResponseFormat = "b64_json"
            };

            using var httpRequest = CreateOpenAiCompatJsonRequest(HttpMethod.Post, "/images/generations", request.ApiKey, wireRequest);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(GenerateImageAsync), request.Model).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GeminiImageGenerationResponseWire>(json, JsonOptions)
                ?? throw new InvalidOperationException("Gemini trả về phản hồi rỗng cho yêu cầu sinh ảnh.");

            return new ImageGenerationResult
            {
                ModelUsed = request.Model,
                IsSuccess = true,
                Images = wire.Data.Select(d => new GeneratedImageItem
                {
                    ImageBytes = d.B64Json is not null ? Convert.FromBase64String(d.B64Json) : null,
                    Url = d.Url,
                    Width = request.Width,
                    Height = request.Height
                }).ToList()
            };
        }

        /// <summary>
        /// Lớp OpenAI-compat của Gemini hiện KHÔNG có endpoint images/edits (khác OpenAI DALL-E).
        /// Chỉnh sửa ảnh trên Gemini chỉ khả dụng qua native generateContent đa phương thức
        /// (gửi ảnh gốc + prompt, model trả ảnh mới trong response) - nằm ngoài phạm vi API
        /// images/generations hiện tại. Báo lỗi rõ ràng thay vì giả lập kết quả không có thật.
        /// Tham khảo: https://ai.google.dev/gemini-api/docs/image-generation
        /// </summary>
        public Task<ImageGenerationResult> EditImageAsync(ImageEditRequest request)
        {
            throw new NotSupportedException(
                "Gemini không cung cấp endpoint OpenAI-compatible để sửa/biến thể ảnh (images/edits). " +
                "Chỉnh sửa ảnh trên Gemini (Nano Banana) chỉ khả dụng qua native generateContent đa " +
                "phương thức - vui lòng tham khảo https://ai.google.dev/gemini-api/docs/image-generation " +
                "và cấu hình luồng riêng ở tầng ứng dụng nếu cần.");
        }

        // ============================================================
        // Helpers - mapping request/response
        // ============================================================

        private static GeminiChatRequestWire BuildChatRequestWire(ChatCompletionRequest request, bool stream)
        {
            var wire = new GeminiChatRequestWire
            {
                Model = request.Model,
                Messages = request.Messages.Select(MapMessageToWire).ToList(),
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                TopP = request.TopP,
                Stop = request.StopSequences,
                Stream = stream,
                User = request.UserIdentifier
            };

            if (stream)
                wire.StreamOptions = new GeminiStreamOptionsWire { IncludeUsage = true };

            if (request.Tools is { Count: > 0 })
            {
                wire.Tools = request.Tools.Select(t => new GeminiToolWire
                {
                    Function = new GeminiFunctionDefWire
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = JsonDocument.Parse(t.ParametersJsonSchema).RootElement.Clone()
                    }
                }).ToList();
                wire.ToolChoice = "auto";
            }

            if (request.ResponseAsJson)
                wire.ResponseFormat = new GeminiResponseFormatWire { Type = "json_object" };

            return wire;
        }

        private static GeminiMessageWire MapMessageToWire(ChatMessage message) => new()
        {
            Role = message.Role,
            Content = BuildMessageContent(message),
            ToolCallId = message.ToolCallId,
            ToolCalls = message.ToolCalls?.Select(tc => new GeminiToolCallWire
            {
                Id = tc.Id,
                Function = new GeminiToolCallFunctionWire { Name = tc.ToolName, Arguments = tc.ArgumentsJson }
            }).ToList()
        };

        /// <summary>
        /// Trả về string đơn giản nếu message không có ảnh đính kèm, hoặc List&lt;object&gt;
        /// (content parts kiểu OpenAI-compatible) nếu có ảnh - Gemini là multimodal gốc nên
        /// hầu hết model chat đều xử lý được image_url ở dạng này.
        /// </summary>
        private static object BuildMessageContent(ChatMessage message)
        {
            if (message.Images is not { Count: > 0 })
                return message.Content;

            var parts = new List<object>();

            if (!string.IsNullOrEmpty(message.Content))
                parts.Add(new { type = "text", text = message.Content });

            foreach (var image in message.Images)
            {
                var url = image.SourceType == ChatImageSourceType.Base64
                    ? $"data:{image.MimeType ?? "image/png"};base64,{image.Source}"
                    : image.Source;

                parts.Add(new { type = "image_url", image_url = new { url } });
            }

            return parts;
        }

        private static ChatToolCall? MapStreamToolCallDelta(GeminiStreamToolCallDeltaWire? delta)
        {
            if (delta is null)
                return null;

            return new ChatToolCall
            {
                Id = delta.Id ?? string.Empty,
                ToolName = delta.Function?.Name ?? string.Empty,
                ArgumentsJson = delta.Function?.Arguments ?? string.Empty
            };
        }

        private static AIModelInfo MapModelWireToInfo(GeminiModelWire m)
        {
            var modelId = m.Name.StartsWith("models/", StringComparison.Ordinal)
                ? m.Name["models/".Length..]
                : m.Name;

            var category = InferModelCategory(m.SupportedGenerationMethods);
            var isChat = category == AIModelCategory.Chat;

            return new AIModelInfo
            {
                ProviderType = CAIProviderType.Gemini,
                ModelId = modelId,
                DisplayName = m.DisplayName ?? modelId,
                Description = m.Description,
                Category = category,
                MaxContextTokens = m.InputTokenLimit,
                MaxOutputTokens = m.OutputTokenLimit,
                // Gemini là multimodal gốc - các model chat (generateContent) hầu như đều đọc được
                // ảnh/audio/video đầu vào, ngoại trừ model chỉ phục vụ embedding/prediction thuần túy.
                SupportsVision = isChat,
                SupportsFunctionCalling = isChat,
                SupportsStreaming = isChat,
                IsDeprecated = m.Description?.Contains("deprecated", StringComparison.OrdinalIgnoreCase) ?? false
            };
        }

        private static AIModelCategory InferModelCategory(List<string> supportedGenerationMethods)
        {
            if (supportedGenerationMethods.Contains("generateContent"))
                return AIModelCategory.Chat;

            if (supportedGenerationMethods.Contains("embedContent"))
                return AIModelCategory.Embedding;

            if (supportedGenerationMethods.Contains("predict"))
                return AIModelCategory.ImageGeneration;

            return AIModelCategory.Chat;
        }

        // ============================================================
        // Helpers - HTTP request building & error handling
        // ============================================================

        /// <summary>
        /// Request tới lớp OpenAI-compatible (/v1beta/openai/...) - xác thực bằng
        /// header Authorization: Bearer {apiKey}, giống hệt OpenAI/Groq.
        /// </summary>
        private HttpRequestMessage CreateOpenAiCompatJsonRequest<T>(HttpMethod method, string path, string apiKey, T body)
        {
            var request = new HttpRequestMessage(method, $"{_baseUrlOpenAiCompat}{path}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }

        /// <summary>
        /// Request tới native REST API (/v1beta/...) - xác thực bằng header "x-goog-api-key"
        /// theo đúng chuẩn của Gemini native API (khác với lớp OpenAI-compat dùng Bearer token).
        /// </summary>
        private HttpRequestMessage CreateNativeAuthorizedRequest(HttpMethod method, string path, string apiKey)
        {
            var request = new HttpRequestMessage(method, $"{_baseUrlNative}{path}");
            request.Headers.Add("x-goog-api-key", apiKey);
            return request;
        }

        private async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string actionName,
            string? modelId = null)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var exception = new AiProviderException(CAIProviderType.Gemini, (int)response.StatusCode, body);

            _logger.LogError(exception,
                "Gemini API call failed. Action: {ActionName}, Model: {ModelId}, HTTP Status: {HttpStatusCode}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                actionName,
                modelId ?? "N/A",
                exception.ProviderStatusCode,
                exception.ProviderErrorCode ?? "N/A",
                exception.ProviderMessage);

            throw exception;
        }
    }
}
