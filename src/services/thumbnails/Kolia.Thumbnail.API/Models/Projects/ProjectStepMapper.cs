using Kolia.Thumbnail.API.Data.Entities.Projects;

namespace Kolia.Thumbnail.API.Models.Projects
{
    public static class ProjectStepMapper
    {
        public static ProjectStepEntity ToEntity(this ProjectStepCreateDto dto)
        {
            return new ProjectStepEntity
            {
                ProjectId = dto.ProjectId,
                StepDefinitionId = dto.StepDefinitionId,
                Status = dto.Status,
                ContentJson = dto.ContentJson,
                StartedAt = dto.StartedAt,
                CompletedAt = dto.CompletedAt,
                ErrorMessage = dto.ErrorMessage
            };
        }

        public static ProjectStepEntity ToEntity(this ProjectStepUpdateDto dto, ProjectStepEntity existingEntity)
        {
            existingEntity.Status = dto.Status;
            existingEntity.ContentJson = dto.ContentJson;
            existingEntity.StartedAt = dto.StartedAt;
            existingEntity.CompletedAt = dto.CompletedAt;
            existingEntity.ErrorMessage = dto.ErrorMessage;
            return existingEntity;
        }

        public static ProjectStepDetailDto ToDetailDto(this ProjectStepEntity entity)
        {
            return new ProjectStepDetailDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                StepDefinitionId = entity.StepDefinitionId,
                Status = entity.Status,
                ContentJson = entity.ContentJson,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                ErrorMessage = entity.ErrorMessage,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted,
                StepDefinition = entity.StepDefinition?.ToDetailDto()
            };
        }
    }
}