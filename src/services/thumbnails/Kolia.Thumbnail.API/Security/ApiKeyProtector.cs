using System.Security.Cryptography;
using System.Text;
using Kolia.Thumbnail.API.Exceptions;
using Microsoft.AspNetCore.DataProtection;

namespace Kolia.Thumbnail.API.Security
{
    /// <summary>
    /// Mã hóa/giải mã ApiKey bằng ASP.NET Core Data Protection API.
    ///
    /// Vì sao dùng Data Protection thay vì tự AES thủ công:
    ///  - Key ring được quản lý, xoay vòng (rotate) tự động, không cần tự viết logic IV/salt.
    ///  - Hỗ trợ lưu key ring tập trung (Azure Blob, Redis, cert...) cho môi trường nhiều instance/pod
    ///    thông qua PersistKeysTo... + ProtectKeysWith... khi đăng ký ở Program.cs.
    ///  - "Purpose string" cho phép cô lập theo mục đích: dù cùng key ring, payload mã hóa cho
    ///    ApiKey KHÔNG thể bị Unprotect bởi 1 protector khác (VD protector cho JWT signing key).
    ///
    /// Versioning: mỗi payload được gắn prefix "v1:" để sau này nếu đổi purpose string hoặc thuật toán,
    /// có thể nhận diện và migrate dữ liệu cũ mà không vỡ dữ liệu đang có trong DB.
    /// </summary>
    public sealed class ApiKeyProtector : IApiKeyProtector
    {
        // Purpose string nên bao gồm tên assembly + mục đích cụ thể để tránh đụng độ
        // nếu sau này có nhiều loại secret khác cũng dùng Data Protection trong cùng app.
        private const string PurposeString = "Kolia.Thumbnail.API.AIConfiguration.ApiKey.v1";
        private const string PayloadVersionPrefix = "v1:";

        private readonly IDataProtector _protector;
        private readonly ILogger<ApiKeyProtector> _logger;

        public ApiKeyProtector(IDataProtectionProvider dataProtectionProvider, ILogger<ApiKeyProtector> logger)
        {
            _protector = dataProtectionProvider.CreateProtector(PurposeString);
            _logger = logger;
        }

        public string Protect(string plainApiKey)
        {
            if (string.IsNullOrWhiteSpace(plainApiKey))
                throw new ArgumentException("ApiKey không được rỗng.", nameof(plainApiKey));

            try
            {
                var cipher = _protector.Protect(plainApiKey);
                return PayloadVersionPrefix + cipher;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "Mã hóa ApiKey thất bại.");
                throw new BusinessException("Không thể mã hóa ApiKey.", ex);
            }
        }

        public string Unprotect(string protectedApiKey)
        {
            if (string.IsNullOrWhiteSpace(protectedApiKey))
                throw new ArgumentException("Payload ApiKey không được rỗng.", nameof(protectedApiKey));

            var cipher = StripVersionPrefix(protectedApiKey);

            try
            {
                return _protector.Unprotect(cipher);
            }
            catch (CryptographicException ex)
            {
                // Xảy ra khi: payload bị chỉnh sửa/hỏng, hoặc key ring đã bị xoay vòng và
                // key cũ dùng để mã hóa payload này không còn tồn tại (VD key ring bị xóa nhầm,
                // hoặc restore DB từ backup cũ hơn key ring hiện tại).
                _logger.LogError(ex, "Giải mã ApiKey thất bại - payload hỏng hoặc key ring không khớp.");
                throw new BusinessException(
                    "Không thể giải mã ApiKey. Cấu hình này có thể cần được nhập lại ApiKey.", ex);
            }
        }

        public string MaskFromProtected(string protectedApiKey)
        {
            try
            {
                var plain = Unprotect(protectedApiKey);
                return ApiKeyMasker.Mask(plain);
            }
            catch (Exception)
            {
                // Không để lỗi giải mã làm sập màn hình danh sách cấu hình - hiển thị placeholder
                // và để hành động "sửa/nhập lại key" xử lý riêng.
                return "(không thể giải mã)";
            }
        }

        /// <summary>
        /// Băm ApiKey bằng SHA256 để so sánh phát hiện thay đổi.
        /// KHÔNG dùng để bảo vệ - chỉ dùng để so sánh (vì IDataProtector trả output
        /// ngẫu nhiên mỗi lần nên không thể dùng Protect/Unprotect để so sánh).
        /// </summary>
        public string Hash(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static string StripVersionPrefix(string protectedApiKey)
        {
            return protectedApiKey.StartsWith(PayloadVersionPrefix, StringComparison.Ordinal)
                ? protectedApiKey[PayloadVersionPrefix.Length..]
                : protectedApiKey; // tương thích ngược nếu có dữ liệu cũ chưa gắn prefix
        }
    }
}