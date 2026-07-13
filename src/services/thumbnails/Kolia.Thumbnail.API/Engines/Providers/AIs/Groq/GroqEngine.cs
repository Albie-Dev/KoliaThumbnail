using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers
{
    /// <summary>
    /// Engine cho Groq (https://groq.com) - triển khai qua API OpenAI-compatible.
    /// Groq hỗ trợ: chat completion (LLM), speech-to-text (Whisper), text-to-speech
    /// (PlayAI / Orpheus). Groq KHÔNG hỗ trợ: sinh ảnh, sinh video, embedding, và cũng
    /// không có endpoint tra cứu balance hay danh sách voice công khai - các method
    /// tương ứng được xử lý rõ ràng bên dưới thay vì giả lập dữ liệu không có thật.
    /// </summary>
    public sealed class GroqEngine :
        IChatCapableEngine,
        ISpeechToTextCapableEngine,
        ITextToSpeechCapableEngine
    {
        private readonly string _baseUrl;
        private readonly ILogger<GroqEngine> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Khởi tạo GroqEngine với BaseUrl động.
        /// Mặc định: "https://api.groq.com/openai/v1".
        /// </summary>
        public GroqEngine(HttpClient httpClient,
            ILogger<GroqEngine> logger,
            string? baseUrl = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            _baseUrl = (baseUrl ?? "https://api.groq.com/openai/v1").TrimEnd('/');
        }

        public CAIProviderType ProviderType => CAIProviderType.Groq;

        // ============================================================
        // IAIEngine - phần chung
        // ============================================================

        public AIProviderCapabilities GetCapabilities() => new()
        {
            SupportsChat = true,
            SupportsStreaming = true,
            SupportsVision = true, // 1 số model Llama trên Groq hỗ trợ vision (image_url content part)
            SupportsFunctionCalling = true,
            SupportsImageGeneration = false,
            SupportsImageEditing = false,
            SupportsTextToSpeech = true,
            SupportsSpeechToText = true,
            SupportsVideoGeneration = false,
            SupportsEmbedding = false
        };

        /// <summary>
        /// Groq API hiện không công khai endpoint tra cứu số dư/quota.
        /// Trả về IsSuccess = false kèm lý do thay vì giả lập số liệu không có thật.
        /// </summary>
        public Task<AIBalanceInfo> GetAIBalanceInfoAsync(string apiKey)
        {
            return Task.FromResult(new AIBalanceInfo
            {
                ProviderType = CAIProviderType.Groq,
                IsSuccess = false,
                ErrorMessage = "Groq API hiện không cung cấp endpoint công khai để tra cứu số dư/quota. " +
                               "Vui lòng kiểm tra trực tiếp tại console.groq.com/settings/billing."
            });
        }

        public async Task<List<AIModelInfo>> GetAIModelInfosAsync(string apiKey)
        {
            using var httpRequest = CreateAuthorizedRequest(HttpMethod.Get, "/models", apiKey);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(GetAIModelInfosAsync)).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GroqModelListWire>(json, JsonOptions) ?? new GroqModelListWire();

            return wire.Data.Select(m =>
            {
                var category = InferModelCategory(m.Id);
                var isChat = category == AIModelCategory.Chat;

                return new AIModelInfo
                {
                    ProviderType = CAIProviderType.Groq,
                    ModelId = m.Id,
                    DisplayName = m.Id,
                    Category = category,
                    MaxContextTokens = m.ContextWindow,
                    MaxOutputTokens = m.MaxCompletionTokens,
                    SupportsStreaming = isChat,
                    SupportsFunctionCalling = isChat,
                    IsDeprecated = !m.Active
                };
            }).ToList();
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                using var httpRequest = CreateAuthorizedRequest(HttpMethod.Get, "/models", apiKey);
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

            using var httpRequest = CreateJsonRequest(HttpMethod.Post, "/chat/completions", request.ApiKey, wireRequest);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(ChatCompletionAsync), request.Model).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GroqChatResponseWire>(json, JsonOptions)
                ?? throw new InvalidOperationException("Groq trả về phản hồi rỗng cho yêu cầu chat completion.");

            var choice = wire.Choices.FirstOrDefault()
                ?? throw new InvalidOperationException("Groq không trả về choice nào cho yêu cầu chat completion.");

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

            using var httpRequest = CreateJsonRequest(
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

                GroqStreamChunkWire? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<GroqStreamChunkWire>(payload, JsonOptions);
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
        /// Groq không cung cấp endpoint đếm token công khai. Đây là ước lượng gần đúng
        /// theo quy tắc phổ biến (~4 ký tự / token), KHÔNG chính xác tuyệt đối -
        /// chỉ nên dùng để ước tính sơ bộ, không dùng để tính phí chính xác.
        /// </summary>
        public Task<int> CountTokensAsync(string model, string text)
        {
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(0);

            var approxTokens = (int)Math.Ceiling(text.Length / 4.0);
            return Task.FromResult(approxTokens);
        }

        // ============================================================
        // ISpeechToTextCapableEngine
        // ============================================================

        public Task<SpeechToTextResult> TranscribeAudioAsync(SpeechToTextRequest request)
            => CallWhisperEndpointAsync(request, "/audio/transcriptions", isTranslation: false);

        public Task<SpeechToTextResult> TranslateAudioAsync(SpeechToTextRequest request)
            => CallWhisperEndpointAsync(request, "/audio/translations", isTranslation: true);

        private async Task<SpeechToTextResult> CallWhisperEndpointAsync(
            SpeechToTextRequest request, string path, bool isTranslation)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.Model), "model");

            // Endpoint /audio/translations luôn dịch sang tiếng Anh và không nhận tham số "language".
            if (!isTranslation && !string.IsNullOrEmpty(request.Language))
                content.Add(new StringContent(request.Language), "language");

            if (!string.IsNullOrEmpty(request.Prompt))
                content.Add(new StringContent(request.Prompt), "prompt");

            var responseFormat = request.IncludeTimestamps ? "verbose_json" : "json";
            content.Add(new StringContent(responseFormat), "response_format");

            if (request.IncludeTimestamps)
            {
                // Yêu cầu cả 2 granularity để có đủ dữ liệu tạo phụ đề (segment) và
                // đồng bộ chi tiết theo từng từ (word) nếu cần - lưu ý word tăng độ trễ.
                content.Add(new StringContent("segment"), "timestamp_granularities[]");
                content.Add(new StringContent("word"), "timestamp_granularities[]");
            }

            var audioContent = new ByteArrayContent(request.AudioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(GetAudioMimeType(request.InputFormat));
            content.Add(audioContent, "file", $"audio.{request.InputFormat}");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{path}") { Content = content };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.ApiKey);

            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, isTranslation ? "TranslateAudio" : "TranscribeAudio", request.Model).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wire = JsonSerializer.Deserialize<GroqTranscriptionResponseWire>(json, JsonOptions)
                ?? throw new InvalidOperationException("Groq trả về phản hồi rỗng cho yêu cầu speech-to-text.");

            return new SpeechToTextResult
            {
                Text = wire.Text,
                DetectedLanguage = wire.Language,
                Segments = wire.Segments?.Select(s => new TranscriptSegment
                {
                    Text = s.Text,
                    StartSeconds = s.Start,
                    EndSeconds = s.End
                }).ToList()
            };
        }

        // ============================================================
        // ITextToSpeechCapableEngine
        // ============================================================

        public async Task<TextToSpeechResult> GenerateSpeechAsync(TextToSpeechRequest request)
        {
            var wireRequest = new GroqSpeechRequestWire
            {
                Model = request.Model,
                Input = request.Text,
                Voice = request.VoiceId,
                ResponseFormat = MapAudioFormat(request.OutputFormat),
                SampleRate = request.SampleRate ?? 48000,
                Speed = request.Speed
            };

            using var httpRequest = CreateJsonRequest(HttpMethod.Post, "/audio/speech", request.ApiKey, wireRequest);
            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            await EnsureSuccessAsync(response, nameof(GenerateSpeechAsync), request.Model).ConfigureAwait(false);

            var audioBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            return new TextToSpeechResult
            {
                AudioBytes = audioBytes,
                Format = request.OutputFormat
            };
        }

        /// <summary>
        /// Groq API hiện KHÔNG có endpoint liệt kê danh sách voice. Danh sách voice khả dụng
        /// phụ thuộc vào model TTS đang dùng (vd: "playai-tts" cho tiếng Anh, "playai-tts-arabic"
        /// cho tiếng Ả Rập, "canopylabs/orpheus-v1-english" với bộ voice riêng) và có thể thay đổi
        /// theo thời gian. Thay vì trả về danh sách cứng có nguy cơ sai lệch, method này báo lỗi rõ
        /// ràng - hệ thống nên cấu hình voice theo model ở tầng trên (vd: lưu trong AIModelInfo hoặc
        /// cấu hình tĩnh được cập nhật thủ công), tham khảo https://console.groq.com/docs/text-to-speech
        /// </summary>
        public Task<List<AIVoiceInfo>> GetAvailableVoicesAsync(string apiKey)
        {
            throw new NotSupportedException(
                "Groq API không cung cấp endpoint để liệt kê danh sách voice động. " +
                "Danh sách voice phụ thuộc vào model TTS đang dùng - vui lòng tham khảo " +
                "https://console.groq.com/docs/text-to-speech và cấu hình voice tĩnh ở tầng ứng dụng.");
        }

        // ============================================================
        // Helpers - mapping request/response
        // ============================================================

        private static GroqChatRequestWire BuildChatRequestWire(ChatCompletionRequest request, bool stream)
        {
            var wire = new GroqChatRequestWire
            {
                Model = request.Model,
                Messages = request.Messages.Select(MapMessageToWire).ToList(),
                Temperature = request.Temperature,
                MaxCompletionTokens = request.MaxTokens,
                TopP = request.TopP,
                Stop = request.StopSequences,
                Stream = stream,
                User = request.UserIdentifier
            };

            if (stream)
                wire.StreamOptions = new GroqStreamOptionsWire { IncludeUsage = true };

            if (request.Tools is { Count: > 0 })
            {
                wire.Tools = request.Tools.Select(t => new GroqToolWire
                {
                    Function = new GroqFunctionDefWire
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = JsonDocument.Parse(t.ParametersJsonSchema).RootElement.Clone()
                    }
                }).ToList();
                wire.ToolChoice = "auto";
            }

            if (request.ResponseAsJson)
                wire.ResponseFormat = new GroqResponseFormatWire { Type = "json_object" };

            return wire;
        }

        private static GroqMessageWire MapMessageToWire(ChatMessage message) => new()
        {
            Role = message.Role,
            Content = BuildMessageContent(message),
            ToolCallId = message.ToolCallId,
            ToolCalls = message.ToolCalls?.Select(tc => new GroqToolCallWire
            {
                Id = tc.Id,
                Function = new GroqToolCallFunctionWire { Name = tc.ToolName, Arguments = tc.ArgumentsJson }
            }).ToList()
        };

        /// <summary>
        /// Trả về string đơn giản nếu message không có ảnh đính kèm, hoặc List&lt;object&gt;
        /// (content parts kiểu OpenAI-compatible) nếu có ảnh - phục vụ các model Llama vision trên Groq.
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

        private static ChatToolCall? MapStreamToolCallDelta(GroqStreamToolCallDeltaWire? delta)
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

        private static AIModelCategory InferModelCategory(string modelId)
        {
            var id = modelId.ToLowerInvariant();

            if (id.Contains("whisper"))
                return AIModelCategory.SpeechToText;

            if (id.Contains("tts") || id.Contains("orpheus"))
                return AIModelCategory.TextToSpeech;

            return AIModelCategory.Chat;
        }

        private static string MapAudioFormat(AudioOutputFormat format) => format switch
        {
            AudioOutputFormat.Mp3 => "mp3",
            AudioOutputFormat.Wav => "wav",
            AudioOutputFormat.Flac => "flac",
            AudioOutputFormat.Ogg => "ogg",
            _ => throw new NotSupportedException(
                $"Groq TTS không hỗ trợ định dạng '{format}'. Các định dạng được hỗ trợ: flac, mp3, mulaw, ogg, wav.")
        };

        private static string GetAudioMimeType(string format) => format.ToLowerInvariant() switch
        {
            "mp3" or "mpeg" or "mpga" => "audio/mpeg",
            "wav" => "audio/wav",
            "flac" => "audio/flac",
            "ogg" => "audio/ogg",
            "m4a" or "mp4" => "audio/mp4",
            "webm" => "audio/webm",
            _ => "application/octet-stream"
        };

        // ============================================================
        // Helpers - HTTP request building & error handling
        // ============================================================

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path, string apiKey)
        {
            var request = new HttpRequestMessage(method, $"{_baseUrl}{path}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            return request;
        }

        private HttpRequestMessage CreateJsonRequest<T>(HttpMethod method, string path, string apiKey, T body)
        {
            var request = CreateAuthorizedRequest(method, path, apiKey);
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
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
            var exception = new AiProviderException(CAIProviderType.Groq, (int)response.StatusCode, body);

            _logger.LogError(exception,
                "Groq API call failed. Action: {ActionName}, Model: {ModelId}, HTTP Status: {HttpStatusCode}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                actionName,
                modelId ?? "N/A",
                exception.ProviderStatusCode,
                exception.ProviderErrorCode ?? "N/A",
                exception.ProviderMessage);

            throw exception;
        }
    }
}