using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Briefs;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Services.Projects;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.Briefs
{
    public class ContentBriefService : IContentBriefService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IBriefAnalysisEngine _analysisEngine;
        private readonly ISheetImportEngine _sheetImportEngine;
        private readonly IProjectService _projectService;

        public ContentBriefService(
            ThumbnailDbContext db,
            IBriefAnalysisEngine analysisEngine,
            ISheetImportEngine sheetImportEngine,
            IProjectService projectService)
        {
            _db = db;
            _analysisEngine = analysisEngine;
            _sheetImportEngine = sheetImportEngine;
            _projectService = projectService;
        }

        public async Task<ContentBriefEntity> GetOrCreateAsync(Guid projectId,
            CancellationToken ct = default)
        {
            var existing = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);

            if (existing != null) return existing;

            var brief = new ContentBriefEntity { ProjectId = projectId };
            _db.ContentBriefs.Add(brief);
            await _db.SaveChangesAsync(ct);
            return brief;
        }

        public async Task SaveManualInputAsync(Guid projectId, string overview,
            string viewpoint, string keyData, CancellationToken ct = default)
        {
            var brief = await GetOrCreateAsync(projectId, ct);
            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã được xác nhận, không thể sửa.");

            brief.OverviewInput = overview;
            brief.ViewpointInput = viewpoint;
            brief.KeyDataInput = keyData;
            brief.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task ImportAsync(Guid projectId, CImportContentSource source,
            string? rawText, string? fileUrl, string? externalLink,
            CancellationToken ct = default)
        {
            var brief = await GetOrCreateAsync(projectId, ct);
            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã được xác nhận, không thể sửa.");

            brief.ImportSource = source;
            brief.ImportedRawText = rawText;
            brief.ImportedFileUrl = fileUrl;
            brief.ImportedExternalLink = externalLink;
            brief.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<ContentBriefEntity> AnalyzeWithAIAsync(Guid projectId,
            string? manualPrompt = null,
            CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct)
                ?? throw new KeyNotFoundException($"Brief của project {projectId} không tìm thấy.");

            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã xác nhận, không thể phân tích lại.");

            var result = await _analysisEngine.AnalyzeAsync(
                brief.OverviewInput,
                brief.ViewpointInput,
                brief.KeyDataInput,
                brief.ImportedRawText,
                brief.SheetImportedText,
                manualPrompt,
                ct);

            brief.TopicOutput = result.Topic;
            brief.MainMessageOutput = result.MainMessage;
            brief.HighlightDataOutput = result.HighlightData;
            brief.SuggestedKeywordsJson = System.Text.Json.JsonSerializer.Serialize(result.SuggestedKeywords);
            brief.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return brief;
        }

        public async Task<ContentBriefEntity> SyncFromSheetAsync(Guid projectId, string sheetUrl, CancellationToken ct = default)
        {
            var brief = await GetOrCreateAsync(projectId, ct);
            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã được xác nhận, không thể sửa.");

            var sheetResult = await _sheetImportEngine.FetchAsync(sheetUrl, ct);

            brief.ExternalSheetUrl = sheetUrl;
            brief.SheetImportedText = sheetResult.RawTextContent; // tách riêng khỏi ImportedRawText
            brief.LastSheetSyncTime = sheetResult.FetchedAt;
            brief.ImportSource = CImportContentSource.ExternalLink;
            brief.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return brief;
        }

        public async Task ConfirmAsync(Guid projectId, CancellationToken ct = default)
        {
            var brief = await _db.ContentBriefs
                .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct)
                ?? throw new KeyNotFoundException($"Brief của project {projectId} không tìm thấy.");

            if (string.IsNullOrWhiteSpace(brief.TopicOutput))
                throw new InvalidOperationException("Phải phân tích AI trước khi xác nhận Brief.");

            brief.IsConfirmed = true;
            brief.ConfirmedAt = DateTimeOffset.UtcNow;
            brief.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            // Đánh dấu step hiện tại hoàn thành và chuyển sang step tiếp theo
            await _projectService.UpdateStepStatusAsync(projectId,
                CProjectStepNumber.ContentBrief, CProjectStepStatus.Completed,
                brief.TopicOutput, ct);
            await _projectService.AdvanceStepAsync(projectId, ct);
        }
    }
}
