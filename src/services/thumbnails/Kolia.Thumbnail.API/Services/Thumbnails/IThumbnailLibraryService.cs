using Kolia.Thumbnail.API.Data.Entities.Thumbnails;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.Thumbnails
{
    /// <summary>
    /// Tìm kiếm, quản lý và phân tích thumbnail tham khảo (Phần 3).
    /// </summary>
    public interface IThumbnailLibraryService
    {
        /// <summary>
        /// Tìm kiếm thumbnail theo keyword — crawl YouTube/Faceless, lưu vào Library.
        /// Không ghi đè kết quả cũ — thêm vào kho theo từng batch.
        /// </summary>
        Task<ThumbnailSearchRequestEntity> SearchAsync(
            Guid projectId,
            string keyword,
            CThumbnailTimeFilter timeFilter,
            CThumbnailSortFilter sortFilter,
            bool wasSuggestedFromNews = false,
            CancellationToken ct = default);

        /// <summary>
        /// Import thủ công thumbnail qua link video ngoài.
        /// </summary>
        Task<ThumbnailLibraryItemEntity> ImportManualLinkAsync(Guid projectId, string videoUrl,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy toàn bộ Library của project (theo batch tag để phân nhóm).
        /// </summary>
        Task<IReadOnlyList<ThumbnailLibraryItemEntity>> GetLibraryAsync(Guid projectId,
            bool excludeIrrelevant = true, CancellationToken ct = default);

        /// <summary>
        /// Phân tích sâu 1 thumbnail — tạo ThumbnailAnalysisEntity nếu chưa có.
        /// </summary>
        Task<ThumbnailAnalysisEntity> DeepAnalyzeAsync(Guid libraryItemId,
            CancellationToken ct = default);

        /// <summary>
        /// Cập nhật trạng thái duyệt của 1 item (Pending/Approved/Rejected).
        /// </summary>
        Task SetUserStatusAsync(Guid libraryItemId, CLibraryUserStatus status,
            CancellationToken ct = default);

        /// <summary>
        /// Đánh dấu 1 thumbnail đã chọn để dùng làm mẫu generate (IsChosenForGeneration).
        /// </summary>
        Task SetChosenForGenerationAsync(Guid libraryItemId, bool chosen,
            CancellationToken ct = default);
    }
}
