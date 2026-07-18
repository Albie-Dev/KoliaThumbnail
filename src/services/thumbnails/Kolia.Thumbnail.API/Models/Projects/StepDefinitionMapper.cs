using Kolia.Thumbnail.API.Data.Entities.Projects;

namespace Kolia.Thumbnail.API.Models.Projects
{
    public static class StepDefinitionMapper
    {
        public static StepDefinitionEntity ToEntity(this StepDefinitionCreateDto dto)
        {
            return new StepDefinitionEntity
            {
                Code = dto.Code,
                Name = dto.Name,
                SortOrder = dto.SortOrder,
                DisplayCode = dto.DisplayCode,
                IsTrackable = dto.IsTrackable
            };
        }

        public static StepDefinitionEntity ToEntity(this StepDefinitionUpdateDto dto, StepDefinitionEntity existingEntity)
        {
            existingEntity.Code = dto.Code;
            existingEntity.Name = dto.Name;
            existingEntity.SortOrder = dto.SortOrder;
            existingEntity.DisplayCode = dto.DisplayCode;
            existingEntity.IsTrackable = dto.IsTrackable;
            return existingEntity;
        }

        public static StepDefinitionDetailDto ToDetailDto(this StepDefinitionEntity entity)
        {
            return new StepDefinitionDetailDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                SortOrder = entity.SortOrder,
                DisplayCode = entity.DisplayCode,
                IsTrackable = entity.IsTrackable,
                ParentId = entity.ParentId,
                Children = entity.Children.Select(c => c.ToDetailDto()).ToList(),
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}