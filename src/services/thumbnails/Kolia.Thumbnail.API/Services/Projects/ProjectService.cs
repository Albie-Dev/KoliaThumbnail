using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.DTOs.Projects;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly ThumbnailDbContext _db;

        public ProjectService(ThumbnailDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResponseDto<ProjectSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            IQueryable<ProjectEntity> query = _db.Projects
                .AsNoTracking();

            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();

                if (deletedOnly == true)
                {
                    query = query.Where(x => x.IsDeleted);
                }
                else
                {
                    query = query.Where(x => !x.IsDeleted);
                }
            }
            else
            {
                // Luôn lọc bỏ soft-deleted để đảm bảo không trả về dữ liệu đã xoá,
                // kết hợp với global query filter (belt-and-suspenders).
                query = query.Where(x => !x.IsDeleted);
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<ProjectEntity, ProjectSummaryDto>(
                request,
                selector: p => new ProjectSummaryDto(
                    p.Id,
                    p.Name,
                    p.Status,
                    p.CurrentStepNumber,
                    p.ThumbnailCoverUrl,
                    p.LastActivityTime,
                    p.CreationTime),
                cancellationToken: ct);
        }

        public async Task<ProjectEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Projects
                .Include(p => p.Steps.OrderBy(s => s.StepNumber))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        }

        public async Task<ProjectEntity> CreateAsync(string name, CancellationToken ct = default)
        {
            var project = new ProjectEntity
            {
                Name = name,
                Status = CProjectStatus.Draft,
                CurrentStepNumber = CProjectStepNumber.ContentBrief,
                LastActivityTime = DateTimeOffset.UtcNow
            };

            // Seed 5 steps ngay khi tạo project
            var stepDefinitions = new[]
            {
                (CProjectStepNumber.ContentBrief,      "Nội dung video"),
                (CProjectStepNumber.News,              "Tin tức liên quan"),
                (CProjectStepNumber.ThumbnailReference,"Thumbnail tham khảo"),
                (CProjectStepNumber.GenerateThumbnail, "Tạo thumbnail"),
                (CProjectStepNumber.VideoTitle,        "Tiêu đề video")
            };

            foreach (var (stepNumber, stepName) in stepDefinitions)
            {
                project.Steps.Add(new ProjectStepEntity
                {
                    ProjectId = project.Id,
                    StepNumber = stepNumber,
                    StepName = stepName,
                    StepStatus = stepNumber == CProjectStepNumber.ContentBrief
                        ? CProjectStepStatus.InProgress
                        : CProjectStepStatus.NotStarted,
                    NeedsApproval = stepNumber == CProjectStepNumber.GenerateThumbnail
                });
            }

            _db.Projects.Add(project);
            await _db.SaveChangesAsync(ct);
            return project;
        }

        public async Task RenameAsync(Guid id, string newName, CancellationToken ct = default)
        {
            var project = await _db.Projects.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"Project {id} không tìm thấy.");

            project.Name = newName;
            project.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var project = await _db.Projects.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"Project {id} không tìm thấy.");

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync(ct);
        }

        public async Task AdvanceStepAsync(Guid projectId, CancellationToken ct = default)
        {
            var project = await _db.Projects
                .Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Project {projectId} không tìm thấy.");

            var currentStep = (int)project.CurrentStepNumber;
            if (currentStep >= (int)CProjectStepNumber.VideoTitle)
                return; // Đã ở bước cuối

            // Chuyển trạng thái project từ Draft → Running khi bắt đầu pipeline
            if (project.Status == CProjectStatus.Draft)
                project.Status = CProjectStatus.Running;

            var nextStepNumber = (CProjectStepNumber)(currentStep + 1);
            project.CurrentStepNumber = nextStepNumber;
            project.LastActivityTime = DateTimeOffset.UtcNow;

            var nextStep = project.Steps.FirstOrDefault(s => s.StepNumber == nextStepNumber);
            if (nextStep != null)
                nextStep.StepStatus = CProjectStepStatus.InProgress;

            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateStepStatusAsync(Guid projectId, CProjectStepNumber stepNumber,
            CProjectStepStatus newStatus, string? outputSummary = null,
            CancellationToken ct = default)
        {
            var step = await _db.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId
                    && s.StepNumber == stepNumber, ct)
                ?? throw new KeyNotFoundException($"Step {stepNumber} của project {projectId} không tìm thấy.");

            step.StepStatus = newStatus;
            if (outputSummary != null)
                step.OutputSummaryText = outputSummary;

            await _db.SaveChangesAsync(ct);
        }
    }
}
