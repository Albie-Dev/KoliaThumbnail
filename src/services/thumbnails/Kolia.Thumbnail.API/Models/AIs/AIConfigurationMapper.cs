using Kolia.Thumbnail.API.Data.Entities.AIs;

namespace Kolia.Thumbnail.API.Models.AIs
{
    public static class AIConfigurationMapper
    {
        public static AIConfigurationEntity ToEntity(this AIConfiurationCreateDto dto)
        {
            return new AIConfigurationEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                ApiKey = dto.ApiKey,
                BaseUrl = dto.BaseUrl,
                Endpoint = dto.Endpoint,
                ApiVersion = dto.ApiVersion,
                TimeoutSeconds = dto.TimeoutSeconds,
                RetryCount = dto.RetryCount,
                Priority = dto.Priority,
                IsEnabled = dto.IsEnabled,
                IsDefault = dto.IsDefault,
                ExtraSettingsJson = dto.ExtraSettingsJson,
                AIProviderId = dto.AIProviderId
            };
        }

        public static AIConfigurationEntity ToEntity(
            this AIConfigurationUpdateDto dto,
            AIConfigurationEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Description = dto.Description;
            existingEntity.ApiKey = dto.ApiKey;
            existingEntity.BaseUrl = dto.BaseUrl;
            existingEntity.Endpoint = dto.Endpoint;
            existingEntity.ApiVersion = dto.ApiVersion;
            existingEntity.TimeoutSeconds = dto.TimeoutSeconds;
            existingEntity.RetryCount = dto.RetryCount;
            existingEntity.Priority = dto.Priority;
            existingEntity.IsEnabled = dto.IsEnabled;
            existingEntity.IsDefault = dto.IsDefault;
            existingEntity.ExtraSettingsJson = dto.ExtraSettingsJson;
            existingEntity.AIProviderId = dto.AIProviderId;

            return existingEntity;
        }

        public static AIConfigurationDetailDto ToDetailDto(this AIConfigurationEntity entity)
        {
            return new AIConfigurationDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ApiKey = entity.ApiKey,
                BaseUrl = entity.BaseUrl,
                Endpoint = entity.Endpoint,
                ApiVersion = entity.ApiVersion,
                TimeoutSeconds = entity.TimeoutSeconds,
                RetryCount = entity.RetryCount,
                Priority = entity.Priority,
                IsEnabled = entity.IsEnabled,
                IsDefault = entity.IsDefault,
                ExtraSettingsJson = entity.ExtraSettingsJson,
                AIProviderId = entity.AIProviderId,

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