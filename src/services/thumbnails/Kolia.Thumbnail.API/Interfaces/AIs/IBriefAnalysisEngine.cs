namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả phân tích Content Brief từ AI.
    /// </summary>
    public record BriefAnalysisResult(
        string Topic,
        string MainMessage,
        string HighlightData,
        IReadOnlyList<string> SuggestedKeywords);

    /// <summary>
    /// Engine AI xử lý Content Brief (Phần 1).
    /// </summary>
    public interface IBriefAnalysisEngine
    {
        Task<BriefAnalysisResult> AnalyzeAsync(
            string overview, string viewpoint, string keyData,
            string? importedRawText,
            string? externalSheetContent,
            string? manualPrompt = null,
            CancellationToken ct = default);
    }
}
