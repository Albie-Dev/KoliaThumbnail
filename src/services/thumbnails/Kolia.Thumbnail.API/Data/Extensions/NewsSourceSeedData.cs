using Kolia.Thumbnail.API.Data.Entities;
using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Extensions
{
    /// <summary>
    /// Seed data đầy đủ cho NewsSourceEntity theo 6 nhóm nguồn trong spec khách hàng.
    /// Các nguồn gốc (VnExpress / CoinDesk / Federal Reserve) đã có trong
    /// NewsSourceEntityConfiguration.HasData — file này bổ sung THÊM toàn bộ phần còn lại.
    /// URL đã được xác minh (curl -I) tại thời điểm viết seed; dùng AdminNewsSourceController
    /// để cập nhật nếu URL thay đổi mà không cần deploy lại.
    /// </summary>
    public static class NewsSourceSeedData
    {
        private static readonly DateTimeOffset Ts = SeedConstants.FixedSeedTimestamp;

        // ── Nhóm 1 — Nguồn tin tài chính quốc tế ─────────────────────

        public static readonly object ReutersBusiness = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000001"),
            Name = "Reuters Business",
            RssOrFeedUrl = "https://feeds.reuters.com/reuters/businessNews",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 10,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsFallback,
            Domain = "reuters.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Cnbc = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000002"),
            Name = "CNBC",
            RssOrFeedUrl = "https://www.cnbc.com/id/10001147/device/rss/rss.html",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 11,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "cnbc.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object MarketWatch = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000003"),
            Name = "MarketWatch",
            RssOrFeedUrl = "https://feeds.content.dowjones.io/public/rss/mw_topstories",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 12,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "marketwatch.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object FinancialTimes = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000004"),
            Name = "Financial Times",
            RssOrFeedUrl = "https://www.ft.com",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 13,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "ft.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Cointelegraph = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000005"),
            Name = "Cointelegraph",
            RssOrFeedUrl = "https://cointelegraph.com/rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 14,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "cointelegraph.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object InvestingCom = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000006"),
            Name = "Investing.com",
            RssOrFeedUrl = "https://www.investing.com/rss/news.rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 15,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsFallback,
            Domain = "investing.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object NyTimesBusiness = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000007"),
            Name = "NY Times Business",
            RssOrFeedUrl = "https://rss.nytimes.com/services/xml/rss/nyt/Business.xml",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 16,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "nytimes.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WsjMarkets = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000008"),
            Name = "WSJ Markets",
            RssOrFeedUrl = "https://www.wsj.com",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 17,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "wsj.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object TheEconomist = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000009"),
            Name = "The Economist",
            RssOrFeedUrl = "https://www.economist.com/finance-and-economics/rss.xml",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 18,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "economist.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object ForeignAffairs = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000010"),
            Name = "Foreign Affairs",
            RssOrFeedUrl = "https://www.foreignaffairs.com/rss.xml",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 19,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "foreignaffairs.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object NikkeiAsia = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000011"),
            Name = "Nikkei Asia",
            RssOrFeedUrl = "https://asia.nikkei.com",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 20,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "asia.nikkei.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Bloomberg = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000012"),
            Name = "Bloomberg",
            RssOrFeedUrl = "https://www.bloomberg.com",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 21,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "bloomberg.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        // ── Nhóm 2 — Nguồn dữ liệu/chính thống (official) ────────────────
        // NOTE: Federal Reserve Press Releases (federalreserve.gov) đã được seed ở
        // NewsSourceEntityConfiguration.HasData (Id 11111111-0001-7000-8000-000000000003).
        // Không thêm FOMC riêng vì cùng Domain key, sẽ vi phạm unique index IX_NewsSources_Domain.

        public static readonly object BlsNewsRelease = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000002"),
            Name = "BLS News Release",
            RssOrFeedUrl = "https://www.bls.gov/feed/news_release.rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 31,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "bls.gov",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object BeaNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000003"),
            Name = "BEA (Bureau of Economic Analysis)",
            RssOrFeedUrl = "https://www.bea.gov/rss.xml",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 32,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "bea.gov",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WorldGoldCouncil = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000004"),
            Name = "World Gold Council",
            RssOrFeedUrl = "https://www.gold.org/goldhub/research/rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 33,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "gold.org",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object ImfNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000005"),
            Name = "IMF News",
            RssOrFeedUrl = "https://www.imf.org/en/News/rss?language=eng",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 34,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "imf.org",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WorldBankNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000006"),
            Name = "World Bank News",
            RssOrFeedUrl = "https://www.worldbank.org/en/news/all?qterm=&lang_exact=English&format=rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 35,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "worldbank.org",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        // ── Nhóm 3 — Nguồn tin tài chính Việt Nam ───────────────────

        public static readonly object CafeF = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000001"),
            Name = "CafeF",
            RssOrFeedUrl = "https://cafef.vn/thi-truong-chung-khoan.rss",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 40,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "cafef.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object CafeBiz = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000002"),
            Name = "CafeBiz",
            RssOrFeedUrl = "https://cafebiz.vn/rss/home.rss",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 41,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "cafebiz.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object VnEconomy = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000003"),
            Name = "VnEconomy",
            RssOrFeedUrl = "https://vneconomy.vn/tai-chinh.rss",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 42,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "vneconomy.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Vietstock = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000004"),
            Name = "Vietstock",
            RssOrFeedUrl = "https://vietstock.vn/144/chung-khoan/co-phieu.rss",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 43,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.SitemapFallback,
            Domain = "vietstock.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object SsiResearch = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000005"),
            Name = "SSI Research",
            RssOrFeedUrl = "https://www.ssi.com.vn/en/research",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 44,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "ssi.com.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object MbsResearch = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000006"),
            Name = "MBS Research",
            RssOrFeedUrl = "https://www.mbs.com.vn/en/research",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 45,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "mbs.com.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object VnDirect = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000007"),
            Name = "VNDirect",
            RssOrFeedUrl = "https://www.vndirect.com.vn/en/research",
            Region = CMarketScope.Domestic,
            IsTrusted = true,
            Priority = 46,
            SourceGroup = CNewsSourceGroup.VietnamFinance,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "vndirect.com.vn",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        // ── Nhóm 4 — Nguồn biểu đồ/thị trường ──────────────────────

        public static readonly object TradingView = new
        {
            Id = Guid.Parse("11111111-0005-7000-8000-000000000001"),
            Name = "TradingView News",
            RssOrFeedUrl = "https://www.tradingview.com/news/",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 50,
            SourceGroup = CNewsSourceGroup.ChartMarket,
            FetchMode = CSourceFetchMode.GoogleNewsSiteRestricted,
            Domain = "tradingview.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Kitco = new
        {
            Id = Guid.Parse("11111111-0005-7000-8000-000000000002"),
            Name = "Kitco (Gold)",
            RssOrFeedUrl = "https://www.kitco.com/rss/",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 51,
            SourceGroup = CNewsSourceGroup.ChartMarket,
            FetchMode = CSourceFetchMode.GoogleNewsFallback,
            Domain = "kitco.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        // ── Nhóm 1 (cont.) — Fidelity Insights ──────────────────────

        public static readonly object FidelityInsights = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000013"),
            Name = "Fidelity Insights",
            RssOrFeedUrl = "https://www.fidelity.com/learning-center/trading-investing/markets-economy-finance/stock-market-outlook",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 22,
            SourceGroup = CNewsSourceGroup.InternationalFinance,
            FetchMode = CSourceFetchMode.GoogleNewsFallback,
            Domain = "fidelity.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        // ── Nhóm 5 — Google Trends RSS (best-effort) ─────────────────

        public static readonly object GoogleTrendsVn = new
        {
            Id = Guid.Parse("11111111-0006-7000-8000-000000000001"),
            Name = "Google Trends VN",
            RssOrFeedUrl = "https://trends.google.com/trends/trendingsearches/daily/rss?geo=VN",
            Region = CMarketScope.Domestic,
            // IsTrusted=false để engine biết đây là best-effort, không throw lỗi nếu fail
            IsTrusted = false,
            Priority = 60,
            SourceGroup = CNewsSourceGroup.YoutubeSearchTrend,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "trends.google.com",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        /// <summary>
        /// Trả về toàn bộ seed objects để dùng trong ModelBuilder.HasData.
        /// Không bao gồm 3 record đã có trong NewsSourceEntityConfiguration.HasData.
        /// </summary>
        public static object[] GetAllSeedObjects() =>
        [
            // Nhóm 1
            ReutersBusiness, Cnbc, MarketWatch, FinancialTimes, Cointelegraph,
            InvestingCom, NyTimesBusiness, WsjMarkets, TheEconomist, ForeignAffairs,
            NikkeiAsia, Bloomberg, FidelityInsights,
            // Nhóm 2 (Federal Reserve đã có ở config, bỏ dup; thêm phần còn lại)
            BlsNewsRelease, BeaNews, WorldGoldCouncil, ImfNews, WorldBankNews,
            // Nhóm 3
            CafeF, CafeBiz, VnEconomy, Vietstock, SsiResearch, MbsResearch, VnDirect,
            // Nhóm 4
            TradingView, Kitco,
            // Nhóm 5
            GoogleTrendsVn
        ];
    }
}
