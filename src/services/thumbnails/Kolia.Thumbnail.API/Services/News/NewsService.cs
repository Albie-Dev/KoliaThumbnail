using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching;
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
        private readonly IArticleContentFetcher _articleFetcher;
        private readonly OperationProgressStore _progressStore;
        private readonly IGoogleNewsUrlResolver _googleNewsResolver;

        private readonly ILogger<NewsService> _logger;

        public NewsService(
            ThumbnailDbContext db,
            ISocialExecutorService socialExecutor,
            INewsScoringEngine scoringEngine,
            INewsDeepAnalysisEngine deepAnalysisEngine,
            IArticleContentFetcher articleFetcher,
            OperationProgressStore progressStore,
            IGoogleNewsUrlResolver googleNewsResolver,
            ILogger<NewsService> logger)
        {
            _db = db;
            _socialExecutor = socialExecutor;
            _scoringEngine = scoringEngine;
            _deepAnalysisEngine = deepAnalysisEngine;
            _articleFetcher = articleFetcher;
            _progressStore = progressStore;
            _googleNewsResolver = googleNewsResolver;
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
            IEnumerable<Guid>? selectedSourceIds = null,
            Guid operationId = default,
            CancellationToken ct = default)
        {
            var progress = StartProgress(operationId, "Tìm kiếm tin tức");
            LogProgress(progress, "🔍 Bắt đầu tìm kiếm tin tức...");

            // Lấy chủ đề từ ContentBrief để chấm điểm relevance
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? keywordsRaw;

            // Lấy danh sách nguồn được chọn (nếu có)
            HashSet<string>? allowedSourceNames = null;
            if (selectedSourceIds != null && selectedSourceIds.Any())
            {
                var sourceIdsList = selectedSourceIds.ToList();
                allowedSourceNames = (await _db.NewsSources
                    .Where(s => sourceIdsList.Contains(s.Id) && !s.IsDeleted)
                    .Select(s => s.Name)
                    .ToListAsync(ct))
                    .ToHashSet();

                if (allowedSourceNames.Count > 0)
                {
                    LogProgress(progress, $"🎯 Lọc theo {allowedSourceNames.Count} nguồn được chọn: {string.Join(", ", allowedSourceNames)}");
                }
            }

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

            const int UnlimitedCountCap = 200;
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
                keywords, marketScope, timeRangeDays, maxCount, projectId,
                onSourceSearched: log =>
                {
                    var msg = log.Success
                        ? $"🔎 [{log.SourceName}] ({log.Keywords}) → {log.ResultCount} tin{(log.ServedFromCache ? " [cache]" : "")}"
                        : $"⚠️ [{log.SourceName}] ({log.Keywords}) lỗi: {log.ErrorMessage}";
                    LogProgress(progress, msg, isError: !log.Success);
                },
                ct);

            // Filter theo nguồn được chọn (nếu có)
            if (allowedSourceNames != null && allowedSourceNames.Count > 0)
            {
                var beforeCount = crawledItems.Count;
                crawledItems = crawledItems
                    .Where(item => allowedSourceNames.Contains(item.SourceName))
                    .ToList();
                var afterCount = crawledItems.Count;
                LogProgress(progress, $"🔍 Lọc theo nguồn: {beforeCount} → {afterCount} tin");
            }

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

            // Dedup: lấy tập hợp SourceUrl đã tồn tại trong project (kể cả soft-deleted để tránh re-insert)
            var existingUrls = await _db.NewsItems
                .IgnoreQueryFilters()
                .Where(n => n.ProjectId == projectId)
                .Select(n => n.SourceUrl)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

            // Map crawled items → NewsItemEntity, loại bỏ URL đã có trong project
            var newsItems = new List<NewsItemEntity>();
            int skippedDuplicates = 0;
            foreach (var crawled in crawledItems)
            {
                var resolvedUrl = await _googleNewsResolver.ResolveAsync(url: crawled.SourceUrl);

                // Bỏ qua nếu URL đã tồn tại trong project
                if (existingUrls.Contains(resolvedUrl))
                {
                    skippedDuplicates++;
                    continue;
                }

                // Đánh dấu URL này để tránh trùng giữa các item trong batch hiện tại
                existingUrls.Add(resolvedUrl);

                var item = new NewsItemEntity
                {
                    ProjectId = projectId,
                    NewsSearchRequestId = searchRequest.Id,
                    SourceType = CSourceType.Crawled,
                    MarketType = crawled.MarketType,
                    Title = crawled.Title,
                    SourceName = crawled.SourceName,
                    SourceUrl = resolvedUrl,
                    PublishedTime = crawled.PublishedTime,
                    ScannedTime = DateTimeOffset.UtcNow,
                    SummaryOverview = crawled.SummaryRaw
                };

                newsItems.Add(item);
            }

            if (skippedDuplicates > 0)
                LogProgress(progress, $"🔁 Bỏ qua {skippedDuplicates} tin trùng URL đã có trong project.");
            LogProgress(progress, $"🤖 Chấm điểm AI cho {newsItems.Count} tin...");
            // Gọi batch scoring 1 lần duy nhất để populate TotalScore (cần thiết cho sort mặc định)
            if (newsItems.Count > 0)
            {
                var batchInput = newsItems
                    .Select(n => (n.Id, n.Title, n.SourceName, n.SummaryOverview))
                    .ToList();

                try
                {
                    // var scores = await _scoringEngine.ScoreBatchAsync(batchInput, topicContext, ct);
                    // foreach (var item in newsItems)
                    // {
                    //     if (scores.TryGetValue(item.Id, out var score))
                    //     {
                    //         item.RelevanceToTopicScore = score.RelevanceToTopicScore;
                    //         item.ImportanceImpactScore = score.ImportanceImpactScore;
                    //         item.EmotionPotentialScore = score.EmotionPotentialScore;
                    //         item.NoveltyDataScore = score.NoveltyDataScore;
                    //         item.DataQualityScore = score.DataQualityScore;
                    //         item.TotalScore = score.TotalScore;
                    //         item.Recommendation = score.Recommendation;
                    //         item.RelevanceLevel = score.RelevanceLevel;
                    //         item.SummaryOverview = score.SummaryOverview;
                    //         item.SuggestedKeywordsForThumbnail = score.SuggestedKeywordsForThumbnail;
                    //         item.EmotionTags = score.EmotionTags;
                    //     }
                    // }
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
            url = await _googleNewsResolver.ResolveAsync(url, ct);

            var marketScope = await _socialExecutor.DetectMarketScopeAsync(url, ct);
            _logger.LogInformation("ImportManualLinkAsync: {Url} → phát hiện scope {Scope}", url, marketScope);

            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
            var topicContext = brief?.TopicOutput ?? url;

            var crawled = await _socialExecutor.RssCrawlAsync(
                [url], marketScope, 30, 1, projectId, null, ct);

            var fetched = crawled.FirstOrDefault()
                ?? new CrawledNewsItem(url, "Thủ công", url, marketScope, null, url);

            var item = new NewsItemEntity
            {
                ProjectId = projectId,
                SourceType = CSourceType.ManualLink,
                MarketType = marketScope,
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
                .Include(n => n.DeepAnalysis)
                .AsNoTracking()
                .Where(n => n.ProjectId == projectId && !n.IsDeleted);

            if (request.Sorts == null || request.Sorts.Count == 0)
            {
                request.Sorts ??= new List<SortRequestDto>();
                request.Sorts.Add(new SortRequestDto
                {
                    Field = nameof(NewsItemEntity.TotalScore),
                    Direction = CSortDirection.Desc
                });
                // Tiebreaker: khi TotalScore bằng nhau (vd: chưa được chấm điểm AI → TotalScore = 0),
                // sắp xếp tiếp theo thời gian đăng mới nhất để đảm bảo thứ tự ổn định và có nghĩa.
                request.Sorts.Add(new SortRequestDto
                {
                    Field = nameof(NewsItemEntity.PublishedTime),
                    Direction = CSortDirection.Desc
                });
            }
            else
            {
                var firstSortField = request.Sorts.First().Field;
                if (firstSortField.Equals(nameof(NewsItemEntity.Recommendation), StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.Sorts.Any(s => s.Field.Equals(nameof(NewsItemEntity.TotalScore), StringComparison.OrdinalIgnoreCase)))
                    {
                        request.Sorts.Add(new SortRequestDto
                        {
                            Field = nameof(NewsItemEntity.TotalScore),
                            Direction = CSortDirection.Desc
                        });
                    }
                }
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<NewsItemEntity, NewsItemDto>(
                request,
                selector: n => new NewsItemDto(
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
                    n.DeepAnalysis != null && n.DeepAnalysis.Status == CDeepAnalysisStatus.Completed,
                    n.NewsSearchRequestId.HasValue ? "Batch" : "Manual",
                    n.EmotionTags
                ),
                ct);
        }

        public async Task<NewsDeepAnalysisEntity> DeepAnalyzeAsync(Guid newsItemId,
            Guid operationId = default,
            CancellationToken ct = default)
        {
            var progress = StartProgress(operationId, "Phân tích sâu tin tức");
            LogProgress(progress, "🔍 Bắt đầu phân tích sâu...");

            var item = await _db.NewsItems
                .FirstOrDefaultAsync(n => n.Id == newsItemId && !n.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"NewsItem {newsItemId} không tìm thấy.");

            // Tìm bản ghi DeepAnalysis trong DB kể cả bản ghi soft-deleted cũ (bỏ qua query filter)
            var analysis = await _db.NewsDeepAnalyses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.NewsItemId == newsItemId, ct);

            if (analysis != null && !analysis.IsDeleted)
            {
                if (analysis.Status == CDeepAnalysisStatus.Completed)
                {
                    LogProgress(progress, "✅ Đã có phân tích sâu từ trước.");
                    CompleteProgress(progress);
                    return analysis;
                }

                // Status == Failed → thử lại bằng cách cập nhật trên entity hiện tại (tránh vi phạm unique constraint)
                LogProgress(progress, "♻️ Phân tích trước đó thất bại — thử lại...");
            }

            LogProgress(progress, "📄 Đang tải nội dung đầy đủ bài báo...");

            var resolvedUrl = await _googleNewsResolver.ResolveAsync(item.SourceUrl, ct);
            if (resolvedUrl != item.SourceUrl)
            {
                item.SourceUrl = resolvedUrl;
                LogProgress(progress, "🔗 Đã giải mã url Google News sang url bài báo gốc.");
            }

            var articleContent = await _articleFetcher.FetchFullTextAsync(item.SourceUrl, ct);

            string textForAnalysis;
            if (articleContent.Success)
            {
                textForAnalysis = articleContent.FullText!;
                LogProgress(progress, $"✅ Đã tải full-text ({articleContent.CharacterCount} ký tự).");
            }
            else
            {
                textForAnalysis = item.SummaryOverview;
                LogProgress(progress,
                    $"⚠️ Không lấy được full-text ({articleContent.FailureReason}) — dùng tóm tắt ngắn thay thế.",
                    isError: true);
            }

            LogProgress(progress, "🤖 Gọi AI phân tích 4 tầng...");

            bool isNew = analysis == null;
            analysis ??= new NewsDeepAnalysisEntity { NewsItemId = newsItemId };
            analysis.IsDeleted = false;
            analysis.DeletionTime = null;

            try
            {
                var result = await _deepAnalysisEngine.AnalyzeAsync(
                    item.Title, item.SourceUrl, item.SourceName, textForAnalysis, item.MarketType, ct);

                analysis.MacroEventSummaryJson = System.Text.Json.JsonSerializer.Serialize(result.MacroEventSummary);
                analysis.MarketReactionJson = System.Text.Json.JsonSerializer.Serialize(result.MarketReaction);
                analysis.ExpectationShortTerm = result.ExpectationShortTerm;
                analysis.ExpectationLongTerm = result.ExpectationLongTerm;
                analysis.SentimentOverviewJson = System.Text.Json.JsonSerializer.Serialize(result.SentimentOverview);
                analysis.EmotionTags = result.EmotionTags;
                analysis.EmotionReason = result.EmotionReason;
                analysis.WasTranslatedFromForeign = result.WasTranslatedFromForeign;
                analysis.MissingDataNote = result.MissingDataNote;
                analysis.Status = CDeepAnalysisStatus.Completed;
                analysis.LastModificationTime = DateTimeOffset.UtcNow;

                if (isNew)
                    _db.NewsDeepAnalyses.Add(analysis);

                // Cập nhật EmotionTags của NewsItem gốc từ kết quả phân tích sâu
                if (result.EmotionTags != CEmotionTag.None)
                    item.EmotionTags = result.EmotionTags;

                await _db.SaveChangesAsync(ct);
                LogProgress(progress, "✅ Phân tích sâu hoàn tất.");
                CompleteProgress(progress);
                return analysis;
            }
            catch (ExternalServiceException ex)
            {
                LogProgress(progress, $"❌ Phân tích sâu thất bại: {ex.Message}", isError: true);

                analysis.MacroEventSummaryJson = "[]";
                analysis.MarketReactionJson = "[]";
                analysis.ExpectationShortTerm = string.Empty;
                analysis.ExpectationLongTerm = string.Empty;
                analysis.SentimentOverviewJson = "{}";
                analysis.EmotionReason = string.Empty;
                analysis.MissingDataNote = "Lỗi phân tích — cần thử lại";
                analysis.Status = CDeepAnalysisStatus.Failed;
                analysis.LastModificationTime = DateTimeOffset.UtcNow;

                if (isNew)
                    _db.NewsDeepAnalyses.Add(analysis);

                await _db.SaveChangesAsync(ct);

                CompleteProgress(progress, ex.Message);
                throw;
            }
        }

        public async Task<NewsDeepAnalysisEntity?> GetDeepAnalysisAsync(Guid newsItemId, CancellationToken ct = default)
        {
            return await _db.NewsDeepAnalyses
                .FirstOrDefaultAsync(a => a.NewsItemId == newsItemId && !a.IsDeleted, ct);
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

        public async Task<PagedResponseDto<NewsSourceSelectDto>> GetNewsSourcesPagingAsync(
            PagedRequestDto request,
            CMarketScope? region = null,
            CancellationToken ct = default)
        {
            var query = _db.NewsSources.AsNoTracking();

            // Filter theo Region (nếu có)
            if (region.HasValue && region.Value != CMarketScope.Both)
            {
                query = query.Where(s => s.Region == region.Value);
            }

            // Search theo tên
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                query = query.Where(s => s.Name.ToLower().Contains(request.SearchText.ToLower()));
            }

            // Sort theo Priority (số nhỏ = ưu tiên cao)
            query = query.OrderBy(s => s.Priority).ThenBy(s => s.Name);

            // Paging
            var totalCount = await query.CountAsync(ct);
            var sources = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new NewsSourceSelectDto(
                    s.Id,
                    s.Name,
                    s.Region,
                    s.Priority))
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new PagedResponseDto<NewsSourceSelectDto>
            {
                Items = sources,
                PageInfo = new PageInfoDto
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = request.PageNumber < totalPages,
                    HasPreviousPage = request.PageNumber > 1
                }
            };
        }
    }
}
