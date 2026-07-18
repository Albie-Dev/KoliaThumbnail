using Kolia.Thumbnail.API.Data.Entities.Projects;

namespace Kolia.Thumbnail.API.Models.Projects
{
    public static class ProjectMapper
    {
        public static ProjectEntity ToEntity(this ProjectCreateDto dto)
        {
            return new ProjectEntity
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                CreatedByUserId = dto.CreatedByUserId,
                StartedAt = dto.StartedAt,
                CompletedAt = dto.CompletedAt,
                FailedAt = dto.FailedAt,
                ErrorMessage = dto.ErrorMessage,
                ErrorDetail = dto.ErrorDetail,
                Status = dto.Status,
                Progress = dto.Progress,
                TotalSteps = dto.TotalSteps,
                CompletedSteps = dto.CompletedSteps
            };
        }

        public static ProjectEntity ToEntity(this ProjectUpdateDto dto, ProjectEntity existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Code = dto.Code;
            existingEntity.Description = dto.Description;
            existingEntity.StartedAt = dto.StartedAt;
            existingEntity.CompletedAt = dto.CompletedAt;
            existingEntity.FailedAt = dto.FailedAt;
            existingEntity.ErrorMessage = dto.ErrorMessage;
            existingEntity.ErrorDetail = dto.ErrorDetail;
            existingEntity.Status = dto.Status;
            existingEntity.Progress = dto.Progress;
            existingEntity.TotalSteps = dto.TotalSteps;
            existingEntity.CompletedSteps = dto.CompletedSteps;
            return existingEntity;
        }

        public static ProjectDetailDto ToDetailDto(this ProjectEntity entity)
        {
            return new ProjectDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUserName = entity.CreatedByUserName,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                FailedAt = entity.FailedAt,
                ErrorMessage = entity.ErrorMessage,
                ErrorDetail = entity.ErrorDetail,
                Status = entity.Status,
                Progress = entity.Progress,
                TotalSteps = entity.TotalSteps,
                CompletedSteps = entity.CompletedSteps,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted,
                Steps = entity.Steps.Select(s => s.ToDetailDto()).ToList()
            };
        }
    }
}