using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.Commons;

namespace Kolia.Thumbnail.API.Services.News
{
    /// <summary>
    /// Tìm kiếm, quản lý và phân tích tin tức (Phần 2).
    /// </summary>
    public interface INewsService
    {
        /// <summary>
        /// Tìm kiếm tin tức theo bộ lọc — tạo NewsSearchRequest và trả về NewsItems.
        /// </summary>
        Task<NewsSearchRequestEntity> SearchAsync(
            Guid projectId,
            CMarketScope marketScope,
            CNewsTimeRange timeRange,
            CNewsCountFilter countFilter,
            string keywordsRaw,
            IEnumerable<string>? suggestedKeywordsSelected,
            IEnumerable<Guid>? selectedSourceIds = null,
            Guid operationId = default,
            CancellationToken ct = default);

        /// <summary>
        /// Import thủ công 1 bài báo qua link ngoài.
        /// </summary>
        Task<NewsItemEntity> ImportManualLinkAsync(Guid projectId, string url,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả NewsItems của 1 project (flat list — tất cả search requests).
        /// </summary>
        Task<IReadOnlyList<NewsItemEntity>> GetByProjectAsync(Guid projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy NewsItems phân trang của 1 project.
        /// </summary>
        Task<PagedResponseDto<NewsItemDto>> GetPagedByProjectAsync(Guid projectId,
            PagedRequestDto request,
            CancellationToken ct = default);

        /// <summary>
        /// Phân tích sâu 4 tầng cho 1 bản tin. Tạo NewsDeepAnalysisEntity mới nếu chưa có.
        /// </summary>
        Task<NewsDeepAnalysisEntity> DeepAnalyzeAsync(Guid newsItemId,
            Guid operationId = default,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy bản ghi phân tích sâu đã lưu của 1 bản tin (không gọi lại AI).
        /// </summary>
        Task<NewsDeepAnalysisEntity?> GetDeepAnalysisAsync(Guid newsItemId, CancellationToken ct = default);

        /// <summary>
        /// Tick/bỏ tick bản tin là "Dùng cho Phần 4 và Phần 5".
        /// </summary>
        Task SetSelectedAsync(Guid newsItemId, bool isSelected,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách keyword gợi ý từ ContentBrief để hiển thị cho user click chọn.
        /// </summary>
        Task<IReadOnlyList<string>> GetSuggestedKeywordsAsync(Guid projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Xoá 1 bản tin (xóa mềm).
        /// </summary>
        Task DeleteNewsItemAsync(Guid newsItemId, CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách NewsSource phân trang để client chọn khi search news.
        /// Hỗ trợ search theo tên, filter theo Region, sort theo Priority.
        /// </summary>
        Task<PagedResponseDto<NewsSourceSelectDto>> GetNewsSourcesPagingAsync(
            PagedRequestDto request,
            CMarketScope? region = null,
            CancellationToken ct = default);
    }
}
