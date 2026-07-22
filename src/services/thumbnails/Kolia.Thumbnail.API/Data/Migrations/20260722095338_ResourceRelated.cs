using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ResourceRelated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveFailureCount",
                table: "NewsSources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "NewsSources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FetchMode",
                table: "NewsSources",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "LastEtag",
                table: "NewsSources",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFailedAt",
                table: "NewsSources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchedAt",
                table: "NewsSources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedHeader",
                table: "NewsSources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceGroup",
                table: "NewsSources",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000001"),
                columns: new[] { "Domain", "FetchMode", "LastEtag", "LastFailedAt", "LastFetchedAt", "LastModifiedHeader", "SourceGroup" },
                values: new object[] { "vnexpress.net", 1, null, null, null, null, 3 });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000002"),
                columns: new[] { "Domain", "FetchMode", "LastEtag", "LastFailedAt", "LastFetchedAt", "LastModifiedHeader", "SourceGroup" },
                values: new object[] { "coindesk.com", 1, null, null, null, null, 1 });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000003"),
                columns: new[] { "Domain", "FetchMode", "LastEtag", "LastFailedAt", "LastFetchedAt", "LastModifiedHeader", "SourceGroup" },
                values: new object[] { "federalreserve.gov", 1, null, null, null, null, 2 });

            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "CreationTime", "DeletionTime", "Domain", "FetchMode", "IsDeleted", "IsTrusted", "LastEtag", "LastFailedAt", "LastFetchedAt", "LastModificationTime", "LastModifiedHeader", "Name", "Priority", "Region", "RssOrFeedUrl", "SourceGroup" },
                values: new object[,]
                {
                    { new Guid("11111111-0002-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "reuters.com", 2, false, true, null, null, null, null, null, "Reuters Business", 10, 2, "https://feeds.reuters.com/reuters/businessNews", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "cnbc.com", 1, false, true, null, null, null, null, null, "CNBC", 11, 2, "https://www.cnbc.com/id/10001147/device/rss/rss.html", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "marketwatch.com", 1, false, true, null, null, null, null, null, "MarketWatch", 12, 2, "https://feeds.content.dowjones.io/public/rss/mw_topstories", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "ft.com", 3, false, true, null, null, null, null, null, "Financial Times", 13, 2, "https://www.ft.com", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "cointelegraph.com", 1, false, true, null, null, null, null, null, "Cointelegraph", 14, 2, "https://cointelegraph.com/rss", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "investing.com", 2, false, true, null, null, null, null, null, "Investing.com", 15, 2, "https://www.investing.com/rss/news.rss", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000007"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "nytimes.com", 1, false, true, null, null, null, null, null, "NY Times Business", 16, 2, "https://rss.nytimes.com/services/xml/rss/nyt/Business.xml", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000008"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "wsj.com", 3, false, true, null, null, null, null, null, "WSJ Markets", 17, 2, "https://www.wsj.com", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000009"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "economist.com", 1, false, true, null, null, null, null, null, "The Economist", 18, 2, "https://www.economist.com/finance-and-economics/rss.xml", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000010"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "foreignaffairs.com", 1, false, true, null, null, null, null, null, "Foreign Affairs", 19, 2, "https://www.foreignaffairs.com/rss.xml", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000011"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "asia.nikkei.com", 3, false, true, null, null, null, null, null, "Nikkei Asia", 20, 2, "https://asia.nikkei.com", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000012"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "bloomberg.com", 3, false, true, null, null, null, null, null, "Bloomberg", 21, 2, "https://www.bloomberg.com", 1 },
                    { new Guid("11111111-0002-7000-8000-000000000013"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "fidelity.com", 2, false, true, null, null, null, null, null, "Fidelity Insights", 22, 2, "https://www.fidelity.com/learning-center/trading-investing/markets-economy-finance/stock-market-outlook", 1 },
                    { new Guid("11111111-0003-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "bls.gov", 1, false, true, null, null, null, null, null, "BLS News Release", 31, 2, "https://www.bls.gov/feed/news_release.rss", 2 },
                    { new Guid("11111111-0003-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "bea.gov", 4, false, true, null, null, null, null, null, "BEA (Bureau of Economic Analysis)", 32, 2, "https://www.bea.gov/rss.xml", 2 },
                    { new Guid("11111111-0003-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "gold.org", 4, false, true, null, null, null, null, null, "World Gold Council", 33, 2, "https://www.gold.org/goldhub/research/rss", 2 },
                    { new Guid("11111111-0003-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "imf.org", 1, false, true, null, null, null, null, null, "IMF News", 34, 2, "https://www.imf.org/en/News/rss?language=eng", 2 },
                    { new Guid("11111111-0003-7000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "worldbank.org", 1, false, true, null, null, null, null, null, "World Bank News", 35, 2, "https://www.worldbank.org/en/news/all?qterm=&lang_exact=English&format=rss", 2 },
                    { new Guid("11111111-0004-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "cafef.vn", 1, false, true, null, null, null, null, null, "CafeF", 40, 1, "https://cafef.vn/thi-truong-chung-khoan.rss", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "cafebiz.vn", 1, false, true, null, null, null, null, null, "CafeBiz", 41, 1, "https://cafebiz.vn/rss/home.rss", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "vneconomy.vn", 1, false, true, null, null, null, null, null, "VnEconomy", 42, 1, "https://vneconomy.vn/tai-chinh.rss", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "vietstock.vn", 4, false, true, null, null, null, null, null, "Vietstock", 43, 1, "https://vietstock.vn/144/chung-khoan/co-phieu.rss", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "ssi.com.vn", 5, false, true, null, null, null, null, null, "SSI Research", 44, 1, "https://www.ssi.com.vn/en/research", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "mbs.com.vn", 5, false, true, null, null, null, null, null, "MBS Research", 45, 1, "https://www.mbs.com.vn/en/research", 3 },
                    { new Guid("11111111-0004-7000-8000-000000000007"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "vndirect.com.vn", 5, false, true, null, null, null, null, null, "VNDirect", 46, 1, "https://www.vndirect.com.vn/en/research", 3 },
                    { new Guid("11111111-0005-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "tradingview.com", 5, false, true, null, null, null, null, null, "TradingView News", 50, 2, "https://www.tradingview.com/news/", 4 },
                    { new Guid("11111111-0005-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "kitco.com", 4, false, true, null, null, null, null, null, "Kitco (Gold)", 51, 2, "https://www.kitco.com/rss/", 4 }
                });

            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "CreationTime", "DeletionTime", "Domain", "FetchMode", "IsDeleted", "LastEtag", "LastFailedAt", "LastFetchedAt", "LastModificationTime", "LastModifiedHeader", "Name", "Priority", "Region", "RssOrFeedUrl", "SourceGroup" },
                values: new object[] { new Guid("11111111-0006-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "trends.google.com", 1, false, null, null, null, null, null, "Google Trends VN", 60, 1, "https://trends.google.com/trends/trendingsearches/daily/rss?geo=VN", 5 });

            migrationBuilder.CreateIndex(
                name: "IX_NewsSources_Domain",
                table: "NewsSources",
                column: "Domain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsSources_Domain",
                table: "NewsSources");

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0006-7000-8000-000000000001"));

            migrationBuilder.DropColumn(
                name: "ConsecutiveFailureCount",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "FetchMode",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "LastEtag",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "LastFailedAt",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "LastFetchedAt",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "LastModifiedHeader",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "SourceGroup",
                table: "NewsSources");
        }
    }
}
