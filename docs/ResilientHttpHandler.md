# ResilientHttpHandler

The `ResilientHttpHandler` is a `DelegatingHandler` that adds transparent resilience to Coolify API calls. It implements retry logic with exponential backoff and jitter, circuit breaker pattern, and per-attempt timeouts.

## Retry Behavior

The handler retries transient failures under the following conditions:

- **HTTP Status Codes**: 408 (Request Timeout), 429 (Too Many Requests), and 5xx (Server Errors)
- **Network Errors**: `HttpRequestException` (connection failures)
- **Timeouts**: `TimeoutException` (per-attempt timeout exceeded)

### Retry Configuration

- **Maximum Attempts**: Configurable via `ResilienceOptions.MaxAttempts` (default: 3)
- **Base Delay**: Configurable via `ResilienceOptions.BaseDelay` (default: 500ms)
- **Maximum Delay**: Configurable via `ResilienceOptions.MaxDelay` (default: 15 seconds)
- **Backoff Strategy**: Exponential backoff with full jitter
  - Delay calculation: `baseDelay * (2^(attempt-1)) + jitter`
  - Jitter: Random value between 0 and `baseDelay`
  - Result capped at `MaxDelay`

### Special Handling for 429 Responses

When receiving a 429 (Too Many Requests) response:
- If the response includes a `Retry-After` header, the handler respects that delay
- `Retry-After` can be either:
  - Delta-seconds: Number of seconds to wait
  - Date: Absolute timestamp until which to wait
- The handler uses the `Retry-After` delay if it's positive and less than `MaxDelay`; otherwise falls back to exponential backoff

## Circuit Breaker

To prevent hanging requests when the server is persistently unavailable, the handler implements a circuit breaker:

- **Failure Threshold**: Configurable via `ResilienceOptions.CircuitBreakerThreshold` (default: 5 consecutive failures)
- **Open Duration**: Configurable via `ResilienceOptions.CircuitBreakDuration` (default: 30 seconds)
- **States**:
  - **Closed**: Normal operation, failures counted
  - **Open**: All requests fail immediately without attempting to call the server
  - **Half-Open**: After open duration elapses, allows exactly one trial request to test if service recovered

### Circuit Breaker Operation

1. **Closed State**:
   - Each failure increments `_consecutiveFailures`
   - When `_consecutiveFailures >= CircuitBreakerThreshold`, circuit transitions to Open

2. **Open State**:
   - All requests throw `HttpRequestException` immediately with message indicating circuit is open
   - After `CircuitBreakDuration` elapses, circuit transitions to Half-Open

3. **Half-Open State**:
   - Exactly one request is allowed to proceed (trial request)
   - If trial succeeds: circuit resets to Closed state
   - If trial fails: circuit returns to Open state with renewed timer

## Timeouts

The handler implements two distinct timeout mechanisms:

1. **Per-Attempt Timeout**:
   - Configurable via `ResilienceOptions.AttemptTimeout` (default: 10 seconds)
   - Applied to each individual HTTP attempt
   - If exceeded, throws `TimeoutException` which is treated as a transient failure (subject to retry)

2. **Overall Operation Timeout**:
   - Provided via the `cancellationToken` parameter in `SendAsync`
   - Bounds the total time spent across all retries and delays
   - If cancelled, throws `OperationCanceledException`

## Logging

The handler logs diagnostic information at different levels when an `ILogger` is provided:

- **Debug Level**: Successful requests after retries
  - Format: `http-retry-success attempt={attempt}/{maxAttempts} status={statusCode} endpoint={path}`
  
- **Warn Level**: Retry attempts
  - Format: `http-retry attempt={nextAttempt}/{maxAttempts} status={statusCode|network-error|timeout} delay={delayMs}ms endpoint={path}`
  
- **Error Level**: Final failure after all attempts exhausted
  - Format: `http-failure attempt={attempt}/{maxAttempts} status={finalStatus} delay=0ms endpoint={path}`
  - `finalStatus` is either the last transient status code, "timeout", or "network-error"

## Exception Handling

The handler may throw the following exceptions:

- **`HttpRequestException`**:
  - When circuit breaker is open and request is rejected
  - When all retry attempts are exhausted due to connection errors or timeouts
  
- **`OperationCanceledException`**:
  - When the overall operation timeout is triggered via cancellation token

- **Note**: Transient HTTP status codes (408, 429, 5xx) are not thrown as exceptions when all retries are exhausted; instead, the last response is returned to allow the caller to map it to a structured API error response.

## Usage

The handler is typically used when configuring an `HttpClient`:

```csharp
var handler = new ResilientHttpHandler(
    innerHandler: new HttpClientHandler(),
    options: new ResilienceOptions {
        MaxAttempts = 3,
        BaseDelay = TimeSpan.FromMilliseconds(500),
        MaxDelay = TimeSpan.FromSeconds(15),
        CircuitBreakerThreshold = 5,
        CircuitBreakDuration = TimeSpan.FromSeconds(30),
        AttemptTimeout = TimeSpan.FromSeconds(10)
    },
    logger: loggerInstance
);

var httpClient = new HttpClient(handler);
```

## Thread Safety

The handler is thread-safe for concurrent use by multiple requests. Circuit breaker state is protected by locks, and all state updates are atomic.