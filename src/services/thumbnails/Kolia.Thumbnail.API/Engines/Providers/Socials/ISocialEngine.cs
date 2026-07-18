using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Providers.Socials
{
    /// <summary>
    /// Interface gốc - mọi social media provider (Youtube, Facebook, Tiktok, X...) trong hệ
    /// thống đều phải implement. Chỉ chứa các thao tác chung: quản lý credentials và khai báo
    /// capabilities. Các khả năng cụ thể (quản lý video, playlist, channel, comment, live
    /// stream...) nằm ở các interface con (IVideoManagementCapableEngine,
    /// IChannelManagementCapableEngine...) - engine nào hỗ trợ gì thì implement thêm interface đó.
    /// </summary>
    public interface ISocialEngine
    {
        /// <summary>
        /// Loại social media provider mà instance này đại diện.
        /// </summary>
        CSocialMediaProviderType ProviderType { get; }

        /// <summary>
        /// Kiểm tra bộ credentials (ApiKey hoặc OAuth token) có hợp lệ / còn hoạt động hay không.
        /// </summary>
        Task<bool> ValidateCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default);

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token (nếu provider dùng OAuth2).
        /// Trả về bản credentials mới với AccessToken đã cập nhật để tầng gọi (Service/Executor)
        /// có thể lưu lại vào DB. Provider không hỗ trợ refresh token (VD chỉ dùng ApiKey tĩnh) thì
        /// trả về nguyên vẹn credentials đầu vào.
        /// </summary>
        Task<SocialCredentials> RefreshCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default);

        /// <summary>
        /// Khai báo các năng lực mà provider này hỗ trợ, dùng để hệ thống
        /// biết cast sang interface con nào (IVideoManagementCapableEngine, IChannelManagementCapableEngine...).
        /// </summary>
        SocialProviderCapabilities GetCapabilities();
    }
}
