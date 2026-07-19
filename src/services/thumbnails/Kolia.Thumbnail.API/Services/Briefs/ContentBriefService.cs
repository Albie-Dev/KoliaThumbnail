using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Briefs;
using Kolia.Thumbnail.API.Engines;
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

        /// <summary>
        /// Các đuôi file text được hỗ trợ upload.
        /// </summary>
        private static readonly HashSet<string> AllowedTextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".md", ".json", ".xml", ".yaml", ".yml",
            ".log", ".ini", ".cfg", ".conf", ".env", ".text", ".tsv"
        };

        /// <summary>
        /// Dung lượng file tối đa: 10MB
        /// </summary>
        private const long MaxFileSize = 10 * 1024 * 1024;

        public async Task<ContentBriefEntity> ImportFileAndAnalyzeAsync(Guid projectId,
            IFormFile file, CancellationToken ct = default)
        {
            // Validate file
            if (file.Length == 0)
                throw new ArgumentException("File rỗng.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"Dung lượng file tối đa {MaxFileSize / 1024 / 1024}MB. File hiện tại: {file.Length / 1024.0 / 1024.0:F2}MB.");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedTextExtensions.Contains(ext))
                throw new ArgumentException($"Đuôi file '{ext}' không được hỗ trợ. Các định dạng hỗ trợ: {string.Join(", ", AllowedTextExtensions)}");

            // Đọc file thành base64 để gửi trực tiếp lên AI (inline_data)
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, ct);
                fileBytes = ms.ToArray();
            }

            var mimeType = GetMimeTypeForExtension(ext);
            var base64Content = Convert.ToBase64String(fileBytes);

            var brief = await GetOrCreateAsync(projectId, ct);
            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã được xác nhận, không thể sửa.");

            // Lưu thông tin file
            brief.ImportSource = CImportContentSource.File;
            brief.ImportedRawText = $"[File: {file.FileName} ({file.Length} bytes)]";
            brief.LastModificationTime = DateTimeOffset.UtcNow;

            // Gửi file trực tiếp lên AI Agent -> AI tự đọc và phân tích 6 trường
            var fileAttachment = new ChatFileAttachment
            {
                FileName = file.FileName,
                Base64Content = base64Content,
                MimeType = mimeType
            };

            var result = await _analysisEngine.AnalyzeFromFilesAsync(
                new List<ChatFileAttachment> { fileAttachment }, ct);

            // Ghi đè tất cả 6 trường nội dung
            brief.OverviewInput = result.OverviewInput;
            brief.ViewpointInput = result.ViewpointInput;
            brief.KeyDataInput = result.KeyDataInput;
            brief.TopicOutput = result.Topic;
            brief.MainMessageOutput = result.MainMessage;
            brief.HighlightDataOutput = result.HighlightData;
            brief.SuggestedKeywordsJson = System.Text.Json.JsonSerializer.Serialize(result.SuggestedKeywords);

            await _db.SaveChangesAsync(ct);
            return brief;
        }

        private static string GetMimeTypeForExtension(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".md" => "text/markdown",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".yaml" or ".yml" => "text/yaml",
                ".log" => "text/plain",
                ".ini" or ".cfg" or ".conf" => "text/plain",
                ".env" => "text/plain",
                ".text" => "text/plain",
                ".tsv" => "text/tab-separated-values",
                _ => "application/octet-stream"
            };
        }

        public async Task<ContentBriefEntity> ImportAndAnalyzeFromPasteAsync(Guid projectId,
            string rawText, CancellationToken ct = default)
        {
            var brief = await GetOrCreateAsync(projectId, ct);
            if (brief.IsConfirmed)
                throw new InvalidOperationException("Brief đã được xác nhận, không thể sửa.");

            // Bước 1: Lưu raw text
            brief.ImportSource = CImportContentSource.PasteText;
            brief.ImportedRawText = rawText;
            brief.LastModificationTime = DateTimeOffset.UtcNow;

            // Bước 2: Gọi AI Agent phân tích văn bản paste -> trích xuất 6 trường
            var result = await _analysisEngine.AnalyzeFromPastedTextAsync(rawText, ct);

            // Bước 3: Ghi đè tất cả 6 trường nội dung
            brief.OverviewInput = result.OverviewInput;
            brief.ViewpointInput = result.ViewpointInput;
            brief.KeyDataInput = result.KeyDataInput;
            brief.TopicOutput = result.Topic;
            brief.MainMessageOutput = result.MainMessage;
            brief.HighlightDataOutput = result.HighlightData;
            brief.SuggestedKeywordsJson = System.Text.Json.JsonSerializer.Serialize(result.SuggestedKeywords);

            await _db.SaveChangesAsync(ct);
            return brief;
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
