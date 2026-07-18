using System;
using Kolia.Thumbnail.API.Enums;
using Microsoft.AspNetCore.Http;

namespace Kolia.Thumbnail.API.Exceptions
{
    /// <summary>
    /// Ngoại lệ đại diện cho các lỗi xảy ra khi gọi đến API của Social Media Provider
    /// (YouTube, Facebook, TikTok, X...). Không phụ thuộc vào SDK cụ thể của provider nào -
    /// các engine (VD: YoutubeEngine) chịu trách nhiệm trích xuất thông tin lỗi từ exception
    /// gốc của SDK (VD: Google.GoogleApiException) rồi map sang exception chuẩn hóa này.
    /// </summary>
    public sealed class SocialProviderException : AppException
    {
        /// <summary>
        /// Loại Social Media Provider gặp lỗi.
        /// </summary>
        public CSocialMediaProviderType ProviderType { get; }

        /// <summary>
        /// HTTP status code thực tế từ API của Provider.
        /// </summary>
        public int ProviderStatusCode { get; }

        /// <summary>
        /// Lý do lỗi chi tiết từ Provider (VD: "quotaExceeded", "authError", "forbidden"...).
        /// </summary>
        public string? ProviderErrorReason { get; }

        /// <summary>
        /// Domain lỗi từ Provider (VD: "youtube.video", "usageLimits"...).
        /// </summary>
        public string? ProviderErrorDomain { get; }

        /// <summary>
        /// Thông báo lỗi chi tiết được trích xuất từ Provider.
        /// </summary>
        public string ProviderMessage { get; }

        public SocialProviderException(
            CSocialMediaProviderType providerType,
            int providerStatusCode,
            string providerMessage,
            string? providerErrorReason = null,
            string? providerErrorDomain = null,
            Exception? innerException = null)
            : base(
                message: BuildMessage(providerType, providerStatusCode, providerMessage, providerErrorReason),
                innerException: innerException!,
                code: MapToBusinessCode(providerStatusCode, providerErrorReason),
                statusCode: MapToHttpStatusCode(providerStatusCode))
        {
            ProviderType = providerType;
            ProviderStatusCode = providerStatusCode;
            ProviderMessage = providerMessage;
            ProviderErrorReason = providerErrorReason;
            ProviderErrorDomain = providerErrorDomain;
        }

        private static string BuildMessage(
            CSocialMediaProviderType providerType,
            int providerStatusCode,
            string providerMessage,
            string? providerErrorReason)
        {
            var reasonPart = string.IsNullOrEmpty(providerErrorReason) ? string.Empty : $" [Reason: {providerErrorReason}]";
            return $"{providerType} API error (HTTP {providerStatusCode}): {providerMessage}{reasonPart}";
        }

        private static string MapToBusinessCode(int statusCode, string? errorReason)
        {
            if (!string.IsNullOrEmpty(errorReason))
            {
                return $"SOCIAL_PROVIDER_{errorReason.ToUpperInvariant()}";
            }

            return statusCode switch
            {
                StatusCodes.Status401Unauthorized => "SOCIAL_PROVIDER_UNAUTHORIZED",
                StatusCodes.Status403Forbidden => "SOCIAL_PROVIDER_FORBIDDEN",
                StatusCodes.Status404NotFound => "SOCIAL_PROVIDER_NOT_FOUND",
                StatusCodes.Status409Conflict => "SOCIAL_PROVIDER_CONFLICT",
                StatusCodes.Status429TooManyRequests => "SOCIAL_PROVIDER_RATE_LIMIT",
                StatusCodes.Status400BadRequest => "SOCIAL_PROVIDER_BAD_REQUEST",
                StatusCodes.Status413PayloadTooLarge => "SOCIAL_PROVIDER_PAYLOAD_TOO_LARGE",
                StatusCodes.Status422UnprocessableEntity => "SOCIAL_PROVIDER_UNPROCESSABLE_ENTITY",
                _ => "SOCIAL_PROVIDER_ERROR"
            };
        }

        private static int MapToHttpStatusCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status401Unauthorized => StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden => StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound => StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict => StatusCodes.Status409Conflict,
                StatusCodes.Status429TooManyRequests => StatusCodes.Status429TooManyRequests,
                StatusCodes.Status400BadRequest => StatusCodes.Status400BadRequest,
                StatusCodes.Status413PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
                StatusCodes.Status422UnprocessableEntity => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status502BadGateway
            };
        }
    }
}
