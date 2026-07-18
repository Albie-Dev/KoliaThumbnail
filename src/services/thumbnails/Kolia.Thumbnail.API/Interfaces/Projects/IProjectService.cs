using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.Projects;

namespace Kolia.Thumbnail.API.Projects
{
    public interface IProjectService
    {
        // CRUD
        Task<PagedResponseDto<ProjectDetailDto>> GetWithPagingAsync(
            PagedRequestDto request, bool? includeDeleted = null, bool? deletedOnly = null,
            CancellationToken cancellationToken = default);

        Task<ProjectDetailDto> CreateAsync(ProjectCreateDto projectCreateDto, CancellationToken cancellationToken = default);

        Task<ProjectEntity?> GetByIdAsync(Guid id, bool asNoTracking = true, bool includeDeleted = false,
            bool includeSteps = false, CancellationToken cancellationToken = default);

        Task<ProjectDetailDto> UpdateAsync(Guid id, ProjectUpdateDto request, CancellationToken cancellationToken = default);

        Task<ProjectDetailDto> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Step management
        Task<IReadOnlyList<ProjectStepDetailDto>> GetStepsAsync(Guid projectId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProjectStepTreeNodeDto>> GetStepTreeAsync(Guid projectId, CancellationToken cancellationToken = default);

        Task<ProjectStepDetailDto> UpdateStepAsync(Guid projectId, Guid stepDefinitionId, ProjectStepUpdateDto request,
            CancellationToken cancellationToken = default);

        // Status transitions (state machine)
        Task<ProjectDetailDto> StartAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProjectDetailDto> PauseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProjectDetailDto> ResumeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProjectDetailDto> CancelAsync(Guid id, string? reason, CancellationToken cancellationToken = default);

        // Dashboard
        Task<ProjectDashboardStatisticsDto> GetDashboardStatisticsAsync(
            DashboardStatisticsRequestDto request, CancellationToken cancellationToken = default);
    }
}