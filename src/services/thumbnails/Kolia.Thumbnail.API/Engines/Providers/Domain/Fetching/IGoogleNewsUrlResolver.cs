namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    public interface IGoogleNewsUrlResolver
    {
        /// Nếu url là link Google News (news.google.com), trả về url bài báo gốc.
        /// Nếu không phải Google News, trả về nguyên url ban đầu.
        Task<string> ResolveAsync(string url, CancellationToken ct = default);
    }
}