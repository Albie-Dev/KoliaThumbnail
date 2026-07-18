using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers.Socials.Youtube
{
    /// <summary>
    /// Xây dựng <see cref="YouTubeService"/> từ <see cref="SocialCredentials"/> được lưu trong DB.
    /// YouTube Data API v3 có 2 kiểu xác thực:
    ///  - OAuth2 (ClientId/ClientSecret/AccessToken/RefreshToken): BẮT BUỘC cho mọi thao tác ghi
    ///    (upload video, cập nhật, xóa, đăng bình luận, tạo playlist, livestream...) và cho các
    ///    thao tác đọc dữ liệu riêng tư (channel/video của chính user).
    ///  - ApiKey: chỉ dùng được cho các thao tác đọc dữ liệu CÔNG KHAI (list/search video, channel...).
    /// </summary>
    internal static class YoutubeCredentialFactory
    {
        internal const string ApplicationName = "Kolia.Thumbnail.API";

        // Không lưu expiry token trong DB nên mặc định yêu cầu đủ scope quản trị kênh phổ biến nhất.
        private static readonly string[] DefaultScopes =
        {
            YouTubeService.Scope.Youtube,
            YouTubeService.Scope.YoutubeUpload,
            YouTubeService.Scope.YoutubeForceSsl
        };

        public static bool HasOAuthCapability(SocialCredentials credentials)
        {
            return !string.IsNullOrWhiteSpace(credentials.ClientId)
                && !string.IsNullOrWhiteSpace(credentials.ClientSecret)
                && (!string.IsNullOrWhiteSpace(credentials.RefreshToken) || !string.IsNullOrWhiteSpace(credentials.AccessToken));
        }

        /// <summary>
        /// Tạo GoogleAuthorizationCodeFlow dùng chung cho việc tạo UserCredential và refresh token.
        /// DataStore = NullDataStore vì hệ thống tự quản lý việc lưu trữ token vào DB (thông qua
        /// SocialMediaProviderConfigurationEntity), không cần thư viện Google tự cache ra file/local.
        /// </summary>
        private static GoogleAuthorizationCodeFlow CreateFlow(SocialCredentials credentials)
        {
            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                },
                Scopes = ResolveScopes(credentials.Scope),
                DataStore = new NullDataStore()
            });
        }

        /// <summary>
        /// Tạo UserCredential từ token đã lưu. Vì hệ thống không lưu thời điểm hết hạn của
        /// AccessToken, token được coi là "đã hết hạn" ngay từ đầu (IssuedUtc lùi về quá khứ) để
        /// buộc Google.Apis.Auth tự động refresh bằng RefreshToken trước request đầu tiên,
        /// tránh lỗi 401 giữa chừng khi AccessToken cũ đã hết hạn thực tế.
        /// </summary>
        public static UserCredential CreateUserCredential(SocialCredentials credentials)
        {
            if (!HasOAuthCapability(credentials))
            {
                throw new ValidationException(
                    "Thiếu ClientId/ClientSecret/AccessToken hoặc RefreshToken để xác thực OAuth2 với YouTube.",
                    "SOCIAL_YOUTUBE_MISSING_OAUTH_CREDENTIALS");
            }

            var flow = CreateFlow(credentials);

            var token = new TokenResponse
            {
                AccessToken = credentials.AccessToken,
                RefreshToken = credentials.RefreshToken,
                Scope = credentials.Scope,
                IssuedUtc = DateTime.UtcNow.AddDays(-1),
                ExpiresInSeconds = 0
            };

            return new UserCredential(flow, "kolia-thumbnail-user", token);
        }

        /// <summary>
        /// Ép làm mới AccessToken bằng RefreshToken và trả về TokenResponse mới nhất.
        /// Ném <see cref="SocialProviderException"/> nếu RefreshToken không hợp lệ/bị thu hồi.
        /// </summary>
        public static async Task<TokenResponse> RefreshAccessTokenAsync(SocialCredentials credentials, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(credentials.RefreshToken))
            {
                throw new ValidationException(
                    "Không có RefreshToken để làm mới AccessToken cho YouTube.",
                    "SOCIAL_YOUTUBE_MISSING_REFRESH_TOKEN");
            }

            var flow = CreateFlow(credentials);

            try
            {
                var token = await flow
                    .RefreshTokenAsync("kolia-thumbnail-user", credentials.RefreshToken, cancellationToken)
                    .ConfigureAwait(false);

                return token;
            }
            catch (TokenResponseException ex)
            {
                throw new SocialProviderException(
                    CSocialMediaProviderType.Youtube,
                    (int)(ex.StatusCode ?? System.Net.HttpStatusCode.Unauthorized),
                    ex.Error?.ErrorDescription ?? ex.Error?.Error ?? ex.Message,
                    providerErrorReason: ex.Error?.Error,
                    innerException: ex);
            }
        }

        public static YouTubeService CreateOAuthService(SocialCredentials credentials)
        {
            var credential = CreateUserCredential(credentials);
            return new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public static YouTubeService CreateApiKeyService(SocialCredentials credentials)
        {
            if (!credentials.HasApiKey)
            {
                throw new ValidationException(
                    "Thiếu ApiKey để gọi YouTube Data API cho thao tác chỉ đọc dữ liệu công khai.",
                    "SOCIAL_YOUTUBE_MISSING_API_KEY");
            }

            return new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = credentials.ApiKey,
                ApplicationName = ApplicationName
            });
        }

        /// <summary>
        /// Ưu tiên OAuth2 nếu có đủ thông tin (hỗ trợ cả đọc + ghi, kể cả dữ liệu riêng tư),
        /// fallback về ApiKey (chỉ đọc dữ liệu công khai) nếu không có OAuth.
        /// </summary>
        public static YouTubeService CreateService(SocialCredentials credentials)
        {
            if (HasOAuthCapability(credentials))
                return CreateOAuthService(credentials);

            return CreateApiKeyService(credentials);
        }

        private static string[] ResolveScopes(string? scope)
        {
            return string.IsNullOrWhiteSpace(scope)
                ? DefaultScopes
                : scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}
