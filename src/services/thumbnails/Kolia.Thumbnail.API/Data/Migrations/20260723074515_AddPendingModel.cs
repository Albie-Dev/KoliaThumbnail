using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000008"),
                column: "RssOrFeedUrl",
                value: "https://news.google.com/rss/search?q=site:wsj.com&hl=en-US&gl=US&ceid=US:en");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"),
                column: "RssOrFeedUrl",
                value: "https://news.google.com/rss/search?q=site:gold.org&hl=en-US&gl=US&ceid=US:en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0002-7000-8000-000000000008"),
                column: "RssOrFeedUrl",
                value: "https://www.wsj.com");

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"),
                column: "RssOrFeedUrl",
                value: "https://www.gold.org/goldhub/research/rss");
        }
    }
}
