namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Tập hợp keyword đã được dịch và mở rộng song ngữ (Anh - Việt).
    /// </summary>
    public sealed record TranslatedKeywordSet(
        IReadOnlyList<string> OriginalKeywords,
        IReadOnlyList<string> VietnameseKeywords,
        IReadOnlyList<string> EnglishKeywords,
        IReadOnlyList<string> CombinedKeywords);

    /// <summary>
    /// Engine dịch và mở rộng keyword phục vụ cho việc crawl tin tức từ cả nguồn nội địa và quốc tế.
    /// </summary>
    public interface IKeywordTranslationEngine
    {
        /// <summary>
        /// Nhận vào danh sách keyword (có thể lẫn lộn tiếng Việt / tiếng Anh)
        /// và trả về các tập keyword tiếng Việt, tiếng Anh và kết hợp.
        /// </summary>
        Task<TranslatedKeywordSet> TranslateAndExpandAsync(
            IEnumerable<string> keywords,
            CancellationToken ct = default);
    }
}
