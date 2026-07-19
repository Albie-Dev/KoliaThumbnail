using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả tạo Video Titles.
    /// </summary>
    public record VideoTitleGenerationResult(IReadOnlyList<string> Titles);

    /// <summary>
    /// Engine AI tạo Video Title (Phần 5).
    /// </summary>
    public interface IVideoTitleGenerationEngine
    {
        /// <summary>
        /// Generate mới (không có feedback).
        /// </summary>
        Task<VideoTitleGenerationResult> GenerateAsync(
            string builtPromptText,
            CTitleStyle style,
            int requestedCount,
            CancellationToken ct = default);

        /// <summary>
        /// Generate theo feedback.
        /// </summary>
        Task<VideoTitleGenerationResult> GenerateWithFeedbackAsync(
            string builtPromptText,
            CTitleStyle style,
            int requestedCount,
            string feedbackText,
            CancellationToken ct = default);

        /// <summary>
        /// Xây dựng prompt tổng hợp từ thumbnail context và news context.
        /// Trả về chuỗi prompt để lưu vào BuiltPromptText.
        /// </summary>
        Task<string> BuildPromptAsync(
            IEnumerable<string> thumbnailDisplayTexts,
            IEnumerable<string> newsSummaries,
            string topicContext,
            CancellationToken ct = default);
    }
}
