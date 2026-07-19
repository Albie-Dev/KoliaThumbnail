using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kolia.Thumbnail.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalRequestUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AIProviderType = table.Column<int>(type: "integer", nullable: true),
                    SocialMediaProviderType = table.Column<int>(type: "integer", nullable: true),
                    RequestCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedTokenUsage = table.Column<long>(type: "bigint", nullable: true),
                    EstimatedCostUsd = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RecordedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalRequestUsageLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RssOrFeedUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Region = table.Column<int>(type: "integer", nullable: false),
                    IsTrusted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentStepNumber = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailCoverUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastActivityTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApiKey = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApiVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TotalTokensUsed = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ApiKeyHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LastTokenResetTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExtraSettingsJson = table.Column<string>(type: "text", nullable: true),
                    AIProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIProviderConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIProviderConfigurations_AIProviders_AIProviderId",
                        column: x => x.AIProviderId,
                        principalTable: "AIProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExpressionLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AngleLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterImages_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentBriefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSource = table.Column<int>(type: "integer", nullable: true),
                    ImportedRawText = table.Column<string>(type: "text", nullable: true),
                    ImportedFileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImportedExternalLink = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExternalSheetUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastSheetSyncTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OverviewInput = table.Column<string>(type: "text", nullable: false),
                    ViewpointInput = table.Column<string>(type: "text", nullable: false),
                    KeyDataInput = table.Column<string>(type: "text", nullable: false),
                    TopicOutput = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MainMessageOutput = table.Column<string>(type: "text", nullable: true),
                    HighlightDataOutput = table.Column<string>(type: "text", nullable: true),
                    SheetImportedText = table.Column<string>(type: "text", nullable: true),
                    SuggestedKeywordsJson = table.Column<string>(type: "text", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBriefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentBriefs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisplayTextRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayTextRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayTextRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalRequestQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProviderConfigurationIdUsed = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalRequestQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalRequestQueues_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NewsSearchRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketScope = table.Column<int>(type: "integer", nullable: false),
                    TimeRange = table.Column<int>(type: "integer", nullable: false),
                    CountFilter = table.Column<int>(type: "integer", nullable: false),
                    KeywordsRaw = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SuggestedKeywordsUsedJson = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsSearchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsSearchRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    StepStatus = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OutputSummaryText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NeedsApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailGenerationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangesRequestText = table.Column<string>(type: "text", nullable: false),
                    LockedElementsJson = table.Column<string>(type: "text", nullable: true),
                    ChangeableElementsJson = table.Column<string>(type: "text", nullable: true),
                    GeneratedPromptText = table.Column<string>(type: "text", nullable: true),
                    Ratio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequestedImageCount = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailGenerationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequests_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailSearchRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Keyword = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TimeFilter = table.Column<int>(type: "integer", nullable: false),
                    SortFilter = table.Column<int>(type: "integer", nullable: false),
                    WasSuggestedFromNews = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailSearchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThumbnailSearchRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedTitleCount = table.Column<int>(type: "integer", nullable: false),
                    Style = table.Column<int>(type: "integer", nullable: false),
                    KeywordsRaw = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BuiltPromptText = table.Column<string>(type: "text", nullable: false),
                    GenerationRound = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ApiVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApiBaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApiKeyHash = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ClientSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AppSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BearerToken = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AccessToken = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalRequest = table.Column<int>(type: "integer", nullable: false),
                    LastRequestResetTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastRateLimitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RateLimitCooldownMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SocialMediaProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaProviderConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialMediaProviderConfigurations_SocialMediaProviders_Soci~",
                        column: x => x.SocialMediaProviderId,
                        principalTable: "SocialMediaProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsSearchRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    PublishedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScannedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SummaryOverview = table.Column<string>(type: "text", nullable: false),
                    RelevanceToTopicScore = table.Column<int>(type: "integer", nullable: false),
                    ImportanceImpactScore = table.Column<int>(type: "integer", nullable: false),
                    EmotionPotentialScore = table.Column<int>(type: "integer", nullable: false),
                    NoveltyDataScore = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    Recommendation = table.Column<int>(type: "integer", nullable: false),
                    RelevanceLevel = table.Column<int>(type: "integer", nullable: false),
                    IsSelectedByTeam = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SuggestedKeywordsForThumbnail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsItems_NewsSearchRequests_NewsSearchRequestId",
                        column: x => x.NewsSearchRequestId,
                        principalTable: "NewsSearchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NewsItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedThumbnailSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThumbnailGenerationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetIndex = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedThumbnailSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedThumbnailSets_ThumbnailGenerationRequests_Thumbnai~",
                        column: x => x.ThumbnailGenerationRequestId,
                        principalTable: "ThumbnailGenerationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailLibraryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThumbnailSearchRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    VideoTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThumbnailImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: true),
                    PublishedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<long>(type: "bigint", nullable: true),
                    KeywordBatchTag = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UserStatus = table.Column<int>(type: "integer", nullable: false),
                    IsFilteredIrrelevant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailLibraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThumbnailLibraryItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThumbnailLibraryItems_ThumbnailSearchRequests_ThumbnailSear~",
                        column: x => x.ThumbnailSearchRequestId,
                        principalTable: "ThumbnailSearchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoTitleRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedbackText = table.Column<string>(type: "text", nullable: false),
                    AppliedToRound = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleFeedbacks_VideoTitleRequests_VideoTitleRequestId",
                        column: x => x.VideoTitleRequestId,
                        principalTable: "VideoTitleRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoTitleRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationRound = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsSelected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleOptions_VideoTitleRequests_VideoTitleRequestId",
                        column: x => x.VideoTitleRequestId,
                        principalTable: "VideoTitleRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisplayTextOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayTextRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNewsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsSelected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayTextOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayTextOptions_DisplayTextRequests_DisplayTextRequestId",
                        column: x => x.DisplayTextRequestId,
                        principalTable: "DisplayTextRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplayTextOptions_NewsItems_SourceNewsItemId",
                        column: x => x.SourceNewsItemId,
                        principalTable: "NewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisplayTextRequestNewsItems",
                columns: table => new
                {
                    DisplayTextRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayTextRequestNewsItems", x => new { x.DisplayTextRequestId, x.NewsItemId });
                    table.ForeignKey(
                        name: "FK_DisplayTextRequestNewsItems_DisplayTextRequests_DisplayText~",
                        column: x => x.DisplayTextRequestId,
                        principalTable: "DisplayTextRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplayTextRequestNewsItems_NewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "NewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NewsDeepAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MacroEventSummaryJson = table.Column<string>(type: "text", nullable: false),
                    MarketReactionJson = table.Column<string>(type: "text", nullable: false),
                    ExpectationShortTerm = table.Column<string>(type: "text", nullable: false),
                    ExpectationLongTerm = table.Column<string>(type: "text", nullable: false),
                    SentimentOverviewJson = table.Column<string>(type: "text", nullable: false),
                    EmotionTags = table.Column<int>(type: "integer", nullable: false),
                    EmotionReason = table.Column<string>(type: "text", nullable: false),
                    WasTranslatedFromForeign = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MissingDataNote = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsDeepAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsDeepAnalyses_NewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "NewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleRequestNewsItems",
                columns: table => new
                {
                    VideoTitleRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleRequestNewsItems", x => new { x.VideoTitleRequestId, x.NewsItemId });
                    table.ForeignKey(
                        name: "FK_VideoTitleRequestNewsItems_NewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "NewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoTitleRequestNewsItems_VideoTitleRequests_VideoTitleReq~",
                        column: x => x.VideoTitleRequestId,
                        principalTable: "VideoTitleRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedThumbnails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedThumbnailSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantIndex = table.Column<int>(type: "integer", nullable: false),
                    ParentGeneratedThumbnailId = table.Column<Guid>(type: "uuid", nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayTextSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CharacterSnapshotName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LastEditTool = table.Column<int>(type: "integer", nullable: true),
                    LastEditRequestText = table.Column<string>(type: "text", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WasDownloaded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPushedToTitleStep = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedThumbnails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedThumbnails_GeneratedThumbnailSets_GeneratedThumbna~",
                        column: x => x.GeneratedThumbnailSetId,
                        principalTable: "GeneratedThumbnailSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedThumbnails_GeneratedThumbnails_ParentGeneratedThum~",
                        column: x => x.ParentGeneratedThumbnailId,
                        principalTable: "GeneratedThumbnails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThumbnailLibraryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThumbnailFactorsJson = table.Column<string>(type: "text", nullable: false),
                    TitleTextAnalysis = table.Column<string>(type: "text", nullable: false),
                    VideoTitleAnalysis = table.Column<string>(type: "text", nullable: false),
                    DisplayTextStyleNote = table.Column<string>(type: "text", nullable: false),
                    IsChosenForGeneration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThumbnailAnalyses_ThumbnailLibraryItems_ThumbnailLibraryIte~",
                        column: x => x.ThumbnailLibraryItemId,
                        principalTable: "ThumbnailLibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailGenerationRequestReferences",
                columns: table => new
                {
                    ThumbnailGenerationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThumbnailLibraryItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailGenerationRequestReferences", x => new { x.ThumbnailGenerationRequestId, x.ThumbnailLibraryItemId });
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequestReferences_ThumbnailGenerationReq~",
                        column: x => x.ThumbnailGenerationRequestId,
                        principalTable: "ThumbnailGenerationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequestReferences_ThumbnailLibraryItems_~",
                        column: x => x.ThumbnailLibraryItemId,
                        principalTable: "ThumbnailLibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailGenerationRequestDisplayTexts",
                columns: table => new
                {
                    ThumbnailGenerationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayTextOptionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailGenerationRequestDisplayTexts", x => new { x.ThumbnailGenerationRequestId, x.DisplayTextOptionId });
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequestDisplayTexts_DisplayTextOptions_D~",
                        column: x => x.DisplayTextOptionId,
                        principalTable: "DisplayTextOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThumbnailGenerationRequestDisplayTexts_ThumbnailGenerationR~",
                        column: x => x.ThumbnailGenerationRequestId,
                        principalTable: "ThumbnailGenerationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedThumbnailId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayTextSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletePackages_GeneratedThumbnails_SelectedThumbnailId",
                        column: x => x.SelectedThumbnailId,
                        principalTable: "GeneratedThumbnails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompletePackages_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleRequestThumbnails",
                columns: table => new
                {
                    VideoTitleRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedThumbnailId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleRequestThumbnails", x => new { x.VideoTitleRequestId, x.GeneratedThumbnailId });
                    table.ForeignKey(
                        name: "FK_VideoTitleRequestThumbnails_GeneratedThumbnails_GeneratedTh~",
                        column: x => x.GeneratedThumbnailId,
                        principalTable: "GeneratedThumbnails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoTitleRequestThumbnails_VideoTitleRequests_VideoTitleRe~",
                        column: x => x.VideoTitleRequestId,
                        principalTable: "VideoTitleRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletePackageTitles",
                columns: table => new
                {
                    CompletePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoTitleOptionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletePackageTitles", x => new { x.CompletePackageId, x.VideoTitleOptionId });
                    table.ForeignKey(
                        name: "FK_CompletePackageTitles_CompletePackages_CompletePackageId",
                        column: x => x.CompletePackageId,
                        principalTable: "CompletePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletePackageTitles_VideoTitleOptions_VideoTitleOptionId",
                        column: x => x.VideoTitleOptionId,
                        principalTable: "VideoTitleOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "NewsSources",
                columns: new[] { "Id", "CreationTime", "DeletionTime", "IsDeleted", "IsTrusted", "LastModificationTime", "Name", "Priority", "Region", "RssOrFeedUrl" },
                values: new object[,]
                {
                    { new Guid("11111111-0001-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, true, null, "VnExpress", 1, 1, "https://vnexpress.net/rss/kinh-doanh.rss" },
                    { new Guid("11111111-0001-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, true, null, "CoinDesk", 2, 2, "https://www.coindesk.com/arc/outboundfeeds/rss/" },
                    { new Guid("11111111-0001-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, true, null, "Federal Reserve", 3, 2, "https://www.federalreserve.gov/feeds/press_all.xml" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIProviderConfigurations_AIProviderId_Name",
                table: "AIProviderConfigurations",
                columns: new[] { "AIProviderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviderConfigurations_Id_IsDeleted",
                table: "AIProviderConfigurations",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_Id_IsDeleted",
                table: "AIProviders",
                columns: new[] { "Id", "IsDeleted" });

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

            migrationBuilder.CreateIndex(
                name: "IX_CharacterImages_CharacterId",
                table: "CharacterImages",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletePackages_ProjectId",
                table: "CompletePackages",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletePackages_SelectedThumbnailId",
                table: "CompletePackages",
                column: "SelectedThumbnailId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletePackageTitles_VideoTitleOptionId",
                table: "CompletePackageTitles",
                column: "VideoTitleOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBriefs_ProjectId",
                table: "ContentBriefs",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisplayTextOptions_DisplayTextRequestId",
                table: "DisplayTextOptions",
                column: "DisplayTextRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayTextOptions_SourceNewsItemId",
                table: "DisplayTextOptions",
                column: "SourceNewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayTextRequestNewsItems_NewsItemId",
                table: "DisplayTextRequestNewsItems",
                column: "NewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayTextRequests_ProjectId",
                table: "DisplayTextRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRequestQueues_ProjectId",
                table: "ExternalRequestQueues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRequestQueues_Status_NextRetryAt",
                table: "ExternalRequestQueues",
                columns: new[] { "Status", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRequestUsageLogs_RecordedDate",
                table: "ExternalRequestUsageLogs",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedThumbnails_GeneratedThumbnailSetId",
                table: "GeneratedThumbnails",
                column: "GeneratedThumbnailSetId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedThumbnails_ParentGeneratedThumbnailId",
                table: "GeneratedThumbnails",
                column: "ParentGeneratedThumbnailId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedThumbnailSets_ThumbnailGenerationRequestId",
                table: "GeneratedThumbnailSets",
                column: "ThumbnailGenerationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsDeepAnalyses_NewsItemId",
                table: "NewsDeepAnalyses",
                column: "NewsItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_NewsSearchRequestId",
                table: "NewsItems",
                column: "NewsSearchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_ProjectId",
                table: "NewsItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsSearchRequests_ProjectId",
                table: "NewsSearchRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSteps_ProjectId_StepNumber",
                table: "ProjectSteps",
                columns: new[] { "ProjectId", "StepNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaProviderConfigurations_SocialMediaProviderId_Name",
                table: "SocialMediaProviderConfigurations",
                columns: new[] { "SocialMediaProviderId", "Name" },
                unique: true);

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
                name: "IX_ThumbnailAnalyses_ThumbnailLibraryItemId",
                table: "ThumbnailAnalyses",
                column: "ThumbnailLibraryItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailGenerationRequestDisplayTexts_DisplayTextOptionId",
                table: "ThumbnailGenerationRequestDisplayTexts",
                column: "DisplayTextOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailGenerationRequestReferences_ThumbnailLibraryItemId",
                table: "ThumbnailGenerationRequestReferences",
                column: "ThumbnailLibraryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailGenerationRequests_CharacterId",
                table: "ThumbnailGenerationRequests",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailGenerationRequests_ProjectId",
                table: "ThumbnailGenerationRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailLibraryItems_ProjectId",
                table: "ThumbnailLibraryItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailLibraryItems_ThumbnailSearchRequestId",
                table: "ThumbnailLibraryItems",
                column: "ThumbnailSearchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailSearchRequests_ProjectId",
                table: "ThumbnailSearchRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleFeedbacks_VideoTitleRequestId",
                table: "VideoTitleFeedbacks",
                column: "VideoTitleRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleOptions_VideoTitleRequestId",
                table: "VideoTitleOptions",
                column: "VideoTitleRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleRequestNewsItems_NewsItemId",
                table: "VideoTitleRequestNewsItems",
                column: "NewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleRequests_ProjectId",
                table: "VideoTitleRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleRequestThumbnails_GeneratedThumbnailId",
                table: "VideoTitleRequestThumbnails",
                column: "GeneratedThumbnailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIProviderConfigurations");

            migrationBuilder.DropTable(
                name: "CharacterImages");

            migrationBuilder.DropTable(
                name: "CompletePackageTitles");

            migrationBuilder.DropTable(
                name: "ContentBriefs");

            migrationBuilder.DropTable(
                name: "DisplayTextRequestNewsItems");

            migrationBuilder.DropTable(
                name: "ExternalRequestQueues");

            migrationBuilder.DropTable(
                name: "ExternalRequestUsageLogs");

            migrationBuilder.DropTable(
                name: "NewsDeepAnalyses");

            migrationBuilder.DropTable(
                name: "NewsSources");

            migrationBuilder.DropTable(
                name: "ProjectSteps");

            migrationBuilder.DropTable(
                name: "SocialMediaProviderConfigurations");

            migrationBuilder.DropTable(
                name: "ThumbnailAnalyses");

            migrationBuilder.DropTable(
                name: "ThumbnailGenerationRequestDisplayTexts");

            migrationBuilder.DropTable(
                name: "ThumbnailGenerationRequestReferences");

            migrationBuilder.DropTable(
                name: "VideoTitleFeedbacks");

            migrationBuilder.DropTable(
                name: "VideoTitleRequestNewsItems");

            migrationBuilder.DropTable(
                name: "VideoTitleRequestThumbnails");

            migrationBuilder.DropTable(
                name: "AIProviders");

            migrationBuilder.DropTable(
                name: "CompletePackages");

            migrationBuilder.DropTable(
                name: "VideoTitleOptions");

            migrationBuilder.DropTable(
                name: "SocialMediaProviders");

            migrationBuilder.DropTable(
                name: "DisplayTextOptions");

            migrationBuilder.DropTable(
                name: "ThumbnailLibraryItems");

            migrationBuilder.DropTable(
                name: "GeneratedThumbnails");

            migrationBuilder.DropTable(
                name: "VideoTitleRequests");

            migrationBuilder.DropTable(
                name: "DisplayTextRequests");

            migrationBuilder.DropTable(
                name: "NewsItems");

            migrationBuilder.DropTable(
                name: "ThumbnailSearchRequests");

            migrationBuilder.DropTable(
                name: "GeneratedThumbnailSets");

            migrationBuilder.DropTable(
                name: "NewsSearchRequests");

            migrationBuilder.DropTable(
                name: "ThumbnailGenerationRequests");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
