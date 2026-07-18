using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Models.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Kolia.Thumbnail.API.Projects
{
    public class ProjectService : IProjectService
    {
        private const string TrackableStepIdsCacheKey = "step-definitions:trackable-ids";
        private const string StepDefinitionTreeCacheKey = "step-definitions:tree";

        // Bảng chuyển trạng thái hợp lệ — mọi transition ngoài danh sách này bị chặn.
        private static readonly Dictionary<CProjectStatus, CProjectStatus[]> AllowedTransitions = new()
        {
            [CProjectStatus.Draft] = [CProjectStatus.Pending, CProjectStatus.Running, CProjectStatus.Cancelled],
            [CProjectStatus.Pending] = [CProjectStatus.Running, CProjectStatus.Cancelled],
            [CProjectStatus.Running] = [CProjectStatus.Paused, CProjectStatus.Completed, CProjectStatus.Failed, CProjectStatus.Cancelled],
            [CProjectStatus.Paused] = [CProjectStatus.Running, CProjectStatus.Cancelled],
            [CProjectStatus.Failed] = [CProjectStatus.Pending, CProjectStatus.Cancelled],
            [CProjectStatus.Completed] = [],
            [CProjectStatus.Cancelled] = [],
        };

        private readonly ThumbnailDbContext _dbContext;
        private readonly ILogger<ProjectService> _logger;
        private readonly IMemoryCache _cache;

        public ProjectService(
            ThumbnailDbContext dbContext,
            ILogger<ProjectService> logger,
            IMemoryCache cache)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cache = cache;
        }

        // ───────────────────────── CRUD ─────────────────────────

        public async Task<PagedResponseDto<ProjectDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<ProjectEntity> query = _dbContext.Projects.AsNoTracking();

            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();
                if (deletedOnly == true) query = query.Where(x => x.IsDeleted);
                else if (includeDeleted == false) query = query.Where(x => !x.IsDeleted);
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<ProjectEntity, ProjectDetailDto>(
                request, selector: p => p.ToDetailDto(), cancellationToken);
        }

        public async Task<ProjectDetailDto> CreateAsync(
            ProjectCreateDto projectCreateDto,
            CancellationToken cancellationToken = default)
        {
            var project = projectCreateDto.ToEntity();

            // ── Backend tự set các field còn lại ──
            project.Code = await GenerateProjectCodeAsync(cancellationToken);
            project.CreatedByUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // TODO: replace with real auth
            project.CreatedByUserName = "System";                                       // TODO: replace with real auth
            project.Status = CProjectStatus.Draft;

            // Seed ProjectStep cho mọi StepDefinition có IsTrackable = true, lấy từ cache
            // (bảng StepDefinition gần như không đổi, không cần query lại mỗi lần tạo project).
            var trackableStepIds = await GetTrackableStepDefinitionIdsAsync(cancellationToken);
            foreach (var stepDefinitionId in trackableStepIds)
            {
                project.Steps.Add(new ProjectStepEntity
                {
                    StepDefinitionId = stepDefinitionId,
                    Status = CProjectStepStatus.NotStarted,
                });
            }
            project.RecalculateProgress();

            await _dbContext.Projects.AddAsync(project, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Check-rồi-insert ở tầng ứng dụng không tuyệt đối an toàn khi có 2 request
                // trùng lúc; unique index ở DB (đã tạo ở ProjectEntityConfiguration) là chốt chặn
                // cuối cùng — convert lỗi DB thành BusinessException dễ hiểu cho client.
                throw new BusinessException(
                    message: $"Dự án có mã '{project.Code}' hoặc tên '{projectCreateDto.Name}' đã tồn tại.",
                    code: "PROJECT_CODE_OR_NAME_ALREADY_EXISTS");
            }

            _logger.LogInformation(
                "Created project {ProjectId} [{Code}] with {StepCount} steps",
                project.Id, project.Code, trackableStepIds.Count);

            return project.ToDetailDto();
        }

        public async Task<ProjectEntity?> GetByIdAsync(
            Guid id, bool asNoTracking = true, bool includeDeleted = false,
            bool includeSteps = false, CancellationToken cancellationToken = default)
        {
            IQueryable<ProjectEntity> query = _dbContext.Projects;

            if (asNoTracking) query = query.AsNoTracking();
            if (includeSteps) query = query.Include(p => p.Steps).ThenInclude(s => s.StepDefinition);
            if (includeDeleted) query = query.IgnoreQueryFilters();

            return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<ProjectDetailDto> UpdateAsync(
            Guid id, ProjectUpdateDto request, CancellationToken cancellationToken = default)
        {
            var existingProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy dự án với Id '{id}'.", "PROJECT_NOT_FOUND");

            var duplicateCode = await _dbContext.Projects.AsNoTracking()
                .AnyAsync(p => p.Code == request.Code && p.Id != id, cancellationToken);
            if (duplicateCode)
                throw new BusinessException($"Dự án có mã '{request.Code}' đã tồn tại.", "PROJECT_CODE_ALREADY_EXISTS");

            var duplicateName = await _dbContext.Projects.AsNoTracking()
                .AnyAsync(p => p.Name == request.Name && p.Id != id, cancellationToken);
            if (duplicateName)
                throw new BusinessException($"Dự án có tên '{request.Name}' đã tồn tại.", "PROJECT_NAME_ALREADY_EXISTS");

            // Lưu ý: UpdateAsync chỉ nên cho sửa Name/Code/Description — KHÔNG cho sửa Status
            // qua đây, Status phải đi qua StartAsync/PauseAsync/ResumeAsync/CancelAsync để
            // đảm bảo state machine không bị bypass. Nếu ProjectUpdateDto hiện có field Status,
            // cần bỏ field đó khỏi DTO này.
            request.ToEntity(existingProject);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProject.ToDetailDto();
        }

        public async Task<ProjectDetailDto> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy dự án với Id '{id}'.", "PROJECT_NOT_FOUND");

            _dbContext.Projects.Remove(existingProject);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProject.ToDetailDto();
        }

        // ───────────────────────── Step management ─────────────────────────

        public async Task<IReadOnlyList<ProjectStepDetailDto>> GetStepsAsync(
            Guid projectId, CancellationToken cancellationToken = default)
        {
            await EnsureProjectExistsAsync(projectId, cancellationToken);

            var steps = await _dbContext.ProjectSteps.AsNoTracking()
                .Include(s => s.StepDefinition).ThenInclude(sd => sd.Parent)
                .Where(s => s.ProjectId == projectId)
                .ToListAsync(cancellationToken);

            return steps
                .OrderBy(s => ComputeOrderKey(s.StepDefinition))
                .Select(s => s.ToDetailDto())
                .ToList();
        }

        public async Task<IReadOnlyList<ProjectStepTreeNodeDto>> GetStepTreeAsync(
            Guid projectId, CancellationToken cancellationToken = default)
        {
            await EnsureProjectExistsAsync(projectId, cancellationToken);

            var tree = await GetStepDefinitionTreeAsync(cancellationToken);
            var stepByDefinitionId = await _dbContext.ProjectSteps.AsNoTracking()
                .Where(s => s.ProjectId == projectId)
                .ToDictionaryAsync(s => s.StepDefinitionId, cancellationToken);

            return tree.Select(root => BuildTreeNode(root, stepByDefinitionId)).ToList();
        }

        public async Task<ProjectStepDetailDto> UpdateStepAsync(
            Guid projectId, Guid stepDefinitionId, ProjectStepUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy dự án với Id '{projectId}'.", "PROJECT_NOT_FOUND");

            if (project.Status is CProjectStatus.Completed or CProjectStatus.Cancelled)
                throw new BusinessException(
                    $"Không thể cập nhật bước của dự án đang ở trạng thái '{project.Status}'.",
                    "PROJECT_IS_TERMINAL");

            var step = project.Steps.FirstOrDefault(s => s.StepDefinitionId == stepDefinitionId)
                ?? throw new NotFoundException(
                    $"Không tìm thấy bước '{stepDefinitionId}' trong dự án '{projectId}'.",
                    "PROJECT_STEP_NOT_FOUND");

            step.Status = request.Status;
            if (request.ContentJson is not null) step.ContentJson = request.ContentJson;
            step.ErrorMessage = request.Status == CProjectStepStatus.Failed ? request.ErrorMessage : null;

            if (request.Status == CProjectStepStatus.InProgress)
                step.StartedAt ??= DateTimeOffset.UtcNow;
            if (request.Status is CProjectStepStatus.Completed or CProjectStepStatus.Failed)
                step.CompletedAt = DateTimeOffset.UtcNow;

            project.RecalculateProgress();

            // Tự động đẩy Project sang Running lần đầu có step chạy, và Failed nếu có step lỗi
            // — nhưng KHÔNG tự đẩy sang Completed ở đây, việc hoàn tất dự án nên là hành động
            // tường minh qua nghiệp vụ khác (vd bước "Bộ hoàn chỉnh" xong không có nghĩa là mọi
            // review/duyệt đã xong), tránh Status tự nhảy ngoài ý muốn người dùng.
            if (project.Status == CProjectStatus.Draft && request.Status == CProjectStepStatus.InProgress)
            {
                project.Status = CProjectStatus.Running;
                project.StartedAt ??= DateTimeOffset.UtcNow;
            }
            else if (request.Status == CProjectStepStatus.Failed && project.Status == CProjectStatus.Running)
            {
                project.Status = CProjectStatus.Failed;
                project.FailedAt = DateTimeOffset.UtcNow;
                project.ErrorMessage = $"Bước '{step.StepDefinitionId}' thất bại: {request.ErrorMessage}";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Project {ProjectId} step {StepDefinitionId} -> {Status}",
                projectId, stepDefinitionId, request.Status);

            return step.ToDetailDto();
        }

        // ───────────────────────── Status transitions ─────────────────────────

        public Task<ProjectDetailDto> StartAsync(Guid id, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, CProjectStatus.Running, p => p.StartedAt ??= DateTimeOffset.UtcNow, cancellationToken);

        public Task<ProjectDetailDto> PauseAsync(Guid id, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, CProjectStatus.Paused, p => p.PausedAt = DateTimeOffset.UtcNow, cancellationToken);

        public Task<ProjectDetailDto> ResumeAsync(Guid id, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, CProjectStatus.Running, p => p.PausedAt = null, cancellationToken);

        public Task<ProjectDetailDto> CancelAsync(Guid id, string? reason, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, CProjectStatus.Cancelled, p =>
            {
                p.CancelledAt = DateTimeOffset.UtcNow;
                p.CancelReason = reason;
            }, cancellationToken);

        private async Task<ProjectDetailDto> TransitionAsync(
            Guid id, CProjectStatus targetStatus, Action<ProjectEntity> applyEffect,
            CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy dự án với Id '{id}'.", "PROJECT_NOT_FOUND");

            if (!AllowedTransitions.TryGetValue(project.Status, out var allowed) || !allowed.Contains(targetStatus))
                throw new BusinessException(
                    $"Không thể chuyển dự án từ trạng thái '{project.Status}' sang '{targetStatus}'.",
                    "INVALID_PROJECT_STATUS_TRANSITION");

            project.Status = targetStatus;
            applyEffect(project);

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Project {ProjectId} transitioned to {Status}", id, targetStatus);

            return project.ToDetailDto();
        }

        // ───────────────────────── Dashboard ─────────────────────────

        public async Task<ProjectDashboardStatisticsDto> GetDashboardStatisticsAsync(
            DashboardStatisticsRequestDto request, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var todayStart = new DateTimeOffset(now.Date, TimeSpan.Zero);
            var weekStart = todayStart.AddDays(-7);
            var trendCutoff = todayStart.AddDays(-request.TrendDays);

            var totalProjectsTask = _dbContext.Projects.AsNoTracking().CountAsync(cancellationToken);

            var statusCountsTask = _dbContext.Projects.AsNoTracking()
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var averageProgressTask = _dbContext.Projects.AsNoTracking()
                .Select(p => (double?)p.Progress)
                .AverageAsync(cancellationToken);

            var createdTodayTask = _dbContext.Projects.AsNoTracking()
                .CountAsync(p => p.CreationTime >= todayStart, cancellationToken);

            var completedThisWeekTask = _dbContext.Projects.AsNoTracking()
                .CountAsync(p => p.CompletedAt != null && p.CompletedAt >= weekStart, cancellationToken);

            var recentProjectsTask = _dbContext.Projects.AsNoTracking()
                .OrderByDescending(p => p.CreationTime)
                .Take(request.RecentProjectsCount)
                .Select(p => p.ToDetailDto())
                .ToListAsync(cancellationToken);

            // Trend + bottleneck cần xử lý ở phía ứng dụng vì group theo Date/TimeSpan
            // không translate ổn định trên mọi DB provider — giới hạn theo trendCutoff
            // để dataset kéo về nhỏ, tránh load toàn bộ bảng.
            var trendRawTask = _dbContext.Projects.AsNoTracking()
                .Where(p => p.CreationTime >= trendCutoff)
                .Select(p => p.CreationTime)
                .ToListAsync(cancellationToken);

            var stepRawTask = _dbContext.ProjectSteps.AsNoTracking()
                .Include(s => s.StepDefinition)
                .Where(s => s.StartedAt != null && s.StartedAt >= trendCutoff)
                .Select(s => new
                {
                    s.StepDefinitionId,
                    s.StepDefinition.Code,
                    s.StepDefinition.Name,
                    s.Status,
                    s.StartedAt,
                    s.CompletedAt,
                })
                .ToListAsync(cancellationToken);

            await Task.WhenAll(
                totalProjectsTask, statusCountsTask, averageProgressTask, createdTodayTask,
                completedThisWeekTask, recentProjectsTask, trendRawTask, stepRawTask);

            var creationTrend = (await trendRawTask)
                .GroupBy(t => DateOnly.FromDateTime(t.UtcDateTime.Date))
                .Select(g => new ProjectTrendPointDto { Date = g.Key, Count = g.Count() })
                .OrderBy(p => p.Date)
                .ToList();

            var stepBottlenecks = (await stepRawTask)
                .GroupBy(s => new { s.StepDefinitionId, s.Code, s.Name })
                .Select(g => new StepBottleneckDto
                {
                    Code = g.Key.Code,
                    Name = g.Key.Name,
                    FailedCount = g.Count(s => s.Status == CProjectStepStatus.Failed),
                    InProgressCount = g.Count(s => s.Status == CProjectStepStatus.InProgress),
                    AverageDurationMinutes = g.Any(s => s.CompletedAt != null && s.StartedAt != null)
                        ? g.Where(s => s.CompletedAt != null && s.StartedAt != null)
                            .Average(s => (s.CompletedAt!.Value - s.StartedAt!.Value).TotalMinutes)
                        : null,
                })
                .OrderByDescending(b => b.FailedCount)
                .ToList();

            return new ProjectDashboardStatisticsDto
            {
                TotalProjects = await totalProjectsTask,
                ProjectsByStatus = (await statusCountsTask).ToDictionary(x => x.Status, x => x.Count),
                AverageProgress = await averageProgressTask ?? 0,
                ProjectsCreatedToday = await createdTodayTask,
                ProjectsCompletedThisWeek = await completedThisWeekTask,
                StepBottlenecks = stepBottlenecks,
                CreationTrend = creationTrend,
                RecentProjects = await recentProjectsTask,
            };
        }

        // ───────────────────────── Helpers ─────────────────────────

        private async Task EnsureProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Projects.AsNoTracking().AnyAsync(p => p.Id == projectId, cancellationToken);
            if (!exists)
                throw new NotFoundException($"Không tìm thấy dự án với Id '{projectId}'.", "PROJECT_NOT_FOUND");
        }

        /// <summary>
        /// Sinh mã dự án tự động theo chuẩn enterprise.
        /// <para/>
        /// Format: <c>Kolia.yyyyMMdd.nnn</c> — với yyyyMMdd là ngày tạo và nnn là số thứ tự
        /// trong ngày (001, 002, …). Cơ chế daily-reset giúp mã ngắn gọn, dễ nhìn, dễ search
        /// theo ngày, và tránh tràn số theo thời gian.
        /// <para/>
        /// Ví dụ: <c>Kolia.20260718.001</c>, <c>Kolia.20260718.042</c>.
        /// </summary>
        private async Task<string> GenerateProjectCodeAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
            var prefix = $"Kolia.{today}.";

            // Tìm số thứ tự lớn nhất trong ngày hôm nay
            var maxSeq = await _dbContext.Projects.AsNoTracking()
                .Where(p => EF.Functions.Like(p.Code, $"{prefix}%"))
                .Select(p => p.Code)
                .ToListAsync(cancellationToken);

            var nextNumber = 1;
            if (maxSeq.Count > 0)
            {
                foreach (var code in maxSeq)
                {
                    var suffix = code[prefix.Length..]; // phần "001", "002", ...
                    if (int.TryParse(suffix, out var seq) && seq >= nextNumber)
                        nextNumber = seq + 1;
                }
            }

            return $"{prefix}{nextNumber:D3}";
        }

        private async Task<List<Guid>> GetTrackableStepDefinitionIdsAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue<List<Guid>>(TrackableStepIdsCacheKey, out var cached) && cached is not null)
                return cached;

            var ids = await _dbContext.StepDefinitions.AsNoTracking()
                .Where(s => s.IsTrackable)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            _cache.Set(TrackableStepIdsCacheKey, ids, TimeSpan.FromMinutes(30));
            return ids;
        }

        private async Task<List<StepDefinitionEntity>> GetStepDefinitionTreeAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue<List<StepDefinitionEntity>>(StepDefinitionTreeCacheKey, out var cached) && cached is not null)
                return cached;

            var all = await _dbContext.StepDefinitions.AsNoTracking().ToListAsync(cancellationToken);
            var roots = all.Where(s => s.ParentId == null).OrderBy(s => s.SortOrder).ToList();
            foreach (var node in all)
                node.Children = all.Where(c => c.ParentId == node.Id).OrderBy(c => c.SortOrder).ToList();

            _cache.Set(StepDefinitionTreeCacheKey, roots, TimeSpan.FromMinutes(30));
            return roots;
        }

        private static int ComputeOrderKey(StepDefinitionEntity sd) =>
            sd.ParentId == null ? sd.SortOrder * 100 : (sd.Parent!.SortOrder * 100 + sd.SortOrder);

        private static ProjectStepTreeNodeDto BuildTreeNode(
            StepDefinitionEntity definition, IReadOnlyDictionary<Guid, ProjectStepEntity> stepByDefinitionId)
        {
            var children = definition.Children
                .Select(c => BuildTreeNode(c, stepByDefinitionId))
                .ToList();

            if (stepByDefinitionId.TryGetValue(definition.Id, out var step))
            {
                return new ProjectStepTreeNodeDto
                {
                    StepDefinitionId = definition.Id,
                    Code = definition.Code,
                    Name = definition.Name,
                    DisplayCode = definition.DisplayCode,
                    IsTrackable = definition.IsTrackable,
                    Status = step.Status,
                    ContentJson = step.ContentJson,
                    StartedAt = step.StartedAt,
                    CompletedAt = step.CompletedAt,
                    ErrorMessage = step.ErrorMessage,
                    Children = children,
                };
            }

            // Node nhóm (IsTrackable = false) — không có ProjectStep riêng, suy ra Status từ children.
            var status = DeriveGroupStatus(children);
            return new ProjectStepTreeNodeDto
            {
                StepDefinitionId = definition.Id,
                Code = definition.Code,
                Name = definition.Name,
                DisplayCode = definition.DisplayCode,
                IsTrackable = definition.IsTrackable,
                Status = status,
                Children = children,
            };
        }

        private static CProjectStepStatus DeriveGroupStatus(List<ProjectStepTreeNodeDto> children)
        {
            if (children.Count == 0) return CProjectStepStatus.NotStarted;
            if (children.Any(c => c.Status == CProjectStepStatus.Failed)) return CProjectStepStatus.Failed;
            if (children.All(c => c.Status == CProjectStepStatus.Completed)) return CProjectStepStatus.Completed;
            if (children.Any(c => c.Status is CProjectStepStatus.InProgress or CProjectStepStatus.Completed))
                return CProjectStepStatus.InProgress;
            return CProjectStepStatus.NotStarted;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            // SQL Server: 2601/2627. Postgres (Npgsql): SqlState "23505".
            ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) == true;
    }
}