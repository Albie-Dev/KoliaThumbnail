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
                ShortName = dto.ShortName
            };
        }

        public static AIProviderEntity ToEntity(this AIProviderUpdateDto dto,
            AIProviderEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.ImageUrl = dto.ImageUrl;
            existingEntity.ShortName = dto.ShortName;
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
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}