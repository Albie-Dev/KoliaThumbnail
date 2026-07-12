namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Request sinh video từ văn bản (text-to-video).
    /// </summary>
    public class VideoGenerationRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string Prompt { get; set; } = default!;
        public string? NegativePrompt { get; set; }

        public int DurationSeconds { get; set; } = 5;

        /// <summary>vd: "1920x1080", hoặc null để dùng mặc định của model.</summary>
        public string? Resolution { get; set; } = "1080p";

        public string? AspectRatio { get; set; } = "16:9";

        /// <summary>Số khung hình/giây mong muốn.</summary>
        public int? Fps { get; set; }

        /// <summary>Seed để tái tạo kết quả (nếu provider hỗ trợ).</summary>
        public long? Seed { get; set; }

        /// <summary>Camera motion gợi ý (vd: "pan-left", "zoom-in") - 1 số provider hỗ trợ riêng.</summary>
        public string? CameraMotion { get; set; }

        /// <summary>Có kèm audio/nhạc nền tự sinh không (vd: Google Veo, Sora hỗ trợ).</summary>
        public bool GenerateAudio { get; set; }
    }

    /// <summary>
    /// Request sinh video từ 1 ảnh có sẵn (image-to-video) - phổ biến cho thumbnail động/intro.
    /// </summary>
    public class ImageToVideoRequest : VideoGenerationRequest
    {
        public byte[] SourceImageBytes { get; set; } = default!;

        /// <summary>Ảnh khung hình cuối mong muốn (nếu provider hỗ trợ chỉ định first+last frame).</summary>
        public byte[]? EndImageBytes { get; set; }
    }

    /// <summary>
    /// Kết quả khởi tạo job sinh video. Video luôn xử lý bất đồng bộ nên trả về JobId để poll.
    /// </summary>
    public class VideoGenerationResult
    {
        public string JobId { get; set; } = default!;
        public string? VideoUrl { get; set; }
        public VideoGenerationStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Trạng thái job sinh video, dùng cho polling.
    /// </summary>
    public class VideoGenerationJobStatus
    {
        public string JobId { get; set; } = default!;
        public VideoGenerationStatus Status { get; set; }

        /// <summary>Tiến độ xử lý (0-100), nếu provider trả về.</summary>
        public int ProgressPercent { get; set; }

        public string? VideoUrl { get; set; }

        /// <summary>URL ảnh thumbnail xem trước của video (nếu provider tạo sẵn).</summary>
        public string? ThumbnailUrl { get; set; }

        public double? DurationSeconds { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>Thời điểm ước tính hoàn thành (nếu provider trả về).</summary>
        public DateTime? EstimatedCompletionAt { get; set; }
    }

    public enum VideoGenerationStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}