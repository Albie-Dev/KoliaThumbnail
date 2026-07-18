using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Thông tin số dư / quota còn lại của API key.
    /// </summary>
    public class AIBalanceInfo
    {
        public CAIProviderType ProviderType { get; set; }

        /// <summary>
        /// Số dư khả dụng (có thể là tiền hoặc credit tùy provider).
        /// </summary>
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ / credit (USD, VND, "credits"...).
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Tổng hạn mức đã cấp (nếu provider trả về, ví dụ gói free-tier).
        /// </summary>
        public decimal? TotalGrant { get; set; }

        /// <summary>
        /// Tổng đã sử dụng.
        /// </summary>
        public decimal? TotalUsed { get; set; }

        /// <summary>
        /// Ngày hết hạn của gói/credit (nếu có).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// true nếu API key hợp lệ và lấy được thông tin balance thành công.
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Thông báo lỗi (nếu IsSuccess = false).
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Thông tin 1 model của provider.
    /// </summary>
    public class AIModelInfo
    {
        public CAIProviderType ProviderType { get; set; }

        /// <summary>
        /// ID model dùng để gọi API (vd: "gpt-4o", "claude-sonnet-4-6").
        /// </summary>
        public string ModelId { get; set; } = default!;

        /// <summary>
        /// Tên hiển thị thân thiện.
        /// </summary>
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Mô tả ngắn về model.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Loại model: Chat, Image, TTS, STT, Video, Embedding.
        /// </summary>
        public AIModelCategory Category { get; set; }

        /// <summary>
        /// Số context token tối đa hỗ trợ (chỉ áp dụng cho model chat/text).
        /// </summary>
        public int? MaxContextTokens { get; set; }

        /// <summary>
        /// Số token output tối đa mỗi lần gọi.
        /// </summary>
        public int? MaxOutputTokens { get; set; }

        /// <summary>
        /// Model có hỗ trợ đọc hiểu hình ảnh đầu vào (vision) không.
        /// </summary>
        public bool SupportsVision { get; set; }

        /// <summary>
        /// Model có hỗ trợ function/tool calling không.
        /// </summary>
        public bool SupportsFunctionCalling { get; set; }

        /// <summary>
        /// Model có hỗ trợ streaming response không.
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Thông tin giá của model (nếu provider công khai).
        /// </summary>
        public ModelPricingInfo? Pricing { get; set; }

        /// <summary>
        /// Model đã deprecated / sắp ngừng hỗ trợ.
        /// </summary>
        public bool IsDeprecated { get; set; }
    }

    public enum AIModelCategory
    {
        Chat = 0,
        ImageGeneration = 1,
        TextToSpeech = 2,
        SpeechToText = 3,
        VideoGeneration = 4,
        Embedding = 5
    }

    /// <summary>
    /// Khai báo các năng lực mà 1 provider/engine hỗ trợ.
    /// Dùng để hệ thống biết cast IAIEngine sang interface con phù hợp.
    /// </summary>
    public class AIProviderCapabilities
    {
        public bool SupportsChat { get; set; }
        public bool SupportsStreaming { get; set; }
        public bool SupportsVision { get; set; }
        public bool SupportsFunctionCalling { get; set; }
        public bool SupportsImageGeneration { get; set; }
        public bool SupportsImageEditing { get; set; }
        public bool SupportsTextToSpeech { get; set; }
        public bool SupportsSpeechToText { get; set; }
        public bool SupportsVideoGeneration { get; set; }
        public bool SupportsEmbedding { get; set; }
    }

    /// <summary>
    /// Thông tin giá theo model, dùng để tính chi phí ước tính / đối soát.
    /// </summary>
    public class ModelPricingInfo
    {
        public string Model { get; set; } = default!;

        /// <summary>Giá cho 1 triệu input token (model chat/text).</summary>
        public decimal? InputPricePer1M { get; set; }

        /// <summary>Giá cho 1 triệu output token (model chat/text).</summary>
        public decimal? OutputPricePer1M { get; set; }

        /// <summary>Giá cho 1 ảnh sinh ra (model image generation).</summary>
        public decimal? PricePerImage { get; set; }

        /// <summary>Giá cho 1 phút audio (TTS/STT).</summary>
        public decimal? PricePerMinuteAudio { get; set; }

        /// <summary>Giá cho 1 giây video (video generation).</summary>
        public decimal? PricePerSecondVideo { get; set; }

        public string Currency { get; set; } = "USD";
    }

    /// <summary>
    /// Kết quả trả về chung khi 1 thao tác thất bại nhưng không muốn throw exception,
    /// dùng làm base class hoặc wrapper tùy nhu cầu.
    /// </summary>
    public class AIOperationError
    {
        /// <summary>Mã lỗi chuẩn hóa nội bộ hệ thống.</summary>
        public string? Code { get; set; }

        /// <summary>Thông báo lỗi gốc trả về từ provider.</summary>
        public string? Message { get; set; }

        /// <summary>HTTP status code trả về từ provider (nếu có).</summary>
        public int? HttpStatusCode { get; set; }
    }
}