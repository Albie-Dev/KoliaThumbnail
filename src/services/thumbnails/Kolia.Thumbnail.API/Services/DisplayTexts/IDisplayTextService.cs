using Kolia.Thumbnail.API.Data.Entities.DisplayTexts;

namespace Kolia.Thumbnail.API.Services.DisplayTexts
{
    /// <summary>
    /// Tạo và quản lý Display Text — chữ hiển thị trên thumbnail (Phần 4.1).
    /// </summary>
    public interface IDisplayTextService
    {
        /// <summary>
        /// Tạo yêu cầu Display Text mới và gọi AI sinh các phương án.
        /// newsItemIds: danh sách id bản tin đã chọn ở Phần 2 (ít nhất 1).
        /// </summary>
        Task<DisplayTextRequestEntity> GenerateAsync(Guid projectId,
            IEnumerable<Guid> newsItemIds,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách request + options của project.
        /// </summary>
        Task<IReadOnlyList<DisplayTextRequestEntity>> GetByProjectAsync(Guid projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Tick chọn/bỏ chọn 1 option Display Text để dùng ở Phần 4.2.
        /// </summary>
        Task SetSelectedAsync(Guid displayTextOptionId, bool isSelected,
            CancellationToken ct = default);
    }
}
