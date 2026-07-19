using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kolia.Thumbnail.API.Services.Projects
{
    /// <summary>
    /// Guard kiểm tra tiến độ project. Mặc định chỉ log warning, không chặn.
    /// Khi HardBlockEnabled = true (bật từ Admin), sẽ throw BusinessException nếu chưa đủ bước.
    /// </summary>
    public class ProjectStepGuard : IProjectStepGuard
    {
        private readonly ThumbnailDbContext _db;
        private readonly ILogger<ProjectStepGuard> _logger;

        public ProjectStepGuard(ThumbnailDbContext db, ILogger<ProjectStepGuard> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Mặc định = false — chỉ log warning, không chặn.
        /// Bật lên true khi FE đã cập nhật đồng bộ với guard.
        /// </summary>
        public bool HardBlockEnabled { get; set; } = false;

        public async Task EnsureStepReachedAsync(Guid projectId, CProjectStepNumber requiredStep, CancellationToken ct = default)
        {
            var project = await _db.Projects.FindAsync([projectId], ct);
            if (project == null)
            {
                _logger.LogWarning("ProjectStepGuard: Project {ProjectId} không tồn tại.", projectId);
                return;
            }

            if ((int)project.CurrentStepNumber >= (int)requiredStep)
                return;

            var message = $"Project {projectId} đang ở bước {(int)project.CurrentStepNumber}, cần hoàn tất tới bước {(int)requiredStep} trước.";

            if (HardBlockEnabled)
            {
                throw new BusinessException(message);
            }

            _logger.LogWarning("ProjectStepGuard (WarningOnly): {Message}", message);
        }
    }
}
