using Kolia.Thumbnail.API.Data.Entities.Briefs;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.Briefs
{
    /// <summary>
    /// Quản lý Content Brief (Phần 1).
    /// </summary>
    public interface IContentBriefService
    {
        /// <summary>
        /// Lấy hoặc tạo mới Content Brief cho project.
        /// </summary>
        Task<ContentBriefEntity> GetOrCreateAsync(Guid projectId, CancellationToken ct = default);

        /// <summary>
        /// Lưu nội dung nhập tay (OverviewInput, ViewpointInput, KeyDataInput).
        /// </summary>
        Task SaveManualInputAsync(Guid projectId, string overview, string viewpoint,
            string keyData, CancellationToken ct = default);

        /// <summary>
        /// Import dữ liệu từ text paste, file, hoặc external link.
        /// </summary>
        Task ImportAsync(Guid projectId, CImportContentSource source,
            string? rawText, string? fileUrl, string? externalLink,
            CancellationToken ct = default);

        /// <summary>
        /// Import dữ liệu từ PasteText và tự động gọi AI Agent để phân tích,
        /// trích xuất toàn bộ 6 trường nội dung (overview, viewpoint, keyData,
        /// topic, mainMessage, highlightData) ngay trong một lần gọi.
        /// </summary>
        Task<ContentBriefEntity> ImportAndAnalyzeFromPasteAsync(Guid projectId, string rawText,
            CancellationToken ct = default);

        /// <summary>
        /// Upload file text và tự động gọi AI Agent để phân tích,
        /// trích xuất toàn bộ 6 trường nội dung — hỗ trợ .txt, .csv, .md, .json, .xml, ...
        /// </summary>
        Task<ContentBriefEntity> ImportFileAndAnalyzeAsync(Guid projectId, IFormFile file,
            CancellationToken ct = default);

        /// <summary>
        /// Gọi AI để phân tích brief và sinh ra TopicOutput, MainMessageOutput, HighlightDataOutput.
        /// </summary>
        Task<ContentBriefEntity> AnalyzeWithAIAsync(Guid projectId, string? manualPrompt = null, CancellationToken ct = default);

        /// <summary>
        /// Xác nhận (khóa) brief — IsConfirmed = true. Sau đó không thể sửa.
        /// </summary>
        Task ConfirmAsync(Guid projectId, CancellationToken ct = default);

        /// <summary>
        /// Đồng bộ nội dung từ Google Sheet nội bộ (team phân tích) vào Content Brief.
        /// Lưu dữ liệu sheet vào SheetImportedText, cập nhật ExternalSheetUrl và LastSheetSyncTime.
        /// </summary>
        Task<ContentBriefEntity> SyncFromSheetAsync(Guid projectId, string sheetUrl, CancellationToken ct = default);
    }
}
