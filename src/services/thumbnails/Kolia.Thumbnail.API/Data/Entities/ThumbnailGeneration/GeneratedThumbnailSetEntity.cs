namespace Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration
{
    /// <summary>
    /// Tập hợp ảnh được tạo trong một lần bấm "Tạo thumbnail".
    /// Mỗi lần generate = 1 Set mới — KHÔNG ghi đè set trước (A.1 #3).
    /// </summary>
    public class GeneratedThumbnailSetEntity : BaseEntity
    {
        /// <summary>
        /// Id yêu cầu generation cha
        /// </summary>
        public Guid ThumbnailGenerationRequestId { get; set; }

        /// <summary>
        /// Thứ tự set trong project (tăng dần, bắt đầu từ 1)
        /// </summary>
        public int SetIndex { get; set; }

        // Navigation
        public virtual ThumbnailGenerationRequestEntity ThumbnailGenerationRequest { get; set; } = null!;
        public virtual ICollection<GeneratedThumbnailEntity> Variants { get; set; } = new List<GeneratedThumbnailEntity>();
    }
}
