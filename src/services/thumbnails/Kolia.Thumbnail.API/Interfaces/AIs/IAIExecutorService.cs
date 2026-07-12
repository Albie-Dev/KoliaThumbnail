using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.Engines;

namespace Kolia.Thumbnail.API.AIs
{
    /// <summary>
    /// Dịch vụ thực thi các tác vụ AI với cơ chế fallback:
    /// - Tự động load thông tin provider + cấu hình (API keys) từ DB
    /// - Thử từng config theo thứ tự ưu tiên, fallback nếu config chính hết quota/lỗi
    /// - Fallback sang provider khác nếu cùng loại (dự phòng)
    /// </summary>
    public interface IAIExecutorService
    {
        /// <summary>
        /// Load thông tin provider và danh sách config từ DB.
        /// Trả về null nếu không tìm thấy provider nào khả dụng.
        /// </summary>
        Task<ProviderExecutionContext?> GetProviderContextAsync(
            CAIProviderType providerType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi chat completion với fallback tự động qua các config.
        /// </summary>
        Task<ChatCompletionResult> ChatCompletionWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi chat completion streaming với fallback tự động qua các config.
        /// Lưu ý: streaming không thể fallback giữa chừng; nếu config đầu lỗi ngay
        /// thì fallback sang config tiếp theo và bắt đầu stream lại từ đầu.
        /// </summary>
        IAsyncEnumerable<ChatCompletionChunk> ChatCompletionStreamWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi sinh ảnh với fallback tự động qua các config.
        /// </summary>
        Task<ImageGenerationResult> GenerateImageWithFallbackAsync(
            CAIProviderType providerType,
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi TTS (text-to-speech) với fallback tự động qua các config.
        /// </summary>
        Task<TextToSpeechResult> GenerateSpeechWithFallbackAsync(
            CAIProviderType providerType,
            TextToSpeechRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi STT (speech-to-text) với fallback tự động qua các config.
        /// </summary>
        Task<SpeechToTextResult> TranscribeWithFallbackAsync(
            CAIProviderType providerType,
            SpeechToTextRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi embedding với fallback tự động qua các config.
        /// </summary>
        Task<EmbeddingResult> CreateEmbeddingWithFallbackAsync(
            CAIProviderType providerType,
            EmbeddingRequest request,
            CancellationToken cancellationToken = default);
    }
}
