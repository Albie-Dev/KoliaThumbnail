using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.VideoTitles
{
    public class VideoTitleService : IVideoTitleService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IVideoTitleGenerationEngine _generationEngine;

        public VideoTitleService(
            ThumbnailDbContext db,
            IVideoTitleGenerationEngine generationEngine)
        {
            _db = db;
            _generationEngine = generationEngine;
        }

        public async Task<VideoTitleRequestEntity> GenerateAsync(
            Guid projectId,
            IEnumerable<Guid> selectedThumbnailIds,
            IEnumerable<Guid> selectedNewsItemIds,
            CTitleStyle style,
            string keywordsRaw,
            int requestedCount,
            CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? keywordsRaw;

            var displayTexts = await _db.GeneratedThumbnails
                .Where(t => selectedThumbnailIds.Contains(t.Id))
                .Select(t => t.DisplayTextSnapshot)
                .ToListAsync(ct);

            var newsSummaries = await _db.NewsItems
                .Where(n => selectedNewsItemIds.Contains(n.Id))
                .Select(n => $"{n.Title}: {n.SummaryOverview}")
                .ToListAsync(ct);

            // Xây dựng prompt tổng hợp qua engine
            var builtPrompt = await _generationEngine.BuildPromptAsync(displayTexts, newsSummaries, topicContext, ct);

            // Gọi AI tạo titles
            var genResult = await _generationEngine.GenerateAsync(builtPrompt, style, requestedCount, ct);

            var request = new VideoTitleRequestEntity
            {
                ProjectId = projectId,
                RequestedTitleCount = requestedCount,
                Style = style,
                KeywordsRaw = keywordsRaw,
                BuiltPromptText = builtPrompt,
                GenerationRound = 1
            };

            foreach (var thumbId in selectedThumbnailIds)
            {
                request.SelectedThumbnails.Add(new VideoTitleRequestThumbnailEntity
                {
                    VideoTitleRequestId = request.Id,
                    GeneratedThumbnailId = thumbId
                });
            }

            foreach (var newsId in selectedNewsItemIds)
            {
                request.SelectedNewsItems.Add(new VideoTitleRequestNewsItemEntity
                {
                    VideoTitleRequestId = request.Id,
                    NewsItemId = newsId
                });
            }

            foreach (var title in genResult.Titles)
            {
                request.Options.Add(new VideoTitleOptionEntity
                {
                    VideoTitleRequestId = request.Id,
                    GenerationRound = 1,
                    Content = title,
                    IsSelected = false
                });
            }

            _db.VideoTitleRequests.Add(request);
            await _db.SaveChangesAsync(ct);
            return request;
        }

        public async Task<VideoTitleRequestEntity> RegenerateAsync(
            Guid videoTitleRequestId, CancellationToken ct = default)
        {
            var request = await _db.VideoTitleRequests
                .Include(r => r.Options)
                .FirstOrDefaultAsync(r => r.Id == videoTitleRequestId && !r.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"VideoTitleRequest {videoTitleRequestId} không tồn tại.");

            var nextRound = request.GenerationRound + 1;
            request.GenerationRound = nextRound;

            var genResult = await _generationEngine.GenerateAsync(
                request.BuiltPromptText, request.Style, request.RequestedTitleCount, ct);

            foreach (var title in genResult.Titles)
            {
                request.Options.Add(new VideoTitleOptionEntity
                {
                    VideoTitleRequestId = request.Id,
                    GenerationRound = nextRound,
                    Content = title,
                    IsSelected = false
                });
            }

            request.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return request;
        }

        public async Task<VideoTitleRequestEntity> RegenerateWithFeedbackAsync(
            Guid videoTitleRequestId, string feedbackText, CancellationToken ct = default)
        {
            var request = await _db.VideoTitleRequests
                .Include(r => r.Options)
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.Id == videoTitleRequestId && !r.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"VideoTitleRequest {videoTitleRequestId} không tồn tại.");

            var nextRound = request.GenerationRound + 1;
            request.GenerationRound = nextRound;

            // Lưu lại feedback
            var feedback = new VideoTitleFeedbackEntity
            {
                VideoTitleRequestId = videoTitleRequestId,
                FeedbackText = feedbackText,
                AppliedToRound = nextRound
            };
            _db.VideoTitleFeedbacks.Add(feedback);

            // Gọi AI gen với feedback
            var genResult = await _generationEngine.GenerateWithFeedbackAsync(
                request.BuiltPromptText, request.Style, request.RequestedTitleCount, feedbackText, ct);

            foreach (var title in genResult.Titles)
            {
                request.Options.Add(new VideoTitleOptionEntity
                {
                    VideoTitleRequestId = request.Id,
                    GenerationRound = nextRound,
                    Content = title,
                    IsSelected = false
                });
            }

            request.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return request;
        }

        public async Task SetSelectedAsync(
            Guid videoTitleOptionId, bool isSelected, CancellationToken ct = default)
        {
            var option = await _db.VideoTitleOptions.FindAsync([videoTitleOptionId], ct)
                ?? throw new KeyNotFoundException($"VideoTitleOption {videoTitleOptionId} không tồn tại.");

            option.IsSelected = isSelected;
            option.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<VideoTitleRequestEntity>> GetByProjectAsync(
            Guid projectId, CancellationToken ct = default)
        {
            return await _db.VideoTitleRequests
                .Include(r => r.Options)
                .Include(r => r.Feedbacks)
                .Where(r => r.ProjectId == projectId && !r.IsDeleted)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync(ct);
        }
    }
}
