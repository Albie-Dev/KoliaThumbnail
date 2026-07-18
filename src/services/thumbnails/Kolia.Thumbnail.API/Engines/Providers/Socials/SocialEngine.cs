using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Providers.Socials
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho mọi social media engine. Cung cấp khung sườn chung,
    /// các engine cụ thể (YoutubeEngine, FacebookEngine, TiktokEngine, XEngine...) kế thừa
    /// và implement thêm các interface capability tương ứng với những gì provider hỗ trợ.
    /// </summary>
    public abstract class SocialEngine : ISocialEngine
    {
        public abstract CSocialMediaProviderType ProviderType { get; }

        public abstract Task<bool> ValidateCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default);

        public abstract Task<SocialCredentials> RefreshCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default);

        public abstract SocialProviderCapabilities GetCapabilities();
    }
}
