#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CoolifyCli.Http;

/// <summary>
/// Wraps outbound Coolify API calls with retry and circuit-breaker behaviour so that
/// transient failures (connection resets, 502/503 during a deploy, rate limiting) do
/// not surface as raw exceptions to CLI callers.
/// </summary>
public sealed class ResiliencePolicy
{
    /// <summary>Maximum number of attempts made per logical request, including the first try.</summary>
    public const int MaxAttempts = 3;

    private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan MaxJitter = TimeSpan.FromMilliseconds(150);

    private readonly CircuitBreaker _circuitBreaker;
    private readonly Random _jitterSource = new();

    /// <summary>
    /// Initialises a new <see cref="ResiliencePolicy"/> backed by its own circuit breaker.
    /// </summary>
    /// <param name="failureThreshold">Consecutive failures before the circuit opens. Default: 5.</param>
    /// <param name="breakDuration">How long the circuit stays open. Default: 30 seconds.</param>
    public ResiliencePolicy(int failureThreshold = 5, TimeSpan? breakDuration = null) =>
        _circuitBreaker = new CircuitBreaker(failureThreshold, breakDuration ?? TimeSpan.FromSeconds(30));

    /// <summary>Gets the current state of the underlying circuit breaker.</summary>
    public CircuitBreakerState State => _circuitBreaker.State;

    /// <summary>
    /// Executes <paramref name="send"/> with retry-on-transient-failure and circuit-breaker
    /// protection. Retries up to <see cref="MaxAttempts"/> times on 408, 429, 5xx responses
    /// or <see cref="HttpRequestException"/>, using exponential backoff with jitter and
    /// honouring the <c>Retry-After</c> header on 429 responses. When the circuit is open,
    /// or all attempts are exhausted, <paramref name="onUnavailable"/> is invoked instead of
    /// throwing.
    /// </summary>
    /// <typeparam name="T">The result type produced by <paramref name="send"/> and <paramref name="onUnavailable"/>.</typeparam>
    /// <param name="send">Performs a single HTTP attempt and maps the response (or throws) into a result. Receives the per-attempt cancellation token.</param>
    /// <param name="perRequestTimeout">Timeout applied to each individual attempt, distinct from any overall operation timeout the caller enforces.</param>
    /// <param name="onUnavailable">Invoked with a human-readable reason when the call cannot be completed - either because the circuit is open or attempts were exhausted.</param>
    /// <param name="cancellationToken">Token that cancels the whole operation, including any pending retry delay.</param>
    /// <returns>The result of the first successful attempt, or the result of <paramref name="onUnavailable"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="send"/> or <paramref name="onUnavailable"/> is <c>null</c>.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<ResilientOutcome<T>>> send,
        TimeSpan perRequestTimeout,
        Func<string, T> onUnavailable,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(send);
        ArgumentNullException.ThrowIfNull(onUnavailable);

        if (!_circuitBreaker.TryAcquire())
        {
            return onUnavailable("Coolify API unreachable, circuit breaker open after repeated failures.");
        }

        Exception? lastException = null;
        TimeSpan? serverRequestedDelay = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attemptCts.CancelAfter(perRequestTimeout);

            try
            {
                var outcome = await send(attemptCts.Token).ConfigureAwait(false);

                if (!outcome.ShouldRetry)
                {
                    _circuitBreaker.OnSuccess();
                    return outcome.Result;
                }

                lastException = null;
                serverRequestedDelay = outcome.RetryAfter;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Per-attempt timeout fired, not the caller's overall cancellation.
                lastException = ex;
            }

            if (attempt == MaxAttempts)
            {
                break;
            }

            var delay = serverRequestedDelay ?? ComputeBackoffDelay(attempt);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        _circuitBreaker.OnFailure();

        return lastException is null
            ? onUnavailable($"Coolify API unreachable, retried {MaxAttempts}x.")
            : onUnavailable($"Coolify API unreachable, retried {MaxAttempts}x: {lastException.Message}");
    }

    private TimeSpan ComputeBackoffDelay(int attempt)
    {
        var exponential = BaseDelay * Math.Pow(2, attempt - 1);
        var jitter = TimeSpan.FromMilliseconds(_jitterSource.NextDouble() * MaxJitter.TotalMilliseconds);
        return exponential + jitter;
    }

    /// <summary>
    /// Determines whether an HTTP status code represents a transient failure worth
    /// retrying: request timeout (408), rate limiting (429), or any server error (5xx).
    /// </summary>
    /// <param name="statusCode">The response status code to evaluate.</param>
    /// <returns><c>true</c> if the status code is retryable; otherwise <c>false</c>.</returns>
    public static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.RequestTimeout ||
        statusCode == HttpStatusCode.TooManyRequests ||
        (int)statusCode >= 500;
}

/// <summary>
/// The result of a single HTTP attempt as evaluated by a <see cref="ResiliencePolicy"/>.
/// </summary>
/// <typeparam name="T">The mapped result type.</typeparam>
public readonly struct ResilientOutcome<T>
{
    /// <summary>Gets the mapped result to return when the attempt is not retried.</summary>
    public T Result { get; }

    /// <summary>Gets a value indicating whether the policy should retry this attempt.</summary>
    public bool ShouldRetry { get; }

    /// <summary>Gets the server-requested delay before the next attempt, from a <c>Retry-After</c> header, if any.</summary>
    public TimeSpan? RetryAfter { get; }

    private ResilientOutcome(T result, bool shouldRetry, TimeSpan? retryAfter)
    {
        Result = result;
        ShouldRetry = shouldRetry;
        RetryAfter = retryAfter;
    }

    /// <summary>Creates an outcome representing a final, non-retried result.</summary>
    /// <param name="result">The result to surface to the caller.</param>
    public static ResilientOutcome<T> Final(T result) => new(result, shouldRetry: false, retryAfter: null);

    /// <summary>Creates an outcome indicating the attempt failed transiently and should be retried.</summary>
    /// <param name="partialResult">The result computed for this attempt, kept in case it is the last one available.</param>
    /// <param name="retryAfter">An optional server-requested delay (e.g. from a 429 <c>Retry-After</c> header).</param>
    public static ResilientOutcome<T> Retry(T partialResult, TimeSpan? retryAfter = null) =>
        new(partialResult, shouldRetry: true, retryAfter: retryAfter);
}
