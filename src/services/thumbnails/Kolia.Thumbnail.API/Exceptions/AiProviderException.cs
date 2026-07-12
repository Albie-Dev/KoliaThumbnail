using System.Text.Json;
using Kolia.Thumbnail.API.Enums;
using Microsoft.AspNetCore.Http;

namespace Kolia.Thumbnail.API.Exceptions
{
    /// <summary>
    /// Ngoại lệ đại diện cho các lỗi xảy ra khi gọi đến dịch vụ của AI Provider (e.g., Groq, OpenAI, Gemini, ...).
    /// Hỗ trợ parse chi tiết lỗi theo định dạng JSON chuẩn của provider và ánh xạ sang mã HTTP Status/Business Code phù hợp.
    /// </summary>
    public sealed class AiProviderException : AppException
    {
        /// <summary>
        /// Loại AI Provider gặp lỗi.
        /// </summary>
        public CAIProviderType ProviderType { get; }

        /// <summary>
        /// HTTP status code thực tế từ API của Provider.
        /// </summary>
        public int ProviderStatusCode { get; }

        /// <summary>
        /// Mã lỗi chi tiết từ Provider (e.g., "rate_limit_exceeded", "invalid_api_key").
        /// </summary>
        public string? ProviderErrorCode { get; }

        /// <summary>
        /// Loại lỗi từ Provider (e.g., "invalid_request_error", "tokens").
        /// </summary>
        public string? ProviderErrorType { get; }

        /// <summary>
        /// Tham số gây lỗi từ Provider (nếu có).
        /// </summary>
        public string? ProviderErrorParam { get; }

        /// <summary>
        /// Nội dung response thô nhận được từ API của Provider.
        /// </summary>
        public string RawResponse { get; }

        /// <summary>
        /// Thông báo lỗi chi tiết được trích xuất từ Provider.
        /// </summary>
        public string ProviderMessage { get; }

        public AiProviderException(
            CAIProviderType providerType,
            int providerStatusCode,
            string rawResponse,
            Exception? innerException = null)
            : base(
                message: BuildFriendlyMessage(providerType, providerStatusCode, rawResponse, out var parsedMsg, out var errorCode, out var errorType, out var errorParam),
                innerException: innerException!,
                code: MapToBusinessCode(providerStatusCode, errorCode),
                statusCode: MapToHttpStatusCode(providerStatusCode))
        {
            ProviderType = providerType;
            ProviderStatusCode = providerStatusCode;
            RawResponse = rawResponse;
            ProviderMessage = parsedMsg ?? rawResponse;
            ProviderErrorCode = errorCode;
            ProviderErrorType = errorType;
            ProviderErrorParam = errorParam;
        }

        private static string BuildFriendlyMessage(
            CAIProviderType providerType,
            int providerStatusCode,
            string rawResponse,
            out string? parsedMsg,
            out string? errorCode,
            out string? errorType,
            out string? errorParam)
        {
            parsedMsg = null;
            errorCode = null;
            errorType = null;
            errorParam = null;

            if (!string.IsNullOrWhiteSpace(rawResponse))
            {
                try
                {
                    using var doc = JsonDocument.Parse(rawResponse);
                    var root = doc.RootElement;

                    // Parse định dạng OpenAI-compatible (được dùng bởi OpenAI, Groq, Anthropic-shim, v.v.)
                    if (root.TryGetProperty("error", out var errorEl))
                    {
                        if (errorEl.ValueKind == JsonValueKind.Object)
                        {
                            if (errorEl.TryGetProperty("message", out var msgEl))
                                parsedMsg = msgEl.GetString();

                            if (errorEl.TryGetProperty("code", out var codeEl))
                                errorCode = codeEl.ValueKind == JsonValueKind.Number ? codeEl.ToString() : codeEl.GetString();

                            if (errorEl.TryGetProperty("type", out var typeEl))
                                errorType = typeEl.GetString();

                            if (errorEl.TryGetProperty("param", out var paramEl))
                                errorParam = paramEl.GetString();
                        }
                        else if (errorEl.ValueKind == JsonValueKind.String)
                        {
                            parsedMsg = errorEl.GetString();
                        }
                    }
                }
                catch (JsonException)
                {
                    // Response không phải JSON hợp lệ, giữ nguyên null để dùng rawResponse
                }
            }

            var displayMessage = parsedMsg ?? (string.IsNullOrWhiteSpace(rawResponse) ? "No response body" : rawResponse);
            var codePart = errorCode != null ? $" [Code: {errorCode}]" : string.Empty;
            return $"{providerType} API error (HTTP {providerStatusCode}): {displayMessage}{codePart}";
        }

        private static string MapToBusinessCode(int statusCode, string? errorCode)
        {
            if (!string.IsNullOrEmpty(errorCode))
            {
                return $"AI_PROVIDER_{errorCode.ToUpperInvariant().Replace(" ", "_")}";
            }

            return statusCode switch
            {
                StatusCodes.Status401Unauthorized => "AI_PROVIDER_UNAUTHORIZED",
                StatusCodes.Status403Forbidden => "AI_PROVIDER_FORBIDDEN",
                StatusCodes.Status404NotFound => "AI_PROVIDER_NOT_FOUND",
                StatusCodes.Status429TooManyRequests => "AI_PROVIDER_RATE_LIMIT",
                498 => "AI_PROVIDER_FLEX_CAPACITY_EXCEEDED",
                StatusCodes.Status400BadRequest => "AI_PROVIDER_BAD_REQUEST",
                StatusCodes.Status413PayloadTooLarge => "AI_PROVIDER_PAYLOAD_TOO_LARGE",
                StatusCodes.Status422UnprocessableEntity => "AI_PROVIDER_UNPROCESSABLE_ENTITY",
                _ => "AI_PROVIDER_ERROR"
            };
        }

        private static int MapToHttpStatusCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status401Unauthorized => StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden => StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound => StatusCodes.Status404NotFound,
                StatusCodes.Status429TooManyRequests => StatusCodes.Status429TooManyRequests,
                498 => StatusCodes.Status429TooManyRequests, // Map Flex capacity to 429
                StatusCodes.Status400BadRequest => StatusCodes.Status400BadRequest,
                StatusCodes.Status413PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
                StatusCodes.Status422UnprocessableEntity => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status502BadGateway
            };
        }
    }
}
