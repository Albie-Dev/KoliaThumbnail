using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    /// <summary>
    /// Bước trong quy trình project. Mỗi project luôn có đúng 5 bản ghi (seed khi tạo project).
    /// </summary>
    public class ProjectStepEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Số thứ tự bước (unique cùng ProjectId)
        /// </summary>
        public CProjectStepNumber StepNumber { get; set; }

        /// <summary>
        /// Trạng thái xử lý của bước
        /// </summary>
        public CProjectStepStatus StepStatus { get; set; } = CProjectStepStatus.NotStarted;

        /// <summary>
        /// Tên hiển thị bước, vd "Nội dung video", "Tin tức"...
        /// </summary>
        public string StepName { get; set; } = null!;

        /// <summary>
        /// Tóm tắt output ngắn hiển thị trên dashboard card, vd "Content Brief đã xác nhận"
        /// </summary>
        public string? OutputSummaryText { get; set; }

        /// <summary>
        /// True nếu bước này cần người dùng duyệt thủ công (Phần 4 — Generate)
        /// </summary>
        public bool NeedsApproval { get; set; } = false;

        /// <summary>
        /// Thời điểm được duyệt
        /// </summary>
        public DateTimeOffset? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi chú duyệt của người dùng
        /// </summary>
        public string? ApprovedByNote { get; set; }

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
    }
}
