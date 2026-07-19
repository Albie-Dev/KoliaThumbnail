namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả tạo Display Text options.
    /// </summary>
    public record DisplayTextGenerationResult(
        IReadOnlyList<(Guid SourceNewsItemId, string Content)> Options);

    /// <summary>
    /// Engine AI sinh Display Text (chữ hiển thị trên thumbnail) — Phần 4.1.
    /// </summary>
    public interface IDisplayTextGenerationEngine
    {
        /// <summary>
        /// newsSummaries: key = NewsItemId, value = tóm tắt bản tin để AI tham khảo.
        /// topicContext: chủ đề từ ContentBrief.
        /// </summary>
        Task<DisplayTextGenerationResult> GenerateAsync(
            Dictionary<Guid, string> newsSummaries,
            string topicContext,
            CancellationToken ct = default);
    }
}
