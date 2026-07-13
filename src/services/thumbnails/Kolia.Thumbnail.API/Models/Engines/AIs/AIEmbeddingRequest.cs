namespace Kolia.Thumbnail.API.Engines
{
    /// <summary>
    /// Request tạo vector embedding - dùng cho search ngữ nghĩa, RAG, gợi ý nội dung tương tự.
    /// </summary>
    public class EmbeddingRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public List<string> Inputs { get; set; } = new();

        /// <summary>
        /// Mục đích sử dụng embedding (một số provider tối ưu vector khác nhau theo mục đích,
        /// vd Cohere: "search_document" vs "search_query").
        /// </summary>
        public EmbeddingInputType? InputType { get; set; }

        /// <summary>Số chiều vector mong muốn (nếu model hỗ trợ rút gọn chiều, vd OpenAI text-embedding-3).</summary>
        public int? Dimensions { get; set; }
    }

    public enum EmbeddingInputType
    {
        Document = 0,
        Query = 1
    }

    public class EmbeddingResult
    {
        public List<float[]> Vectors { get; set; } = new();
        public int Dimensions { get; set; }

        public int PromptTokens { get; set; }

        public string? ModelUsed { get; set; }
    }
}