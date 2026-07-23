#nullable enable
using System;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Configuration for the retry and circuit-breaker behavior applied to Coolify API calls
/// by <see cref="ResilientHttpHandler"/>.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Total number of attempts per request, including the initial one. Default: 3.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay used for exponential backoff between retries. The actual delay is
    /// <c>BaseDelay * 2^(attempt-1)</c> plus random jitter. Default: 500 ms.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Upper bound for any single backoff delay (including a server-supplied
    /// <c>Retry-After</c> value). Default: 15 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Timeout applied to each individual attempt, independent of the overall
    /// per-operation timeout enforced by <c>CoolifyApiClient</c>. An attempt that
    /// exceeds this limit is treated as a transient failure and retried. Default: 20 seconds.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Number of consecutive failed attempts after which the circuit opens and
    /// requests fail fast without reaching the network. Default: 5.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// How long the circuit stays open before a single trial request is allowed
    /// through (half-open state). Default: 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}
