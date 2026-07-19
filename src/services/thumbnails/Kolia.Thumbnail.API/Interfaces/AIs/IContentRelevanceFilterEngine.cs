using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.AI
{
    public record RelevanceFilterResult(bool IsIrrelevant, string? Reason, CMarketScope? InferredMarketType);

    /// <summary>
    /// Phân loại nhanh 1 video có phải nội dung không liên quan hay không
    /// (MV nhạc, quảng cáo, giải trí, kênh vùng miền không liên quan) — chạy ngay khi crawl xong,
    /// trước khi hiển thị vào Thumbnail Library.
    /// </summary>
    public interface IContentRelevanceFilterEngine
    {
        Task<RelevanceFilterResult> ClassifyAsync(string videoTitle, string channelName, CancellationToken ct = default);
    }
}
