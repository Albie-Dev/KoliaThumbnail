namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Per-domain token bucket + cooldown registry (in-memory, singleton).
    /// Ensures at most 1 concurrent request per domain and enforces a minimum
    /// delay between consecutive requests to the same domain.
    /// Thread-safe: uses SemaphoreSlim per domain.
    /// </summary>
    public sealed class DomainRateLimiterRegistry
    {
        private readonly ILogger<DomainRateLimiterRegistry> _logger;

        // Per-domain: semaphore (max 1 concurrent) + timestamp of last release
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, DomainState> _states = new();

        // Minimum gap between two requests to the same domain (configurable via appsettings)
        private static readonly TimeSpan DefaultInterRequestDelay = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan CooldownAfter429 = TimeSpan.FromMinutes(5);

        public DomainRateLimiterRegistry(ILogger<DomainRateLimiterRegistry> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Acquires a slot for the given domain.
        /// Waits for the domain semaphore (concurrency=1) and then enforces the
        /// inter-request delay before returning.  Dispose the returned handle to
        /// release the semaphore and record the release timestamp.
        /// </summary>
        public async Task<IDisposable> AcquireAsync(string domain, CancellationToken ct)
        {
            var state = _states.GetOrAdd(domain, _ => new DomainState());

            await state.Semaphore.WaitAsync(ct);

            // Enforce minimum gap between requests
            var elapsed = DateTimeOffset.UtcNow - state.LastReleaseTime;
            if (elapsed < DefaultInterRequestDelay)
            {
                var waitMs = (int)(DefaultInterRequestDelay - elapsed).TotalMilliseconds;
                await Task.Delay(waitMs, ct);
            }

            return new ReleaseHandle(state);
        }

        /// <summary>
        /// Called when a 429 response is received.
        /// Sets a domain-level cooldown (respecting Retry-After header when provided).
        /// The circuit breaker (CircuitBreakerRegistry) handles whether to skip the domain
        /// altogether; this method only records the cooldown timestamp for metrics/logging.
        /// </summary>
        public void RecordRateLimited(string domain, TimeSpan? retryAfter = null)
        {
            var cooldown = retryAfter ?? CooldownAfter429;
            _logger.LogWarning(
                "Domain {Domain} returned 429 — cooldown for {Cooldown}.", domain, cooldown);

            var state = _states.GetOrAdd(domain, _ => new DomainState());
            state.CooldownUntil = DateTimeOffset.UtcNow.Add(cooldown);
        }

        /// <summary>
        /// Returns true if the domain is currently in a rate-limit cooldown window.
        /// Used by SourceFetchPipeline to fast-fail before even trying.
        /// </summary>
        public bool IsInCooldown(string domain)
        {
            if (!_states.TryGetValue(domain, out var state)) return false;
            return state.CooldownUntil.HasValue && DateTimeOffset.UtcNow < state.CooldownUntil.Value;
        }

        // ── Inner types ──────────────────────────────────────────────

        private sealed class DomainState
        {
            public SemaphoreSlim Semaphore { get; } = new(1, 1);
            public DateTimeOffset LastReleaseTime { get; set; } = DateTimeOffset.MinValue;
            public DateTimeOffset? CooldownUntil { get; set; }
        }

        private sealed class ReleaseHandle : IDisposable
        {
            private readonly DomainState _state;
            private bool _disposed;

            public ReleaseHandle(DomainState state) => _state = state;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _state.LastReleaseTime = DateTimeOffset.UtcNow;
                _state.Semaphore.Release();
            }
        }
    }
}
