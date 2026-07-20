namespace Kolia.Thumbnail.API.DTOs.GoogleServices
{
    /// <summary>
    /// Một entry trong log của Scheduled Import Job.
    /// </summary>
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = "Info"; // Info, Warning, Error
        public string Message { get; set; } = null!;

        public static LogEntry Info(string message)
            => new() { Timestamp = DateTimeOffset.UtcNow, Level = "Info", Message = message };

        public static LogEntry Warning(string message)
            => new() { Timestamp = DateTimeOffset.UtcNow, Level = "Warning", Message = message };

        public static LogEntry Error(string message)
            => new() { Timestamp = DateTimeOffset.UtcNow, Level = "Error", Message = message };
    }
}
