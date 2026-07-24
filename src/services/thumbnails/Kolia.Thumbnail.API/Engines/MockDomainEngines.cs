using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Engines.Providers.Domain;
using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines
{
    public class MockBriefAnalysisEngine : IBriefAnalysisEngine
    {
        public Task<BriefAnalysisResult> AnalyzeAsync(
            string overview, string viewpoint, string keyData, string? importedRawText,
            string? externalSheetContent, string? manualPrompt = null,
            CancellationToken ct = default)
        {
            var result = new BriefAnalysisResult(
                Topic: "Chủ đề livestream: Tương lai thị trường Crypto 2026",
                MainMessage: "Thị trường sẽ bứt phá nhờ dòng tiền lớn và chính sách nới lỏng.",
                HighlightData: "Fed hạ lãi suất 50 điểm cơ bản; dòng vốn ETF đạt kỷ lục.",
                SuggestedKeywords: new List<string> { "Crypto 2026", "Fed hạ lãi suất", "ETF Crypto", "Bitcoin bứt phá" }
            );
            return Task.FromResult(result);
        }

        public Task<BriefAnalysisFromPasteResult> AnalyzeFromPastedTextAsync(
            string rawText, CancellationToken ct = default)
        {
            var result = new BriefAnalysisFromPasteResult(
                OverviewInput: "Tổng quan: thị trường crypto đang có nhiều biến động tích cực nhờ các chính sách vĩ mô mới.",
                ViewpointInput: "Quan điểm: cần tập trung vào các altcoin tiềm năng và xu hướng DeFi đang nổi lên.",
                KeyDataInput: "BTC: $85,000; ETH: $4,200; TVL DeFi: $120B; Fed hạ lãi suất 50bps.",
                Topic: "Chủ đề livestream: Tương lai thị trường Crypto 2026",
                MainMessage: "Thị trường sẽ bứt phá nhờ dòng tiền lớn và chính sách nới lỏng.",
                HighlightData: "Fed hạ lãi suất 50 điểm cơ bản; dòng vốn ETF đạt kỷ lục.",
                SuggestedKeywords: new List<string> { "Crypto 2026", "Fed hạ lãi suất", "ETF Crypto", "Bitcoin bứt phá" }
            );
            return Task.FromResult(result);
        }

        public Task<BriefAnalysisFromPasteResult> AnalyzeFromFilesAsync(
            List<ChatFileAttachment> files, CancellationToken ct = default)
        {
            var result = new BriefAnalysisFromPasteResult(
                OverviewInput: "Tổng quan từ file: nội dung đã được phân tích từ file đính kèm.",
                ViewpointInput: "Quan điểm: file cung cấp nhiều dữ liệu đầu tư quan trọng.",
                KeyDataInput: "Dữ liệu được trích xuất từ file.",
                Topic: "Chủ đề từ file: Phân tích thị trường",
                MainMessage: "Thông điệp chính từ file.",
                HighlightData: "Dữ liệu nổi bật từ file.",
                SuggestedKeywords: new List<string> { "Phân tích", "Đầu tư", "Thị trường" }
            );
            return Task.FromResult(result);
        }
    }

    public class MockNewsScoringEngine : INewsScoringEngine
    {
        public Task<Dictionary<Guid, NewsScoringResult>> ScoreBatchAsync(
            IReadOnlyList<(Guid NewsItemId, string Title, string SourceName, string SummaryRaw)> items,
            string topicContext,
            CancellationToken ct = default)
        {
            var res = new Dictionary<Guid, NewsScoringResult>();
            foreach (var item in items)
            {
                res[item.NewsItemId] = new NewsScoringResult(
                    RelevanceToTopicScore: 25,
                    ImportanceImpactScore: 18,
                    EmotionPotentialScore: 17,
                    NoveltyDataScore: 12,
                    DataQualityScore: 5,
                    TotalScore: 77,
                    Recommendation: CNewsRecommendation.ShouldSelect,
                    RelevanceLevel: CRelevanceLevel.High,
                    SummaryOverview: $"Tóm tắt: {item.Title}",
                    SuggestedKeywordsForThumbnail: "Bitcoin;Finance;Fed",
                    EmotionTags: CEmotionTag.Anger
                );
            }
            return Task.FromResult(res);
        }
    }

    public class MockNewsDeepAnalysisEngine : INewsDeepAnalysisEngine
    {
        public Task<NewsDeepAnalysisResult> AnalyzeAsync(
            string title, string sourceUrl, string sourceName, string fullArticleText,
            CMarketScope marketScope, CancellationToken ct = default)
        {
            var result = new NewsDeepAnalysisResult(
                MacroEventSummary: MacroEventCategories.Fixed
                    .Select(cat => new MacroEventCategoryItem(cat,
                        cat == MacroEventCategories.MonetaryPolicy ? "Fed hạ lãi suất 50 điểm cơ bản" : "Chưa rõ"))
                    .ToList(),
                MarketReaction: new List<MarketReactionItem>
                {
                    new("Thị trường Bitcoin", "Tăng 20% sau tin Fed hạ lãi suất."),
                    new("Ý kiến nhà đầu tư / Chuyên gia", "Chưa rõ")
                },
                ExpectationShortTerm: "Tác động ngắn hạn (1-3 tháng tới): Giá Bitcoin có thể test lại đỉnh cũ.",
                ExpectationLongTerm: "Tác động dài hạn (6-12 tháng tới): Xu hướng uptrend kéo dài suốt năm 2026.",
                SentimentOverview: new SentimentOverview(CMarketSentiment.Optimistic, "Dòng tiền đầu cơ tích cực sau tin Fed."),
                EmotionTags: CEmotionTag.Hope | CEmotionTag.Surprise,
                EmotionReason: "Tin tức Fed kích hoạt dòng tiền đầu cơ.",
                WasTranslatedFromForeign: marketScope == CMarketScope.International,
                MissingDataNote: null
            );
            return Task.FromResult(result);
        }
    }

    public class MockThumbnailAnalysisEngine : IThumbnailAnalysisEngine
    {
        public Task<ThumbnailDeepAnalysisResult> AnalyzeAsync(
            string thumbnailImageUrl, string videoTitle, CancellationToken ct = default)
        {
            var result = new ThumbnailDeepAnalysisResult(
                ThumbnailFactorsJson: "{\"background\": \"Đỏ đen tương phản\", \"person\": \"KOL ngạc nhiên\"}",
                TitleTextAnalysis: "Sử dụng text 'SẮP SẬP?' màu vàng viền đen.",
                VideoTitleAnalysis: "Tiêu đề giật gân, kích thích tò mò.",
                DisplayTextStyleNote: "Font chữ không chân dày, in hoa, đặt ở 1/3 bên trái."
            );
            return Task.FromResult(result);
        }
    }

    public class MockDisplayTextGenerationEngine : IDisplayTextGenerationEngine
    {
        public Task<DisplayTextGenerationResult> GenerateAsync(
            Dictionary<Guid, string> newsSummaries, string topicContext, CancellationToken ct = default)
        {
            var options = new List<(Guid SourceNewsItemId, string Content)>();
            foreach (var newsId in newsSummaries.Keys)
            {
                options.Add((newsId, "ĐỪNG BỎ LỠ!"));
                options.Add((newsId, "SẬP HAY BỨT PHÁ?"));
            }
            return Task.FromResult(new DisplayTextGenerationResult(options));
        }
    }

    public class MockThumbnailImageGenerationEngine : IThumbnailImageGenerationEngine
    {
        public Task<ThumbnailGenerationResult> GenerateAsync(
            string promptText, string ratio, string resolution, int requestedCount, CancellationToken ct = default)
        {
            var urls = new List<string>();
            for (int i = 0; i < requestedCount; i++)
            {
                urls.Add($"https://mockstorage.kolia.io/generated-thumbnails/thumb-{Guid.NewGuid()}.png");
            }
            return Task.FromResult(new ThumbnailGenerationResult(urls));
        }

        public Task<string> EditAsync(string originalImageUrl, string editRequestText, string? secondaryReferenceImageUrl = null, CancellationToken ct = default)
        {
            return Task.FromResult($"https://mockstorage.kolia.io/generated-thumbnails/edited-{Guid.NewGuid()}.png");
        }
    }

    public class MockVideoTitleGenerationEngine : IVideoTitleGenerationEngine
    {
        public Task<VideoTitleGenerationResult> GenerateAsync(
            string builtPromptText, CTitleStyle style, int requestedCount, CancellationToken ct = default)
        {
            var titles = new List<string>();
            for (int i = 0; i < requestedCount; i++)
            {
                titles.Add($"Tiêu đề gợi ý số {i + 1} theo phong cách {style}");
            }
            return Task.FromResult(new VideoTitleGenerationResult(titles));
        }

        public Task<VideoTitleGenerationResult> GenerateWithFeedbackAsync(
            string builtPromptText, CTitleStyle style, int requestedCount, string feedbackText, CancellationToken ct = default)
        {
            var titles = new List<string>();
            for (int i = 0; i < requestedCount; i++)
            {
                titles.Add($"Tiêu đề cải tiến số {i + 1} dựa trên phản hồi '{feedbackText}'");
            }
            return Task.FromResult(new VideoTitleGenerationResult(titles));
        }

        public Task<string> BuildPromptAsync(
            IEnumerable<string> thumbnailDisplayTexts, IEnumerable<string> newsSummaries, string topicContext, CancellationToken ct = default)
        {
            return Task.FromResult($"Prompt tổng hợp cho chủ đề '{topicContext}'. Chữ trên ảnh: [{string.Join(", ", thumbnailDisplayTexts)}].");
        }
    }

    public class MockRssNewsSourceEngine : IRssNewsSourceEngine
    {
        public Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
            IEnumerable<string> keywords, CMarketScope marketScope, int timeRangeDays, int maxCount, CancellationToken ct = default)
        {
            var list = new List<CrawledNewsItem>
            {
                new CrawledNewsItem(
                    Title: "Fed chính thức hạ lãi suất 0.5%, thị trường tài chính dậy sóng",
                    SourceName: "VnExpress",
                    SourceUrl: "https://vnexpress.net/fed-ha-lai-suat-2026.html",
                    MarketType: marketScope,
                    PublishedTime: DateTimeOffset.UtcNow.AddHours(-3),
                    SummaryRaw: "Cục Dự trữ Liên bang Mỹ (Fed) đã quyết định cắt giảm lãi suất 50 điểm cơ bản..."
                ),
                new CrawledNewsItem(
                    Title: "Dòng vốn chảy mạnh vào các quỹ ETF Crypto sau quyết định của Fed",
                    SourceName: "CoinDesk",
                    SourceUrl: "https://coindesk.com/etf-crypto-flows.html",
                    MarketType: marketScope,
                    PublishedTime: DateTimeOffset.UtcNow.AddHours(-5),
                    SummaryRaw: "Các quỹ ETF Bitcoin và Ethereum ghi nhận mức ròng dương kỷ lục trong tuần qua..."
                )
            };
            return Task.FromResult<IReadOnlyList<CrawledNewsItem>>(list.Take(maxCount).ToList());
        }

        public Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(IEnumerable<string> keywords, CMarketScope marketScope, int timeRangeDays, int maxCount, Action<NewsSourceSearchLog>? onSourceSearched = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CMarketScope> DetectScopeForUrlAsync(string url, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CrawledNewsItem?> FetchSingleAsync(string url, CancellationToken ct = default)
        {
            var item = new CrawledNewsItem(
                Title: "Bài viết nhập thủ công từ: " + url,
                SourceName: "Báo ngoài",
                SourceUrl: url,
                MarketType: CMarketScope.Domestic,
                PublishedTime: DateTimeOffset.UtcNow,
                SummaryRaw: "Tóm tắt nội dung bài viết ngoài..."
            );
            return Task.FromResult<CrawledNewsItem?>(item);
        }
    }

    public class MockContentRelevanceFilterEngine : IContentRelevanceFilterEngine
    {
        private static readonly string[] IrrelevantHints = ["official mv", "lyrics", "quảng cáo", "sponsored", "#shorts meme"];
        public Task<RelevanceFilterResult> ClassifyAsync(string videoTitle, string channelName, CancellationToken ct = default)
        {
            var isIrrelevant = IrrelevantHints.Any(h => videoTitle.Contains(h, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(new RelevanceFilterResult(isIrrelevant, isIrrelevant ? "Khớp mẫu nội dung giải trí/MV/quảng cáo" : null, null));
        }
    }

    public class MockSheetImportEngine : ISheetImportEngine
    {
        public Task<SheetImportResult> FetchAsync(string sheetUrl, CancellationToken ct = default)
            => Task.FromResult(new SheetImportResult(
                "Tổng quan livestream tuần: Giá vàng tăng mạnh...\nQuan điểm chính: Không nên FOMO...",
                DateTimeOffset.UtcNow));
    }

    public class MockYouTubeSearchEngine : IYouTubeSearchEngine
    {
        public Task<IReadOnlyList<YouTubeVideoResult>> SearchAsync(
            string keyword, CThumbnailTimeFilter timeFilter, CThumbnailSortFilter sortFilter, int maxResults, CancellationToken ct = default)
        {
            var list = new List<YouTubeVideoResult>();
            for (int i = 0; i < maxResults; i++)
            {
                list.Add(new YouTubeVideoResult(
                    VideoId: $"vid-{Guid.NewGuid()}",
                    Title: $"Video mẫu về {keyword} số {i + 1}",
                    ChannelName: "Kênh Crypto Finance",
                    ThumbnailImageUrl: $"https://img.youtube.com/vi/mock-{i}/hqdefault.jpg",
                    VideoUrl: $"https://youtube.com/watch?v=mock-{i}",
                    PublishedTime: DateTimeOffset.UtcNow.AddDays(-i),
                    ViewCount: 150000 - (i * 10000)
                ));
            }
            return Task.FromResult<IReadOnlyList<YouTubeVideoResult>>(list);
        }

        public Task<YouTubeVideoResult?> FetchByUrlAsync(string videoUrl, CancellationToken ct = default)
        {
            var video = new YouTubeVideoResult(
                VideoId: "manual-vid",
                Title: "Video YouTube nhập tay: " + videoUrl,
                ChannelName: "Kênh nhập tay",
                ThumbnailImageUrl: "https://img.youtube.com/vi/manual/hqdefault.jpg",
                VideoUrl: videoUrl,
                PublishedTime: DateTimeOffset.UtcNow,
                ViewCount: 50000
            );
            return Task.FromResult<YouTubeVideoResult?>(video);
        }
    }
}
