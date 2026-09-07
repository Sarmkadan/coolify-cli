#nullable enable
using System.Collections.Concurrent;
using CoolifyCli.Http;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the CircuitBreaker state transitions and thread safety.
/// </summary>
public class CircuitBreakerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectNonPositiveFailureThreshold(int failureThreshold)
    {
        Action act = () => new CircuitBreaker(failureThreshold, TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(failureThreshold));
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeBreakDuration()
    {
        Action act = () => new CircuitBreaker(1, TimeSpan.FromMilliseconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("breakDuration");
    }

    [Fact]
    public void OnFailure_ShouldStayClosed_BelowFailureThreshold()
    {
        var breaker = new CircuitBreaker(3, TimeSpan.FromSeconds(1));

        breaker.OnFailure();
        breaker.OnFailure();

        breaker.State.Should().Be(CircuitBreakerState.Closed);
        breaker.TryAcquire().Should().BeTrue();
    }

    [Fact]
    public void OnFailure_ShouldOpenCircuit_WhenFailureThresholdIsReached()
    {
        var breaker = new CircuitBreaker(3, TimeSpan.FromSeconds(1));

        breaker.OnFailure();
        breaker.OnFailure();
        breaker.OnFailure();

        breaker.State.Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public void TryAcquire_ShouldRejectCalls_WhileCircuitIsOpen()
    {
        var breaker = new CircuitBreaker(1, TimeSpan.FromSeconds(1));
        breaker.OnFailure();

        breaker.TryAcquire().Should().BeFalse();
        breaker.TryAcquire().Should().BeFalse();
        breaker.State.Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public async Task State_ShouldBecomeHalfOpen_AfterBreakDuration()
    {
        var breaker = CreateOpenBreaker();

        await WaitForHalfOpenAsync(breaker);

        breaker.State.Should().Be(CircuitBreakerState.HalfOpen);
        breaker.TryAcquire().Should().BeTrue();
        breaker.TryAcquire().Should().BeFalse();
    }

    [Fact]
    public async Task OnSuccess_ShouldCloseCircuit_AfterSuccessfulTrialCall()
    {
        var breaker = CreateOpenBreaker();
        await WaitForHalfOpenAsync(breaker);
        breaker.TryAcquire().Should().BeTrue();

        breaker.OnSuccess();

        breaker.State.Should().Be(CircuitBreakerState.Closed);
        breaker.TryAcquire().Should().BeTrue();
    }

    [Fact]
    public async Task OnFailure_ShouldReopenCircuit_AfterFailedTrialCall()
    {
        var breaker = CreateOpenBreaker();
        await WaitForHalfOpenAsync(breaker);
        breaker.TryAcquire().Should().BeTrue();

        breaker.OnFailure();

        breaker.State.Should().Be(CircuitBreakerState.Open);
        breaker.TryAcquire().Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentCalls_ShouldBeThreadSafe_WithOnlyExpectedRejections()
    {
        var breaker = new CircuitBreaker(1, TimeSpan.FromSeconds(5));
        var exceptions = new ConcurrentQueue<Exception>();
        var rejectionCount = 0;

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                try
                {
                    if (breaker.TryAcquire())
                    {
                        breaker.OnFailure();
                    }
                    else
                    {
                        Interlocked.Increment(ref rejectionCount);
                    }
                }
                catch (Exception exception)
                {
                    exceptions.Enqueue(exception);
                }
            }
        }));

        await Task.WhenAll(tasks);

        exceptions.Should().BeEmpty();
        rejectionCount.Should().BeGreaterThan(0);
        breaker.State.Should().Be(CircuitBreakerState.Open);
    }

    private static CircuitBreaker CreateOpenBreaker()
    {
        var breaker = new CircuitBreaker(1, TimeSpan.FromMilliseconds(50));
        breaker.OnFailure();
        return breaker;
    }

    private static async Task WaitForHalfOpenAsync(CircuitBreaker breaker)
    {
        var timeout = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(2);

        while (breaker.State != CircuitBreakerState.HalfOpen && DateTimeOffset.UtcNow < timeout)
        {
            await Task.Delay(10);
        }
    }
}
