using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Data.Entities.AIs
{
    /// <summary>
    /// Cấu hình AI cho một chức năng nghiệp vụ cụ thể.
    /// Xác định provider nào, config nào, model nào sẽ được dùng,
    /// và danh sách fallback nếu config chính thất bại.
    /// </summary>
    public class AIFunctionConfigEntity : BaseEntity
    {
        /// <summary>
        /// Chức năng nghiệp vụ (ContentBriefAnalysis, NewsScoring, ...).
        /// </summary>
        public CAIFunctionType FunctionType { get; set; }

        /// <summary>
        /// Model mặc định cho chức năng này (vd: "gemini-2.0-flash", "gpt-4o").
        /// Có thể được ghi đè bởi từng item trong <see cref="Items"/>.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Temperature mặc định (0.0 - 1.0).
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Max output tokens mặc định.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Danh sách các cấu hình provider+config+model cho chức năng này,
        /// sắp xếp theo Priority (0 = primary, 1+ = fallback).
        /// </summary>
        public virtual ICollection<AIFunctionConfigItemEntity> Items { get; set; }
            = new List<AIFunctionConfigItemEntity>();
    }
}
