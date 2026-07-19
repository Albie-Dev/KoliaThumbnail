namespace Kolia.Thumbnail.API.Data.Entities.Characters
{
    /// <summary>
    /// Nhân vật/Avatar dùng tham khảo phong cách khi tạo thumbnail.
    /// Entity GLOBAL — không gắn ProjectId, dùng lại xuyên mọi project.
    /// </summary>
    public class CharacterEntity : BaseEntity
    {
        /// <summary>
        /// Tên nhân vật, vd "Bác Đoàn", "Kolia Phan", "Không dùng nhân vật"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả đặc điểm nhận dạng của nhân vật (phong cách, trang phục đặc trưng...)
        /// </summary>
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<CharacterImageEntity> Images { get; set; } = new List<CharacterImageEntity>();
    }
}
