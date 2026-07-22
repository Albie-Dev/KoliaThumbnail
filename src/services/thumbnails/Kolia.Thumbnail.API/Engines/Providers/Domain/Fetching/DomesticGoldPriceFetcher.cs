using Kolia.Thumbnail.API.Engines.Social;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// [Feature-Flag: EnableGoldPriceFetch]
    /// Best-effort custom scraper for domestic gold prices (SJC/PNJ/DOJI).
    /// These sites do not have RSS or sitemaps — this fetcher creates synthetic
    /// CrawledNewsItem records from publicly accessible price pages.
    ///
    /// IMPORTANT: This is a best-effort, non-blocking component.
    /// If disabled via appsettings (EnableGoldPriceFetch = false), the pipeline
    /// simply skips it. Never throws — failures are logged and swallowed.
    /// </summary>
    public interface IGoldPriceFetcher
    {
        Task<List<CrawledNewsItem>> FetchAsync(CancellationToken ct);
    }

    /// <summary>
    /// Stub implementation — returns empty list.
    /// Replace with actual HTML scraping logic when EnableGoldPriceFetch is enabled.
    /// </summary>
    public sealed class DomesticGoldPriceFetcher : IGoldPriceFetcher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DomesticGoldPriceFetcher> _logger;

        public DomesticGoldPriceFetcher(
            IConfiguration configuration,
            ILogger<DomesticGoldPriceFetcher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<List<CrawledNewsItem>> FetchAsync(CancellationToken ct)
        {
            var enabled = _configuration.GetValue<bool>("Features:EnableGoldPriceFetch");
            if (!enabled)
            {
                return Task.FromResult(new List<CrawledNewsItem>());
            }

            // TODO (Phase 2): Implement actual SJC/PNJ/DOJI HTML scraping.
            // Suggested approach:
            //   1. GET https://sjc.com.vn/GoldPrice/Index.aspx
            //   2. Parse the price table with HtmlAgilityPack or AngleSharp.
            //   3. Create CrawledNewsItem with:
            //      Title = "Giá vàng SJC hôm nay: X triệu/lượng (mua) / Y triệu/lượng (bán)"
            //      SourceName = "SJC"
            //      SourceUrl = "https://sjc.com.vn"
            //      MarketType = CMarketScope.Domestic
            //      PublishedTime = DateTimeOffset.UtcNow (price page has no pubDate)
            //      SummaryRaw = "Cập nhật giá vàng SJC lúc HH:mm ngày dd/MM/yyyy."

            _logger.LogDebug("DomesticGoldPriceFetcher: feature flag disabled — skipping gold price fetch.");
            return Task.FromResult(new List<CrawledNewsItem>());
        }
    }
}
