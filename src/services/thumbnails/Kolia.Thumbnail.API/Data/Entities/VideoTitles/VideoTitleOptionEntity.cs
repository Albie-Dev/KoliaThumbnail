namespace Kolia.Thumbnail.API.Data.Entities.VideoTitles
{
    /// <summary>
    /// Một phương án Video Title do AI generate (Phần 5).
    /// Cho phép chọn NHIỀU title cùng lúc — không giới hạn 1 (A.3 fix theo yêu cầu chị Linh).
    /// </summary>
    public class VideoTitleOptionEntity : BaseEntity
    {
        /// <summary>
        /// Id yêu cầu Video Title cha
        /// </summary>
        public Guid VideoTitleRequestId { get; set; }

        /// <summary>
        /// Round generate mà option này được tạo ra
        /// </summary>
        public int GenerationRound { get; set; }

        /// <summary>
        /// Nội dung title, vd "Vàng Sụp 10%: Đừng Mua Vào Lúc Này!"
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// True khi user đã chọn title này.
        /// CHO PHÉP chọn nhiều cùng lúc — IsSelected trên nhiều option = true đồng thời.
        /// </summary>
        public bool IsSelected { get; set; } = false;

        // Navigation
        public virtual VideoTitleRequestEntity VideoTitleRequest { get; set; } = null!;
    }
}
