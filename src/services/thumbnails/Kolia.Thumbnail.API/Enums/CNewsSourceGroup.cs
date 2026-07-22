namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Nhóm nguồn tin tức theo spec khách hàng (6 nhóm).
    /// </summary>
    public enum CNewsSourceGroup
    {
        /// <summary>Nguồn tin tài chính quốc tế (Bloomberg, Reuters, WSJ, FT, CNBC...)</summary>
        InternationalFinance = 1,

        /// <summary>Nguồn dữ liệu/chính thống (FED, FOMC, BLS, BEA, IMF, World Bank...)</summary>
        OfficialData = 2,

        /// <summary>Nguồn tin tài chính Việt Nam (CafeF, VnEconomy, Vietstock, SSI, MBS, VNDirect...)</summary>
        VietnamFinance = 3,

        /// <summary>Nguồn biểu đồ/thị trường (TradingView, Investing, Kitco, giá vàng...)</summary>
        ChartMarket = 4,

        /// <summary>YouTube và Google Trends</summary>
        YoutubeSearchTrend = 5
    }
}
