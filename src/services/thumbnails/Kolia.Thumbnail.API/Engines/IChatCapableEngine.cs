namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Provider hỗ trợ chat completion (OpenAI, Gemini, Deepseek, Groq, XAI, Anthropic, Mistral...).
    /// </summary>
    public interface IChatCapableEngine : IAIEngine
    {
        /// <summary>
        /// Gọi chat completion (không streaming).
        /// </summary>
        Task<ChatCompletionResult> ChatCompletionAsync(ChatCompletionRequest request);

        /// <summary>
        /// Gọi chat completion dạng streaming (trả token/chunk realtime).
        /// </summary>
        IAsyncEnumerable<ChatCompletionChunk> ChatCompletionStreamAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ước lượng số token của một đoạn văn bản theo model cụ thể
        /// (phục vụ tính chi phí / kiểm tra giới hạn context trước khi gọi).
        /// </summary>
        Task<int> CountTokensAsync(string model, string text);
    }

    /// <summary>
    /// Provider hỗ trợ sinh ảnh (StabilityAI, BlackForestLabs, Ideogram, LeonardoAI, OpenAI DALL-E, Gemini Imagen...).
    /// </summary>
    public interface IImageGenerationCapableEngine : IAIEngine
    {
        /// <summary>
        /// Sinh ảnh mới từ văn bản (text-to-image).
        /// </summary>
        Task<ImageGenerationResult> GenerateImageAsync(ImageGenerationRequest request);

        /// <summary>
        /// Sửa/biến thể ảnh có sẵn (image-to-image, inpainting, outpainting, upscale...).
        /// </summary>
        Task<ImageGenerationResult> EditImageAsync(ImageEditRequest request);
    }

    /// <summary>
    /// Provider hỗ trợ chuyển văn bản thành giọng nói (OpenAI TTS, ElevenLabs, PlayHT, Google TTS...).
    /// </summary>
    public interface ITextToSpeechCapableEngine : IAIEngine
    {
        /// <summary>
        /// Sinh audio từ văn bản.
        /// </summary>
        Task<TextToSpeechResult> GenerateSpeechAsync(TextToSpeechRequest request);

        /// <summary>
        /// Lấy danh sách giọng đọc (voice) khả dụng của provider (bao gồm cả voice tự clone nếu có).
        /// </summary>
        Task<List<AIVoiceInfo>> GetAvailableVoicesAsync(string apiKey);
    }

    /// <summary>
    /// Provider hỗ trợ nhận diện giọng nói / tạo phụ đề (OpenAI Whisper, Groq Whisper, AssemblyAI, Deepgram...).
    /// </summary>
    public interface ISpeechToTextCapableEngine : IAIEngine
    {
        /// <summary>
        /// Chuyển audio thành văn bản (giữ nguyên ngôn ngữ gốc).
        /// </summary>
        Task<SpeechToTextResult> TranscribeAudioAsync(SpeechToTextRequest request);

        /// <summary>
        /// Dịch audio ngôn ngữ khác sang tiếng Anh (một số provider hỗ trợ endpoint riêng, vd Whisper translate).
        /// </summary>
        Task<SpeechToTextResult> TranslateAudioAsync(SpeechToTextRequest request);
    }

    /// <summary>
    /// Provider hỗ trợ sinh video (RunwayML, Kling, Luma, Pika, Google Veo, OpenAI Sora, HeyGen, Synthesia...).
    /// </summary>
    public interface IVideoGenerationCapableEngine : IAIEngine
    {
        /// <summary>
        /// Khởi tạo job sinh video từ văn bản (text-to-video).
        /// </summary>
        Task<VideoGenerationResult> GenerateVideoFromTextAsync(VideoGenerationRequest request);

        /// <summary>
        /// Khởi tạo job sinh video từ 1 ảnh có sẵn (image-to-video) - phổ biến cho thumbnail động.
        /// </summary>
        Task<VideoGenerationResult> GenerateVideoFromImageAsync(ImageToVideoRequest request);

        /// <summary>
        /// Video sinh thường là tác vụ bất đồng bộ (job) - cần polling trạng thái.
        /// </summary>
        Task<VideoGenerationJobStatus> GetVideoGenerationStatusAsync(string jobId, string apiKey);

        /// <summary>
        /// Hủy job sinh video đang chạy (nếu provider hỗ trợ).
        /// </summary>
        Task<bool> CancelVideoGenerationAsync(string jobId, string apiKey);
    }

    /// <summary>
    /// Provider hỗ trợ tạo vector embedding (OpenAI, Gemini, Cohere...) - dùng cho search/RAG.
    /// </summary>
    public interface IEmbeddingCapableEngine : IAIEngine
    {
        Task<EmbeddingResult> CreateEmbeddingAsync(EmbeddingRequest request);
    }
}