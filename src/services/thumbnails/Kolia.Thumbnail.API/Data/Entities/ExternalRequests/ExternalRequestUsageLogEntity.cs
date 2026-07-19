using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.ExternalRequests
{
    /// <summary>
    /// Log sử dụng tài nguyên theo ngày — phục vụ dựng bảng ước tính chi phí cho sếp (A.1 #4).
    /// Đơn giá đọc từ appsettings, không hard-code vào đây.
    /// </summary>
    public class ExternalRequestUsageLogEntity : BaseEntity
    {
        /// <summary>
        /// Loại AI Provider dùng (null nếu là Social Provider)
        /// </summary>
        public CAIProviderType? AIProviderType { get; set; }

        /// <summary>
        /// Loại Social Provider dùng (null nếu là AI Provider)
        /// </summary>
        public CSocialMediaProviderType? SocialMediaProviderType { get; set; }

        /// <summary>
        /// Số lượng request trong ngày
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Ước tính token đã dùng (null với Social Provider)
        /// </summary>
        public long? EstimatedTokenUsage { get; set; }

        /// <summary>
        /// Ước tính chi phí USD (đọc đơn giá từ config, không hard-code)
        /// </summary>
        public decimal? EstimatedCostUsd { get; set; }

        /// <summary>
        /// Ngày ghi nhận (gộp theo ngày để dựng báo cáo)
        /// </summary>
        public DateOnly RecordedDate { get; set; }
    }
}
