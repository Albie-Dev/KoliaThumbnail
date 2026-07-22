namespace Kolia.Thumbnail.API.Engines.Providers.Domain.Fetching
{
    /// <summary>
    /// Per-domain circuit breaker (Closed / Open / HalfOpen) — in-memory, singleton.
    /// Complements Polly's HTTP-level circuit breaker with domain-aware state
    /// so the orchestrator can skip entire domains without attempting a request.
    ///
    /// State machine:
    ///   Closed  → consecutive failures ≥ FailureThreshold  → Open
    ///   Open    → BreakDuration elapsed                    → HalfOpen
    ///   HalfOpen → next call succeeds                      → Closed
    ///   HalfOpen → next call fails                         → Open (reset timer)
    /// </summary>
    public sealed class CircuitBreakerRegistry
    {
        private readonly ILogger<CircuitBreakerRegistry> _logger;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CircuitState> _circuits = new();

        private const int FailureThreshold = 3;
        private static readonly TimeSpan BreakDuration = TimeSpan.FromMinutes(5);

        public CircuitBreakerRegistry(ILogger<CircuitBreakerRegistry> logger)
        {
            _logger = logger;
        }

        /// <summary>Returns true when the domain circuit is Open (should be skipped).</summary>
        public bool IsOpen(string domain)
        {
            if (!_circuits.TryGetValue(domain, out var state)) return false;

            if (state.Status == CircuitStatus.Open)
            {
                // Transition to HalfOpen if break duration elapsed
                if (DateTimeOffset.UtcNow >= state.OpenedAt + BreakDuration)
                {
                    state.Status = CircuitStatus.HalfOpen;
                    _logger.LogInformation("Circuit for {Domain} transitioned to HalfOpen.", domain);
                    return false; // allow one probe request
                }
                return true;
            }

            return false;
        }

        /// <summary>Records a successful fetch — resets failure count and closes the circuit.</summary>
        public void RecordSuccess(string domain)
        {
            var state = _circuits.GetOrAdd(domain, _ => new CircuitState());
            state.ConsecutiveFailures = 0;
            if (state.Status != CircuitStatus.Closed)
            {
                _logger.LogInformation("Circuit for {Domain} closed (recovered).", domain);
            }
            state.Status = CircuitStatus.Closed;
        }

        /// <summary>
        /// Records a failure.  Opens the circuit when FailureThreshold is reached.
        /// Should only be called after ALL tiers (including cache) have failed.
        /// </summary>
        public void RecordFailure(string domain)
        {
            var state = _circuits.GetOrAdd(domain, _ => new CircuitState());
            state.ConsecutiveFailures++;

            if (state.ConsecutiveFailures >= FailureThreshold && state.Status == CircuitStatus.Closed)
            {
                state.Status = CircuitStatus.Open;
                state.OpenedAt = DateTimeOffset.UtcNow;
                _logger.LogWarning(
                    "Circuit for {Domain} opened after {Failures} consecutive failures.",
                    domain, state.ConsecutiveFailures);
            }
            else if (state.Status == CircuitStatus.HalfOpen)
            {
                // Probe request failed → re-open
                state.Status = CircuitStatus.Open;
                state.OpenedAt = DateTimeOffset.UtcNow;
                _logger.LogWarning("Circuit for {Domain} re-opened (probe failed).", domain);
            }
        }

        // ── Inner types ──────────────────────────────────────────────

        private enum CircuitStatus { Closed, Open, HalfOpen }

        private sealed class CircuitState
        {
            public CircuitStatus Status { get; set; } = CircuitStatus.Closed;
            public int ConsecutiveFailures { get; set; }
            public DateTimeOffset OpenedAt { get; set; }
        }
    }
}
