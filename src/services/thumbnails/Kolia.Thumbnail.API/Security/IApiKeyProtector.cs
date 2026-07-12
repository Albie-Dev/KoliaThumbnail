namespace Kolia.Thumbnail.API.Security
{
    public interface IApiKeyProtector
    {
        /// <summary>Mã hoá ApiKey trước khi lưu vào DB.</summary>
        string Protect(string plainApiKey);

        /// <summary>Giải mã ApiKey từ DB.</summary>
        string Unprotect(string protectedApiKey);

        /// <summary>
        /// Giải mã rồi che giấu ApiKey (chỉ hiện 4 ký tự cuối) để hiển thị an toàn ở FE.
        /// Trả về "(không thể giải mã)" nếu payload hỏng.
        /// </summary>
        string MaskFromProtected(string protectedApiKey);

        /// <summary>Băm ApiKey để so sánh phát hiện thay đổi (SHA256 hex).</summary>
        string Hash(string apiKey);
    }
}