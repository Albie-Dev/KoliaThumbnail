using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Exceptions;

namespace Kolia.Thumbnail.API.Engines.Providers.Sheets
{
    /// <summary>
    /// Đọc nội dung Google Sheet công khai bằng cách convert URL sang CSV export format.
    /// Chỉ hoạt động với sheet được chia sẻ "Anyone with the link can view".
    /// Không cần OAuth — dùng CSV export URL của Google Sheets.
    /// </summary>
    public class GoogleSheetImportEngine : ISheetImportEngine
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleSheetImportEngine> _logger;

        public GoogleSheetImportEngine(
            HttpClient httpClient,
            ILogger<GoogleSheetImportEngine> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SheetImportResult> FetchAsync(string sheetUrl, CancellationToken ct = default)
        {
            var csvUrl = ConvertToCsvExportUrl(sheetUrl);
            _logger.LogInformation("Fetching Google Sheet CSV: {CsvUrl}", csvUrl);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(csvUrl, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new ExternalServiceException(
                    $"Không thể kết nối tới Google Sheet: {ex.Message}", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(
                    $"Không đọc được Google Sheet (HTTP {(int)response.StatusCode}) — " +
                    "kiểm tra quyền chia sẻ (Anyone with the link can view).");
            }

            var rawText = await response.Content.ReadAsStringAsync(ct);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                throw new ExternalServiceException("Google Sheet rỗng — không có dữ liệu để import.");
            }

            return new SheetImportResult(rawText, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Chuyển đổi URL Google Sheet dạng:
        ///   https://docs.google.com/spreadsheets/d/{id}/edit#gid={sheetId}
        /// thành:
        ///   https://docs.google.com/spreadsheets/d/{id}/export?format=csv&amp;gid={sheetId}
        /// Nếu không có gid, bỏ qua tham số gid (lấy sheet đầu tiên).
        /// </summary>
        private static string ConvertToCsvExportUrl(string sheetUrl)
        {
            // Chuẩn hoá URL: loại bỏ fragment (#) và các query params không cần
            var uri = new Uri(sheetUrl);

            // Regex: tìm document id giữa /d/ và /
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // pathSegments: ["spreadsheets", "d", "{docId}", ...]

            var docIdIndex = Array.IndexOf(pathSegments, "d");
            if (docIdIndex < 0 || docIdIndex + 1 >= pathSegments.Length)
            {
                throw new ArgumentException(
                    "URL Google Sheet không hợp lệ — không tìm thấy document ID. " +
                    "Ví dụ: https://docs.google.com/spreadsheets/d/abc123/edit");
            }

            var docId = pathSegments[docIdIndex + 1];

            // Trích xuất gid từ query string hoặc fragment
            var gid = string.Empty;
            var fragment = uri.Fragment;
            if (!string.IsNullOrEmpty(fragment))
            {
                var fragmentQuery = fragment.TrimStart('#');
                var fragParts = fragmentQuery.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in fragParts)
                {
                    if (part.StartsWith("gid=", StringComparison.OrdinalIgnoreCase))
                    {
                        gid = part[4..];
                        break;
                    }
                }
            }

            var csvUrl = $"https://docs.google.com/spreadsheets/d/{docId}/export?format=csv";
            if (!string.IsNullOrEmpty(gid))
            {
                csvUrl += $"&gid={gid}";
            }

            return csvUrl;
        }
    }
}
