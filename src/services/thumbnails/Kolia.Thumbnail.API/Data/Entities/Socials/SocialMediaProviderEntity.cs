using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Socials
{
    /// <summary>
    /// Đại diện cho một nền tảng mạng xã hội được hệ thống hỗ trợ.
    /// </summary>
    public class SocialMediaProviderEntity : BaseEntity
    {
        /// <summary>
        /// Tên hiển thị của nền tảng mạng xã hội.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Tên viết tắt của nền tảng mạng xã hội.
        /// </summary>
        public string ShortName { get; set; } = null!;

        /// <summary>
        /// Đường dẫn gốc (Base URL) của nền tảng mạng xã hội.
        /// </summary>
        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// Đường dẫn hoặc URL đến logo của nền tảng mạng xã hội.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Loại nền tảng mạng xã hội.
        /// </summary>
        public CSocialMediaProviderType ProviderType { get; set; }

        /// <summary>
        /// Danh sách cấu hình chi tiết.
        /// </summary>
        public virtual ICollection<SocialMediaProviderConfigurationEntity> Configurations { get; set; }
            = new List<SocialMediaProviderConfigurationEntity>();
    }
}