using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Services.Projects
{
    /// <summary>
    /// Guard kiểm tra tiến độ project trước khi cho phép thực hiện các bước nghiệp vụ.
    /// Ở chế độ WarningOnly (mặc định), chỉ log warning — không chặn.
    /// Khi FE đã cập nhật đồng bộ, chuyển sang chế độ chặn cứng (throw BusinessException).
    /// </summary>
    public interface IProjectStepGuard
    {
        /// <summary>
        /// Throw BusinessException nếu project chưa hoàn tất tới bước yêu cầu.
        /// Ở chế độ WarningOnly: log warning + return, không throw.
        /// </summary>
        Task EnsureStepReachedAsync(Guid projectId, CProjectStepNumber requiredStep, CancellationToken ct = default);

        /// <summary>
        /// Bật/tắt chế độ chặn cứng. Mặc định = false (chỉ warning).
        /// Gọi từ Admin API khi FE đã sẵn sàng.
        /// </summary>
        bool HardBlockEnabled { get; set; }
    }
}
