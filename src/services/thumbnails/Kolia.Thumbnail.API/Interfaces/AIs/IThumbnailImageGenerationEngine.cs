namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>
    /// Kết quả tạo 1 lô ảnh thumbnail AI.
    /// ImageUrls: URL các ảnh đã upload lên storage. Đúng bằng requestedCount.
    /// </summary>
    public record ThumbnailGenerationResult(IReadOnlyList<string> ImageUrls);

    /// <summary>
    /// Engine AI tạo ảnh thumbnail (Phần 4.2).
    /// PHẢI trả đúng requestedCount ảnh, không ít hơn.
    /// </summary>
    public interface IThumbnailImageGenerationEngine
    {
        Task<ThumbnailGenerationResult> GenerateAsync(
            string promptText,
            string ratio,
            string resolution,
            int requestedCount,
            CancellationToken ct = default);

        /// <summary>
        /// Sửa ảnh đã có dựa theo editRequestText (sử dụng image-edit API).
        /// secondaryReferenceImageUrl: dùng cho Style (ref thumbnail khác) và Avatar (ảnh biểu cảm đã upload).
        /// Trả về URL ảnh mới đã upload lên storage.
        /// </summary>
        Task<string> EditAsync(
            string originalImageUrl,
            string editRequestText,
            string? secondaryReferenceImageUrl = null,
            CancellationToken ct = default);
    }
}
