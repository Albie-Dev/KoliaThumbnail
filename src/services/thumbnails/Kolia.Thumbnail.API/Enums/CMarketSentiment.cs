using System.Text.Json.Serialization;

namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>Tâm lý tổng quan thị trường ở Tầng 4 — chỉ định tính, không có chỉ số bịa đặt.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CMarketSentiment
    {
        Optimistic = 1,  // Lạc quan
        Pessimistic = 2, // Bi quan
        Neutral = 3,     // Trung lập
        Mixed = 4        // Giằng co / trái chiều
    }
}
