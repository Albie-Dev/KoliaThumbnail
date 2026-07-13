using Kolia.Thumbnail.API.Data.Entities.Socials;

namespace Kolia.Thumbnail.API.Models.SocialMedias
{
    public static class SocialMediaProviderMapper
    {
        public static SocialMediaProviderEntity ToEntity(this SocialMediaProviderCreateDto dto)
        {
            return new SocialMediaProviderEntity
            {
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                ShortName = dto.ShortName,
                ProviderType = dto.ProviderType,
                BaseUrl = dto.BaseUrl
            };
        }

        public static SocialMediaProviderEntity ToEntity(this SocialMediaProviderUpdateDto dto,
            SocialMediaProviderEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.ImageUrl = dto.ImageUrl;
            existingEntity.ShortName = dto.ShortName;
            existingEntity.BaseUrl = dto.BaseUrl;
            existingEntity.ProviderType = dto.ProviderType;
            return existingEntity;
        }

        public static SocialMediaProviderDetailDto ToDetailDto(this SocialMediaProviderEntity entity)
        {
            return new SocialMediaProviderDetailDto
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