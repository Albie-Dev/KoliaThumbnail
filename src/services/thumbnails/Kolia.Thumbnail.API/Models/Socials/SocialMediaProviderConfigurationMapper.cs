using Kolia.Thumbnail.API.Data.Entities.Socials;
using Kolia.Thumbnail.API.Security;

namespace Kolia.Thumbnail.API.Models.SocialMedias
{
    /// <summary>
    /// Mapper chuyển đổi giữa Entity và DTO cho AI Configuration.
    /// Dùng IApiKeyProtector để mã hoá ApiKey khi lưu và giải mã/mask khi đọc.
    /// </summary>
    public class SocialMediaProviderConfigurationMapper
    {
        private readonly IApiKeyProtector _apiKeyProtector;

        public SocialMediaProviderConfigurationMapper(IApiKeyProtector apiKeyProtector)
        {
            _apiKeyProtector = apiKeyProtector;
        }

        public SocialMediaProviderConfigurationEntity ToEntity(SocialMediaProviderConfigurationCreateDto dto)
        {
            return new SocialMediaProviderConfigurationEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                ApiKey = _apiKeyProtector.Protect(dto.ApiKey),
                ApiKeyHash = _apiKeyProtector.Hash(dto.ApiKey),
                AccessToken = _apiKeyProtector.Protect(dto.AccessToken ?? string.Empty),
                RefreshToken = _apiKeyProtector.Protect(dto.RefreshToken ?? string.Empty),
                AppId = dto.AppId,
                AppSecret = _apiKeyProtector.Protect(dto.AppSecret ?? string.Empty),
                BearerToken = _apiKeyProtector.Protect(dto.BearerToken ?? string.Empty),
                ClientId = dto.ClientId,
                ClientSecret = _apiKeyProtector.Protect(dto.ClientSecret ?? string.Empty),
                ApiBaseUrl = dto.ApiBaseUrl,
                Scope = dto.Scope,
                ApiVersion = dto.ApiVersion,
                TimeoutSeconds = dto.TimeoutSeconds,
                RetryCount = dto.RetryCount,
                Priority = dto.Priority,
                IsEnabled = dto.IsEnabled,
                IsDefault = dto.IsDefault,
                SocialMediaProviderId = dto.SocialMediaProviderId,
                TotalRequest = 1
            };
        }

        public SocialMediaProviderConfigurationEntity ToEntity(
            SocialMediaProviderConfigurationUpdateDto dto,
            SocialMediaProviderConfigurationEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Description = dto.Description;

            existingEntity.AppId = dto.AppId;
            existingEntity.ClientId = dto.ClientId;
            existingEntity.ApiBaseUrl = dto.ApiBaseUrl;
            existingEntity.Scope = dto.Scope;

            existingEntity.ApiVersion = dto.ApiVersion;
            existingEntity.TimeoutSeconds = dto.TimeoutSeconds;
            existingEntity.RetryCount = dto.RetryCount;
            existingEntity.Priority = dto.Priority;

            existingEntity.IsEnabled = dto.IsEnabled;
            existingEntity.IsDefault = dto.IsDefault;

            // ApiKey
            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
            {
                var newApiKeyHash = _apiKeyProtector.Hash(dto.ApiKey);

                bool apiKeyChanged = !string.Equals(
                    existingEntity.ApiKeyHash,
                    newApiKeyHash,
                    StringComparison.Ordinal);

                if (apiKeyChanged)
                {
                    existingEntity.ApiKey = _apiKeyProtector.Protect(dto.ApiKey);
                    existingEntity.ApiKeyHash = newApiKeyHash;
                    existingEntity.TotalRequest = 1;
                    existingEntity.LastRequestResetTime = DateTimeOffset.UtcNow;
                }
            }

            // AccessToken
            if (!string.IsNullOrWhiteSpace(dto.AccessToken))
            {
                existingEntity.AccessToken =
                    _apiKeyProtector.Protect(dto.AccessToken);
            }

            // RefreshToken
            if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                existingEntity.RefreshToken =
                    _apiKeyProtector.Protect(dto.RefreshToken);
            }

            // AppSecret
            if (!string.IsNullOrWhiteSpace(dto.AppSecret))
            {
                existingEntity.AppSecret =
                    _apiKeyProtector.Protect(dto.AppSecret);
            }

            // BearerToken
            if (!string.IsNullOrWhiteSpace(dto.BearerToken))
            {
                existingEntity.BearerToken =
                    _apiKeyProtector.Protect(dto.BearerToken);
            }

            // ClientSecret
            if (!string.IsNullOrWhiteSpace(dto.ClientSecret))
            {
                existingEntity.ClientSecret =
                    _apiKeyProtector.Protect(dto.ClientSecret);
            }

            return existingEntity;
        }

        public SocialMediaProviderConfigurationDetailDto ToDetailDto(
            SocialMediaProviderConfigurationEntity entity)
        {
            return new SocialMediaProviderConfigurationDetailDto
            {
                Id = entity.Id,

                Name = entity.Name,
                Description = entity.Description,

                ApiKey = string.Empty,
                ApiKeyMasked = _apiKeyProtector.MaskFromProtected(entity.ApiKey ?? string.Empty),

                AccessToken = string.Empty,
                AccessTokenMasked = _apiKeyProtector.MaskFromProtected(entity.AccessToken ?? string.Empty),

                RefreshToken = string.Empty,
                RefreshTokenMasked = _apiKeyProtector.MaskFromProtected(entity.RefreshToken ?? string.Empty),

                AppId = entity.AppId,

                AppSecret = string.Empty,
                AppSecretMasked = _apiKeyProtector.MaskFromProtected(entity.AppSecret ?? string.Empty),

                BearerToken = string.Empty,
                BearerTokenMasked = _apiKeyProtector.MaskFromProtected(entity.BearerToken ?? string.Empty),

                ClientId = entity.ClientId,

                ClientSecret = string.Empty,
                ClientSecretMasked = _apiKeyProtector.MaskFromProtected(entity.ClientSecret ?? string.Empty),

                ApiBaseUrl = entity.ApiBaseUrl,
                Scope = entity.Scope,

                ApiVersion = entity.ApiVersion,
                TimeoutSeconds = entity.TimeoutSeconds,
                RetryCount = entity.RetryCount,
                Priority = entity.Priority,

                IsEnabled = entity.IsEnabled,
                IsDefault = entity.IsDefault,

                SocialMediaProviderId = entity.SocialMediaProviderId,

                TotalRequest = entity.TotalRequest,
                LastRequestResetTime = entity.LastRequestResetTime,

                SocialMediaProviderName = entity.SocialMediaProvider?.Name ?? string.Empty,
                SocialMediaProviderShortName = entity.SocialMediaProvider?.ShortName ?? string.Empty,
                SocialMediaProviderLogo = entity.SocialMediaProvider?.ImageUrl,

                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}