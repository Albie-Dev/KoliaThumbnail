using Kolia.Thumbnail.API.Data.Entities.VideoTitles;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.VideoTitles
{
    /// <summary>
    /// Tạo và quản lý Video Title (Phần 5).
    /// </summary>
    public interface IVideoTitleService
    {
        /// <summary>
        /// Tạo yêu cầu generate titles mới.
        /// requestedCount: chỉ nhận 3, 5, 7 hoặc 10.
        /// </summary>
        Task<VideoTitleRequestEntity> GenerateAsync(
            Guid projectId,
            IEnumerable<Guid> selectedThumbnailIds,
            IEnumerable<Guid> selectedNewsItemIds,
            CTitleStyle style,
            string keywordsRaw,
            int requestedCount,
            CancellationToken ct = default);

        /// <summary>
        /// Gen lại thường — tăng GenerationRound, không dùng feedback cũ.
        /// </summary>
        Task<VideoTitleRequestEntity> RegenerateAsync(Guid videoTitleRequestId,
            CancellationToken ct = default);

        /// <summary>
        /// Gen lại theo feedback — tăng GenerationRound, đưa feedbackText vào prompt.
        /// </summary>
        Task<VideoTitleRequestEntity> RegenerateWithFeedbackAsync(Guid videoTitleRequestId,
            string feedbackText, CancellationToken ct = default);

        /// <summary>
        /// Tick/bỏ tick 1 title option. Cho phép nhiều IsSelected = true đồng thời.
        /// </summary>
        Task SetSelectedAsync(Guid videoTitleOptionId, bool isSelected,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả VideoTitleRequests của project.
        /// </summary>
        Task<IReadOnlyList<VideoTitleRequestEntity>> GetByProjectAsync(Guid projectId,
            CancellationToken ct = default);
    }
}
