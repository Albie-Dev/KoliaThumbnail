using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines.AI
{
    /// <summary>Một mục trong Tầng 1 — sự kiện vĩ mô theo từng hạng mục cố định.</summary>
    public sealed record MacroEventCategoryItem(string Category, string Content);

    /// <summary>Một mục trong Tầng 2 — phản ứng của một thị trường/nhóm liên quan.</summary>
    public sealed record MarketReactionItem(string MarketOrTopic, string Content);

    /// <summary>Tầng 4 — đánh giá tâm lý thị trường (định tính, KHÔNG bịa số liệu).</summary>
    public sealed record SentimentOverview(CMarketSentiment Sentiment, string Rationale);

    /// <summary>
    /// Kết quả phân tích sâu 4 tầng của 1 bản tin, đúng theo tài liệu nghiệp vụ:
    /// Tầng 1 (vĩ mô theo hạng mục) → Tầng 2 (phản ứng thị trường + ý kiến chuyên gia/NĐT)
    /// → Tầng 3 (kỳ vọng ngắn/dài hạn) → Tầng 4 (tâm lý tổng quan).
    /// </summary>
    public sealed record NewsDeepAnalysisResult(
        IReadOnlyList<MacroEventCategoryItem> MacroEventSummary,
        IReadOnlyList<MarketReactionItem> MarketReaction,
        string ExpectationShortTerm,   // Khung 1-3 tháng
        string ExpectationLongTerm,    // Khung 6-12 tháng
        SentimentOverview SentimentOverview,
        CEmotionTag EmotionTags,
        string EmotionReason,
        bool WasTranslatedFromForeign,
        string? MissingDataNote);

    /// <summary>
    /// Danh sách hạng mục cố định PHẢI xuất hiện đủ trong Tầng 1
    /// (ghi "Chưa rõ" nếu bài không đề cập). Thứ tự khớp ví dụ mẫu trong tài liệu nghiệp vụ.
    /// </summary>
    public static class MacroEventCategories
    {
        public const string GoldPrice = "Giá vàng";
        public const string Geopolitics = "Địa chính trị";
        public const string Forex = "Ngoại hối";
        public const string Energy = "Năng lượng";
        public const string MonetaryPolicy = "Chính sách tiền tệ";
        public const string StockMarket = "Chứng khoán";
        public const string RealEstate = "Bất động sản";
        public const string Other = "Khác";

        public static readonly IReadOnlyList<string> Fixed =
        [
            GoldPrice, Geopolitics, Forex, Energy, MonetaryPolicy, StockMarket, RealEstate
        ];
    }

    /// <summary>
    /// Engine AI phân tích sâu 4 tầng cho 1 bản tin (on-demand, sau khi team tick chọn tin).
    /// Input PHẢI là full-text bài báo (không phải meta description) — xem <see cref="Kolia.Thumbnail.API.Engines.Social.IArticleContentFetcher"/>.
    /// Mọi field chưa có dữ liệu PHẢI trả về "Chưa rõ", không để trống, không tự bịa số liệu.
    /// </summary>
    public interface INewsDeepAnalysisEngine
    {
        Task<NewsDeepAnalysisResult> AnalyzeAsync(
            string title, string sourceUrl, string sourceName,
            string fullArticleText, CMarketScope marketScope,
            CancellationToken ct = default);
    }
}
