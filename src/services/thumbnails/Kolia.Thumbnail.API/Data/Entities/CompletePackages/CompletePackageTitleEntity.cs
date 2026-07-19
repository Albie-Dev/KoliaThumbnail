using Kolia.Thumbnail.API.Data.Entities.VideoTitles;

namespace Kolia.Thumbnail.API.Data.Entities.CompletePackages
{
    /// <summary>
    /// Bảng nối n-n: CompletePackage ↔ VideoTitleOption (các title đã chọn trong bộ hoàn chỉnh).
    /// </summary>
    public class CompletePackageTitleEntity
    {
        /// <summary>
        /// Id bộ hoàn chỉnh
        /// </summary>
        public Guid CompletePackageId { get; set; }

        /// <summary>
        /// Id option title đã chọn
        /// </summary>
        public Guid VideoTitleOptionId { get; set; }

        // Navigation
        public virtual CompletePackageEntity CompletePackage { get; set; } = null!;
        public virtual VideoTitleOptionEntity VideoTitleOption { get; set; } = null!;
    }
}
