using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;
using Kolia.Thumbnail.API.Engines.AI;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.DisplayTexts
{
    public class DisplayTextService : IDisplayTextService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IDisplayTextGenerationEngine _generationEngine;

        public DisplayTextService(
            ThumbnailDbContext db,
            IDisplayTextGenerationEngine generationEngine)
        {
            _db = db;
            _generationEngine = generationEngine;
        }

        public async Task<DisplayTextRequestEntity> GenerateAsync(
            Guid projectId,
            IEnumerable<Guid> newsItemIds,
            CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? "Topic";

            var newsItems = await _db.NewsItems
                .Where(n => newsItemIds.Contains(n.Id) && !n.IsDeleted)
                .ToListAsync(ct);

            var newsSummaries = newsItems.ToDictionary(
                n => n.Id,
                n => $"{n.Title}: {n.SummaryOverview}");

            var genResult = await _generationEngine.GenerateAsync(newsSummaries, topicContext, ct);

            var request = new DisplayTextRequestEntity
            {
                ProjectId = projectId
            };

            foreach (var newsId in newsItemIds)
            {
                request.SelectedNewsItems.Add(new DisplayTextRequestNewsItemEntity
                {
                    DisplayTextRequestId = request.Id,
                    NewsItemId = newsId
                });
            }

            foreach (var opt in genResult.Options)
            {
                request.Options.Add(new DisplayTextOptionEntity
                {
                    DisplayTextRequestId = request.Id,
                    SourceNewsItemId = opt.SourceNewsItemId,
                    Content = opt.Content,
                    IsSelected = false
                });
            }

            _db.DisplayTextRequests.Add(request);
            await _db.SaveChangesAsync(ct);
            return request;
        }

        public async Task<IReadOnlyList<DisplayTextRequestEntity>> GetByProjectAsync(
            Guid projectId, CancellationToken ct = default)
        {
            return await _db.DisplayTextRequests
                .Include(r => r.Options)
                .Include(r => r.SelectedNewsItems)
                .Where(r => r.ProjectId == projectId && !r.IsDeleted)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync(ct);
        }

        public async Task SetSelectedAsync(
            Guid displayTextOptionId, bool isSelected, CancellationToken ct = default)
        {
            var option = await _db.DisplayTextOptions.FindAsync([displayTextOptionId], ct)
                ?? throw new KeyNotFoundException($"Display text option {displayTextOptionId} không tồn tại.");

            option.IsSelected = isSelected;
            option.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
