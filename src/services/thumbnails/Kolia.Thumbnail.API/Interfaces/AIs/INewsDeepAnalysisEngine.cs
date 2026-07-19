using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả phân tích sâu 4 tầng của 1 bản tin.
    /// </summary>
    public record NewsDeepAnalysisResult(
        IReadOnlyList<string> MacroEventSummary,
        string MarketReactionJson,
        string ExpectationShortTerm,
        string ExpectationLongTerm,
        string SentimentOverviewJson,
        CEmotionTag EmotionTags,
        string EmotionReason,
        bool WasTranslatedFromForeign,
        string? MissingDataNote);

    /// <summary>
    /// Engine AI phân tích sâu 4 tầng cho 1 bản tin (Phần 2 — on-demand).
    /// Mọi field chưa có dữ liệu PHẢI trả về "Chưa rõ", không để trống.
    /// </summary>
    public interface INewsDeepAnalysisEngine
    {
        Task<NewsDeepAnalysisResult> AnalyzeAsync(
            string title, string sourceUrl, string sourceName,
            string fullContentOrSummary,
            CancellationToken ct = default);
    }
}
