using Kolia.Thumbnail.API.Data.Entities.Projects;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.Briefs
{
    /// <summary>
    /// Nội dung video (Phần 1 — Content Brief).
    /// Quan hệ 1-1 với ProjectEntity. Khi IsConfirmed = true → khóa định hướng cho mọi bước sau.
    /// </summary>
    public class ContentBriefEntity : BaseEntity
    {
        /// <summary>
        /// Id project (unique — 1-1)
        /// </summary>
        public Guid ProjectId { get; set; }

        // ----- Nguồn nhập liệu -----

        /// <summary>
        /// Cách nhập dữ liệu (paste/file/link) — null nếu nhập tay thuần
        /// </summary>
        public CImportContentSource? ImportSource { get; set; }

        /// <summary>
        /// Nội dung văn bản đã paste hoặc OCR từ file
        /// </summary>
        public string? ImportedRawText { get; set; }

        /// <summary>
        /// URL file đã upload (nếu nhập qua file)
        /// </summary>
        public string? ImportedFileUrl { get; set; }

        /// <summary>
        /// Link ngoài (Google Sheet, bài viết, video...) để AI tự đọc
        /// </summary>
        public string? ImportedExternalLink { get; set; }

        /// <summary>
        /// URL Google Sheet nội bộ của team phân tích (optional, AI tự đọc nếu có)
        /// </summary>
        public string? ExternalSheetUrl { get; set; }

        /// <summary>
        /// Lần cuối sync dữ liệu từ Google Sheet
        /// </summary>
        public DateTimeOffset? LastSheetSyncTime { get; set; }

        // ----- Nhập tay -----

        /// <summary>
        /// Tổng quan livestream tuần — nhập tay
        /// </summary>
        public string OverviewInput { get; set; } = string.Empty;

        /// <summary>
        /// Quan điểm muốn nhấn mạnh — nhập tay
        /// </summary>
        public string ViewpointInput { get; set; } = string.Empty;

        /// <summary>
        /// Dữ liệu quan trọng, số liệu nổi bật — nhập tay
        /// </summary>
        public string KeyDataInput { get; set; } = string.Empty;

        // ----- Output AI -----

        /// <summary>
        /// AI sinh — Chủ đề video
        /// </summary>
        public string? TopicOutput { get; set; }

        /// <summary>
        /// AI sinh — Thông điệp chính của video
        /// </summary>
        public string? MainMessageOutput { get; set; }

        /// <summary>
        /// AI sinh — Dữ liệu/điểm nổi bật cần nhấn mạnh
        /// </summary>
        public string? HighlightDataOutput { get; set; }

        /// <summary>
        /// Nội dung đã đọc từ Google Sheet (qua ISheetImportEngine), tách riêng khỏi ImportedRawText
        /// để không lẫn giữa "user paste tay" và "AI tự đọc sheet".
        /// </summary>
        public string? SheetImportedText { get; set; }

        /// <summary>
        /// Keywords gợi ý do AI sinh (JSON serialized List&lt;string&gt;).
        /// Dùng làm nguồn cho NewsService.GetSuggestedKeywordsAsync thay vì tách chuỗi thủ công.
        /// </summary>
        public string? SuggestedKeywordsJson { get; set; }

        // ----- Trạng thái -----

        /// <summary>
        /// Khi true = KHÓA định hướng — các bước sau chỉ đọc, không sửa được Brief
        /// </summary>
        public bool IsConfirmed { get; set; } = false;

        /// <summary>
        /// Thời điểm xác nhận khóa Brief
        /// </summary>
        public DateTimeOffset? ConfirmedAt { get; set; }

        // Navigation
        public virtual ProjectEntity Project { get; set; } = null!;
    }
}
