using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Util;
using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Security;

namespace Kolia.Thumbnail.API.Services.GoogleServices
{
    /// <summary>
    /// Helper xử lý xác thực và truy xuất Google Sheets/Docs bằng Google API chính thức.
    /// Dùng GoogleCredential + Google.Apis.Sheets.v4 / Google.Apis.Docs.v1.
    /// </summary>
    public class GoogleServiceAccountHelper
    {
        private readonly IApiKeyProtector _protector;
        private readonly ILogger<GoogleServiceAccountHelper> _logger;

        public GoogleServiceAccountHelper(
            IApiKeyProtector protector,
            ILogger<GoogleServiceAccountHelper> logger)
        {
            _protector = protector;
            _logger = logger;
        }

        /// <summary>
        /// Tạo GoogleCredential từ Service Account credentials đã mã hoá.
        /// </summary>
        private async Task<GoogleCredential> CreateCredentialAsync(
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sa.RawCredentialJson))
                throw new ExternalServiceException(
                    "Service account thiếu credential JSON. Vui lòng import lại file JSON.");

            var rawJson = _protector.Unprotect(sa.RawCredentialJson);

            var scopes = !string.IsNullOrWhiteSpace(sa.Scopes)
                ? sa.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : new[] { SheetsService.Scope.SpreadsheetsReadonly };

            var serviceCredential = Google.Apis.Auth.OAuth2.CredentialFactory
                .FromJson<Google.Apis.Auth.OAuth2.ServiceAccountCredential>(rawJson);
            var credential = serviceCredential.ToGoogleCredential();

            if (credential.IsCreateScopedRequired)
                credential = credential.CreateScoped(scopes);

            return credential;
        }

        /// <summary>
        /// Lấy toàn bộ nội dung Google Sheet dạng text (tab-separated).
        /// Dùng Google Sheets API v4 chính thức.
        /// </summary>
        public async Task<string> FetchSheetContentAsync(
            string sheetUrl,
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default)
        {
            var credential = await CreateCredentialAsync(sa, ct);
            var docId = ExtractDocId(sheetUrl);

            using var sheetsService = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "KoliaThumbnail"
            });

            // Lấy tất cả sheets trong spreadsheet
            var spreadsheet = await sheetsService.Spreadsheets.Get(docId).ExecuteAsync(ct);
            var allText = new List<string>();

            foreach (var sheet in spreadsheet.Sheets)
            {
                var range = $"{sheet.Properties.Title}!A1:ZZZ";
                var request = sheetsService.Spreadsheets.Values.Get(docId, range);

                var response = await request.ExecuteAsync(ct);
                if (response.Values == null || response.Values.Count == 0)
                    continue;

                // Convert rows thành tab-separated text
                foreach (var row in response.Values)
                {
                    var cells = row.Select(cell => cell?.ToString() ?? "");
                    allText.Add(string.Join("\t", cells));
                }
            }

            var result = string.Join(Environment.NewLine, allText);
            _logger.LogInformation("Fetched Sheet {DocId}: {Length} chars, {Sheets} sheets",
                docId, result.Length, spreadsheet.Sheets.Count);

            return result;
        }

        /// <summary>
        /// Lấy nội dung Google Doc dạng plain text.
        /// Dùng Google Docs API v1 chính thức.
        /// </summary>
        public async Task<string> FetchDocContentAsync(
            string docsUrl,
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default)
        {
            var credential = await CreateCredentialAsync(sa, ct);
            var docId = ExtractDocId(docsUrl);

            using var docsService = new DocsService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "KoliaThumbnail"
            });

            var document = await docsService.Documents.Get(docId).ExecuteAsync(ct);

            var text = ExtractPlainText(document);
            _logger.LogInformation("Fetched Doc {DocId}: {Length} chars", docId, text.Length);

            return text;
        }

        /// <summary>
        /// Kiểm tra quyền truy cập của service account vào tài liệu.
        /// Dùng API metadata call — chỉ cần HTTP 200 là có quyền.
        /// </summary>
        public async Task<bool> CheckAccessAsync(
            string url,
            CGoogleServiceType type,
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default)
        {
            try
            {
                var credential = await CreateCredentialAsync(sa, ct);
                var docId = ExtractDocId(url);

                if (type == CGoogleServiceType.GoogleSheets)
                {
                    using var sheetsService = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "KoliaThumbnail"
                    });
                    // Chỉ cần get metadata là đủ kiểm tra quyền
                    await sheetsService.Spreadsheets.Get(docId).ExecuteAsync(ct);
                }
                else
                {
                    using var docsService = new DocsService(new Google.Apis.Services.BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "KoliaThumbnail"
                    });
                    await docsService.Documents.Get(docId).ExecuteAsync(ct);
                }

                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogWarning("Access check failed for {Url}: HTTP {Status} - {Msg}",
                    url, ex.HttpStatusCode, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trích xuất plain text từ Document content (paragraphs + inline text).
        /// </summary>
        private static string ExtractPlainText(Document document)
        {
            var parts = new List<string>();

            if (document.Body?.Content == null)
                return string.Empty;

            foreach (var element in document.Body.Content)
            {
                if (element.Paragraph?.Elements == null)
                    continue;

                foreach (var paraElement in element.Paragraph.Elements)
                {
                    if (paraElement.TextRun?.Content != null)
                    {
                        parts.Add(paraElement.TextRun.Content);
                    }
                }

                // Thêm newline sau mỗi paragraph
                parts.Add(Environment.NewLine);
            }

            return string.Join("", parts).Trim();
        }

        /// <summary>
        /// Trích xuất Document ID từ URL Google (Sheet hoặc Doc).
        /// </summary>
        private static string ExtractDocId(string url)
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var docIdIndex = Array.IndexOf(pathSegments, "d");
            if (docIdIndex < 0 || docIdIndex + 1 >= pathSegments.Length)
                throw new ArgumentException($"URL không hợp lệ: {url}");

            return pathSegments[docIdIndex + 1];
        }
    }
}
