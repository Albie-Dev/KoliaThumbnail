using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialMediaProviders_Name",
                table: "SocialMediaProviders");

            migrationBuilder.DropIndex(
                name: "IX_SocialMediaProviders_ShortName",
                table: "SocialMediaProviders");

            migrationBuilder.DropIndex(
                name: "IX_AIProviders_Name",
                table: "AIProviders");

            migrationBuilder.DropIndex(
                name: "IX_AIProviders_ShortName",
                table: "AIProviders");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaProviders_Name_IsDeleted",
                table: "SocialMediaProviders",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaProviders_ShortName_IsDeleted",
                table: "SocialMediaProviders",
                columns: new[] { "ShortName", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_Name_IsDeleted",
                table: "AIProviders",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_ShortName_IsDeleted",
                table: "AIProviders",
                columns: new[] { "ShortName", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialMediaProviders_Name_IsDeleted",
                table: "SocialMediaProviders");

            migrationBuilder.DropIndex(
                name: "IX_SocialMediaProviders_ShortName_IsDeleted",
                table: "SocialMediaProviders");

            migrationBuilder.DropIndex(
                name: "IX_AIProviders_Name_IsDeleted",
                table: "AIProviders");

            migrationBuilder.DropIndex(
                name: "IX_AIProviders_ShortName_IsDeleted",
                table: "AIProviders");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaProviders_Name",
                table: "SocialMediaProviders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaProviders_ShortName",
                table: "SocialMediaProviders",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_Name",
                table: "AIProviders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_ShortName",
                table: "AIProviders",
                column: "ShortName",
                unique: true);
        }
    }
}
