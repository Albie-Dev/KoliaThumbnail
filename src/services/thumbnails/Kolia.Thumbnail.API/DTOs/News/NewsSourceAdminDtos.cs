using Kolia.Thumbnail.API.Data.Entities.News;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.DTOs.News
{
    /// <summary>DTO tạo mới một NewsSource qua Admin API.</summary>
    public sealed record NewsSourceCreateDto(
        string Name,
        string RssOrFeedUrl,
        CMarketScope Region,
        bool IsTrusted,
        int Priority,
        CNewsSourceGroup SourceGroup,
        CSourceFetchMode FetchMode,
        string Domain);

    /// <summary>DTO cập nhật một NewsSource qua Admin API.</summary>
    public sealed record NewsSourceUpdateDto(
        string Name,
        string RssOrFeedUrl,
        CMarketScope Region,
        bool IsTrusted,
        int Priority,
        CNewsSourceGroup SourceGroup,
        CSourceFetchMode FetchMode,
        string Domain);

    /// <summary>
    /// DTO trả về danh sách nguồn tin, kèm thông tin trạng thái vận hành
    /// để admin biết ngay nguồn nào đang có vấn đề.
    /// <c>OperationalStatus</c> được tính từ <c>ConsecutiveFailureCount</c> + <c>LastFailedAt</c>.
    /// </summary>
    public sealed record NewsSourceListItemDto(
        Guid Id,
        string Name,
        string RssOrFeedUrl,
        CMarketScope Region,
        bool IsTrusted,
        int Priority,
        CNewsSourceGroup SourceGroup,
        CSourceFetchMode FetchMode,
        string Domain,
        DateTimeOffset? LastFetchedAt,
        DateTimeOffset? LastFailedAt,
        int ConsecutiveFailureCount,
        string OperationalStatus);

    /// <summary>DTO chi tiết một NewsSource.</summary>
    public sealed record NewsSourceDetailDto(
        Guid Id,
        string Name,
        string RssOrFeedUrl,
        CMarketScope Region,
        bool IsTrusted,
        int Priority,
        CNewsSourceGroup SourceGroup,
        CSourceFetchMode FetchMode,
        string Domain,
        DateTimeOffset? LastFetchedAt,
        DateTimeOffset? LastFailedAt,
        int ConsecutiveFailureCount,
        string? LastEtag,
        string? LastModifiedHeader,
        string OperationalStatus,
        DateTimeOffset CreationTime,
        DateTimeOffset? LastModificationTime);

    /// <summary>
    /// DTO kết quả test fetch qua POST /admin/news-sources/{id}/test.
    /// Trả về preview dữ liệu thật (không ảnh hưởng circuit breaker vận hành).
    /// </summary>
    public sealed record NewsSourceTestFetchResultDto(
        bool Success,
        string TierUsed,
        int ItemCount,
        IReadOnlyList<NewsSourcePreviewItemDto> Items,
        string? ErrorMessage);

    public sealed record NewsSourcePreviewItemDto(
        string Title,
        string SourceUrl,
        DateTimeOffset? PublishedTime,
        string SummaryRaw);

    /// <summary>Mapper: NewsSourceEntity → các DTOs.</summary>
    public static class NewsSourceMapper
    {
        private const int FailureThresholdWarning = 1;
        private const int FailureThresholdCritical = 3;

        public static NewsSourceListItemDto ToListItem(NewsSourceEntity e) =>
            new(
                Id: e.Id,
                Name: e.Name,
                RssOrFeedUrl: e.RssOrFeedUrl,
                Region: e.Region,
                IsTrusted: e.IsTrusted,
                Priority: e.Priority,
                SourceGroup: e.SourceGroup,
                FetchMode: e.FetchMode,
                Domain: e.Domain,
                LastFetchedAt: e.LastFetchedAt,
                LastFailedAt: e.LastFailedAt,
                ConsecutiveFailureCount: e.ConsecutiveFailureCount,
                OperationalStatus: ComputeStatus(e));

        public static NewsSourceDetailDto ToDetail(NewsSourceEntity e) =>
            new(
                Id: e.Id,
                Name: e.Name,
                RssOrFeedUrl: e.RssOrFeedUrl,
                Region: e.Region,
                IsTrusted: e.IsTrusted,
                Priority: e.Priority,
                SourceGroup: e.SourceGroup,
                FetchMode: e.FetchMode,
                Domain: e.Domain,
                LastFetchedAt: e.LastFetchedAt,
                LastFailedAt: e.LastFailedAt,
                ConsecutiveFailureCount: e.ConsecutiveFailureCount,
                LastEtag: e.LastEtag,
                LastModifiedHeader: e.LastModifiedHeader,
                OperationalStatus: ComputeStatus(e),
                CreationTime: e.CreationTime,
                LastModificationTime: e.LastModificationTime);

        private static string ComputeStatus(NewsSourceEntity e)
        {
            if (!e.IsTrusted) return "Disabled";
            if (e.ConsecutiveFailureCount >= FailureThresholdCritical) return "Critical";
            if (e.ConsecutiveFailureCount >= FailureThresholdWarning) return "Warning";
            return "Healthy";
        }
    }
}
