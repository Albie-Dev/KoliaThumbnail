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
        // ── Provider-based methods (existing) ──────────────────────────

        /// <summary>
        /// Load thông tin provider và danh sách config từ DB.
        /// </summary>
        Task<ProviderExecutionContext?> GetProviderContextAsync(
            CAIProviderType providerType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi chat completion với fallback tự động qua các config của một provider.
        /// </summary>
        Task<ChatCompletionResult> ChatCompletionWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi chat completion streaming với fallback tự động qua các config của một provider.
        /// </summary>
        IAsyncEnumerable<ChatCompletionChunk> ChatCompletionStreamWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi sinh ảnh với fallback tự động qua các config của một provider.
        /// </summary>
        Task<ImageGenerationResult> GenerateImageWithFallbackAsync(
            CAIProviderType providerType,
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi TTS với fallback tự động qua các config của một provider.
        /// </summary>
        Task<TextToSpeechResult> GenerateSpeechWithFallbackAsync(
            CAIProviderType providerType,
            TextToSpeechRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi STT với fallback tự động qua các config của một provider.
        /// </summary>
        Task<SpeechToTextResult> TranscribeWithFallbackAsync(
            CAIProviderType providerType,
            SpeechToTextRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi embedding với fallback tự động qua các config của một provider.
        /// </summary>
        Task<EmbeddingResult> CreateEmbeddingWithFallbackAsync(
            CAIProviderType providerType,
            EmbeddingRequest request,
            CancellationToken cancellationToken = default);

        // ── Function-based methods (mới) ───────────────────────────────

        /// <summary>
        /// Gọi chat completion dùng <see cref="CAIFunctionType"/> để tự động
        /// tra cứu provider, config, model từ function config.
        /// Tự động fallback qua các item (primary → fallback 1 → ...).
        /// </summary>
        Task<ChatCompletionResult> ChatCompletionWithFunctionAsync(
            CAIFunctionType functionType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gọi sinh ảnh dùng <see cref="CAIFunctionType"/> để tự động
        /// tra cứu provider, config, model.
        /// </summary>
        Task<ImageGenerationResult> GenerateImageWithFunctionAsync(
            CAIFunctionType functionType,
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default);
    }
}
