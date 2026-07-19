using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.Thumbnails
{
    public class ThumbnailLibraryService : IThumbnailLibraryService
    {
        private readonly ThumbnailDbContext _db;
        private readonly ISocialExecutorService _socialExecutor;
        private readonly IThumbnailAnalysisEngine _analysisEngine;
        private readonly IContentRelevanceFilterEngine _relevanceFilterEngine;

        public ThumbnailLibraryService(
            ThumbnailDbContext db,
            ISocialExecutorService socialExecutor,
            IThumbnailAnalysisEngine analysisEngine,
            IContentRelevanceFilterEngine relevanceFilterEngine)
        {
            _db = db;
            _socialExecutor = socialExecutor;
            _analysisEngine = analysisEngine;
            _relevanceFilterEngine = relevanceFilterEngine;
        }

        public async Task<ThumbnailSearchRequestEntity> SearchAsync(
            Guid projectId,
            string keyword,
            CThumbnailTimeFilter timeFilter,
            CThumbnailSortFilter sortFilter,
            bool wasSuggestedFromNews = false,
            CancellationToken ct = default)
        {
            var searchRequest = new ThumbnailSearchRequestEntity
            {
                ProjectId = projectId,
                Keyword = keyword,
                TimeFilter = timeFilter,
                SortFilter = sortFilter,
                WasSuggestedFromNews = wasSuggestedFromNews
            };

            _db.ThumbnailSearchRequests.Add(searchRequest);

            // Tìm kiếm trên YouTube qua social executor (rate-limit safe)
            var youtubeVideos = await _socialExecutor.YouTubeSearchAsync(
                keyword, timeFilter, sortFilter, maxResults: 15, projectId, ct);

            var items = new List<ThumbnailLibraryItemEntity>();
            foreach (var video in youtubeVideos)
            {
                var libraryItem = new ThumbnailLibraryItemEntity
                {
                    ProjectId = projectId,
                    ThumbnailSearchRequestId = searchRequest.Id,
                    SourceType = CSourceType.Crawled, // Hoặc crawl tự động
                    Platform = CThumbnailPlatform.Youtube,
                    VideoTitle = video.Title,
                    VideoUrl = video.VideoUrl,
                    ChannelName = video.ChannelName,
                    ThumbnailImageUrl = video.ThumbnailImageUrl,
                    PublishedTime = video.PublishedTime,
                    ViewCount = video.ViewCount,
                    MarketType = InferMarketTypeFromChannel(video.ChannelName),
                    KeywordBatchTag = keyword,
                    UserStatus = CLibraryUserStatus.Pending
                };
                items.Add(libraryItem);
            }

            // Phân loại nội dung không liên quan ngay khi crawl xong
            foreach (var item in items)
            {
                var classification = await _relevanceFilterEngine.ClassifyAsync(item.VideoTitle, item.ChannelName ?? string.Empty, ct);
                item.IsFilteredIrrelevant = classification.IsIrrelevant;
                if (item.MarketType == null && classification.InferredMarketType != null)
                    item.MarketType = classification.InferredMarketType;
            }

            _db.ThumbnailLibraryItems.AddRange(items);
            await _db.SaveChangesAsync(ct);

            searchRequest.LibraryItems = items;
            return searchRequest;
        }

        public async Task<ThumbnailLibraryItemEntity> ImportManualLinkAsync(
            Guid projectId, string videoUrl, CancellationToken ct = default)
        {
            var videoInfo = await _socialExecutor.YouTubeFetchByUrlAsync(videoUrl, projectId, ct)
                ?? new YouTubeVideoResult(
                    VideoId: "Manual",
                    Title: "Video nhập thủ công",
                    ChannelName: "Thủ công",
                    ThumbnailImageUrl: videoUrl, // Fallback nếu URL chính là ảnh
                    VideoUrl: videoUrl,
                    PublishedTime: null,
                    ViewCount: null);

            var item = new ThumbnailLibraryItemEntity
            {
                ProjectId = projectId,
                SourceType = CSourceType.ManualLink,
                Platform = CThumbnailPlatform.Youtube,
                VideoTitle = videoInfo.Title,
                VideoUrl = videoInfo.VideoUrl,
                ChannelName = videoInfo.ChannelName,
                ThumbnailImageUrl = videoInfo.ThumbnailImageUrl,
                KeywordBatchTag = "Manual",
                UserStatus = CLibraryUserStatus.Approved
            };

            _db.ThumbnailLibraryItems.Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }

        public async Task<IReadOnlyList<ThumbnailLibraryItemEntity>> GetLibraryAsync(
            Guid projectId, bool excludeIrrelevant = true, CancellationToken ct = default)
        {
            var query = _db.ThumbnailLibraryItems
                .Where(t => t.ProjectId == projectId && !t.IsDeleted);

            if (excludeIrrelevant)
            {
                query = query.Where(t => !t.IsFilteredIrrelevant);
            }

            return await query
                .OrderByDescending(t => t.CreationTime)
                .ToListAsync(ct);
        }

        public async Task<ThumbnailAnalysisEntity> DeepAnalyzeAsync(
            Guid libraryItemId, CancellationToken ct = default)
        {
            var item = await _db.ThumbnailLibraryItems
                .Include(t => t.Analysis)
                .FirstOrDefaultAsync(t => t.Id == libraryItemId && !t.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Thumbnail item {libraryItemId} không tồn tại.");

            if (item.Analysis != null)
                return item.Analysis;

            var analysisResult = await _analysisEngine.AnalyzeAsync(
                item.ThumbnailImageUrl, item.VideoTitle, ct);

            var analysis = new ThumbnailAnalysisEntity
            {
                ThumbnailLibraryItemId = libraryItemId,
                ThumbnailFactorsJson = analysisResult.ThumbnailFactorsJson,
                TitleTextAnalysis = analysisResult.TitleTextAnalysis,
                VideoTitleAnalysis = analysisResult.VideoTitleAnalysis,
                DisplayTextStyleNote = analysisResult.DisplayTextStyleNote,
                IsChosenForGeneration = false
            };

            _db.ThumbnailAnalyses.Add(analysis);
            await _db.SaveChangesAsync(ct);
            return analysis;
        }

        public async Task SetUserStatusAsync(
            Guid libraryItemId, CLibraryUserStatus status, CancellationToken ct = default)
        {
            var item = await _db.ThumbnailLibraryItems.FindAsync([libraryItemId], ct)
                ?? throw new KeyNotFoundException($"Thumbnail item {libraryItemId} không tồn tại.");

            item.UserStatus = status;
            if (status == CLibraryUserStatus.Rejected)
            {
                item.IsFilteredIrrelevant = true;
            }
            item.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task SetChosenForGenerationAsync(
            Guid libraryItemId, bool chosen, CancellationToken ct = default)
        {
            var item = await _db.ThumbnailLibraryItems
                .Include(t => t.Analysis)
                .FirstOrDefaultAsync(t => t.Id == libraryItemId && !t.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Thumbnail item {libraryItemId} không tồn tại.");

            var analysis = item.Analysis 
                ?? await DeepAnalyzeAsync(libraryItemId, ct);

            analysis.IsChosenForGeneration = chosen;
            analysis.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Tạm suy luận MarketType từ channel name — luôn return null ở Phase 1
        /// để tránh gán bừa 'Domestic' mặc định gây sai dữ liệu.
        /// Phase 5.2 sẽ dùng AI thật để phân loại chính xác hơn.
        /// </summary>
        private static CMarketScope? InferMarketTypeFromChannel(string? channelName) => null;
    }
}
