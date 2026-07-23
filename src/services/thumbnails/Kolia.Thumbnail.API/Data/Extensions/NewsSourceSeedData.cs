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
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:reuters.com&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object InvestingCom = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000006"),
            Name = "Investing.com",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:investing.com/news&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WsjMarkets = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000008"),
            Name = "WSJ Markets",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:wsj.com&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Bloomberg = new
        {
            Id = Guid.Parse("11111111-0002-7000-8000-000000000012"),
            Name = "Bloomberg",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:bloomberg.com&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            RssOrFeedUrl = "https://www.bls.gov/feed/bls_latest.rss",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object BeaNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000003"),
            Name = "BEA (Bureau of Economic Analysis)",
            RssOrFeedUrl = "https://apps.bea.gov/rss/rss.xml",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 32,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.RssDirect,
            Domain = "bea.gov",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WorldGoldCouncil = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000004"),
            Name = "World Gold Council",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:gold.org&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object ImfNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000005"),
            Name = "IMF News",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:imf.org/en/News&hl=en-US&gl=US&ceid=US:en",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 34,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.GoogleNewsFallback,
            Domain = "imf.org",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object WorldBankNews = new
        {
            Id = Guid.Parse("11111111-0003-7000-8000-000000000006"),
            Name = "World Bank News (REST API)",
            RssOrFeedUrl = "https://www.worldbank.org/en/news/all?qterm=&lang_exact=English&format=rss",
            Region = CMarketScope.International,
            IsTrusted = true,
            Priority = 35,
            SourceGroup = CNewsSourceGroup.OfficialData,
            FetchMode = CSourceFetchMode.RestApi,
            Domain = "worldbank.org",
            LastFailedAt = (DateTimeOffset?)null,
            ConsecutiveFailureCount = 0,
            LastEtag = (string?)null,
            LastModifiedHeader = (string?)null,
            LastFetchedAt = (DateTimeOffset?)null,
            ApiEndpoint = "https://search.worldbank.org/api/v2/news",
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = "format=json&rows={maxCount}&displayconttype_exact=Press%20Release&lang_exact=English",
            ApiResponseJsonPath = "documents",
            ApiPaginationType = CApiPaginationType.Page,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object SsiResearch = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000005"),
            Name = "SSI Research",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:ssi.com.vn&hl=vi&gl=VN&ceid=VN:vi",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object MbsResearch = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000006"),
            Name = "MBS Research",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:mbs.com.vn&hl=vi&gl=VN&ceid=VN:vi",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object VnDirect = new
        {
            Id = Guid.Parse("11111111-0004-7000-8000-000000000007"),
            Name = "VNDirect",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:vndirect.com.vn&hl=vi&gl=VN&ceid=VN:vi",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:tradingview.com/news/tradingview&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
            CreationTime = Ts,
            LastModificationTime = (DateTimeOffset?)null,
            IsDeleted = false,
            DeletionTime = (DateTimeOffset?)null
        };

        public static readonly object Kitco = new
        {
            Id = Guid.Parse("11111111-0005-7000-8000-000000000002"),
            Name = "Kitco (Gold)",
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:kitco.com/news&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            RssOrFeedUrl = "https://news.google.com/rss/search?q=site:fidelity.com&hl=en-US&gl=US&ceid=US:en",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
            RssOrFeedUrl = "https://news.google.com/rss/search?q=th%E1%BB%8B+tr%C6%B0%E1%BB%9Dng+ch%E1%BB%A9ng+kho%C3%A1n+Vi%E1%BB%87t+Nam&hl=vi&gl=VN&ceid=VN:vi",
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
            ApiEndpoint = (string?)null,
            ApiKey = (string?)null,
            ApiQueryParamsTemplate = (string?)null,
            ApiResponseJsonPath = (string?)null,
            ApiPaginationType = (CApiPaginationType?)null,
            ApiRequestHeaders = (string?)null,
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
