namespace Kolia.Thumbnail.API.Engines.AI
{
    public record SheetImportResult(string RawTextContent, DateTimeOffset FetchedAt);

    /// <summary>
    /// Đọc nội dung Google Sheet công khai (link "Xuất bản lên web" hoặc link chia sẻ dạng CSV export)
    /// để AI phân tích ở Phần 1. Không xử lý Sheet yêu cầu đăng nhập.
    /// </summary>
    public interface ISheetImportEngine
    {
        Task<SheetImportResult> FetchAsync(string sheetUrl, CancellationToken ct = default);
    }
}
