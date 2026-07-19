namespace Kolia.Thumbnail.API.Data.Entities.Characters
{
    /// <summary>
    /// Ảnh của nhân vật với nhãn biểu cảm và góc chụp.
    /// Dùng làm tham khảo phong cách khi tạo thumbnail ở Phần 4.2.
    /// </summary>
    public class CharacterImageEntity : BaseEntity
    {
        /// <summary>
        /// Id nhân vật chủ quản
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// URL ảnh nhân vật đã upload
        /// </summary>
        public string ImageUrl { get; set; } = null!;

        /// <summary>
        /// Nhãn biểu cảm, vd "lo lắng", "ngạc nhiên", "tự tin"
        /// </summary>
        public string? ExpressionLabel { get; set; }

        /// <summary>
        /// Nhãn góc chụp, vd "góc gần", "góc xa" (tham khảo phong cách story card)
        /// </summary>
        public string? AngleLabel { get; set; }

        /// <summary>
        /// True nếu đây là ảnh đại diện chính để hiển thị mini-preview trong danh sách
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        // Navigation
        public virtual CharacterEntity Character { get; set; } = null!;
    }
}
