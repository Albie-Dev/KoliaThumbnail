namespace Kolia.Thumbnail.API.Data.Entities
{
    /// <summary>
    /// Hằng số dùng chung cho mọi entity seed qua HasData trong toàn hệ thống,
    /// đảm bảo migration luôn deterministic (không đổi giữa các lần build).
    /// </summary>
    public static class SeedConstants
    {
        public static readonly DateTimeOffset FixedSeedTimestamp =
            new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}