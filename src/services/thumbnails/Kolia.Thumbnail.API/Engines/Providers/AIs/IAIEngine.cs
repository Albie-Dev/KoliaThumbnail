using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Interface gốc - mọi provider/agent trong hệ thống đều phải implement.
    /// Chỉ chứa các thao tác chung: quản lý key, danh sách model, balance, capabilities.
    /// Các khả năng cụ thể (chat, sinh ảnh, TTS, STT, video, embedding) nằm ở các
    /// interface con (IChatCapableEngine, IImageGenerationCapableEngine...) - engine nào
    /// hỗ trợ gì thì implement thêm interface đó.
    /// </summary>
    public interface IAIEngine
    {
        /// <summary>
        /// Loại provider mà instance này đại diện.
        /// </summary>
        CAIProviderType ProviderType { get; }

        /// <summary>
        /// Lấy thông tin số dư / quota còn lại của API key.
        /// </summary>
        Task<AIBalanceInfo> GetAIBalanceInfoAsync(string apiKey);

        /// <summary>
        /// Lấy danh sách thông tin model khả dụng của provider.
        /// </summary>
        Task<List<AIModelInfo>> GetAIModelInfosAsync(string apiKey);

        /// <summary>
        /// Kiểm tra API key có hợp lệ / còn hoạt động hay không.
        /// </summary>
        Task<bool> ValidateApiKeyAsync(string apiKey);

        /// <summary>
        /// Khai báo các năng lực mà provider này hỗ trợ, dùng để hệ thống
        /// biết cast sang interface con nào (IChatCapableEngine, IImageGenerationCapableEngine...).
        /// </summary>
        AIProviderCapabilities GetCapabilities();
    }
}