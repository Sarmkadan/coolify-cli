#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Delegating handler that adds transparent resilience to Coolify API calls:
/// retries transient failures (HTTP 408/429/5xx and connection errors) with
/// exponential backoff and jitter, honors <c>Retry-After</c> on 429 responses,
/// enforces a per-attempt timeout distinct from the overall operation timeout,
/// and trips a circuit breaker after repeated consecutive failures so a dead
/// server fails fast instead of hanging every command.
/// </summary>
public sealed class ResilientHttpHandler : DelegatingHandler
{
    private readonly ResilienceOptions _options;
    private readonly object _circuitLock = new();

    private int _consecutiveFailures;
    private DateTimeOffset _circuitOpenedUntil = DateTimeOffset.MinValue;
    private bool _halfOpenTrialInFlight;

    /// <summary>
    /// Creates the handler around an inner HTTP handler.
    /// </summary>
    /// <param name="innerHandler">Handler that performs the actual network I/O.</param>
    /// <param name="options">Resilience settings; defaults are used when null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerHandler"/> is null.</exception>
    public ResilientHttpHandler(HttpMessageHandler innerHandler, ResilienceOptions? options = null)
        : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
        => _options = options ?? new ResilienceOptions();

    /// <summary>
    /// Sends the request, retrying transient failures up to the configured attempt count.
    /// </summary>
    /// <param name="request">Outgoing HTTP request.</param>
    /// <param name="cancellationToken">Token bounding the overall operation.</param>
    /// <returns>The first non-transient (or final) HTTP response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when every attempt failed with a connection error or per-attempt timeout,
    /// or when the circuit breaker is open and the request is rejected without being sent.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled (overall timeout or user abort).
    /// </exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ThrowIfCircuitOpen();

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await SendAttemptAsync(request, cancellationToken);

                if (!IsTransientStatus(response.StatusCode))
                {
                    OnSuccess();
                    return response;
                }

                OnFailure();

                if (attempt == _options.MaxAttempts)
                {
                    // Out of retries: surface the transient status to the caller,
                    // which maps it to a structured API error response.
                    return response;
                }

                var delay = ComputeDelay(attempt, response);
                response.Dispose();
                await Task.Delay(delay, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                OnFailure();
                lastException = ex;

                if (attempt < _options.MaxAttempts)
                {
                    await Task.Delay(ComputeDelay(attempt, response: null), cancellationToken);
                }
            }
            catch (TimeoutException ex)
            {
                OnFailure();
                lastException = ex;

                if (attempt < _options.MaxAttempts)
                {
                    await Task.Delay(ComputeDelay(attempt, response: null), cancellationToken);
                }
            }
        }

        throw new HttpRequestException(
            $"Coolify API unreachable, retried {_options.MaxAttempts}x: {lastException?.Message}",
            lastException);
    }

    private async Task<HttpResponseMessage> SendAttemptAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        attemptCts.CancelAfter(_options.AttemptTimeout);

        try
        {
            return await base.SendAsync(request, attemptCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Only the per-attempt timer fired; convert to a retryable timeout.
            throw new TimeoutException(
                $"attempt exceeded per-request timeout of {_options.AttemptTimeout.TotalSeconds:0}s");
        }
    }

    private static bool IsTransientStatus(HttpStatusCode status) => status
        is HttpStatusCode.RequestTimeout          // 408
        or HttpStatusCode.TooManyRequests         // 429
        or >= HttpStatusCode.InternalServerError; // 5xx

    private TimeSpan ComputeDelay(int attempt, HttpResponseMessage? response)
    {
        // Honor Retry-After on 429 (either delta-seconds or an absolute date).
        if (response is { StatusCode: HttpStatusCode.TooManyRequests, Headers.RetryAfter: { } retryAfter })
        {
            TimeSpan? serverDelay = retryAfter.Delta
                ?? (retryAfter.Date is { } date ? date - DateTimeOffset.UtcNow : null);

            if (serverDelay is { } wait && wait > TimeSpan.Zero)
            {
                return wait <= _options.MaxDelay ? wait : _options.MaxDelay;
            }
        }

        double backoffMs = _options.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        double jitterMs = Random.Shared.NextDouble() * _options.BaseDelay.TotalMilliseconds;
        double totalMs = Math.Min(backoffMs + jitterMs, _options.MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(totalMs);
    }

    private void ThrowIfCircuitOpen()
    {
        lock (_circuitLock)
        {
            var now = DateTimeOffset.UtcNow;

            if (now < _circuitOpenedUntil)
            {
                var remaining = _circuitOpenedUntil - now;
                throw new HttpRequestException(
                    $"Coolify API unreachable, retried {_options.MaxAttempts}x; " +
                    $"circuit breaker is open for another {remaining.TotalSeconds:0}s after " +
                    $"{_options.CircuitBreakerThreshold} consecutive failures.");
            }

            // Break window elapsed: half-open. Let exactly one trial request through;
            // reject concurrent callers until it resolves.
            if (_consecutiveFailures >= _options.CircuitBreakerThreshold)
            {
                if (_halfOpenTrialInFlight)
                {
                    throw new HttpRequestException(
                        "Coolify API circuit breaker is half-open; a trial request is already in flight.");
                }

                _halfOpenTrialInFlight = true;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_circuitLock)
        {
            _consecutiveFailures = 0;
            _halfOpenTrialInFlight = false;
            _circuitOpenedUntil = DateTimeOffset.MinValue;
        }
    }

    private void OnFailure()
    {
        lock (_circuitLock)
        {
            _consecutiveFailures++;
            _halfOpenTrialInFlight = false;

            if (_consecutiveFailures >= _options.CircuitBreakerThreshold)
            {
                _circuitOpenedUntil = DateTimeOffset.UtcNow + _options.CircuitBreakDuration;
            }
        }
    }
}
