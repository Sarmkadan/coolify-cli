#nullable enable
using System;
using System.Threading;

namespace CoolifyCli.Http;

/// <summary>
/// The observable state of a <see cref="CircuitBreaker"/>.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Calls are allowed through; failures are being counted.</summary>
    Closed,

    /// <summary>Calls are being rejected immediately until the break duration elapses.</summary>
    Open,

    /// <summary>The break duration elapsed; a single trial call is allowed through.</summary>
    HalfOpen
}

/// <summary>
/// A minimal thread-safe circuit breaker used to stop hammering a Coolify instance
/// that is already failing. After a configurable number of consecutive failures the
/// circuit trips ("opens") and every call is short-circuited for a configurable
/// duration, after which a single trial call is allowed through ("half-open") to
/// probe recovery.
/// </summary>
public sealed class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;
    private readonly object _gate = new();

    private int _consecutiveFailures;
    private DateTimeOffset _openedUntil = DateTimeOffset.MinValue;
    private bool _halfOpenTrialInFlight;

    /// <summary>
    /// Initialises a new <see cref="CircuitBreaker"/>.
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures required to trip the circuit. Must be greater than zero.</param>
    /// <param name="breakDuration">How long the circuit stays open before allowing a trial call.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="failureThreshold"/> is not positive or <paramref name="breakDuration"/> is negative.</exception>
    public CircuitBreaker(int failureThreshold, TimeSpan breakDuration)
    {
        if (failureThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be greater than zero.");
        }

        if (breakDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(breakDuration), "Break duration cannot be negative.");
        }

        _failureThreshold = failureThreshold;
        _breakDuration = breakDuration;
    }

    /// <summary>Gets the current state of the circuit.</summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_gate)
            {
                return CurrentStateNoLock();
            }
        }
    }

    /// <summary>
    /// Determines whether a call is currently allowed to proceed. When the circuit is
    /// open and the break duration has not yet elapsed, the call is rejected. When the
    /// break duration has elapsed, exactly one caller is admitted as a half-open trial;
    /// concurrent callers are rejected until that trial completes via
    /// <see cref="OnSuccess"/> or <see cref="OnFailure"/>.
    /// </summary>
    /// <returns><c>true</c> if the call may proceed; otherwise <c>false</c>.</returns>
    public bool TryAcquire()
    {
        lock (_gate)
        {
            return CurrentStateNoLock() switch
            {
                CircuitBreakerState.Closed => true,
                CircuitBreakerState.HalfOpen when !_halfOpenTrialInFlight => SetHalfOpenTrial(),
                _ => false
            };
        }
    }

    /// <summary>
    /// Records a successful call, resetting the failure count and closing the circuit.
    /// </summary>
    public void OnSuccess()
    {
        lock (_gate)
        {
            _consecutiveFailures = 0;
            _openedUntil = DateTimeOffset.MinValue;
            _halfOpenTrialInFlight = false;
        }
    }

    /// <summary>
    /// Records a failed call. Trips the circuit open once the configured failure
    /// threshold is reached; re-opens it immediately if a half-open trial call fails.
    /// </summary>
    public void OnFailure()
    {
        lock (_gate)
        {
            _halfOpenTrialInFlight = false;
            _consecutiveFailures++;

            if (_consecutiveFailures >= _failureThreshold)
            {
                _openedUntil = DateTimeOffset.UtcNow + _breakDuration;
            }
        }
    }

    private CircuitBreakerState CurrentStateNoLock()
    {
        if (_openedUntil == DateTimeOffset.MinValue)
        {
            return CircuitBreakerState.Closed;
        }

        return DateTimeOffset.UtcNow >= _openedUntil
            ? CircuitBreakerState.HalfOpen
            : CircuitBreakerState.Open;
    }

    private bool SetHalfOpenTrial()
    {
        _halfOpenTrialInFlight = true;
        return true;
    }
}
