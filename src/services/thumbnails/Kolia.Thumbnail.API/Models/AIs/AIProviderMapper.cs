using Kolia.Thumbnail.API.Data.Entities.AIs;

namespace Kolia.Thumbnail.API.Models.AIs
{
    public static class AIProviderMapper
    {
        public static AIProviderEntity ToEntity(this AIProviderCreateDto dto)
        {
            return new AIProviderEntity
            {
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                ShortName = dto.ShortName,
                ProviderType = dto.ProviderType,
                BaseUrl = dto.BaseUrl
            };
        }

        public static AIProviderEntity ToEntity(this AIProviderUpdateDto dto,
            AIProviderEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.ImageUrl = dto.ImageUrl;
            existingEntity.ShortName = dto.ShortName;
            existingEntity.BaseUrl = dto.BaseUrl;
            existingEntity.ProviderType = dto.ProviderType;
            return existingEntity;
        }

        public static AIProviderDetailDto ToDetailDto(this AIProviderEntity entity)
        {
            return new AIProviderDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ImageUrl = entity.ImageUrl,
                ShortName = entity.ShortName,
                BaseUrl = entity.BaseUrl,
                ProviderType = entity.ProviderType,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}