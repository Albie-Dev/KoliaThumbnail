using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIFunctionConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIFunctionConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FunctionType = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Temperature = table.Column<double>(type: "double precision", precision: 3, scale: 2, nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFunctionConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIFunctionConfigItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FunctionConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AIProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AIProviderConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Temperature = table.Column<double>(type: "double precision", precision: 3, scale: 2, nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFunctionConfigItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIFunctionConfigItems_AIFunctionConfigs_FunctionConfigId",
                        column: x => x.FunctionConfigId,
                        principalTable: "AIFunctionConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIFunctionConfigItems_AIProviderConfigurations_AIProviderCo~",
                        column: x => x.AIProviderConfigurationId,
                        principalTable: "AIProviderConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIFunctionConfigItems_AIProviders_AIProviderId",
                        column: x => x.AIProviderId,
                        principalTable: "AIProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AIFunctionConfigs",
                columns: new[] { "Id", "CreationTime", "DeletionTime", "FunctionType", "LastModificationTime", "MaxTokens", "Model", "Temperature" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0001-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1, null, null, null, null },
                    { new Guid("aaaaaaaa-0002-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 2, null, null, null, null },
                    { new Guid("aaaaaaaa-0003-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 3, null, null, null, null },
                    { new Guid("aaaaaaaa-0004-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 4, null, null, null, null },
                    { new Guid("aaaaaaaa-0005-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 5, null, null, null, null },
                    { new Guid("aaaaaaaa-0006-7000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 6, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigItems_AIProviderConfigurationId",
                table: "AIFunctionConfigItems",
                column: "AIProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigItems_AIProviderId",
                table: "AIFunctionConfigItems",
                column: "AIProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigItems_FunctionConfigId_Priority",
                table: "AIFunctionConfigItems",
                columns: new[] { "FunctionConfigId", "Priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigItems_Id_IsDeleted",
                table: "AIFunctionConfigItems",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigs_FunctionType",
                table: "AIFunctionConfigs",
                column: "FunctionType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIFunctionConfigs_Id_IsDeleted",
                table: "AIFunctionConfigs",
                columns: new[] { "Id", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIFunctionConfigItems");

            migrationBuilder.DropTable(
                name: "AIFunctionConfigs");
        }
    }
}
