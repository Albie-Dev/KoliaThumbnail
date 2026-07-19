namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Số thứ tự bước trong quy trình 5 bước của project
    /// </summary>
    public enum CProjectStepNumber
    {
        /// <summary>
        /// Phần 1: Nội dung video (Content Brief)
        /// </summary>
        ContentBrief = 1,

        /// <summary>
        /// Phần 2: Tin tức liên quan
        /// </summary>
        News = 2,

        /// <summary>
        /// Phần 3: Thumbnail tham khảo (Thumbnail Library)
        /// </summary>
        ThumbnailReference = 3,

        /// <summary>
        /// Phần 4: Generate thumbnail (Display Text + Tạo thumbnail)
        /// </summary>
        GenerateThumbnail = 4,

        /// <summary>
        /// Phần 5: Tạo Video Title
        /// </summary>
        VideoTitle = 5
    }
}
