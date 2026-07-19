using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;

namespace Kolia.Thumbnail.API.Data.Entities.CompletePackages
{
    /// <summary>
    /// Bộ hoàn chỉnh (Complete Package) — kết quả cuối của toàn bộ quy trình 5 bước.
    /// Lưu lại lịch sử, xem lại được. Một project có thể có nhiều complete package.
    /// </summary>
    public class CompletePackageEntity : BaseEntity
    {
        /// <summary>
        /// Id project chủ quản
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id ảnh thumbnail đã duyệt và chọn làm ảnh chính
        /// </summary>
        public Guid SelectedThumbnailId { get; set; }

        /// <summary>
        /// Snapshot Display Text tại thời điểm xác nhận package (bất biến)
        /// </summary>
        public string DisplayTextSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm xác nhận bộ hoàn chỉnh
        /// </summary>
        public DateTimeOffset ConfirmedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
        public virtual GeneratedThumbnailEntity SelectedThumbnail { get; set; } = null!;
        public virtual ICollection<CompletePackageTitleEntity> SelectedTitles { get; set; } = new List<CompletePackageTitleEntity>();
    }
}
