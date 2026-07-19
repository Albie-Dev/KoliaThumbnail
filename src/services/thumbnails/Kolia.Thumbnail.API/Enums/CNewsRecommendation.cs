namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Nhãn đề xuất của AI cho từng tin tức
    /// </summary>
    public enum CNewsRecommendation
    {
        /// <summary>
        /// Nên chọn — tin có tiềm năng cao cho thumbnail
        /// </summary>
        ShouldSelect = 1,

        /// <summary>
        /// Có thể chọn — tin phù hợp nhưng không nổi bật
        /// </summary>
        CanSelect = 2,

        /// <summary>
        /// Không ưu tiên — tin ít liên quan hoặc không khai thác được cảm xúc
        /// </summary>
        NotPriority = 3
    }
}
