using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000003"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000005"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000006"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000007"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000001"),
                column: "FetchMode",
                value: 3);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000002"),
                column: "FetchMode",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000003"),
                column: "FetchMode",
                value: 4);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0003-7000-8000-000000000004"),
                column: "FetchMode",
                value: 4);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000005"),
                column: "FetchMode",
                value: 5);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000006"),
                column: "FetchMode",
                value: 5);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0004-7000-8000-000000000007"),
                column: "FetchMode",
                value: 5);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000001"),
                column: "FetchMode",
                value: 5);

            migrationBuilder.UpdateData(
                table: "NewsSources",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0005-7000-8000-000000000002"),
                column: "FetchMode",
                value: 4);
        }
    }
}
