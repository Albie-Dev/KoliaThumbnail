using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.CompletePackages;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.CompletePackages
{
    public class CompletePackageService : ICompletePackageService
    {
        private readonly ThumbnailDbContext _db;

        public CompletePackageService(ThumbnailDbContext db)
        {
            _db = db;
        }

        public async Task<CompletePackageEntity> ConfirmAsync(
            Guid projectId,
            Guid selectedThumbnailId,
            IEnumerable<Guid> selectedTitleOptionIds,
            CancellationToken ct = default)
        {
            // Kiểm tra ảnh thumbnail có tồn tại và thuộc project không
            var thumbnail = await _db.GeneratedThumbnails
                .FirstOrDefaultAsync(t => t.Id == selectedThumbnailId, ct)
                ?? throw new KeyNotFoundException($"GeneratedThumbnail {selectedThumbnailId} không tồn tại.");

            // Lưu snapshot Display Text tại thời điểm xác nhận
            var textSnapshot = thumbnail.DisplayTextSnapshot;

            var package = new CompletePackageEntity
            {
                ProjectId = projectId,
                SelectedThumbnailId = selectedThumbnailId,
                DisplayTextSnapshot = textSnapshot,
                ConfirmedAt = DateTimeOffset.UtcNow
            };

            foreach (var titleId in selectedTitleOptionIds)
            {
                package.SelectedTitles.Add(new CompletePackageTitleEntity
                {
                    CompletePackageId = package.Id,
                    VideoTitleOptionId = titleId
                });
            }

            _db.CompletePackages.Add(package);

            // Tự động chuyển trạng thái Project sang Completed khi lưu xong package cuối cùng
            var project = await _db.Projects.FindAsync([projectId], ct);
            if (project != null)
            {
                project.Status = Enums.CProjectStatus.Completed;
                project.LastActivityTime = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            return package;
        }

        public async Task<IReadOnlyList<CompletePackageEntity>> GetByProjectAsync(
            Guid projectId, CancellationToken ct = default)
        {
            return await _db.CompletePackages
                .Include(p => p.SelectedThumbnail)
                .Include(p => p.SelectedTitles)
                    .ThenInclude(t => t.VideoTitleOption)
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .OrderByDescending(p => p.ConfirmedAt)
                .ToListAsync(ct);
        }
    }
}
