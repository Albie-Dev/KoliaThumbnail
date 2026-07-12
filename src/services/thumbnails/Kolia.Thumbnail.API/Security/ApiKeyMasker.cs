namespace Kolia.Thumbnail.API.Security
{
    /// <summary>
    /// Logic thuần (pure function) để che một phần chuỗi nhạy cảm khi hiển thị ra client.
    /// Tách riêng khỏi ApiKeyProtector để:
    ///  - Unit test không cần setup Data Protection / DI.
    ///  - Tái sử dụng cho các chuỗi nhạy cảm khác ngoài ApiKey nếu cần (webhook secret, token...).
    /// </summary>
    public static class ApiKeyMasker
    {
        private const int DefaultPrefixLength = 4;
        private const int DefaultSuffixLength = 4;
        private const int MinLengthToMask = 8;
        private const char MaskChar = '*';

        /// <summary>
        /// Che ApiKey theo định dạng: giữ N ký tự đầu + N ký tự cuối, phần giữa thay bằng "*".
        /// Ví dụ: "sk-proj-A1B2C3D4E5F6G7H8" -> "sk-p****************G7H8"
        ///
        /// Quy tắc:
        ///  - Chuỗi rỗng/null -> trả về chuỗi rỗng, không throw (an toàn khi dùng ở UI).
        ///  - Chuỗi quá ngắn (&lt; 8 ký tự) -> mask toàn bộ, không lộ prefix/suffix vì
        ///    prefix+suffix có thể chiếm gần hết độ dài, làm mất tác dụng che giấu.
        ///  - Số lượng "*" ở giữa CỐ ĐỊNH (không phản ánh đúng độ dài thật) để tránh lộ
        ///    thông tin độ dài chính xác của key qua số ký tự mask.
        /// </summary>
        public static string Mask(
            string? value,
            int prefixLength = DefaultPrefixLength,
            int suffixLength = DefaultSuffixLength,
            int fixedMaskLength = 8)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length < MinLengthToMask)
                return new string(MaskChar, fixedMaskLength);

            // Đảm bảo prefix + suffix không vượt quá độ dài chuỗi thật
            // (trường hợp key ngắn bất thường nhưng vẫn >= MinLengthToMask)
            var safePrefix = Math.Min(prefixLength, value.Length / 3);
            var safeSuffix = Math.Min(suffixLength, value.Length / 3);

            var prefix = value[..safePrefix];
            var suffix = value[^safeSuffix..];

            return $"{prefix}{new string(MaskChar, fixedMaskLength)}{suffix}";
        }
    }
}