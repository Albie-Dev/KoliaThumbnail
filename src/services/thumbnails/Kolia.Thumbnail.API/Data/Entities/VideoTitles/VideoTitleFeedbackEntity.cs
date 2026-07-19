namespace Kolia.Thumbnail.API.Data.Entities.VideoTitles
{
    /// <summary>
    /// Feedback của người dùng về kết quả title để AI học cho lần gen sau.
    /// Phân biệt "Gen lại thường" (không dùng feedback) vs "Gen lại theo feedback" (dùng entity này).
    /// </summary>
    public class VideoTitleFeedbackEntity : BaseEntity
    {
        /// <summary>
        /// Id yêu cầu Video Title cha
        /// </summary>
        public Guid VideoTitleRequestId { get; set; }

        /// <summary>
        /// Nội dung feedback, vd "Title quá dài, cần ngắn hơn 10 từ" hoặc "Cần có số liệu cụ thể hơn"
        /// </summary>
        public string FeedbackText { get; set; } = null!;

        /// <summary>
        /// Round mà feedback này được áp dụng khi "Gen lại theo feedback".
        /// 0 = chưa áp dụng.
        /// </summary>
        public int AppliedToRound { get; set; } = 0;

        // Navigation
        public virtual VideoTitleRequestEntity VideoTitleRequest { get; set; } = null!;
    }
}
