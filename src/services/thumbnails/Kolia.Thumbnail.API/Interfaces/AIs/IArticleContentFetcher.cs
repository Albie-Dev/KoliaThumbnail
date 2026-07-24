namespace Kolia.Thumbnail.API.Engines.Social
{
    /// <summary>
    /// Kết quả trích xuất nội dung đầy đủ của 1 bài báo từ URL gốc.
    /// </summary>
    public sealed record ArticleContentResult(
        bool Success,
        string? FullText,       // Nội dung thân bài, đã strip HTML, giới hạn độ dài (xem ArticleContentFetcher)
        int CharacterCount,
        string? FailureReason); // null nếu Success = true

    /// <summary>
    /// Fetcher lấy full-text bài báo (khác với RSS description chỉ 1-2 câu).
    /// Dùng riêng cho bước "Phân tích sâu" — KHÔNG gọi cho toàn bộ batch crawl
    /// vì chi phí mạng cao, chỉ gọi on-demand cho từng NewsItem được chọn.
    /// </summary>
    public interface IArticleContentFetcher
    {
        Task<ArticleContentResult> FetchFullTextAsync(string url, CancellationToken ct = default);
    }
}
