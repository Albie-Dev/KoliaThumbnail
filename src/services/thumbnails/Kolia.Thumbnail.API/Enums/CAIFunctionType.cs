namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Định danh tất cả chức năng nghiệp vụ có thể sử dụng AI trong hệ thống.
    /// Mỗi chức năng sẽ được cấu hình provider, model, config và fallback riêng
    /// thông qua entity <see cref="Data.Entities.AIs.AIFunctionConfigEntity"/>.
    /// </summary>
    public enum CAIFunctionType
    {
        /// <summary>Phân tích nội dung livestream/video → Topic, MainMessage, HighlightData</summary>
        ContentBriefAnalysis = 1,

        /// <summary>Chấm điểm tin tức dựa trên chủ đề (Relevance, Sentiment, Priority)</summary>
        NewsScoring = 2,

        /// <summary>Sinh thumbnail từ nội dung video</summary>
        ThumbnailGeneration = 3,

        /// <summary>Sinh display text cho thumbnail</summary>
        DisplayTextGeneration = 4,

        /// <summary>Sinh tiêu đề video</summary>
        VideoTitleGeneration = 5,

        /// <summary>Sinh gói hoàn chỉnh (thumbnail + text + title)</summary>
        CompletePackageGeneration = 6,
    }
}
