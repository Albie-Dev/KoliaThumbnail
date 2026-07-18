namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Request sinh ảnh mới từ văn bản (text-to-image).
    /// </summary>
    public class ImageGenerationRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string Prompt { get; set; } = default!;
        public string? NegativePrompt { get; set; }

        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 1024;

        /// <summary>Số lượng ảnh sinh ra trong 1 lần gọi.</summary>
        public int NumberOfImages { get; set; } = 1;

        /// <summary>Seed để tái tạo kết quả (nếu provider hỗ trợ).</summary>
        public long? Seed { get; set; }

        /// <summary>Mức độ bám sát prompt (guidance scale / cfg scale).</summary>
        public double? GuidanceScale { get; set; }

        /// <summary>Số bước khử nhiễu (diffusion steps) - ảnh hưởng chất lượng vs tốc độ.</summary>
        public int? Steps { get; set; }

        /// <summary>Style preset (nếu provider có, vd: "photographic", "anime", "3d-model"...).</summary>
        public string? StylePreset { get; set; }

        /// <summary>Định dạng ảnh output.</summary>
        public ImageOutputFormat OutputFormat { get; set; } = ImageOutputFormat.Png;
    }

    /// <summary>
    /// Request sửa/biến thể ảnh có sẵn (image-to-image, inpainting, outpainting).
    /// </summary>
    public class ImageEditRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string Prompt { get; set; } = default!;

        /// <summary>Ảnh gốc cần chỉnh sửa.</summary>
        public byte[] SourceImageBytes { get; set; } = default!;

        /// <summary>
        /// Mask vùng cần sửa (trắng = sửa, đen = giữ nguyên) - dùng cho inpainting.
        /// Để null nếu là image-to-image toàn ảnh.
        /// </summary>
        public byte[]? MaskImageBytes { get; set; }

        /// <summary>Mức độ giữ lại ảnh gốc (0 = giữ nguyên hoàn toàn, 1 = tạo mới hoàn toàn).</summary>
        public double? Strength { get; set; }

        public int NumberOfImages { get; set; } = 1;

        public ImageEditMode Mode { get; set; } = ImageEditMode.ImageToImage;
    }

    public enum ImageEditMode
    {
        ImageToImage = 0,
        Inpainting = 1,
        Outpainting = 2,
        BackgroundRemoval = 3,
        Upscale = 4
    }

    public enum ImageOutputFormat
    {
        Png = 0,
        Jpeg = 1,
        Webp = 2
    }

    /// <summary>
    /// Kết quả sinh/sửa ảnh.
    /// </summary>
    public class ImageGenerationResult
    {
        public List<GeneratedImageItem> Images { get; set; } = new();

        /// <summary>Model thực tế đã xử lý (một số provider tự fallback sang model khác).</summary>
        public string? ModelUsed { get; set; }

        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    public class GeneratedImageItem
    {
        /// <summary>URL ảnh nếu provider host sẵn (thường có hạn sử dụng).</summary>
        public string? Url { get; set; }

        /// <summary>Dữ liệu ảnh trả về trực tiếp (nếu provider trả base64/binary).</summary>
        public byte[]? ImageBytes { get; set; }

        /// <summary>Seed thực tế đã dùng để sinh ảnh này (phục vụ tái tạo lại).</summary>
        public long? Seed { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Lý do ảnh bị chặn/lọc bởi content filter của provider (nếu có, ảnh sẽ null).
        /// </summary>
        public string? ContentFilterReason { get; set; }
    }
}