using Kolia.Thumbnail.API.Engines;

namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả phân tích Content Brief từ AI (luồng nhập tay / có sẵn dữ liệu).
    /// </summary>
    public record BriefAnalysisResult(
        string Topic,
        string MainMessage,
        string HighlightData,
        IReadOnlyList<string> SuggestedKeywords);

    /// <summary>
    /// Kết quả phân tích từ văn bản paste — trả về đầy đủ 6 trường nội dung.
    /// </summary>
    public record BriefAnalysisFromPasteResult(
        string OverviewInput,
        string ViewpointInput,
        string KeyDataInput,
        string Topic,
        string MainMessage,
        string HighlightData,
        IReadOnlyList<string> SuggestedKeywords);

    /// <summary>
    /// Engine AI xử lý Content Brief (Phần 1).
    /// </summary>
    public interface IBriefAnalysisEngine
    {
        /// <summary>
        /// Phân tích brief từ dữ liệu có sẵn (manual + imported + sheet).
        /// </summary>
        Task<BriefAnalysisResult> AnalyzeAsync(
            string overview, string viewpoint, string keyData,
            string? importedRawText,
            string? externalSheetContent,
            string? manualPrompt = null,
            CancellationToken ct = default);

        /// <summary>
        /// Phân tích văn bản paste (PasteText) — AI tự động trích xuất
        /// toàn bộ 6 trường nội dung (Overview, Viewpoint, KeyData, Topic, MainMessage, HighlightData).
        /// </summary>
        Task<BriefAnalysisFromPasteResult> AnalyzeFromPastedTextAsync(
            string rawText,
            CancellationToken ct = default);

        /// <summary>
        /// Phân tích từ file đính kèm — gửi file trực tiếp lên AI (inline_data)
        /// thay vì đọc text server-side. Hỗ trợ PDF, ảnh, Word, text, ...
        /// </summary>
        Task<BriefAnalysisFromPasteResult> AnalyzeFromFilesAsync(
            List<ChatFileAttachment> files,
            CancellationToken ct = default);
    }
}
