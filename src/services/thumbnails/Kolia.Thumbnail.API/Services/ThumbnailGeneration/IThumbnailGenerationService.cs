using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.ThumbnailGeneration
{
    /// <summary>
    /// Tạo và quản lý thumbnail AI (Phần 4.2).
    /// </summary>
    public interface IThumbnailGenerationService
    {
        /// <summary>
        /// Build prompt text từ các tham số đầu vào, bao gồm thông tin khoá/không khoá
        /// (character identity, reference factors, display text, changes request).
        /// Dùng cho chức năng "Xuất prompt" — cho phép user sửa tay prompt trước khi generate.
        /// </summary>
        Task<string> BuildPromptAsync(
            Guid projectId,
            IEnumerable<Guid> displayTextOptionIds,
            IEnumerable<Guid> referenceLibraryItemIds,
            Guid? characterId,
            string changesRequestText,
            string ratio,
            string resolution,
            CancellationToken ct = default);

        /// <summary>
        /// Tạo 1 yêu cầu generate mới + gọi AI.
        /// Mỗi lần gọi = 1 GeneratedThumbnailSet mới, KHÔNG ghi đè set cũ.
        /// requestedCount: số ảnh yêu cầu (1–5), service PHẢI trả đúng số này.
        /// </summary>
        Task<GeneratedThumbnailSetEntity> GenerateAsync(
            Guid projectId,
            IEnumerable<Guid> displayTextOptionIds,
            IEnumerable<Guid> referenceLibraryItemIds,
            Guid? characterId,
            string changesRequestText,
            string ratio,
            string resolution,
            int requestedCount,
            string? overridePromptText = null,
            CancellationToken ct = default);

        /// <summary>
        /// Sửa 1 ảnh đã generate — tạo bản ghi GeneratedThumbnailEntity mới (version chain).
        /// KHÔNG UPDATE bản gốc, chỉ INSERT bản mới với ParentGeneratedThumbnailId trỏ về bản cũ.
        /// secondaryReferenceLibraryItemId: dùng khi editTool == Style — ref thumbnail khác.
        /// secondaryCharacterImageId: dùng khi editTool == Avatar — ảnh biểu cảm đã upload.
        /// </summary>
        Task<GeneratedThumbnailEntity> EditAsync(
            Guid generatedThumbnailId,
            CThumbnailEditTool editTool,
            string editRequestText,
            Guid? secondaryReferenceLibraryItemId,
            Guid? secondaryCharacterImageId,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả GeneratedThumbnailSets của project theo thứ tự SetIndex DESC.
        /// </summary>
        Task<IReadOnlyList<GeneratedThumbnailSetEntity>> GetSetsAsync(Guid projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Duyệt 1 ảnh — IsApproved = true.
        /// </summary>
        Task ApproveAsync(Guid generatedThumbnailId, CancellationToken ct = default);

        /// <summary>
        /// Đẩy ảnh đã duyệt sang Phần 5 — IsPushedToTitleStep = true.
        /// </summary>
        Task PushToTitleStepAsync(Guid generatedThumbnailId, CancellationToken ct = default);

        /// <summary>
        /// Đánh dấu đã download ảnh — WasDownloaded = true.
        /// </summary>
        Task MarkDownloadedAsync(Guid generatedThumbnailId, CancellationToken ct = default);
    }
}
