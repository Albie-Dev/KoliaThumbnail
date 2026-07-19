namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Tag cảm xúc có thể khai thác từ tin tức (Flags enum — có thể kết hợp nhiều cảm xúc)
    /// </summary>
    [Flags]
    public enum CEmotionTag
    {
        /// <summary>
        /// Không có cảm xúc đặc biệt
        /// </summary>
        None = 0,

        /// <summary>
        /// Sợ hãi — lo ngại về tổn thất tài chính
        /// </summary>
        Fear = 1,

        /// <summary>
        /// Nghi ngờ — không chắc chắn về thông tin
        /// </summary>
        Doubt = 2,

        /// <summary>
        /// Tò mò — muốn tìm hiểu thêm
        /// </summary>
        Curiosity = 4,

        /// <summary>
        /// Khẩn cấp — cần hành động ngay
        /// </summary>
        Urgency = 8,

        /// <summary>
        /// Áp lực quyết định — phải chọn lựa sớm
        /// </summary>
        DecisionPressure = 16,

        /// <summary>
        /// Ngạc nhiên — thông tin bất ngờ
        /// </summary>
        Surprise = 32,

        /// <summary>
        /// Tức giận — phẫn nộ về diễn biến thị trường
        /// </summary>
        Anger = 64,

        /// <summary>
        /// Hy vọng — kỳ vọng tích cực về tương lai
        /// </summary>
        Hope = 128
    }
}
