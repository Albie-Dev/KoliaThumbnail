using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration
{
    /// <summary>
    /// Một ảnh thumbnail đã được generate.
    /// Hỗ trợ version chain qua ParentGeneratedThumbnailId (A.1 #3 — sửa không mất bản cũ).
    /// Khi sửa: tạo bản ghi MỚI với ParentGeneratedThumbnailId = id ảnh đang sửa, KHÔNG UPDATE bản cũ.
    /// </summary>
    public class GeneratedThumbnailEntity : BaseEntity
    {
        /// <summary>
        /// Id set ảnh cha
        /// </summary>
        public Guid GeneratedThumbnailSetId { get; set; }

        /// <summary>
        /// Vị trí ảnh trong set (ảnh thứ mấy, bắt đầu từ 1)
        /// </summary>
        public int VariantIndex { get; set; }

        /// <summary>
        /// Id ảnh cha trong version chain. Null nếu là bản gốc (v1).
        /// </summary>
        public Guid? ParentGeneratedThumbnailId { get; set; }

        /// <summary>
        /// Số version trong chain (v1=1, v2=2...). Tăng dần khi sửa.
        /// </summary>
        public int VersionNumber { get; set; } = 1;

        /// <summary>
        /// URL ảnh thumbnail đã generate
        /// </summary>
        public string ImageUrl { get; set; } = null!;

        /// <summary>
        /// Snapshot Display Text tại thời điểm tạo ảnh.
        /// Không thay đổi dù DisplayTextOption bị sửa/xóa sau này.
        /// </summary>
        public string DisplayTextSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot tên nhân vật tại thời điểm tạo (null nếu không dùng nhân vật)
        /// </summary>
        public string? CharacterSnapshotName { get; set; }

        /// <summary>
        /// Công cụ sửa cuối cùng được dùng (chỉ khác null khi đây là bản sửa)
        /// </summary>
        public CThumbnailEditTool? LastEditTool { get; set; }

        /// <summary>
        /// Nội dung yêu cầu sửa cuối (mô tả user nhập khi sửa)
        /// </summary>
        public string? LastEditRequestText { get; set; }

        /// <summary>
        /// True khi ảnh đã được team duyệt
        /// </summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Thời điểm duyệt ảnh
        /// </summary>
        public DateTimeOffset? ApprovedAt { get; set; }

        /// <summary>
        /// True khi ảnh đã được download
        /// </summary>
        public bool WasDownloaded { get; set; } = false;

        /// <summary>
        /// True khi ảnh đã được đẩy sang Phần 5 để tạo Video Title
        /// </summary>
        public bool IsPushedToTitleStep { get; set; } = false;

        // Navigation
        public virtual GeneratedThumbnailSetEntity GeneratedThumbnailSet { get; set; } = null!;
        public virtual GeneratedThumbnailEntity? ParentGeneratedThumbnail { get; set; }
        public virtual ICollection<GeneratedThumbnailEntity> ChildVersions { get; set; } = new List<GeneratedThumbnailEntity>();
    }
}
