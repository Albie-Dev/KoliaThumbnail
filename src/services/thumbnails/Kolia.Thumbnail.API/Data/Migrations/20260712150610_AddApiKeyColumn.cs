using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "Endpoint",
                table: "AIConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "AIConfigurations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHash",
                table: "AIConfigurations",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastTokenResetTime",
                table: "AIConfigurations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TotalTokensUsed",
                table: "AIConfigurations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyHash",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "LastTokenResetTime",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "TotalTokensUsed",
                table: "AIConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "AIConfigurations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "AIConfigurations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endpoint",
                table: "AIConfigurations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
