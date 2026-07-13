using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Security;

namespace Kolia.Thumbnail.API.Models.AIs
{
    /// <summary>
    /// Mapper chuyển đổi giữa Entity và DTO cho AI Configuration.
    /// Dùng IApiKeyProtector để mã hoá ApiKey khi lưu và giải mã/mask khi đọc.
    /// </summary>
    public class AIProviderConfigurationMapper
    {
        private readonly IApiKeyProtector _apiKeyProtector;

        public AIProviderConfigurationMapper(IApiKeyProtector apiKeyProtector)
        {
            _apiKeyProtector = apiKeyProtector;
        }

        public AIProviderConfigurationEntity ToEntity(AIConfiurationCreateDto dto)
        {
            return new AIProviderConfigurationEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                ApiKey = _apiKeyProtector.Protect(dto.ApiKey),
                ApiKeyHash = _apiKeyProtector.Hash(dto.ApiKey),
                ApiVersion = dto.ApiVersion,
                TimeoutSeconds = dto.TimeoutSeconds,
                RetryCount = dto.RetryCount,
                Priority = dto.Priority,
                IsEnabled = dto.IsEnabled,
                IsDefault = dto.IsDefault,
                ExtraSettingsJson = dto.ExtraSettingsJson,
                AIProviderId = dto.AIProviderId,
                TotalTokensUsed = 0,
            };
        }

        public AIProviderConfigurationEntity ToEntity(
            AIProviderConfigurationUpdateDto dto,
            AIProviderConfigurationEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Description = dto.Description;
            existingEntity.ApiVersion = dto.ApiVersion;
            existingEntity.TimeoutSeconds = dto.TimeoutSeconds;
            existingEntity.RetryCount = dto.RetryCount;
            existingEntity.Priority = dto.Priority;
            existingEntity.IsEnabled = dto.IsEnabled;
            existingEntity.IsDefault = dto.IsDefault;
            existingEntity.ExtraSettingsJson = dto.ExtraSettingsJson;
            existingEntity.AIProviderId = dto.AIProviderId;

            // Chỉ cập nhật ApiKey nếu FE gửi key mới (không rỗng)
            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
            {
                var newApiKeyHash = _apiKeyProtector.Hash(dto.ApiKey);
                bool apiKeyChanged = !string.Equals(
                    existingEntity.ApiKeyHash, newApiKeyHash, StringComparison.Ordinal);

                if (apiKeyChanged)
                {
                    existingEntity.ApiKey = _apiKeyProtector.Protect(dto.ApiKey);
                    existingEntity.ApiKeyHash = newApiKeyHash;
                    existingEntity.TotalTokensUsed = 0;
                    existingEntity.LastTokenResetTime = DateTimeOffset.UtcNow;
                }
            }

            return existingEntity;
        }

        public AIProviderConfigurationDetailDto ToDetailDto(AIProviderConfigurationEntity entity)
        {
            return new AIProviderConfigurationDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ApiKey = string.Empty,
                ApiKeyMasked = _apiKeyProtector.MaskFromProtected(entity.ApiKey),
                ApiVersion = entity.ApiVersion,
                TimeoutSeconds = entity.TimeoutSeconds,
                RetryCount = entity.RetryCount,
                Priority = entity.Priority,
                IsEnabled = entity.IsEnabled,
                IsDefault = entity.IsDefault,
                ExtraSettingsJson = entity.ExtraSettingsJson,
                AIProviderId = entity.AIProviderId,
                TotalTokensUsed = entity.TotalTokensUsed,
                LastTokenResetTime = entity.LastTokenResetTime,

                AIProviderName = entity.AIProvider.Name,
                AIProviderShortName = entity.AIProvider.ShortName,
                AIProviderLogo = entity.AIProvider.ImageUrl,

                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}