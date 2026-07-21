using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZoneToScheduledJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "ScheduledImportJobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "UTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "ScheduledImportJobs");
        }
    }
}
