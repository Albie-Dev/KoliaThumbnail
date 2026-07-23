# PLAN KỸ THUẬT: Multi-Source Financial News Crawling + Enterprise Anti-429 Fallback
### Dự án: Kolia.Thumbnail.API — Module Bước 1 (Nghiên cứu tổng quan)
### Đối tượng đọc: AI Coding Agent (Claude Code) — thực thi tuần tự, không bỏ bước

---

## 0. TÓM TẮT ROOT CAUSE (đã đọc trực tiếp source code)

Tôi đã đọc: `NewsService.cs`, `SocialExecutorService.cs`, `RealRssNewsSourceEngine.cs`,
`IRssNewsSourceEngine.cs`, `NewsSourceEntity.cs`, `ExternalRequestQueueEntity.cs`,
`ExternalRequestRetryJob.cs`, `DependencyInjection.cs`, `.csproj`. Kết luận:

| # | Bug / Gap | File | Hậu quả |
|---|---|---|---|
| 1 | `RealRssNewsSourceEngine` **hard-code chỉ 6 RSS feed** (VnExpress, Tuổi Trẻ, Thanh Niên, BBC, NYT, Reuters) | `RealRssNewsSourceEngine.cs` dòng 17-37 | Không có CafeF, VnEconomy, Vietstock, SSI, MBS, VNDirect, Bloomberg, CNBC, MarketWatch, FT, WSJ, CoinDesk, Cointelegraph, Investing, TradingView, Kitco, FED/FOMC/BLS/BEA, World Gold Council, IMF, World Bank, Nikkei Asia, Economist, Fidelity, Foreign Affairs → **thiếu 5/6 nhóm nguồn khách hàng yêu cầu** |
| 2 | Bảng `NewsSourceEntity` (whitelist nguồn, có `Priority`, `IsTrusted`, `Region`) **đã tồn tại trong DB nhưng KHÔNG được dùng ở đâu cả** | Toàn bộ codebase | Admin không thể thêm/bớt nguồn qua DB — mọi nguồn đều bị khoá cứng trong code |
| 3 | `AddHttpClient<RealRssNewsSourceEngine>()` **không cấu hình gì** — không User-Agent, không timeout, không Polly retry, không circuit breaker | `DependencyInjection.cs` dòng ~120 | Hầu hết site tài chính quốc tế (Bloomberg, WSJ, FT, Reuters, Nikkei Asia...) chặn request có User-Agent mặc định `.NET` → trả 403/429 ngay lập tức, không có cơ chế thử lại thông minh |
| 4 | Không có **per-domain rate limiter / concurrency control** — vòng lặp `foreach` gọi tuần tự từng feed, nếu 1 feed treo (timeout) sẽ làm chậm toàn bộ | `RealRssNewsSourceEngine.CrawlAsync` | Research bị chậm, dễ bị site đánh dấu là bot do gọi dồn dập |
| 5 | Cơ chế "key rotation + cooldown" **chỉ áp dụng cho YouTube** (`SocialMediaProviderConfiguration` với `LastRateLimitedAt`) — RSS **không có cooldown theo domain**, khi bị 429 chỉ log warning và bỏ qua feed đó, lần sau vẫn gọi lại ngay | `SocialExecutorService.RssCrawlAsync` | Site tiếp tục bị request dồn dập → càng dễ bị 429/ban IP vĩnh viễn |
| 6 | `NewsService.SearchAsync` **comment out toàn bộ đoạn gọi `_scoringEngine.ScoreBatchAsync`** (dòng 115-141) | `NewsService.cs` | Tin crawl về **không được chấm điểm** (`RelevanceToTopicScore`, `Recommendation`, `EmotionTags`...) → bảng output "Đề xuất chọn" luôn rỗng, không đúng spec khách hàng |
| 7 | Google News RSS fallback chỉ kích hoạt khi **toàn bộ** `allItems.Count == 0` (fallback toàn cục), không fallback **theo từng nhóm nguồn** khi 1 nhóm bị lỗi | `RealRssNewsSourceEngine.CrawlAsync` dòng 84 | Nếu nhóm "quốc tế" die nhưng nhóm "nội địa" có vài tin → coi như quốc tế bị bỏ trắng, không tự bù |
| 8 | Không có cache (ETag/Last-Modified/response cache) → mỗi lần research gọi lại y hệt cùng feed dù vừa fetch cách đó vài phút | Toàn bộ | Tăng số request không cần thiết → tăng nguy cơ 429 |
| 9 | Nhiều nguồn khách hàng liệt kê (Bloomberg, WSJ, FT, FOMC, BLS, BEA, World Gold Council, IMF, World Bank, Kitco, TradingView, Investing, Google Trends) **không có RSS feed công khai ổn định** — engine hiện tại chỉ biết xử lý RSS/Atom, không có nhánh xử lý HTML/sitemap/API cho các nguồn này | `IRssNewsSourceEngine` chỉ có `CrawlAsync` + `FetchSingleAsync` | Không thể "crawl đầy đủ mọi trang" như yêu cầu nếu không mở rộng kiến trúc nguồn |
| 10 | `.csproj` **chưa có package Polly** (`Polly`, `Microsoft.Extensions.Http.Resilience`) | `.csproj` | Không thể enterprise-grade retry/circuit-breaker nếu không thêm package |

---

## 1. KIẾN TRÚC MỚI — TỔNG QUAN

```
NewsService.SearchAsync
        │
        ▼
ISocialExecutorService.RssCrawlAsync (giữ nguyên vai trò "cổng duy nhất")
        │
        ▼
IRssNewsSourceEngine.CrawlAsync   ← RENAME LOGIC THÀNH: INewsSourceOrchestrator (xem mục 3)
        │
        ├──► NewsSourceRegistry (đọc từ DB NewsSourceEntity, KHÔNG hard-code)
        │
        ├──► SourceFetchPipeline (áp dụng cho từng NewsSourceEntity)
        │        │
        │        ├─ Tier 1: RSS/Atom feed trực tiếp (feed chính chủ)
        │        ├─ Tier 2: Google News RSS theo domain (site:xxx.com + keyword)
        │        ├─ Tier 3: Sitemap.xml / news-sitemap (cho nguồn không có RSS)
        │        └─ Tier 4: Cached snapshot gần nhất trong DB (degrade gracefully, không 500)
        │
        ├──► DomainRateLimiterRegistry (per-domain token bucket + circuit breaker)
        │
        ├──► ResilientHttpClient (Polly: retry + exponential backoff + jitter + circuit breaker)
        │
        └──► ResponseCacheStore (ETag/Last-Modified + TTL theo domain)
        │
        ▼
NewsItemEntity (lưu DB) → AiNewsScoringEngine.ScoreBatchAsync (BẬT LẠI, KHÔNG COMMENT NỮA)
        │
        ▼
Output bảng tin tổng hợp (đúng spec khách hàng: Tick, Đề xuất chọn, Nhóm, Tóm tắt, Mức độ liên quan, Cảm xúc, Keyword)
```

Nguyên tắc cốt lõi để **KHÔNG BAO GIỜ 429**:
1. **Không bao giờ gọi trực tiếp domain đã biết đang cooldown.**
2. **Luôn có fallback tier kế tiếp thay vì throw lỗi lên trên.**
3. **Per-domain concurrency = 1-2, có delay + jitter giữa các request tới cùng 1 domain.**
4. **Circuit breaker mở khi 3 lần lỗi liên tiếp → domain bị "nghỉ" N phút, tự động dùng cache/tier khác trong lúc đó.**
5. **Không throw exception ra ngoài NewsService nếu ít nhất 1 tier trả được dữ liệu (kể cả cache cũ)** — chỉ throw khi tất cả tier + cache đều rỗng.

---

## 2. DANH SÁCH NGUỒN ĐẦY ĐỦ THEO 6 NHÓM (Seed data cho `NewsSourceEntity`)

> Ghi chú bắt buộc: cột "FetchMode" quyết định tier nào được dùng đầu tiên. AI Agent phải seed đúng
> `RssOrFeedUrl`, và với các nguồn không có RSS ổn định, `RssOrFeedUrl` lưu URL trang chủ/section
> để dùng làm base cho Tier 2 (Google News site-restricted) hoặc Tier 3 (sitemap).

### Nhóm 1 — Nguồn tin tài chính quốc tế
| Name | RssOrFeedUrl (Tier 1 nếu có) | FetchMode | Region |
|---|---|---|---|
| Reuters Business | `https://feeds.reuters.com/reuters/businessNews` | RSS→GoogleNewsFallback | International |
| CNBC | `https://www.cnbc.com/id/10001147/device/rss/rss.html` | RSS | International |
| MarketWatch | `https://feeds.content.dowjones.io/public/rss/mw_topstories` | RSS | International |
| Financial Times | (không RSS công khai ổn định) `https://www.ft.com` | GoogleNewsSiteRestricted | International |
| CoinDesk | `https://www.coindesk.com/arc/outboundfeeds/rss/` | RSS | International |
| Cointelegraph | `https://cointelegraph.com/rss` | RSS | International |
| Investing.com | `https://www.investing.com/rss/news.rss` | RSS→GoogleNewsFallback | International |
| TradingView News | `https://www.tradingview.com/news/` | SitemapOrGoogleNews | International |
| NY Times Business | `https://rss.nytimes.com/services/xml/rss/nyt/Business.xml` | RSS | International |
| WSJ Markets | (paywall, không RSS công khai ổn định) | GoogleNewsSiteRestricted | International |
| The Economist | `https://www.economist.com/finance-and-economics/rss.xml` | RSS | International |
| Fidelity Insights | `https://www.fidelity.com/learning-center/rss/investing.xml` (verify) | RSS→GoogleNewsFallback | International |
| Foreign Affairs | `https://www.foreignaffairs.com/rss.xml` | RSS | International |
| Nikkei Asia | (paywall) | GoogleNewsSiteRestricted | International |
| Bloomberg | (không RSS công khai) | GoogleNewsSiteRestricted | International |

### Nhóm 2 — Nguồn dữ liệu/chính thống (official)
| Name | RssOrFeedUrl | FetchMode | Region |
|---|---|---|---|
| Federal Reserve Press Releases | `https://www.federalreserve.gov/feeds/press_all.xml` | RSS | International |
| FOMC Calendar/Statements | `https://www.federalreserve.gov/feeds/fomc_press.xml` | RSS | International |
| BLS (Bureau of Labor Statistics) | `https://www.bls.gov/feed/news_release.rss` | RSS | International |
| BEA (Bureau of Economic Analysis) | `https://www.bea.gov/rss.xml` (verify) | RSS→SitemapFallback | International |
| World Gold Council | `https://www.gold.org/rss.xml` (verify khi implement — WGC hay đổi cấu trúc) | RSS→SitemapFallback | International |
| IMF News | `https://www.imf.org/en/News/rss?language=eng` | RSS | International |
| World Bank News | `https://www.worldbank.org/en/news/all?qterm=&lang_exact=English&format=rss` | RSS | International |

### Nhóm 3 — Nguồn tin tài chính Việt Nam
| Name | RssOrFeedUrl | FetchMode | Region |
|---|---|---|---|
| CafeF | `https://cafef.vn/thi-truong-chung-khoan.rss` | RSS | Domestic |
| CafeBiz | `https://cafebiz.vn/rss/home.rss` | RSS | Domestic |
| VnExpress Kinh doanh | `https://vnexpress.net/rss/kinh-doanh.rss` | RSS | Domestic |
| VnEconomy | `https://vneconomy.vn/tai-chinh.rss` | RSS | Domestic |
| Vietstock | `https://vietstock.vn/144/chung-khoan/co-phieu.rss` (verify path) | RSS→SitemapFallback | Domestic |
| SSI Research | (thường không RSS) | SitemapOrGoogleNews | Domestic |
| MBS Research | (thường không RSS) | SitemapOrGoogleNews | Domestic |
| VNDirect | (thường không RSS) | SitemapOrGoogleNews | Domestic |

### Nhóm 4 — Nguồn biểu đồ/thị trường
| Name | RssOrFeedUrl | FetchMode | Region |
|---|---|---|---|
| TradingView | như Nhóm 1 | SitemapOrGoogleNews | International |
| Investing.com | như Nhóm 1 | RSS | International |
| Kitco (giá vàng) | `https://www.kitco.com/rss/` (verify) | RSS→SitemapFallback | International |
| Giá vàng trong nước (SJC/PNJ/DOJI) | Không có RSS — cần **API/HTML scraping riêng**, KHÔNG dùng chung pipeline RSS | Custom scraper (xem mục 3.5) | Domestic |

### Nhóm 5 — YouTube/Search trend
| Name | Cơ chế | Ghi chú |
|---|---|---|
| YouTube Search | Đã có `IYouTubeSearchEngine` / `YoutubeEngine` qua `SocialExecutorService.YouTubeSearchAsync` | **Tái sử dụng nguyên bản, KHÔNG viết lại** — team xác nhận đã có YouTube Engine hoạt động |
| Google Trends | Không có API chính thức miễn phí ổn định → dùng `Google Trends RSS` (`https://trends.google.com/trends/trendingsearches/daily/rss?geo=VN`) làm best-effort, đánh dấu `IsTrusted=false`, không throw lỗi nếu fail |

> **Lưu ý quan trọng cho AI Agent:** một số URL RSS ở trên (đánh dấu "verify") **phải được xác minh còn hoạt động tại thời điểm code** bằng cách gọi thử `curl -I <url>` trước khi seed vào migration, vì các trang tài chính hay đổi path RSS. Không được seed URL "đoán" mà không test.

---

## 3. THAY ĐỔI CODE CHI TIẾT (theo đúng file/class hiện có)

### 3.1. Thêm NuGet packages (`Kolia.Thumbnail.API.csproj`)
```xml
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.*" />
<PackageReference Include="Polly" Version="8.*" />
```

### 3.2. `NewsSourceEntity` — bổ sung field còn thiếu
File: `Data/Entities/News/NewsSourceEntity.cs`

Thêm các field bắt buộc cho cơ chế fallback + rate-limit:
```csharp
/// <summary>Nhóm nguồn theo spec khách hàng (1-6, xem enum CNewsSourceGroup mới)</summary>
public CNewsSourceGroup SourceGroup { get; set; }

/// <summary>Cách fetch: RssDirect, GoogleNewsFallback, SitemapFallback, Custom</summary>
public CSourceFetchMode FetchMode { get; set; }

/// <summary>Domain gốc (vd "cafef.vn") — dùng để tra DomainRateLimiterRegistry</summary>
public string Domain { get; set; } = null!;

/// <summary>Thời điểm domain này bị lỗi/429 gần nhất</summary>
public DateTimeOffset? LastFailedAt { get; set; }

/// <summary>Số lỗi liên tiếp — dùng cho circuit breaker cấp DB (bổ trợ cho in-memory)</summary>
public int ConsecutiveFailureCount { get; set; } = 0;

/// <summary>ETag / Last-Modified của lần fetch gần nhất — dùng conditional GET</summary>
public string? LastEtag { get; set; }
public string? LastModifiedHeader { get; set; }

/// <summary>Thời điểm fetch thành công gần nhất</summary>
public DateTimeOffset? LastFetchedAt { get; set; }
```
→ Tạo EF Core migration mới: `AddNewsSourceFallbackFields`.

Thêm 2 enum mới: `Enums/CNewsSourceGroup.cs` (InternationalFinance=1, OfficialData=2, VietnamFinance=3,
ChartMarket=4, YoutubeSearchTrend=5) và `Enums/CSourceFetchMode.cs`
(RssDirect=1, GoogleNewsFallback=2, SitemapFallback=3, Custom=4).

### 3.3. Seed dữ liệu nguồn (KHÔNG hard-code trong Engine nữa)
File mới: `Data/Extensions/NewsSourceSeedData.cs` — chứa toàn bộ danh sách ở Mục 2, seed qua
`ModelBuilderExtensions.cs` (file đã tồn tại, có seed pattern sẵn — theo đúng convention
`SeedConstants.cs` đang dùng cho dữ liệu khác trong dự án).

**Không xoá dữ liệu seed cũ (VnExpress/BBC/NYT/Reuters) — chỉ bổ sung thêm**, để tránh phá vỡ dữ liệu
đang chạy trong môi trường hiện tại.

### 3.4. `IRssNewsSourceEngine` → giữ interface, đổi implementation bên trong
Interface **giữ nguyên chữ ký** (`CrawlAsync`, `FetchSingleAsync`) để không phải sửa
`ISocialExecutorService`/`NewsService` — chỉ thay nội dung `RealRssNewsSourceEngine`.

**Cấu trúc lớp mới (thêm, không xoá lớp cũ hoàn toàn — refactor tại chỗ):**

```
Engines/Providers/Domain/
├── RealRssNewsSourceEngine.cs          (giữ tên, sửa nội dung để orchestrate)
├── Fetching/
│   ├── INewsSourceOrchestrator.cs      (interface mới, RealRssNewsSourceEngine implement + delegate)
│   ├── SourceFetchPipeline.cs          (logic 4-tier fallback cho 1 NewsSourceEntity)
│   ├── DomainRateLimiterRegistry.cs    (in-memory, per-domain SemaphoreSlim + cooldown timestamp)
│   ├── CircuitBreakerRegistry.cs       (per-domain: Closed/Open/HalfOpen, ngưỡng 3 lỗi liên tiếp)
│   ├── GoogleNewsFallbackFetcher.cs    (site-restricted Google News RSS, dùng chung)
│   ├── SitemapFallbackFetcher.cs       (parse sitemap.xml / news-sitemap.xml khi không có RSS)
│   └── ResponseCacheStore.cs           (IMemoryCache wrapper, key theo domain+keyword, TTL 10-15 phút)
```

**Luồng `CrawlAsync` mới (thay thế logic dòng 45-107 hiện tại):**

```csharp
public async Task<IReadOnlyList<CrawledNewsItem>> CrawlAsync(
    IEnumerable<string> keywords, CMarketScope marketScope,
    int timeRangeDays, int maxCount, CancellationToken ct = default)
{
    var keywordList = keywords.ToList();
    if (keywordList.Count == 0) return [];

    // 1. Lấy danh sách nguồn từ DB (KHÔNG hard-code), lọc theo Region + IsTrusted + Priority
    var sources = await _sourceRegistry.GetActiveSourcesAsync(marketScope, ct);

    var cutoffTime = DateTimeOffset.UtcNow.AddDays(-timeRangeDays);
    var results = new ConcurrentBag<CrawledNewsItem>();

    // 2. Fetch song song NHƯNG giới hạn concurrency toàn cục (vd 6) +
    //    concurrency per-domain = 1 (qua DomainRateLimiterRegistry)
    var globalThrottle = new SemaphoreSlim(6);

    await Parallel.ForEachAsync(sources, new ParallelOptions
    {
        MaxDegreeOfParallelism = 6,
        CancellationToken = ct
    },
    async (source, token) =>
    {
        // Circuit breaker: nếu domain đang Open → bỏ qua thẳng, dùng cache
        if (_circuitBreaker.IsOpen(source.Domain))
        {
            var cached = await _cache.TryGetAsync(source.Domain, keywordList, token);
            if (cached is { Count: > 0 }) foreach (var c in cached) results.Add(c);
            return;
        }

        var items = await _pipeline.FetchWithFallbackAsync(
            source, keywordList, cutoffTime, maxCount, token);

        if (items.Count > 0)
        {
            _circuitBreaker.RecordSuccess(source.Domain);
            await _cache.SetAsync(source.Domain, keywordList, items, token);
            foreach (var i in items) results.Add(i);
        }
        else
        {
            _circuitBreaker.RecordFailure(source.Domain); // chỉ tăng khi TIER CUỐI cũng fail
        }
    });

    // 3. KHÔNG throw nếu có ít nhất 1 kết quả — kể cả từ cache
    return results
        .DistinctBy(i => i.SourceUrl)
        .OrderByDescending(i => i.PublishedTime)
        .Take(maxCount)
        .ToList();
}
```

**`SourceFetchPipeline.FetchWithFallbackAsync` — 4-tier logic:**
```csharp
public async Task<List<CrawledNewsItem>> FetchWithFallbackAsync(
    NewsSourceEntity source, List<string> keywords, DateTimeOffset cutoff,
    int maxCount, CancellationToken ct)
{
    // Rate limiter: chờ slot cho domain này (per-domain token bucket, vd 1 request / 3-5 giây)
    using var _ = await _rateLimiter.AcquireAsync(source.Domain, ct);

    // TIER 1: RSS trực tiếp (nếu FetchMode cho phép)
    if (source.FetchMode is CSourceFetchMode.RssDirect or CSourceFetchMode.GoogleNewsFallback)
    {
        var tier1 = await TryFetchRssAsync(source, keywords, cutoff, maxCount, ct);
        if (tier1.Count > 0) { MarkFetchedOk(source); return tier1; }
    }

    // TIER 2: Google News RSS site-restricted (site:domain + keyword)
    if (source.FetchMode is CSourceFetchMode.GoogleNewsFallback or CSourceFetchMode.SitemapFallback)
    {
        var tier2 = await _googleNewsFetcher.FetchAsync(source.Domain, keywords, cutoff, maxCount, ct);
        if (tier2.Count > 0) { MarkFetchedOk(source); return tier2; }
    }

    // TIER 3: Sitemap.xml / news-sitemap.xml
    if (source.FetchMode is CSourceFetchMode.SitemapFallback or CSourceFetchMode.Custom)
    {
        var tier3 = await _sitemapFetcher.FetchAsync(source, keywords, cutoff, maxCount, ct);
        if (tier3.Count > 0) { MarkFetchedOk(source); return tier3; }
    }

    // TIER 4: cache DB gần nhất (đã fetch ở lần research trước, kể cả quá TTL)
    var tier4 = await _cache.TryGetStaleAsync(source.Domain, ct);
    MarkFetchFailed(source);
    return tier4 ?? [];
}
```

Mỗi lần gọi HTTP thật (`TryFetchRssAsync`, `GoogleNewsFallbackFetcher`, `SitemapFallbackFetcher`)
đều đi qua **`ResilientHttpClient`** (mục 3.6) — không gọi `_httpClient.GetAsync` trần như code cũ.

### 3.5. Custom scraper cho giá vàng trong nước (Nhóm 4)
Vì SJC/PNJ/DOJI không có RSS/sitemap chuẩn, tạo riêng:
`Engines/Providers/Domain/Fetching/DomesticGoldPriceFetcher.cs` — implement interface riêng
`IGoldPriceFetcher`, output là `CrawledNewsItem` giả lập (title = "Giá vàng SJC hôm nay: X triệu/lượng").
Đây là **best-effort, không bắt buộc phải xong ở bản đầu** — đánh dấu `[Feature-Flag: EnableGoldPriceFetch]`
trong config để bật/tắt độc lập, không chặn release.

### 3.6. `ResilientHttpClient` — cấu hình Polly (SỬA `DependencyInjection.cs`)

```csharp
services.AddHttpClient<RealRssNewsSourceEngine>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(12);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/124.0.0.0 Safari/537.36 KoliaNewsBot/1.0 (+https://kolia.example/bot)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml, */*");
})
.AddResilienceHandler("rss-fetch-pipeline", builder =>
{
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        Delay = TimeSpan.FromSeconds(1),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests
            || args.Outcome.Result?.StatusCode is >= HttpStatusCode.InternalServerError
            || args.Outcome.Exception is HttpRequestException or TaskCanceledException)
    });
    builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(60),
        MinimumThroughput = 4,
        BreakDuration = TimeSpan.FromMinutes(5)
    });
    builder.AddTimeout(TimeSpan.FromSeconds(12));
});
```

**Xử lý riêng `429 Too Many Requests` — đọc header `Retry-After`:**
Trong `DomainRateLimiterRegistry`, khi nhận response 429, đọc `response.Headers.RetryAfter`
và set cooldown domain đó = giá trị header (nếu có) hoặc mặc định 5 phút — **không phải retry
ngay theo Polly exponential** vì 429 đã có chỉ dẫn thời gian rõ ràng từ server, tôn trọng nó là
best practice enterprise (tránh bị site liệt vào blacklist IP vĩnh viễn).

### 3.7. Sửa `NewsService.SearchAsync` — BẬT LẠI đoạn scoring bị comment
File: `Services/News/NewsService.cs` dòng 115-141

**Bỏ comment**, sửa lại cho đúng — vấn đề hiện tại của đoạn comment là dùng
`Guid.CreateVersion7` tạo ID mới **không gán vào `item.Id`** (biến `scoringBatch` ở dòng 91-93
bị orphan, không liên kết gì với `newsItems`). Sửa đúng như sau:

```csharp
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
```
Đồng thời **xoá biến `scoringBatch` orphan ở dòng 91-93** — nó không được dùng ở đâu, chỉ gây rối.

Kiểm tra thêm: `AiNewsScoringEngine.ScoreBatchAsync` gọi qua `AIExecutorService` (Gemini/Groq) —
**AI provider này cũng phải qua cùng nguyên tắc chống 429** (đã có sẵn cơ chế
`AIProviderConfiguration` multi-key + cooldown, xem `AIExecutorService.cs` — giữ nguyên, không cần
sửa, chỉ xác nhận hoạt động đúng khi test end-to-end).

### 3.8. `SocialExecutorService.RssCrawlAsync` — nới điều kiện enqueue retry
Hiện tại (dòng 96-106): chỉ enqueue + throw khi `HttpRequestException`/`TaskCanceledException`.
Với kiến trúc mới, **tầng dưới (`RealRssNewsSourceEngine`) hầu như không còn throw nữa** (luôn có
fallback + cache), nên `SocialExecutorService` chỉ throw `ExternalServiceException` khi
**pipeline trả về danh sách rỗng VÀ không có cache nào** — đây là tín hiệu thật sự nghiêm trọng
(toàn bộ nguồn + cache đều chết), lúc đó mới đúng là enqueue vào `ExternalRequestQueueEntity` để
`ExternalRequestRetryJob` xử lý lại sau.

```csharp
public async Task<IReadOnlyList<CrawledNewsItem>> RssCrawlAsync(
    IEnumerable<string> keywords, CMarketScope marketScope,
    int timeRangeDays, int maxCount, Guid? projectId, CancellationToken ct = default)
{
    var keywordList = keywords.ToList();
    try
    {
        var items = await _rssEngine.CrawlAsync(keywordList, marketScope, timeRangeDays, maxCount, ct);
        if (items.Count == 0)
        {
            _logger.LogWarning("RssCrawlAsync: 0 kết quả sau toàn bộ fallback cho keywords={Keywords}",
                string.Join(",", keywordList));
            await EnqueueRetryAsync(CExternalRequestPurpose.NewsCrawl,
                string.Join(",", keywordList), null, null, maxCount, projectId, ct);
            // KHÔNG throw — trả về rỗng để NewsService vẫn tạo được NewsSearchRequestEntity,
            // team thấy "0 tin" thay vì lỗi 500 khó hiểu. Retry job sẽ tự chạy lại sau.
        }
        return items;
    }
    catch (Exception ex) // safety net — về lý thuyết không nên xảy ra vì pipeline tự bắt hết
    {
        _logger.LogError(ex, "RssCrawlAsync: lỗi không mong đợi ngoài pipeline fallback");
        await EnqueueRetryAsync(CExternalRequestPurpose.NewsCrawl,
            string.Join(",", keywordList), null, null, maxCount, projectId, ct);
        throw new ExternalServiceException("RSS crawl thất bại toàn bộ — yêu cầu đã được ghi vào hàng đợi.", ex);
    }
}
```

> **Quyết định thiết kế quan trọng:** không để 1 nguồn lỗi làm hỏng cả research request (trả 500
> cho user). Thà trả về ít tin hơn còn hơn chặn cả luồng — đúng tinh thần "enterprise, không bao
> giờ down vì 1 nguồn ngoài".

### 3.9. `ExternalRequestRetryJob` — không cần sửa nhiều
Payload retry hiện tại chỉ lưu `Keyword/TimeFilter/SortFilter/MaxResults/TimeRangeDays` nhưng
**không lưu `MarketScope`** (dòng retry NewsCrawl hard-code `CMarketScope.Domestic` — đây cũng là
1 bug nhỏ). Sửa `RetryPayload` thêm field `MarketScope`, và khi enqueue trong
`SocialExecutorService.EnqueueRetryAsync` phải serialize đúng `marketScope` truyền vào thay vì bỏ qua.

### 3.10. Quản lý nguồn tin HOÀN TOÀN DYNAMIC qua Admin CRUD (không hard-code, không cần deploy khi site đổi)

Mục 3.2-3.3 đã đưa `NewsSourceEntity` từ DB thay vì hard-code trong Engine — nhưng nếu chỉ seed 1 lần
rồi để im thì mỗi khi 1 site đổi path RSS, đội vẫn phải nhờ dev sửa migration/seed data rồi deploy lại,
không khác gì hard-code. Để thực sự linh hoạt, bổ sung 1 lớp quản trị đầy đủ:

**a. `Controllers/Admins/AdminNewsSourceController.cs`** (theo đúng convention đang có của
`AdminAIProviderController.cs`, `AdminGoogleServiceAccountController.cs`):

| Method | Route | Mục đích |
|---|---|---|
| `GET` | `/admin/news-sources` | List toàn bộ nguồn, filter theo `SourceGroup`/`Region`/`IsTrusted`, kèm status vận hành (`ConsecutiveFailureCount`, `LastFetchedAt`, `LastFailedAt`) |
| `GET` | `/admin/news-sources/{id}` | Chi tiết 1 nguồn |
| `POST` | `/admin/news-sources` | Thêm nguồn mới (khi khách hàng yêu cầu bổ sung nguồn ngoài 6 nhóm ban đầu) |
| `PUT` | `/admin/news-sources/{id}` | Sửa `RssOrFeedUrl`/`FetchMode`/`Priority`/`Domain` khi site đổi cấu trúc — **đây là thao tác chính giải quyết đúng vấn đề bạn nêu** |
| `PATCH` | `/admin/news-sources/{id}/toggle` | Bật/tắt nhanh `IsTrusted` cho 1 nguồn đang lỗi liên tục, không cần chờ sửa xong URL mới |
| `POST` | `/admin/news-sources/{id}/test` | Test thử fetch ngay (gọi `SourceFetchPipeline` 1 lần, trả về preview kết quả + tier nào đã dùng) để admin biết sửa đúng chưa **trước khi** để hệ thống dùng thật |
| `DELETE` | `/admin/news-sources/{id}` | Soft delete (dùng field `IsDeleted` có sẵn từ `BaseEntity`/`ISoftDelete`, đúng convention project) |

**b. `Services/News/IAdminNewsSourceService.cs` + `AdminNewsSourceService.cs`** — theo đúng pattern
`AIProviderConfigurationService.cs` đã có (CRUD + validate). Khi `Create`/`Update`, **bắt buộc gọi thử
`HEAD`/`GET` request tới URL mới qua `ResilientHttpClient`** trước khi lưu — nếu trả lỗi/404 thì
trả về `ValidationException` cho admin biết ngay, tránh nhập URL sai mà không hay biết cho tới khi
research chạy thật mới phát hiện.

**c. `DTOs/News/NewsSourceAdminDtos.cs`** — `NewsSourceCreateDto`, `NewsSourceUpdateDto`,
`NewsSourceListItemDto` (có thêm field hiển thị "trạng thái vận hành" tính từ `ConsecutiveFailureCount`
+ `LastFailedAt` để admin nhìn là biết nguồn nào đang có vấn đề) + `NewsSourceMapper.cs`.

**d. `Validators/News/NewsSourceValidators.cs`** — FluentValidation, validate `RssOrFeedUrl` là URL
hợp lệ, `Domain` khớp với domain trong URL (tránh admin gõ nhầm domain dùng cho rate-limiter),
`Priority >= 0`.

**e. Cache invalidation khi admin sửa nguồn** — `NewsSourceRegistry` (đọc danh sách active sources từ
DB) phải cache trong `IMemoryCache` với TTL ngắn (khuyến nghị 5 phút) để tránh mỗi request research
đều query DB. Khi `AdminNewsSourceService.Update/Create/Delete/Toggle` thành công, **phải invalidate
cache key ngay lập tức** (`IMemoryCache.Remove(...)`) — nếu không, admin sửa xong URL nhưng Engine vẫn
dùng bản cache cũ thêm vài phút, gây hiểu lầm "sửa mà không có tác dụng".

**f. `BackgroundJobs/NewsSourceHealthCheckJob.cs`** — biến việc phát hiện "nguồn chết" từ **bị động**
(chờ research fail nhiều lần rồi mới có ai đó để ý) sang **chủ động**:
- Chạy định kỳ mỗi 6 giờ (dùng `Cronos` — package đã có sẵn trong project cho lịch trình khác).
- Ping thử từng `NewsSourceEntity.RssOrFeedUrl` qua `ResilientHttpClient`.
- Nếu lỗi/404 liên tục ≥ 3 lần (dùng chung field `ConsecutiveFailureCount` đã thêm ở mục 3.2) →
  tự động set `IsTrusted = false` + ghi log cảnh báo mức `Warning` có tên miền cụ thể, để admin vào
  `GET /admin/news-sources?isTrusted=false` là thấy ngay danh sách cần sửa, không phải đoán.
- Đây là job **có thể làm ở Phase 2** nếu team muốn ship phần crawl chính trước — không bắt buộc phải
  xong cùng lúc với Mục 3.1-3.9, nhưng bắt buộc phải có trong roadmap vì đây chính là cơ chế giữ cho
  bảng nguồn "dynamic" luôn đúng theo thời gian thay vì chỉ đúng tại thời điểm seed ban đầu.

> **Tóm lại trả lời câu hỏi của khách hàng:** đúng, chuyển hẳn sang dynamic table + admin CRUD là hướng
> đúng và cần thiết — DB table không có CRUD/cache-invalidation đi kèm thì chỉ là "hard-code có thêm
> 1 lớp seed", không giải quyết được vấn đề gốc là "lỡ sau này dịch vụ đổi". Phần a-e ở trên là bắt
> buộc cho bản đầu; phần f (health-check job) có thể để Phase 2 nhưng phải nằm trong roadmap ngay từ
> đầu để không bị quên.

---

## 4. DANH SÁCH FILE CẦN TẠO MỚI / SỬA (checklist cho AI Agent)

**Tạo mới:**
- [ ] `Enums/CNewsSourceGroup.cs`
- [ ] `Enums/CSourceFetchMode.cs`
- [ ] `Engines/Providers/Domain/Fetching/INewsSourceOrchestrator.cs`
- [ ] `Engines/Providers/Domain/Fetching/SourceFetchPipeline.cs`
- [ ] `Engines/Providers/Domain/Fetching/DomainRateLimiterRegistry.cs`
- [ ] `Engines/Providers/Domain/Fetching/CircuitBreakerRegistry.cs`
- [ ] `Engines/Providers/Domain/Fetching/GoogleNewsFallbackFetcher.cs`
- [ ] `Engines/Providers/Domain/Fetching/SitemapFallbackFetcher.cs`
- [ ] `Engines/Providers/Domain/Fetching/ResponseCacheStore.cs`
- [ ] `Engines/Providers/Domain/Fetching/DomesticGoldPriceFetcher.cs` (feature-flag, optional bản đầu)
- [ ] `Data/Extensions/NewsSourceSeedData.cs`
- [ ] `Migrations/xxxx_AddNewsSourceFallbackFields.cs` (EF Core `dotnet ef migrations add`)
- [ ] `Controllers/Admins/AdminNewsSourceController.cs` (mục 3.10.a — CRUD + toggle + test-fetch)
- [ ] `Services/News/IAdminNewsSourceService.cs` + `AdminNewsSourceService.cs` (mục 3.10.b)
- [ ] `DTOs/News/NewsSourceAdminDtos.cs` + `NewsSourceMapper.cs` (mục 3.10.c)
- [ ] `Validators/News/NewsSourceValidators.cs` (mục 3.10.d)
- [ ] `Data/Configurations/News/NewsSourceEntityConfiguration.cs` (nếu chưa có sẵn — map field mới + unique index trên `Domain`)
- [ ] `BackgroundJobs/NewsSourceHealthCheckJob.cs` (mục 3.10.f — có thể để Phase 2, vẫn phải tạo interface/khung sườn ngay)

**Sửa:**
- [ ] `Kolia.Thumbnail.API.csproj` — thêm Polly + Http.Resilience
- [ ] `Data/Entities/News/NewsSourceEntity.cs` — thêm field mục 3.2
- [ ] `Data/Configurations/News/NewsSourceEntityConfiguration.cs` — map field mới (kiểm tra file này tồn tại chưa, nếu chưa thì tạo)
- [ ] `Engines/Providers/Domain/RealRssNewsSourceEngine.cs` — refactor orchestrate qua pipeline mới
- [ ] `Engines/Social/SocialExecutorService.cs` — sửa `RssCrawlAsync` theo mục 3.8
- [ ] `Services/News/NewsService.cs` — bỏ comment + fix scoring theo mục 3.7
- [ ] `DependencyInjection.cs` — đăng ký toàn bộ class mới (kể cả `IAdminNewsSourceService`,
      `NewsSourceRegistry` với `IMemoryCache`, `NewsSourceHealthCheckJob` qua `AddHostedService`) +
      cấu hình `AddResilienceHandler`
- [ ] `BackgroundJobs/ExternalRequestRetryJob.cs` — thêm `MarketScope` vào `RetryPayload`
- [ ] `Data/Extensions/ModelBuilderExtensions.cs` — gọi seed data mới
- [ ] `Engines/Providers/Domain/Fetching/NewsSourceRegistry.cs` — thêm logic invalidate `IMemoryCache`
      ngay khi `AdminNewsSourceService` Create/Update/Delete/Toggle thành công (mục 3.10.e)

**KHÔNG sửa (out of scope, giữ nguyên vì đã hoạt động đúng):**
- `YoutubeEngine.cs`, `RealYouTubeSearchEngine.cs` — team xác nhận YouTube Engine đã có, tái sử dụng nguyên bản qua `ISocialExecutorService.YouTubeSearchAsync` hiện có.
- `AIExecutorService.cs` — cơ chế multi-key + cooldown cho AI provider đã đúng chuẩn enterprise, dùng làm **mẫu tham chiếu** cho `DomainRateLimiterRegistry` (cùng pattern: `LastRateLimitedAt` + `CooldownMinutes` + vòng lặp thử provider tiếp theo).

---

## 5. THỨ TỰ THỰC HIỆN (bắt buộc, không đảo bước)

1. Thêm NuGet packages → build thử để chắc chắn resolve được.
2. Tạo 2 enum mới → thêm field `NewsSourceEntity` → tạo migration → `dotnet ef database update`.
3. Viết `NewsSourceSeedData.cs` với danh sách mục 2 (đã verify URL bằng `curl -I` trước khi seed).
4. Viết `DomainRateLimiterRegistry` + `CircuitBreakerRegistry` (thuần in-memory, có unit test riêng, không phụ thuộc DB).
5. Viết `ResponseCacheStore` (dùng `IMemoryCache`, TTL cấu hình được qua `appsettings.json`, key = `domain + keyword-hash`).
6. Viết `GoogleNewsFallbackFetcher` + `SitemapFallbackFetcher`.
7. Viết `SourceFetchPipeline` ghép 4 tier lại.
8. Refactor `RealRssNewsSourceEngine.CrawlAsync` để orchestrate qua pipeline (giữ `FetchSingleAsync` gần như cũ, chỉ đổi sang dùng `ResilientHttpClient`).
9. Cấu hình `AddResilienceHandler` trong `DependencyInjection.cs`.
10. Sửa `SocialExecutorService.RssCrawlAsync` theo mục 3.8.
11. Sửa `NewsService.SearchAsync` bật lại scoring theo mục 3.7.
12. Sửa `ExternalRequestRetryJob` + `RetryPayload` thêm `MarketScope`.
13. Test tích hợp end-to-end (mục 6) trước khi coi là done.

---

## 6. TIÊU CHÍ NGHIỆM THU / TEST CHECKLIST (để không có bug sót)

- [ ] Gọi `POST` search với `MarketScope=Both`, `TimeRange=7 ngày`, keyword `"vàng, FED"` → trả về tin từ **ít nhất 4/5 nhóm nguồn** (trừ nhóm 5 test riêng qua YouTube endpoint).
- [ ] Giả lập 1 domain trả 429 liên tục (mock `HttpMessageHandler` trả `TooManyRequests`) → xác nhận: (a) không throw lên `NewsService`, (b) domain đó chuyển sang Circuit Open sau 3 lỗi liên tiếp trong `SamplingDuration`, (c) các domain khác vẫn fetch bình thường song song.
- [ ] Tắt mạng hoàn toàn cho 1 domain (timeout) → xác nhận Tier 2/3 được thử, và nếu tất cả tier đều fail thì Tier 4 (cache) trả về dữ liệu cũ thay vì rỗng, miễn là có cache từ lần chạy trước.
- [ ] Kiểm tra `NewsItemEntity` sau khi search có đầy đủ `TotalScore`, `Recommendation`, `RelevanceLevel`, `EmotionTags`, `SuggestedKeywordsForThumbnail` — **không còn null** (bug #6 đã fix).
- [ ] Kiểm tra `ExternalRequestQueueEntity` chỉ được tạo khi **thực sự 0 kết quả toàn cục**, không tạo tràn lan mỗi khi 1 nguồn lẻ lỗi (tránh spam bảng queue).
- [ ] Load test: gọi liên tiếp 5 lần search trong 1 phút với cùng keyword → xác nhận số HTTP request thật ra ngoài **giảm dần** nhờ `ResponseCacheStore` (không gọi lại domain vừa fetch trong TTL).
- [ ] Xác nhận `User-Agent` mới không bị các site quốc tế chặn ngay từ đầu (test thủ công `curl -A "<UA mới>" <url>` với vài domain khó như Reuters, Investing).
- [ ] Review log: không còn dòng `LogWarning("Failed to fetch RSS feed...")` bị nuốt im lặng mà không ghi nhận vào circuit breaker / metrics.
- [ ] Đảm bảo migration EF Core chạy sạch trên DB hiện tại (không mất dữ liệu `NewsSourceEntity` cũ).
- [ ] Đảm bảo `dotnet build` không warning liên quan đến nullable/CS8618 cho các entity mới.
- [ ] **Admin CRUD dynamic (mục 3.10):** sửa `RssOrFeedUrl` của 1 nguồn qua `PUT /admin/news-sources/{id}`,
      gọi lại research ngay sau đó (trong vòng < 5 phút TTL cache) → xác nhận Engine dùng URL MỚI, không
      phải URL cũ (chứng minh cache invalidation hoạt động đúng).
- [ ] `POST /admin/news-sources/{id}/test` trả về preview đúng dữ liệu thật, không phải mock, và không
      làm ảnh hưởng tới trạng thái circuit breaker/cooldown thật của domain đó (test không tính là 1 lần
      fetch thật để tránh làm sai lệch số liệu vận hành).
- [ ] Thêm 1 nguồn hoàn toàn mới qua `POST /admin/news-sources` (vd 1 domain chưa từng seed) → research
      chạy ngay lần tiếp theo phải bao gồm nguồn này mà không cần deploy lại code.
- [ ] `PATCH /admin/news-sources/{id}/toggle` set `IsTrusted=false` → xác nhận nguồn đó bị loại khỏi
      `NewsSourceRegistry.GetActiveSourcesAsync` ngay từ lần gọi tiếp theo.

---

## 7. GHI CHÚ QUAN TRỌNG VỀ TÍNH KHẢ THI PHÁP LÝ/KỸ THUẬT CỦA VIỆC "CRAWL ĐẦY ĐỦ MỌI TRANG"

Cần nói rõ với khách hàng (không che giấu trong lúc lập kế hoạch kỹ thuật):

1. **Bloomberg, WSJ, Nikkei Asia, FT** là các trang **có paywall + activevly chống bot** (Cloudflare/Akamai bot management). Dù có retry/backoff/circuit-breaker "enterprise" đến đâu, việc scrape trực tiếp các trang này **vẫn có thể bị chặn ở tầng WAF trước khi tới được HTTP layer của mình** — đây là giới hạn kỹ thuật khách quan, không phải do code viết sai. Giải pháp thực tế duy nhất bền vững là dùng **Google News RSS site-restricted** (Tier 2) làm nguồn thay thế hợp pháp, hoặc mua **data license chính thức** (Bloomberg Terminal API, Refinitiv, v.v.) nếu cần dữ liệu gốc 100%.
2. **World Gold Council, BEA** có thể đổi cấu trúc RSS bất kỳ lúc nào — cần cơ chế health-check định kỳ (khuyến nghị thêm 1 `HostedService` nhẹ chạy mỗi 6h ping thử tất cả `RssOrFeedUrl` trong `NewsSourceEntity`, cập nhật cờ `IsTrusted=false` tự động nếu 404 liên tục — **có thể làm ở phase 2**, không bắt buộc trong bản đầu này).
3. **"Không bao giờ 429"** về mặt tuyệt đối là không thể cam kết 100% cho bên thứ 3 ngoài tầm kiểm soát — điều pipeline này đảm bảo là: **429 ở 1 nguồn không làm chết cả hệ thống**, luôn có fallback/cache để trả kết quả cho user, và tự phục hồi khi domain hết cooldown. Đây là cách các hệ thống enterprise thật sự (Bloomberg Terminal, Google News aggregator...) cũng vận hành — không có công ty nào "miễn nhiễm 429 tuyệt đối" khi gọi API/site người khác.

---

## 8. TÓM TẮT CHO AI AGENT THỰC THI

Đọc kỹ Mục 3 + Mục 4 (checklist file) + Mục 5 (thứ tự bước) trước khi viết bất kỳ dòng code nào.
Không tạo lại các lớp đã có (`AIExecutorService`, `YoutubeEngine`, `ExternalRequestQueueEntity`).
Chỉ mở rộng đúng những gì liệt kê. Mọi domain mới thêm vào `NewsSourceSeedData.cs` phải test
`curl -I` thành công trước khi đưa vào migration. Sau khi code xong, chạy đủ checklist Mục 6 —
không báo "done" nếu còn mục nào chưa pass.
