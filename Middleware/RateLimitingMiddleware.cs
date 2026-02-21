#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Services;

namespace CoolifiCli.Middleware;

/// <summary>
/// Middleware for rate limiting. Implements token-bucket algorithm to prevent
/// excessive API calls. Tracks requests per user and enforces limits per time window.
/// Helps prevent abuse and ensures fair resource usage across CLI users.
/// </summary>
public class RateLimitingMiddleware : ICommandMiddleware
{
    private readonly ILogger _logger;
    private readonly int _maxRequestsPerMinute;
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly object _lockObject = new();

    public RateLimitingMiddleware(ILogger logger, int maxRequestsPerMinute = 100)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    /// <summary>
    /// Checks rate limit for the current user/API key before allowing command execution.
    /// Uses token bucket algorithm with per-minute refill.
    /// </summary>
    public async Task ProcessAsync(CommandContext context, CommandMiddlewareDelegate next)
    {
        // Skip rate limiting for unauthenticated commands
        if (!context.IsAuthenticated)
        {
            await next(context);
            return;
        }

        var userId = context.AuthenticatedUser ?? "anonymous";
        var bucket = GetOrCreateBucket(userId);

        if (!bucket.TryConsumeToken())
        {
            var waitSeconds = bucket.GetWaitTimeSeconds();
            throw new RateLimitExceededException(
                $"Rate limit exceeded. Maximum {_maxRequestsPerMinute} requests per minute. " +
                $"Try again in {waitSeconds} seconds.");
        }

        context.RequestCount = bucket.GetConsumedTokens();
        _logger.Debug($"Rate limit check passed for {userId} ({context.RequestCount}/{_maxRequestsPerMinute})");

        await next(context);
    }

    /// <summary>
    /// Gets or creates a token bucket for the given user.
    /// </summary>
    private TokenBucket GetOrCreateBucket(string userId)
    {
        lock (_lockObject)
        {
            if (!_buckets.ContainsKey(userId))
            {
                _buckets[userId] = new TokenBucket(_maxRequestsPerMinute, TimeSpan.FromMinutes(1));
            }

            return _buckets[userId];
        }
    }

    /// <summary>
    /// Internal class implementing token bucket algorithm for rate limiting.
    /// </summary>
    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly TimeSpan _refillWindow;
        private int _tokens;
        private DateTime _lastRefillTime;

        public TokenBucket(int capacity, TimeSpan refillWindow)
        {
            _capacity = capacity;
            _tokens = capacity;
            _refillWindow = refillWindow;
            _lastRefillTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Attempts to consume one token. Refills bucket based on elapsed time.
        /// </summary>
        public bool TryConsumeToken()
        {
            RefillTokens();

            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refills tokens based on elapsed time since last refill.
        /// </summary>
        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefillTime;

            if (elapsed >= _refillWindow)
            {
                _tokens = _capacity;
                _lastRefillTime = now;
            }
            else
            {
                var tokensToAdd = (int)(_capacity * (elapsed.TotalMilliseconds / _refillWindow.TotalMilliseconds));
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            }
        }

        /// <summary>
        /// Gets the number of tokens consumed (capacity - remaining).
        /// </summary>
        public int GetConsumedTokens() => _capacity - _tokens;

        /// <summary>
        /// Gets the approximate wait time in seconds until a token is available.
        /// </summary>
        public int GetWaitTimeSeconds()
        {
            var elapsed = DateTime.UtcNow - _lastRefillTime;
            var remaining = _refillWindow - elapsed;
            return Math.Max(1, (int)remaining.TotalSeconds);
        }
    }
}

/// <summary>
/// Exception thrown when rate limit is exceeded.
/// </summary>
public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message) { }
}
