using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.ExternalRequests
{
    /// <summary>
    /// Hàng đợi request bên ngoài (YouTube API, RSS crawl...) bị rate-limit hoặc lỗi.
    /// Không auto-retry ngay — phải chờ cooldown (A.4 #3).
    /// Admin có thể xem và tự động/thủ công chạy lại qua trang quản lý.
    /// </summary>
    public class ExternalRequestQueueEntity : BaseEntity
    {
        /// <summary>
        /// Id project liên quan. Null nếu là request nền không gắn với project cụ thể.
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Mục đích request: crawl tin, crawl thumbnail, YouTube search, import link
        /// </summary>
        public CExternalRequestPurpose Purpose { get; set; }

        /// <summary>
        /// Trạng thái xử lý hiện tại
        /// </summary>
        public CExternalRequestStatus Status { get; set; } = CExternalRequestStatus.Pending;

        /// <summary>
        /// Tham số gốc để chạy lại request (keyword, filter, url...) — JSON object
        /// </summary>
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>
        /// Id config/API key đã dùng cho request này (phục vụ round-robin thống kê)
        /// </summary>
        public Guid? ProviderConfigurationIdUsed { get; set; }

        /// <summary>
        /// Thông báo lỗi khi thất bại
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Số lần đã thử lại
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Thời điểm dự kiến thử lại tiếp theo (sau cooldown)
        /// </summary>
        public DateTimeOffset? NextRetryAt { get; set; }

        /// <summary>
        /// Thời điểm hoàn thành (Success hoặc Abandoned)
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        // Navigation
        public virtual ProjectEntity? Project { get; set; }
    }
}
