namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả phân tích sâu 1 thumbnail tham khảo.
    /// </summary>
    public record ThumbnailDeepAnalysisResult(
        string ThumbnailFactorsJson,
        string TitleTextAnalysis,
        string VideoTitleAnalysis,
        string DisplayTextStyleNote);

    /// <summary>
    /// Engine AI phân tích visual và text của thumbnail tham khảo (Phần 3 — on-demand).
    /// </summary>
    public interface IThumbnailAnalysisEngine
    {
        Task<ThumbnailDeepAnalysisResult> AnalyzeAsync(
            string thumbnailImageUrl,
            string videoTitle,
            CancellationToken ct = default);
    }
}
