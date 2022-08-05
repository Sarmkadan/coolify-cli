# HealthCheckService

`HealthCheckService` provides a centralized client for querying application and system health status within the Coolify infrastructure. It exposes methods to perform single and bulk health checks, retrieve historical health data, fetch real-time and aggregated metrics, manage application alerts, and establish long-lived streaming monitors for continuous health observation.

## API

### HealthCheckService

Instantiates the service. Construction details are internal to the Coolify CLI dependency injection or client bootstrapping; no public parameters are exposed on the constructor itself.

### CheckApplicationHealthAsync

```csharp
public async Task<ApiResponse<ServiceHealth>> CheckApplicationHealthAsync(/* parameters internal */)
```

Performs an on-demand health check against a single application and returns its current `ServiceHealth` status wrapped in an `ApiResponse`.

- **Returns:** `ApiResponse<ServiceHealth>` containing the health status of the target application.
- **Throws:** May throw `HttpRequestException` or task-cancelled exceptions when the underlying HTTP call fails or times out. The `ApiResponse` envelope itself may carry error details rather than throwing for API-level failures.

### CheckBulkHealthAsync

```csharp
public async Task<ApiResponse<Dictionary<int, ServiceHealth>>> CheckBulkHealthAsync(/* parameters internal */)
```

Executes health checks against multiple applications in a single request. The resulting dictionary maps application identifiers (integers) to their corresponding `ServiceHealth` objects.

- **Returns:** `ApiResponse<Dictionary<int, ServiceHealth>>` where each key is an application ID and each value is its health status.
- **Throws:** Network and timeout exceptions as with single checks. Partial failures may be represented inside the dictionary values rather than via exceptions.

### GetHealthHistoryAsync

```csharp
public async Task<ApiResponse<List<ServiceHealth>>> GetHealthHistoryAsync(/* parameters internal */)
```

Retrieves a time-ordered list of historical health records for one or more applications. The exact scope (single application vs. all) is determined by internal parameters.

- **Returns:** `ApiResponse<List<ServiceHealth>>` containing past health snapshots.
- **Throws:** Standard network exceptions. An empty list is returned when no history exists for the requested scope.

### GetMetricsAsync

```csharp
public async Task<ApiResponse<object>> GetMetricsAsync(/* parameters internal */)
```

Fetches aggregated metrics (e.g., CPU, memory, request counts) for a specified resource. The returned `object` payload is deserialized from the Coolify API JSON response and should be cast to the expected metrics shape by the caller.

- **Returns:** `ApiResponse<object>` with the metrics payload.
- **Throws:** Network exceptions. The `object` is not null on success but may represent an error structure if the API rejects the request.

### GetRealtimeMetricsAsync

```csharp
public async Task<ApiResponse<object>> GetRealtimeMetricsAsync(/* parameters internal */)
```

Fetches near-instantaneous metrics, typically from a live metrics endpoint or websocket fallback. The return type is `object` to accommodate varying metric schemas across resource types.

- **Returns:** `ApiResponse<object>` containing real-time metric data.
- **Throws:** Network and timeout exceptions. Callers should handle stale-data scenarios when the real-time feed is temporarily unavailable.

### GetApplicationAlertsAsync

```csharp
public async Task<ApiResponse<List<object>>> GetApplicationAlertsAsync(/* parameters internal */)
```

Retrieves active or recent alerts associated with applications. Each alert is represented as an `object` whose concrete structure depends on the alert type (threshold breach, downtime, etc.).

- **Returns:** `ApiResponse<List<object>>` with alert objects.
- **Throws:** Network exceptions. An empty list indicates no current alerts.

### AcknowledgeAlertAsync

```csharp
public async Task<ApiResponse<object>> AcknowledgeAlertAsync(/* parameters internal */)
```

Marks one or more alerts as acknowledged, typically suppressing further notifications. The returned `object` contains the acknowledgement confirmation payload.

- **Returns:** `ApiResponse<object>` with acknowledgement status.
- **Throws:** Network exceptions. Attempting to acknowledge an already-resolved alert may return an error inside the `ApiResponse` rather than throwing.

### GetSystemHealthAsync

```csharp
public async Task<ApiResponse<object>> GetSystemHealthAsync(/* parameters internal */)
```

Queries the overall Coolify system health (orchestrator, database connectivity, internal services). The `object` return type encapsulates system-wide status information.

- **Returns:** `ApiResponse<object>` with system health details.
- **Throws:** Network exceptions. This method may fail entirely if the system health endpoint itself is unreachable.

### MonitorHealthAsync

```csharp
public async IAsyncEnumerable<ServiceHealth> MonitorHealthAsync(/* parameters internal */)
```

Opens a streaming enumeration that yields `ServiceHealth` updates as they become available. This method is designed for long-running monitoring loops; the enumeration continues until the underlying connection closes, the cancellation token is triggered, or the stream is disposed.

- **Returns:** `IAsyncEnumerable<ServiceHealth>` that asynchronously produces health snapshots.
- **Throws:** May throw on initial connection failure. Once streaming, errors typically surface as iteration exceptions or early termination of the enumerable. Callers must wrap enumeration in try-catch blocks and respect `CancellationToken` propagation.

## Usage

### Example 1: Single Health Check with Error Handling

```csharp
var healthService = new HealthCheckService();
ApiResponse<ServiceHealth> response = await healthService.CheckApplicationHealthAsync();

if (response.IsSuccess && response.Data != null)
{
    Console.WriteLine($"Status: {response.Data.Status}");
    Console.WriteLine($"Uptime: {response.Data.UptimeSeconds}s");
}
else
{
    Console.WriteLine($"Health check failed: {response.ErrorMessage}");
}
```

### Example 2: Continuous Monitoring with Cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var healthService = new HealthCheckService();

try
{
    await foreach (ServiceHealth health in healthService.MonitorHealthAsync()
                       .WithCancellation(cts.Token))
    {
        Console.WriteLine($"[{DateTime.UtcNow:T}] Health: {health.Status}");

        if (health.Status == HealthStatus.Unhealthy)
        {
            // Trigger alert acknowledgement
            await healthService.AcknowledgeAlertAsync();
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Monitoring stopped after timeout.");
}
catch (Exception ex)
{
    Console.WriteLine($"Monitoring stream interrupted: {ex.Message}");
}
```

## Notes

- **Thread safety:** All methods return `Task` or `IAsyncEnumerable` and are designed for concurrent use. The underlying HTTP client should be presumed thread-safe; however, callers should avoid sharing `CancellationTokenSource` instances across unrelated invocations without synchronization.
- **Streaming lifetime:** `MonitorHealthAsync` returns an `IAsyncEnumerable` that holds an open connection. Failure to dispose the enumerator (e.g., breaking early from `await foreach` without cancellation) may leak resources until the next garbage collection or timeout. Always pair streaming calls with a `CancellationToken` or explicit disposal.
- **Object-typed returns:** Methods returning `ApiResponse<object>` (`GetMetricsAsync`, `GetRealtimeMetricsAsync`, `GetSystemHealthAsync`, `AcknowledgeAlertAsync`) require the caller to know the expected schema. Deserialization to a concrete type should be guarded with try-catch or safe-cast patterns, as API version changes can alter the shape of the returned object.
- **Empty collections:** `GetHealthHistoryAsync` and `GetApplicationAlertsAsync` return empty lists when no data exists. This is not an error condition; the `ApiResponse.IsSuccess` flag remains `true`.
- **Bulk check partial failures:** `CheckBulkHealthAsync` may return a dictionary where some entries indicate unhealthy or error states rather than throwing an aggregate exception. Always inspect individual `ServiceHealth` values in the dictionary.
- **Alert acknowledgement idempotency:** Repeated calls to `AcknowledgeAlertAsync` for the same alert may succeed but return a payload indicating the alert was already acknowledged. Callers should not assume the operation is strictly idempotent in terms of side effects on notification channels.
