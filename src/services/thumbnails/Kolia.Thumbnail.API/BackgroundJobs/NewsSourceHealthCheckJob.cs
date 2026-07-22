using Cronos;
using Kolia.Thumbnail.API.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.BackgroundJobs
{
    /// <summary>
    /// [Phase 2] Background job that proactively checks the health of every NewsSource URL
    /// every 6 hours and auto-disables sources that fail 3 times consecutively.
    ///
    /// Rationale: Without proactive health checks, a dead source is discovered only when a
    /// research request fails — too late. This job shifts detection from reactive to proactive.
    ///
    /// Schedule: every 6 hours (0 */6 * * *)
    /// </summary>
    public sealed class NewsSourceHealthCheckJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NewsSourceHealthCheckJob> _logger;

        private static readonly CronExpression Schedule =
            CronExpression.Parse("0 */6 * * *", CronFormat.Standard);

        private const int ConsecutiveFailureThreshold = 3;

        public NewsSourceHealthCheckJob(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<NewsSourceHealthCheckJob> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NewsSourceHealthCheckJob started. Schedule: every 6 hours.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTimeOffset.UtcNow.UtcDateTime;
                var next = Schedule.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);
                if (next == null) break;

                var delay = next.Value - utcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await RunHealthCheckAsync(stoppingToken);
            }

            _logger.LogInformation("NewsSourceHealthCheckJob stopped.");
        }

        private async Task RunHealthCheckAsync(CancellationToken ct)
        {
            _logger.LogInformation("NewsSourceHealthCheckJob: starting health check run.");

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();

                // Only check trusted sources (disabled ones are already skipped by the engine)
                var sources = await db.NewsSources
                    .Where(s => s.IsTrusted && !s.IsDeleted)
                    .ToListAsync(ct);

                var client = _httpClientFactory.CreateClient("HealthCheck");
                var changed = false;

                foreach (var source in sources)
                {
                    if (ct.IsCancellationRequested) break;
                    try
                    {
                        // Use HEAD first; fall back to GET if server doesn't support it
                        var request = new HttpRequestMessage(HttpMethod.Head, source.RssOrFeedUrl);
                        var response = await client.SendAsync(request, ct);

                        if (!response.IsSuccessStatusCode &&
                            response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed)
                        {
                            source.ConsecutiveFailureCount++;
                            source.LastFailedAt = DateTimeOffset.UtcNow;

                            if (source.ConsecutiveFailureCount >= ConsecutiveFailureThreshold)
                            {
                                source.IsTrusted = false;
                                _logger.LogWarning(
                                    "NewsSourceHealthCheckJob: auto-disabled {Name} ({Domain}) after {Count} consecutive failures.",
                                    source.Name, source.Domain, source.ConsecutiveFailureCount);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "NewsSourceHealthCheckJob: {Name} ({Domain}) failed check " +
                                    "({Count}/{Threshold}).",
                                    source.Name, source.Domain,
                                    source.ConsecutiveFailureCount, ConsecutiveFailureThreshold);
                            }
                            changed = true;
                        }
                        else
                        {
                            // Healthy — reset counter
                            if (source.ConsecutiveFailureCount > 0)
                            {
                                source.ConsecutiveFailureCount = 0;
                                changed = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "NewsSourceHealthCheckJob: exception pinging {Domain}.", source.Domain);
                        source.ConsecutiveFailureCount++;
                        source.LastFailedAt = DateTimeOffset.UtcNow;
                        changed = true;
                    }

                    // Small delay between pings to avoid hammering all sources at once
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }

                if (changed) await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "NewsSourceHealthCheckJob: completed. Checked {Count} sources.", sources.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NewsSourceHealthCheckJob: unexpected error during health check.");
            }
        }
    }
}
