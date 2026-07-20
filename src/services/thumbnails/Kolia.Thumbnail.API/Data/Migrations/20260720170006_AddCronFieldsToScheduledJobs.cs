using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCronFieldsToScheduledJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CronDescription",
                table: "ScheduledImportJobs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "ScheduledImportJobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronDescription",
                table: "ScheduledImportJobs");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "ScheduledImportJobs");
        }
    }
}
