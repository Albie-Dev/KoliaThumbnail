using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PausedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorDetail = table.Column<string>(type: "text", nullable: true),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalSteps = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedSteps = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StepDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DisplayCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsTrackable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepDefinitions_StepDefinitions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "StepDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ContentJson = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSteps_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSteps_StepDefinitions_StepDefinitionId",
                        column: x => x.StepDefinitionId,
                        principalTable: "StepDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "StepDefinitions",
                columns: new[] { "Id", "Code", "CreationTime", "DeletionTime", "DisplayCode", "IsTrackable", "LastModificationTime", "Name", "ParentId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "video_content", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "1", true, null, "Nội dung video", null, 1 },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "news", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "2", true, null, "Tin tức", null, 2 }
                });

            migrationBuilder.InsertData(
                table: "StepDefinitions",
                columns: new[] { "Id", "Code", "CreationTime", "DeletionTime", "DisplayCode", "LastModificationTime", "Name", "ParentId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111103"), "thumbnail_reference", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "3", null, "Thumbnail tham khảo", null, 3 },
                    { new Guid("11111111-1111-1111-1111-111111111104"), "thumbnail", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "4", null, "Thumbnail", null, 4 }
                });

            migrationBuilder.InsertData(
                table: "StepDefinitions",
                columns: new[] { "Id", "Code", "CreationTime", "DeletionTime", "DisplayCode", "IsTrackable", "LastModificationTime", "Name", "ParentId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111105"), "video_title", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "5", true, null, "Tạo video title", null, 5 },
                    { new Guid("11111111-1111-1111-1111-111111111106"), "complete_set", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "6", true, null, "Bộ hoàn chỉnh", null, 6 },
                    { new Guid("11111111-1111-1111-1111-111111111131"), "thumbnail_reference_library", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "3.1", true, null, "Thumbnail library", new Guid("11111111-1111-1111-1111-111111111103"), 1 },
                    { new Guid("11111111-1111-1111-1111-111111111141"), "thumbnail_display_text", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "4.1", true, null, "Tạo display text", new Guid("11111111-1111-1111-1111-111111111104"), 1 },
                    { new Guid("11111111-1111-1111-1111-111111111142"), "thumbnail_generate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "4.2", true, null, "Tạo thumbnail", new Guid("11111111-1111-1111-1111-111111111104"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Code",
                table: "Projects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Id_IsDeleted",
                table: "Projects",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_Id_IsDeleted",
                table: "ProjectSteps",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_ProjectId_StepDefinitionId",
                table: "ProjectSteps",
                columns: new[] { "ProjectId", "StepDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_Status",
                table: "ProjectSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_StepDefinitionId",
                table: "ProjectSteps",
                column: "StepDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StepDefinitions_Code",
                table: "StepDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StepDefinitions_Id_IsDeleted",
                table: "StepDefinitions",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_StepDefinitions_ParentId_SortOrder",
                table: "StepDefinitions",
                columns: new[] { "ParentId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectSteps");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "StepDefinitions");
        }
    }
}
