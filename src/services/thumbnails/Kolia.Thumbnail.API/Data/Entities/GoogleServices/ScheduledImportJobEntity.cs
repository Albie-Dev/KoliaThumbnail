using Kolia.Thumbnail.API.Attributes;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.GoogleServices
{
    /// <summary>
    /// Scheduled Import Job — lên lịch import nội dung từ Google Sheets/Docs.
    /// Khi job chạy thành công, tự động tạo Project và Content Brief.
    /// </summary>
    public class ScheduledImportJobEntity : BaseEntity
    {
        /// <summary>Tên job do người dùng đặt</summary>
        [Queryable(
            Searchable = true,
            Sortable = true
        )]
        public string Name { get; set; } = null!;

        /// <summary>Mô tả job</summary>
        public string? Description { get; set; }

        /// <summary>Loại nguồn: Google Sheets hoặc Google Docs</summary>
        public CGoogleServiceType SourceType { get; set; }

        /// <summary>URL Google Sheets hoặc Google Docs</summary>
        public string SourceUrl { get; set; } = null!;

        /// <summary>
        /// Cron expression cho lịch chạy lặp (enterprise scheduling).
        /// VD: "*/5 * * * *" = mỗi 5 phút, "0 9 * * 1" = 9h sáng thứ 2 hàng tuần.
        /// Nếu có CronExpression, ScheduledAt bị bỏ qua.
        /// </summary>
        [Queryable(
            Searchable = true,
            Sortable = true
        )]
        public string? CronExpression { get; set; }

        /// <summary>Mô tả cron bằng tiếng Việt để hiển thị (VD: "Mỗi 5 phút", "9h sáng thứ 2 hàng tuần")</summary>
        public string? CronDescription { get; set; }

        /// <summary>
        /// Múi giờ của cron expression (VD: "Asia/Ho_Chi_Minh", "UTC").
        /// Mặc định UTC. Dùng để tính toán lịch chạy chính xác theo giờ địa phương.
        /// </summary>
        public string? TimeZone { get; set; }

        /// <summary>
        /// Thời điểm lên lịch chạy một lần.
        /// Chỉ dùng khi CronExpression = null.
        /// Nếu cả CronExpression và ScheduledAt đều null, job chạy ngay khi tạo.
        /// </summary>
        [Queryable(
            Filterable = true,
            RangeFilterable = true,
            Sortable = true
        )]
        public DateTimeOffset? ScheduledAt { get; set; }

        /// <summary>Thời điểm job thực sự bắt đầu chạy</summary>
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary>Thời điểm job hoàn thành (thành công hoặc thất bại)</summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>Trạng thái hiện tại của job</summary>
        [Queryable(
            Filterable = true,
            Sortable = true
        )]
        public CJobScheduleStatus Status { get; set; } = CJobScheduleStatus.Pending;

        /// <summary>Thông báo lỗi nếu job thất bại</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Log chi tiết quá trình chạy job (JSON array of LogEntry).
        /// </summary>
        public string? LogJson { get; set; }

        /// <summary>
        /// Project ID được tạo tự động sau khi job chạy thành công.
        /// </summary>
        public Guid? CreatedProjectId { get; set; }

        /// <summary>
        /// ID của Content Brief được tạo/sau khi xử lý.
        /// </summary>
        public Guid? CreatedBriefId { get; set; }

        /// <summary>
        /// Nội dung đã import để gửi lên AI (lưu lại để debug).
        /// </summary>
        public string? ImportedContent { get; set; }

        /// <summary>
        /// Số lần thử lại.
        /// </summary>
        [Queryable(
            Filterable = true,
            Sortable = true
        )]
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Số lần thử lại tối đa.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>Id của Google Service Account sử dụng</summary>
        public Guid GoogleServiceAccountId { get; set; }

        /// <summary>Navigation property</summary>
        public virtual GoogleServiceAccountEntity GoogleServiceAccount { get; set; } = null!;
    }
}
