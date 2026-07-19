using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.News
{
    public class NewsService : INewsService
    {
        private readonly ThumbnailDbContext _db;
        private readonly ISocialExecutorService _socialExecutor;
        private readonly INewsScoringEngine _scoringEngine;
        private readonly INewsDeepAnalysisEngine _deepAnalysisEngine;

        public NewsService(
            ThumbnailDbContext db,
            ISocialExecutorService socialExecutor,
            INewsScoringEngine scoringEngine,
            INewsDeepAnalysisEngine deepAnalysisEngine)
        {
            _db = db;
            _socialExecutor = socialExecutor;
            _scoringEngine = scoringEngine;
            _deepAnalysisEngine = deepAnalysisEngine;
        }

        public async Task<NewsSearchRequestEntity> SearchAsync(
            Guid projectId,
            CMarketScope marketScope,
            CNewsTimeRange timeRange,
            CNewsCountFilter countFilter,
            string keywordsRaw,
            IEnumerable<string>? suggestedKeywordsSelected,
            CancellationToken ct = default)
        {
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
                CNewsTimeRange.Last7Days   => 7,
                CNewsTimeRange.Last30Days  => 30,
                _ => 7
            };

            const int UnlimitedCountCap = 200; // trần kỹ thuật để tránh crawl vô hạn — có thể đưa ra config sau
            var maxCount = countFilter switch
            {
                CNewsCountFilter.Top10 => 10,
                CNewsCountFilter.Top20 => 20,
                CNewsCountFilter.Top30 => 30,
                CNewsCountFilter.All   => UnlimitedCountCap,
                _                      => 10
            };

            var keywords = keywordsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Concat(suggestedKeywordsSelected ?? [])
                .Distinct()
                .ToList();

            // Crawl từ social executor (rate-limit safe)
            var crawledItems = await _socialExecutor.RssCrawlAsync(
                keywords, marketScope, timeRangeDays, maxCount, projectId, ct);

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

            // Batch scoring
            var scoringBatch = crawledItems
                .Select(c => (Guid.CreateVersion7(DateTimeOffset.UtcNow), c.Title, c.SourceName, c.SummaryRaw))
                .ToList();

            // Map crawled items → NewsItemEntity, chấm điểm
            var newsItems = new List<NewsItemEntity>();
            foreach (var crawled in crawledItems)
            {
                var item = new NewsItemEntity
                {
                    ProjectId = projectId,
                    NewsSearchRequestId = searchRequest.Id,
                    SourceType = CSourceType.Crawled,
                    MarketType = marketScope,
                    Title = crawled.Title,
                    SourceName = crawled.SourceName,
                    SourceUrl = crawled.SourceUrl,
                    PublishedTime = crawled.PublishedTime,
                    ScannedTime = DateTimeOffset.UtcNow,
                    SummaryOverview = crawled.SummaryRaw
                };
                newsItems.Add(item);
            }

            // Gọi batch scoring 1 lần duy nhất
            if (newsItems.Count > 0)
            {
                var batchInput = newsItems
                    .Select(n => (n.Id, n.Title, n.SourceName, n.SummaryOverview))
                    .ToList();

                var scores = await _scoringEngine.ScoreBatchAsync(batchInput, topicContext, ct);

                foreach (var item in newsItems)
                {
                    if (scores.TryGetValue(item.Id, out var score))
                    {
                        item.RelevanceToTopicScore = score.RelevanceToTopicScore;
                        item.ImportanceImpactScore = score.ImportanceImpactScore;
                        item.EmotionPotentialScore = score.EmotionPotentialScore;
                        item.NoveltyDataScore = score.NoveltyDataScore;
                        item.TotalScore = score.TotalScore;
                        item.Recommendation = score.Recommendation;
                        item.RelevanceLevel = score.RelevanceLevel;
                        item.SummaryOverview = score.SummaryOverview;
                        item.SuggestedKeywordsForThumbnail = score.SuggestedKeywordsForThumbnail;
                    }
                }
            }

            _db.NewsItems.AddRange(newsItems);
            await _db.SaveChangesAsync(ct);

            // Load lại entity để trả về với navigation đầy đủ
            searchRequest.NewsItems = newsItems;
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
                item.TotalScore = score.TotalScore;
                item.Recommendation = score.Recommendation;
                item.RelevanceLevel = score.RelevanceLevel;
                item.SummaryOverview = score.SummaryOverview;
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

        public async Task<NewsDeepAnalysisEntity> DeepAnalyzeAsync(Guid newsItemId,
            CancellationToken ct = default)
        {
            var item = await _db.NewsItems
                .Include(n => n.DeepAnalysis)
                .FirstOrDefaultAsync(n => n.Id == newsItemId && !n.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"NewsItem {newsItemId} không tìm thấy.");

            if (item.DeepAnalysis != null)
                return item.DeepAnalysis;

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
            await _db.SaveChangesAsync(ct);
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
