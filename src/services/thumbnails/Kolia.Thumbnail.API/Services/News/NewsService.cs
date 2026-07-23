using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.News
{
    public class NewsService : INewsService
    {
        private readonly ThumbnailDbContext _db;
        private readonly ISocialExecutorService _socialExecutor;
        private readonly INewsScoringEngine _scoringEngine;
        private readonly INewsDeepAnalysisEngine _deepAnalysisEngine;
        private readonly OperationProgressStore _progressStore;
        private readonly ILogger<NewsService> _logger;

        public NewsService(
            ThumbnailDbContext db,
            ISocialExecutorService socialExecutor,
            INewsScoringEngine scoringEngine,
            INewsDeepAnalysisEngine deepAnalysisEngine,
            OperationProgressStore progressStore,
            ILogger<NewsService> logger)
        {
            _db = db;
            _socialExecutor = socialExecutor;
            _scoringEngine = scoringEngine;
            _deepAnalysisEngine = deepAnalysisEngine;
            _progressStore = progressStore;
            _logger = logger;
        }

        private OperationProgress? StartProgress(Guid operationId, string title)
        {
            if (operationId == Guid.Empty) return null;
            return _progressStore.Create(operationId, title);
        }

        private void LogProgress(OperationProgress? progress, string message, bool isError = false)
        {
            if (progress == null) return;
            _progressStore.AppendLog(progress.OperationId, message, isError);
        }

        private void CompleteProgress(OperationProgress? progress, string? error = null)
        {
            if (progress == null) return;
            _progressStore.Complete(progress.OperationId, error);
        }

        public async Task<NewsSearchRequestEntity> SearchAsync(
            Guid projectId,
            CMarketScope marketScope,
            CNewsTimeRange timeRange,
            CNewsCountFilter countFilter,
            string keywordsRaw,
            IEnumerable<string>? suggestedKeywordsSelected,
            Guid operationId = default,
            CancellationToken ct = default)
        {
            var progress = StartProgress(operationId, "Tìm kiếm tin tức");
            LogProgress(progress, "🔍 Bắt đầu tìm kiếm tin tức...");

            // Lấy chủ đề từ ContentBrief để chấm điểm relevance
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? keywordsRaw;

            // Tính số ngày và max count
            var timeRangeDays = timeRange switch
            {
                CNewsTimeRange.Last24Hours => 1,
                CNewsTimeRange.Last48Hours => 2,
                CNewsTimeRange.Last72Hours => 3,
                CNewsTimeRange.Last7Days => 7,
                CNewsTimeRange.Last30Days => 30,
                _ => 7
            };

            const int UnlimitedCountCap = 200; // trần kỹ thuật để tránh crawl vô hạn — có thể đưa ra config sau
            var maxCount = countFilter switch
            {
                CNewsCountFilter.Top10 => 10,
                CNewsCountFilter.Top20 => 20,
                CNewsCountFilter.Top30 => 30,
                CNewsCountFilter.All => UnlimitedCountCap,
                _ => 10
            };

            var keywords = keywordsRaw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => k.Length > 0)
                .Concat(suggestedKeywordsSelected ?? [])
                .Distinct()
                .ToList();

            LogProgress(progress, $"📡 Crawl {maxCount} tin từ {keywords.Count} keyword...");
            var crawledItems = await _socialExecutor.RssCrawlAsync(
                keywords, marketScope, timeRangeDays, maxCount, projectId, ct);
            LogProgress(progress, $"✅ Crawl xong — {crawledItems.Count} tin được thu thập.");

            // Tạo NewsSearchRequest
            var searchRequest = new NewsSearchRequestEntity
            {
                ProjectId = projectId,
                MarketScope = marketScope,
                TimeRange = timeRange,
                CountFilter = countFilter,
                KeywordsRaw = keywordsRaw,
                SuggestedKeywordsUsedJson = suggestedKeywordsSelected == null
                    ? null
                    : System.Text.Json.JsonSerializer.Serialize(suggestedKeywordsSelected)
            };
            _db.NewsSearchRequests.Add(searchRequest);

            // Batch scoring (executed below after newsItems are built)

            // Map crawled items → NewsItemEntity, chấm điểm
            var newsItems = new List<NewsItemEntity>();
            foreach (var crawled in crawledItems)
            {
                var item = new NewsItemEntity
                {
                    ProjectId = projectId,
                    NewsSearchRequestId = searchRequest.Id,
                    SourceType = CSourceType.Crawled,
                    MarketType = crawled.MarketType,
                    Title = crawled.Title,
                    SourceName = crawled.SourceName,
                    SourceUrl = crawled.SourceUrl,
                    PublishedTime = crawled.PublishedTime,
                    ScannedTime = DateTimeOffset.UtcNow,
                    SummaryOverview = crawled.SummaryRaw
                };
                newsItems.Add(item);
            }
            LogProgress(progress, $"🤖 Chấm điểm AI cho {newsItems.Count} tin...");
            // Gọi batch scoring 1 lần duy nhất (Bug #6 fix — bật lại đoạn bị comment)
            if (newsItems.Count > 0)
            {
                var batchInput = newsItems
                    .Select(n => (n.Id, n.Title, n.SourceName, n.SummaryOverview))
                    .ToList();

                try
                {
                    var scores = await _scoringEngine.ScoreBatchAsync(batchInput, topicContext, ct);
                    foreach (var item in newsItems)
                    {
                        if (scores.TryGetValue(item.Id, out var score))
                        {
                            item.RelevanceToTopicScore = score.RelevanceToTopicScore;
                            item.ImportanceImpactScore = score.ImportanceImpactScore;
                            item.EmotionPotentialScore = score.EmotionPotentialScore;
                            item.NoveltyDataScore = score.NoveltyDataScore;
                            item.DataQualityScore = score.DataQualityScore;
                            item.TotalScore = score.TotalScore;
                            item.Recommendation = score.Recommendation;
                            item.RelevanceLevel = score.RelevanceLevel;
                            item.SummaryOverview = score.SummaryOverview;
                            item.SuggestedKeywordsForThumbnail = score.SuggestedKeywordsForThumbnail;
                            item.EmotionTags = score.EmotionTags;
                        }
                    }
                }
                catch (ExternalServiceException ex)
                {
                    _logger.LogError(ex,
                        "Scoring thất bại toàn bộ cho request này — trả về tin CHƯA chấm điểm thay vì lỗi 500.");
                }
            }

            _db.NewsItems.AddRange(newsItems);
            await _db.SaveChangesAsync(ct);

            LogProgress(progress, $"✅ Lưu thành công {newsItems.Count} tin vào database.");

            // Load lại entity để trả về với navigation đầy đủ
            searchRequest.NewsItems = newsItems;
            CompleteProgress(progress);
            return searchRequest;
        }

        public async Task<NewsItemEntity> ImportManualLinkAsync(Guid projectId, string url,
            CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? url;

            var crawled = await _socialExecutor.RssCrawlAsync(
                [url], CMarketScope.Domestic, 30, 1, projectId, ct);

            var fetched = crawled.FirstOrDefault()
                ?? new CrawledNewsItem(url, "Thủ công", url, CMarketScope.Domestic, null, url);

            var item = new NewsItemEntity
            {
                ProjectId = projectId,
                SourceType = CSourceType.ManualLink,
                MarketType = CMarketScope.Domestic,
                Title = fetched.Title,
                SourceName = fetched.SourceName,
                SourceUrl = fetched.SourceUrl,
                PublishedTime = fetched.PublishedTime,
                ScannedTime = DateTimeOffset.UtcNow,
                SummaryOverview = fetched.SummaryRaw
            };

            var scores = await _scoringEngine.ScoreBatchAsync(
                [(item.Id, item.Title, item.SourceName, item.SummaryOverview)],
                topicContext, ct);

            if (scores.TryGetValue(item.Id, out var score))
            {
                item.RelevanceToTopicScore = score.RelevanceToTopicScore;
                item.ImportanceImpactScore = score.ImportanceImpactScore;
                item.EmotionPotentialScore = score.EmotionPotentialScore;
                item.NoveltyDataScore = score.NoveltyDataScore;
                item.DataQualityScore = score.DataQualityScore;
                item.TotalScore = score.TotalScore;
                item.Recommendation = score.Recommendation;
                item.RelevanceLevel = score.RelevanceLevel;
                item.SummaryOverview = score.SummaryOverview;
                item.SuggestedKeywordsForThumbnail = score.SuggestedKeywordsForThumbnail;
                item.EmotionTags = score.EmotionTags;
            }

            _db.NewsItems.Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }

        public async Task<IReadOnlyList<NewsItemEntity>> GetByProjectAsync(Guid projectId,
            CancellationToken ct = default)
        {
            return await _db.NewsItems
                .Where(n => n.ProjectId == projectId && !n.IsDeleted)
                .OrderByDescending(n => n.TotalScore)
                .ToListAsync(ct);
        }

        public async Task<PagedResponseDto<NewsItemDto>> GetPagedByProjectAsync(Guid projectId,
            PagedRequestDto request,
            CancellationToken ct = default)
        {
            IQueryable<NewsItemEntity> query = _db.NewsItems
                .AsNoTracking()
                .Where(n => n.ProjectId == projectId && !n.IsDeleted)
                .OrderByDescending(n => n.TotalScore);

            int totalRecords = 0;
            if (request.IncludeTotalCount)
                totalRecords = await query.CountAsync(ct);

            IReadOnlyList<NewsItemDto> items = [];
            if (request.IncludeItems && request.PageSize > 0)
            {
                var paged = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(ct);

                items = paged.Select(n => new NewsItemDto(
                    n.Id,
                    n.ProjectId,
                    n.NewsSearchRequestId,
                    n.SourceType,
                    n.Title,
                    n.SourceName,
                    n.SourceUrl,
                    n.MarketType,
                    n.PublishedTime,
                    n.ScannedTime,
                    n.SummaryOverview,
                    n.RelevanceToTopicScore,
                    n.ImportanceImpactScore,
                    n.EmotionPotentialScore,
                    n.NoveltyDataScore,
                    n.TotalScore,
                    n.Recommendation,
                    n.RelevanceLevel,
                    n.IsSelectedByTeam,
                    n.SuggestedKeywordsForThumbnail,
                    n.DeepAnalysis != null,
                    n.NewsSearchRequestId.HasValue ? "Batch" : "Manual"
                )).ToList();
            }

            return new PagedResponseDto<NewsItemDto>
            {
                Items = items,
                PageInfo = new PageInfoDto
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalRecords / request.PageSize) : 0,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber * request.PageSize < totalRecords
                }
            };
        }

        public async Task<NewsDeepAnalysisEntity> DeepAnalyzeAsync(Guid newsItemId,
            Guid operationId = default,
            CancellationToken ct = default)
        {
            var progress = StartProgress(operationId, "Phân tích sâu tin tức");
            LogProgress(progress, "🔍 Bắt đầu phân tích sâu...");

            var item = await _db.NewsItems
                .Include(n => n.DeepAnalysis)
                .FirstOrDefaultAsync(n => n.Id == newsItemId && !n.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"NewsItem {newsItemId} không tìm thấy.");

            if (item.DeepAnalysis != null)
            {
                LogProgress(progress, "✅ Đã có phân tích sâu từ trước.");
                CompleteProgress(progress);
                return item.DeepAnalysis;
            }

            LogProgress(progress, "🤖 Gọi AI phân tích 4 tầng...");
            var result = await _deepAnalysisEngine.AnalyzeAsync(
                item.Title, item.SourceUrl, item.SourceName,
                item.SummaryOverview, ct);

            var analysis = new NewsDeepAnalysisEntity
            {
                NewsItemId = newsItemId,
                MacroEventSummaryJson = System.Text.Json.JsonSerializer.Serialize(result.MacroEventSummary),
                MarketReactionJson = result.MarketReactionJson,
                ExpectationShortTerm = result.ExpectationShortTerm,
                ExpectationLongTerm = result.ExpectationLongTerm,
                SentimentOverviewJson = result.SentimentOverviewJson,
                EmotionTags = result.EmotionTags,
                EmotionReason = result.EmotionReason,
                WasTranslatedFromForeign = result.WasTranslatedFromForeign,
                MissingDataNote = result.MissingDataNote
            };

            _db.NewsDeepAnalyses.Add(analysis);

            // Cập nhật EmotionTags của NewsItem gốc từ kết quả phân tích sâu
            // (phân tích sâu cho cảm xúc chính xác hơn scoring ban đầu)
            if (result.EmotionTags != CEmotionTag.None)
            {
                item.EmotionTags = result.EmotionTags;
            }

            await _db.SaveChangesAsync(ct);

            LogProgress(progress, "✅ Phân tích sâu hoàn tất.");
            CompleteProgress(progress);
            return analysis;
        }

        public async Task SetSelectedAsync(Guid newsItemId, bool isSelected,
            CancellationToken ct = default)
        {
            var item = await _db.NewsItems.FindAsync([newsItemId], ct)
                ?? throw new KeyNotFoundException($"NewsItem {newsItemId} không tìm thấy.");

            item.IsSelectedByTeam = isSelected;
            item.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<string>> GetSuggestedKeywordsAsync(Guid projectId,
            CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);

            if (string.IsNullOrWhiteSpace(brief?.SuggestedKeywordsJson)) return [];

            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(brief.SuggestedKeywordsJson) ?? [];
        }

        public async Task DeleteNewsItemAsync(Guid newsItemId, CancellationToken ct = default)
        {
            var item = await _db.NewsItems.FindAsync([newsItemId], ct)
                ?? throw new KeyNotFoundException($"NewsItem {newsItemId} không tìm thấy.");

            item.IsDeleted = true;
            item.DeletionTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
