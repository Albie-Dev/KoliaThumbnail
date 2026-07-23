using System.Collections.Concurrent;

namespace Kolia.Thumbnail.API.Services.News
{
    /// <summary>
    /// Lưu trữ tiến trình và log thời gian thực của các operation dài (search, deep analyze).
    /// Dùng với SSE endpoint để FE stream log realtime.
    /// </summary>
    public class OperationProgressStore
    {
        private readonly ConcurrentDictionary<Guid, OperationProgress> _store = new();

        public OperationProgress Create(Guid operationId, string title)
        {
            var entry = new OperationProgress
            {
                OperationId = operationId,
                Title = title,
                Status = "running",
                Logs = [],
                StartedAt = DateTimeOffset.UtcNow
            };
            _store[operationId] = entry;
            return entry;
        }

        public OperationProgress? Get(Guid operationId) =>
            _store.TryGetValue(operationId, out var entry) ? entry : null;

        public void AppendLog(Guid operationId, string message, bool isError = false)
        {
            if (_store.TryGetValue(operationId, out var entry))
            {
                var log = new ProgressLog
                {
                    Message = message,
                    IsError = isError,
                    Timestamp = DateTimeOffset.UtcNow
                };
                entry.Logs.Add(log);
                entry.LastUpdated = DateTimeOffset.UtcNow;
            }
        }

        public void Complete(Guid operationId, string? errorMessage = null)
        {
            if (_store.TryGetValue(operationId, out var entry))
            {
                entry.Status = errorMessage == null ? "completed" : "failed";
                entry.ErrorMessage = errorMessage;
                entry.LastUpdated = DateTimeOffset.UtcNow;
            }
        }

        /// <summary>Dọn dẹp các operation cũ hơn 5 phút.</summary>
        public void Cleanup()
        {
            var cutoff = DateTimeOffset.UtcNow.AddMinutes(-5);
            foreach (var kv in _store)
            {
                if (kv.Value.StartedAt < cutoff)
                    _store.TryRemove(kv.Key, out _);
            }
        }
    }

    public class OperationProgress
    {
        public Guid OperationId { get; set; }
        public string Title { get; set; } = "";
        public string Status { get; set; } = "running"; // running | completed | failed
        public List<ProgressLog> Logs { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }

    public class ProgressLog
    {
        public string Message { get; set; } = "";
        public bool IsError { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
