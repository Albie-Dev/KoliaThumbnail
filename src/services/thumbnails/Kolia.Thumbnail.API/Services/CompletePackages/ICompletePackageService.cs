using Kolia.Thumbnail.API.Data.Entities.CompletePackages;

namespace Kolia.Thumbnail.API.Services.CompletePackages
{
    /// <summary>
    /// Quản lý Complete Package — bộ hoàn chỉnh cuối quy trình.
    /// </summary>
    public interface ICompletePackageService
    {
        /// <summary>
        /// Xác nhận bộ hoàn chỉnh: chọn 1 thumbnail + nhiều title options.
        /// Lưu snapshot display text tại thời điểm xác nhận (bất biến về sau).
        /// </summary>
        Task<CompletePackageEntity> ConfirmAsync(
            Guid projectId,
            Guid selectedThumbnailId,
            IEnumerable<Guid> selectedTitleOptionIds,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả complete packages của 1 project.
        /// </summary>
        Task<IReadOnlyList<CompletePackageEntity>> GetByProjectAsync(Guid projectId,
            CancellationToken ct = default);
    }
}
