namespace Kolia.Thumbnail.API.Data.Entities.Projects
{
    public class StepDefinitionEntity : BaseEntity
    {
        /// <summary>Dùng cho luồng nghiệp vụ bình thường (admin tạo step mới qua UI, nếu có sau này).</summary>
        public StepDefinitionEntity() { }

        /// <summary>Dùng riêng cho seed data (HasData) — Id cố định để migration snapshot ổn định.</summary>
        public StepDefinitionEntity(Guid id)
        {
            Id = id;
            CreationTime = SeedConstants.FixedSeedTimestamp;
        }


        /// <summary>
        /// "video_content", "news", "thumbnail_reference", "thumbnail_reference_library", ...
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// "Nội dung video", "Tin tức", ...
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Thứ tự hiển thị trong CÙNG một cấp (giữa các anh em), KHÔNG mang ý nghĩa cấp bậc.
        /// Cấp gốc: 1,2,3,4,5,6. Con của bước 4 (4.1, 4.2): SortOrder = 1,2.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Nhãn hiển thị đầy đủ, gán tay lúc seed — KHÔNG suy ra từ SortOrder để tránh
        /// magic number khi cấu trúc sâu hơn 2 cấp hoặc chèn thêm bước ở giữa.
        /// Ví dụ: "1", "2", "3", "3.1", "4", "4.1", "4.2", "5", "6"
        /// </summary>
        public string DisplayCode { get; set; } = null!;

        /// <summary>
        /// True nếu bước này thực sự sinh ra 1 ProjectStep để track trạng thái/nội dung.
        /// False nếu chỉ là bước "nhóm" cho UI (vd bước 3, 4 chỉ là container chứa
        /// con 3.1 / 4.1, 4.2 — bản thân bước cha không có nội dung riêng để làm).
        /// </summary>
        public bool IsTrackable { get; set; } = true;

        /// <summary>
        /// null nếu là bước gốc (1,2,3,4,5,6)
        /// </summary>
        public Guid? ParentId { get; set; }
        public virtual StepDefinitionEntity? Parent { get; set; } = null;
        public ICollection<StepDefinitionEntity> Children { get; set; } = new List<StepDefinitionEntity>();
    }
}