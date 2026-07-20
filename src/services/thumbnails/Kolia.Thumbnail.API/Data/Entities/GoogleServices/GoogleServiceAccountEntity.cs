namespace Kolia.Thumbnail.API.Data.Entities.GoogleServices
{
    /// <summary>
    /// Tài khoản Google Service Account dùng để truy cập Google Sheets và Google Docs.
    /// Credential được import từ file JSON của Google Cloud Console,
    /// các trường nhạy cảm được bảo vệ bởi <see cref="Security.IApiKeyProtector"/>.
    /// </summary>
    public class GoogleServiceAccountEntity : BaseEntity
    {
        /// <summary>Tên hiển thị của service account (do người dùng đặt)</summary>
        public string Name { get; set; } = null!;

        /// <summary>Mô tả ngắn về mục đích sử dụng</summary>
        public string? Description { get; set; }

        /// <summary>
        /// Email của service account (client_email từ JSON).
        /// Dùng để hiển thị và hướng dẫn người dùng share quyền.
        /// </summary>
        public string ClientEmail { get; set; } = null!;

        /// <summary>
        /// Client ID từ JSON — không nhạy cảm, lưu plaintext.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Project ID từ JSON.
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Token URI lấy từ JSON (vd: https://oauth2.googleapis.com/token).
        /// </summary>
        public string? TokenUri { get; set; }

        /// <summary>
        /// Auth URI lấy từ JSON.
        /// </summary>
        public string? AuthUri { get; set; }

        /// <summary>
        /// Auth provider x509 cert URL lấy từ JSON.
        /// </summary>
        public string? AuthProviderX509CertUrl { get; set; }

        /// <summary>
        /// Private key ID — được hash để phát hiện thay đổi.
        /// </summary>
        public string? PrivateKeyIdHash { get; set; }

        /// <summary>
        /// Private key — ĐÃ ĐƯỢC MÃ HOÁ bởi IApiKeyProtector.
        /// </summary>
        public string PrivateKey { get; set; } = null!;

        /// <summary>
        /// Toàn bộ nội dung file JSON credential — ĐÃ ĐƯỢC MÃ HOÁ bởi IApiKeyProtector.
        /// Lưu để sau này có thể export lại nếu cần.
        /// </summary>
        public string? RawCredentialJson { get; set; }

        /// <summary>
        /// Hash của raw JSON để phát hiện thay đổi.
        /// </summary>
        public string? CredentialJsonHash { get; set; }

        /// <summary>
        /// Danh sách scope (cách nhau bằng dấu phẩy).
        /// VD: "https://www.googleapis.com/auth/spreadsheets.readonly,https://www.googleapis.com/auth/documents.readonly"
        /// </summary>
        public string? Scopes { get; set; }

        /// <summary>Có đang được kích hoạt hay không</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Navigation property: danh sách jobs sử dụng service account này.
        /// </summary>
        public virtual ICollection<ScheduledImportJobEntity> ScheduledJobs { get; set; }
            = new List<ScheduledImportJobEntity>();
    }
}
