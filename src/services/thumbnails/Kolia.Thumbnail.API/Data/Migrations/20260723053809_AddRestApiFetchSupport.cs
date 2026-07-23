using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestApiFetchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiEndpoint",
                table: "NewsSources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "NewsSources",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApiPaginationType",
                table: "NewsSources",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiQueryParamsTemplate",
                table: "NewsSources",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiRequestHeaders",
                table: "NewsSources",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiResponseJsonPath",
                table: "NewsSources",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000001"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000002"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0001-7000-8000-000000000003"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000001"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:reuters.com&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000002"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000003"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000004"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000005"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000006"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:investing.com/news&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000007"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000008"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000009"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000010"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000011"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000012"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:bloomberg.com&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000013"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:fidelity.com&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000002"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://www.bls.gov/feed/bls_latest.rss" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000003"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "FetchMode", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, 1, "https://apps.bea.gov/rss/rss.xml" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000005"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "FetchMode", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, 2, "https://news.google.com/rss/search?q=site:imf.org/en/News&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000006"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "FetchMode", "Name" },
                values: new object[] { "https://search.worldbank.org/api/v2/news", null, 2, "format=json&rows={maxCount}&displayconttype_exact=Press%20Release&lang_exact=English", null, "documents", 7, "World Bank News (REST API)" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000001"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000002"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000003"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000004"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000005"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:ssi.com.vn&hl=vi&gl=VN&ceid=VN:vi" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000006"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:mbs.com.vn&hl=vi&gl=VN&ceid=VN:vi" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000007"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:vndirect.com.vn&hl=vi&gl=VN&ceid=VN:vi" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000001"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:tradingview.com/news/tradingview&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000002"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=site:kitco.com/news&hl=en-US&gl=US&ceid=US:en" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0006-7000-8000-000000000001"),
                columns: new[] { "ApiEndpoint", "ApiKey", "ApiPaginationType", "ApiQueryParamsTemplate", "ApiRequestHeaders", "ApiResponseJsonPath", "RssOrFeedUrl" },
                values: new object[] { null, null, null, null, null, null, "https://news.google.com/rss/search?q=th%E1%BB%8B+tr%C6%B0%E1%BB%9Dng+ch%E1%BB%A9ng+kho%C3%A1n+Vi%E1%BB%87t+Nam&hl=vi&gl=VN&ceid=VN:vi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiEndpoint",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "ApiPaginationType",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "ApiQueryParamsTemplate",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "ApiRequestHeaders",
                table: "NewsSources");

            migrationBuilder.DropColumn(
                name: "ApiResponseJsonPath",
                table: "NewsSources");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000001"),
                column: "RssOrFeedUrl",
                value: "https://feeds.reuters.com/reuters/businessNews");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000006"),
                column: "RssOrFeedUrl",
                value: "https://www.investing.com/rss/news.rss");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000012"),
                column: "RssOrFeedUrl",
                value: "https://www.bloomberg.com");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000013"),
                column: "RssOrFeedUrl",
                value: "https://www.fidelity.com/learning-center/trading-investing/markets-economy-finance/stock-market-outlook");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000002"),
                column: "RssOrFeedUrl",
                value: "https://www.bls.gov/feed/news_release.rss");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000003"),
                columns: new[] { "FetchMode", "RssOrFeedUrl" },
                values: new object[] { 3, "https://www.bea.gov/rss.xml" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000005"),
                columns: new[] { "FetchMode", "RssOrFeedUrl" },
                values: new object[] { 1, "https://www.imf.org/en/News/rss?language=eng" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000006"),
                columns: new[] { "FetchMode", "Name" },
                values: new object[] { 1, "World Bank News" });

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000005"),
                column: "RssOrFeedUrl",
                value: "https://www.ssi.com.vn/en/research");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000006"),
                column: "RssOrFeedUrl",
                value: "https://www.mbs.com.vn/en/research");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000007"),
                column: "RssOrFeedUrl",
                value: "https://www.vndirect.com.vn/en/research");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000001"),
                column: "RssOrFeedUrl",
                value: "https://www.tradingview.com/news/");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000002"),
                column: "RssOrFeedUrl",
                value: "https://www.kitco.com/rss/");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0006-7000-8000-000000000001"),
                column: "RssOrFeedUrl",
                value: "https://trends.google.com/trends/trendingsearches/daily/rss?geo=VN");
        }
    }
}
