using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Sheets.v4;
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
        /// Dùng <paramref name="requiredScopes"/> nếu được chỉ định, nếu không dùng scopes từ entity,
        /// nếu entity cũng không có thì dùng mặc định (Sheets + Docs readonly).
        /// </summary>
        private async Task<GoogleCredential> CreateCredentialAsync(
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default,
            params string[] requiredScopes)
        {
            if (string.IsNullOrWhiteSpace(sa.RawCredentialJson))
                throw new ExternalServiceException(
                    "Service account thiếu credential JSON. Vui lòng import lại file JSON.");

            var rawJson = _protector.Unprotect(sa.RawCredentialJson);

            // Ưu tiên: requiredScopes > entity.Scopes > default (cả Sheets + Docs readonly)
            string[] scopes;
            if (requiredScopes.Length > 0)
            {
                scopes = requiredScopes;
            }
            else if (!string.IsNullOrWhiteSpace(sa.Scopes))
            {
                scopes = sa.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            else
            {
                scopes = new[]
                {
                    SheetsService.Scope.SpreadsheetsReadonly,
                    DocsService.Scope.DocumentsReadonly
                };
            }

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
            var credential = await CreateCredentialAsync(sa, ct, SheetsService.Scope.SpreadsheetsReadonly);
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
        /// Dùng Google Docs API v1 chính thức với includeTabsContent=true
        /// để hỗ trợ document có nhiều tabs.
        /// </summary>
        public async Task<string> FetchDocContentAsync(
            string docsUrl,
            GoogleServiceAccountEntity sa,
            CancellationToken ct = default)
        {
            var credential = await CreateCredentialAsync(sa, ct, DocsService.Scope.DocumentsReadonly);
            var docId = ExtractDocId(docsUrl);
            var targetTabId = ExtractTargetTabId(docsUrl);

            using var docsService = new DocsService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "KoliaThumbnail"
            });

            var getRequest = docsService.Documents.Get(docId);
            getRequest.IncludeTabsContent = true;
            var document = await getRequest.ExecuteAsync(ct);

            var text = ExtractPlainText(document, targetTabId);
            _logger.LogInformation("Fetched Doc {DocId}: {Length} chars (tab: {TabId})",
                docId, text.Length, targetTabId ?? "all");

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
                var requiredScopes = type == CGoogleServiceType.GoogleSheets
                    ? new[] { SheetsService.Scope.SpreadsheetsReadonly }
                    : new[] { DocsService.Scope.DocumentsReadonly };
                var credential = await CreateCredentialAsync(sa, ct, requiredScopes);
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
                    var getRequest = docsService.Documents.Get(docId);
                    getRequest.IncludeTabsContent = true;
                    await getRequest.ExecuteAsync(ct);
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
        /// Trích xuất plain text từ Document.
        /// Hỗ trợ cả Document.Body (legacy) và Document.Tabs (khi includeTabsContent=true).
        /// Xử lý đệ quy Paragraph, Table, TableOfContents.
        /// Nếu <paramref name="targetTabId"/> != null, chỉ đọc nội dung từ tab đó.
        /// </summary>
        private static string ExtractPlainText(Document document, string? targetTabId = null)
        {
            var parts = new List<string>();

            // Khi includeTabsContent=true, nội dung nằm trong Document.Tabs
            if (document.Tabs != null && document.Tabs.Count > 0)
            {
                ExtractFromTabs(document.Tabs, parts, targetTabId);
            }
            // Fallback: Document.Body (khi includeTabsContent=false, single-tab docs)
            else if (document.Body?.Content != null)
            {
                ExtractFromStructuralElements(document.Body.Content, parts);
            }

            return string.Join("", parts).Trim();
        }

        /// <summary>
        /// Đệ quy trích xuất text từ danh sách Tab (bao gồm child tabs).
        /// Nếu <paramref name="targetTabId"/> != null, chỉ đọc tab có ID khớp.
        /// </summary>
        private static void ExtractFromTabs(
            IList<Google.Apis.Docs.v1.Data.Tab> tabs,
            List<string> parts,
            string? targetTabId = null)
        {
            if (tabs == null)
                return;

            foreach (var tab in tabs)
            {
                var currentTabId = tab.TabProperties?.TabId;

                // Nếu có targetTabId, chỉ xử lý tab có ID khớp (hoặc child của nó)
                if (targetTabId != null)
                {
                    // Thử match cả format có và không có prefix "t."
                    if (currentTabId == targetTabId || currentTabId == "t." + targetTabId || "t." + currentTabId == targetTabId)
                    {
                        // Tab mục tiêu — chỉ đọc nội dung tab này
                        if (tab.TabProperties?.Title != null)
                            parts.Add($"\n--- {tab.TabProperties.Title} ---\n");

                        if (tab.DocumentTab?.Body?.Content != null)
                            ExtractFromStructuralElements(tab.DocumentTab.Body.Content, parts);

                        return; // Không cần tìm tiếp
                    }

                    // Không khớp → tìm trong child tabs
                    if (tab.ChildTabs != null && tab.ChildTabs.Count > 0)
                    {
                        ExtractFromTabs(tab.ChildTabs, parts, targetTabId);
                    }

                    continue;
                }

                // Không có targetTabId → đọc tất cả tabs
                if (tab.TabProperties?.Title != null)
                    parts.Add($"\n--- Tab: {tab.TabProperties.Title} ---\n");

                if (tab.DocumentTab?.Body?.Content != null)
                    ExtractFromStructuralElements(tab.DocumentTab.Body.Content, parts);

                if (tab.ChildTabs != null && tab.ChildTabs.Count > 0)
                    ExtractFromTabs(tab.ChildTabs, parts);
            }
        }

        /// <summary>
        /// Đệ quy trích xuất text từ danh sách StructuralElement.
        /// Xử lý Paragraph, Table, TableOfContents.
        /// </summary>
        private static void ExtractFromStructuralElements(
            IList<Google.Apis.Docs.v1.Data.StructuralElement> elements,
            List<string> parts)
        {
            if (elements == null)
                return;

            foreach (var element in elements)
            {
                if (element.Paragraph != null)
                {
                    ExtractFromParagraph(element.Paragraph, parts);
                }
                else if (element.Table != null)
                {
                    ExtractFromTable(element.Table, parts);
                }
                else if (element.TableOfContents?.Content != null)
                {
                    ExtractFromStructuralElements(element.TableOfContents.Content, parts);
                }
                // SectionBreak: bỏ qua, không có nội dung text
            }
        }

        /// <summary>
        /// Trích xuất text từ Paragraph (TextRun).
        /// </summary>
        private static void ExtractFromParagraph(
            Google.Apis.Docs.v1.Data.Paragraph paragraph,
            List<string> parts)
        {
            if (paragraph.Elements == null)
                return;

            foreach (var paraElement in paragraph.Elements)
            {
                if (paraElement.TextRun?.Content != null)
                {
                    parts.Add(paraElement.TextRun.Content);
                }
            }

            // Xuống dòng sau mỗi paragraph
            parts.Add(Environment.NewLine);
        }

        /// <summary>
        /// Trích xuất text từ Table (đệ quy vào từng cell, hỗ trợ table lồng nhau).
        /// </summary>
        private static void ExtractFromTable(
            Google.Apis.Docs.v1.Data.Table table,
            List<string> parts)
        {
            if (table.TableRows == null)
                return;

            foreach (var row in table.TableRows)
            {
                if (row.TableCells == null)
                    continue;

                foreach (var cell in row.TableCells)
                {
                    if (cell.Content != null)
                    {
                        // Cell có thể chứa Paragraph, Table (lồng nhau), v.v.
                        ExtractFromStructuralElements(cell.Content, parts);
                    }
                    // Phân cách giữa các cell
                    parts.Add("\t");
                }
                // Xuống dòng sau mỗi row
                parts.Add(Environment.NewLine);
            }

            // Xuống dòng sau khi kết thúc table
            parts.Add(Environment.NewLine);
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

        /// <summary>
        /// Trích xuất Tab ID từ query parameter `tab` trong URL Google Doc.
        /// VD: .../edit?tab=t.ygde6l8uem9j → trả về "ygde6l8uem9j"
        /// Nếu không có tab parameter, trả về null.
        /// </summary>
        private static string? ExtractTargetTabId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var tabValue = query["tab"];
            if (string.IsNullOrWhiteSpace(tabValue))
                return null;

            // Google dùng prefix "t." trong URL, TabId thực tế không có prefix này
            return tabValue.StartsWith("t.")
                ? tabValue[2..]
                : tabValue;
        }
    }
}
