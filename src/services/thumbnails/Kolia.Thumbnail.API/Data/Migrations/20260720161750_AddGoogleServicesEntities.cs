using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleServicesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleServiceAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClientEmail = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TokenUri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthUri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthProviderX509CertUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrivateKeyIdHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PrivateKey = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    RawCredentialJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    CredentialJsonHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Scopes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleServiceAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LogJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    CreatedProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBriefId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImportedContent = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    GoogleServiceAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledImportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledImportJobs_GoogleServiceAccounts_GoogleServiceAcco~",
                        column: x => x.GoogleServiceAccountId,
                        principalTable: "GoogleServiceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleServiceAccounts_ClientEmail",
                table: "GoogleServiceAccounts",
                column: "ClientEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleServiceAccounts_Id_IsDeleted",
                table: "GoogleServiceAccounts",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledImportJobs_GoogleServiceAccountId",
                table: "ScheduledImportJobs",
                column: "GoogleServiceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledImportJobs_Id_IsDeleted",
                table: "ScheduledImportJobs",
                columns: new[] { "Id", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledImportJobs");

            migrationBuilder.DropTable(
                name: "GoogleServiceAccounts");
        }
    }
}
