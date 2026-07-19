namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Nguồn nhập dữ liệu nội dung cho Content Brief (Phần 1)
    /// </summary>
    public enum CImportContentSource
    {
        /// <summary>
        /// Dán trực tiếp văn bản vào ô nhập
        /// </summary>
        PasteText = 1,

        /// <summary>
        /// Upload file (PDF, Word, ảnh chụp màn hình...)
        /// </summary>
        File = 2,

        /// <summary>
        /// Nhập link ngoài để AI tự đọc (Google Sheet, bài viết, video...)
        /// </summary>
        ExternalLink = 3
    }
}
