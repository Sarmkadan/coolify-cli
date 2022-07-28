# ServiceHealth

Represents the observed health state of a service monitored by the Coolify CLI. This type aggregates telemetry data—response times, resource consumption, error rates, and status history—into a single snapshot. It also exposes methods to mutate its own state in response to check outcomes, making it the primary unit of record for service reliability tracking.

## API

### Properties

#### `public int Id`
Unique identifier for this health record within the local data store. Used for persistence and lookup operations.

#### `public string ServiceId`
The logical identifier of the service this health record belongs to. Corresponds to the service name or key configured in Coolify.

#### `public HealthStatus Status`
Current enumerated health status. The underlying `HealthStatus` type defines values such as `Healthy`, `Degraded`, `Unhealthy`, or `Unknown`.

#### `public DateTime CheckedAt`
Timestamp of the most recent health check execution, regardless of outcome.

#### `public double ResponseTimeMs`
Response time of the last check in milliseconds. Set to `0.0` if no HTTP check was performed or the request timed out without a measurable response.

#### `public int HttpStatusCode`
HTTP status code returned by the last health check request. Set to `0` when the check did not produce an HTTP response (e.g., connection failure or non-HTTP check type).

#### `public double CpuUsagePercent`
CPU utilisation percentage sampled at check time. Value range is typically `0.0`–`100.0`, but may exceed `100.0` on multi-core systems if aggregated naively.

#### `public double MemoryUsageMb`
Memory consumption in megabytes sampled at check time.

#### `public int ActiveConnections`
Number of active connections observed during the check. Interpretation is service-specific (e.g., database connections, concurrent HTTP sessions).

#### `public double ErrorRatePercent`
Error rate percentage over the sampling window. Range is `0.0`–`100.0`.

#### `public DateTime? LastSuccessfulCheck`
Timestamp of the most recent check that resulted in a healthy status. `null` if the service has never passed a health check.

#### `public int FailureCount`
Consecutive failure count since the last successful check. Resets to `0` on a successful check.

#### `public string? FailureReason`
Human-readable description of the most recent failure. `null` when the last check succeeded or no failure has been recorded yet.

#### `public List<string> Warnings`
Non-fatal issues detected during the last check. An empty list when no warnings were generated. Typical entries include threshold breaches that do not yet constitute a failure.

#### `public bool IsHealthy`
Convenience boolean derived from `Status`. Returns `true` when `Status` equals `HealthStatus.Healthy`; otherwise `false`.

#### `public bool RequiresAttention`
Convenience boolean indicating whether operator intervention is recommended. Returns `true` when `Status` is `Degraded`, `Unhealthy`, or `Unknown`, or when `Warnings` is non-empty despite a nominally healthy status.

### Methods

#### `public void RecordSuccess(double responseTimeMs, int httpStatusCode)`
Records a successful health check outcome.

**Parameters:**
- `responseTimeMs` — Measured response time in milliseconds. Must be non-negative.
- `httpStatusCode` — HTTP status code returned. Pass `0` for non-HTTP checks.

**Behaviour:**
Sets `Status` to `Healthy`, stores the provided metrics, updates `CheckedAt` to the current UTC time, sets `LastSuccessfulCheck` to the current time, resets `FailureCount` to `0`, clears `FailureReason`, and recalculates `IsHealthy` and `RequiresAttention`.

**Throws:**
`ArgumentOutOfRangeException` when `responseTimeMs` is negative.

#### `public void RecordFailure(string reason, double? responseTimeMs = null, int httpStatusCode = 0)`
Records a failed health check outcome.

**Parameters:**
- `reason` — Description of the failure. Must not be null or whitespace.
- `responseTimeMs` — Optional response time if a partial response was received.
- `httpStatusCode` — HTTP status code if a response was received. Pass `0` otherwise.

**Behaviour:**
Sets `Status` to `Unhealthy`, increments `FailureCount`, stores `reason` in `FailureReason`, updates `CheckedAt`, and recalculates `IsHealthy` and `RequiresAttention`. Does not modify `LastSuccessfulCheck`.

**Throws:**
`ArgumentException` when `reason` is null, empty, or consists only of whitespace.

#### `public void UpdateResources(double cpuUsagePercent, double memoryUsageMb, int activeConnections, double errorRatePercent)`
Updates the resource utilisation snapshot without changing the health status or check outcome history.

**Parameters:**
- `cpuUsagePercent` — CPU usage percentage. Negative values are clamped to `0.0`.
- `memoryUsageMb` — Memory usage in megabytes. Negative values are clamped to `0.0`.
- `activeConnections` — Active connection count. Negative values are clamped to `0`.
- `errorRatePercent` — Error rate percentage. Negative values are clamped to `0.0`; values above `100.0` are clamped to `100.0`.

**Behaviour:**
Overwrites the corresponding resource properties. Does not alter `Status`, `CheckedAt`, `FailureCount`, `FailureReason`, `LastSuccessfulCheck`, or the derived booleans. Intended for out-of-band resource sampling separate from health check pass/fail determination.

## Usage

### Example 1: Recording a successful HTTP health check

```csharp
var health = new ServiceHealth
{
    Id = 1,
    ServiceId = "api-gateway",
    Status = HealthStatus.Unknown,
    CheckedAt = DateTime.UtcNow.AddMinutes(-5)
};

// A check returns HTTP 200 in 42.3 ms
health.RecordSuccess(responseTimeMs: 42.3, httpStatusCode: 200);

Console.WriteLine($"Status: {health.Status}");            // Healthy
Console.WriteLine($"IsHealthy: {health.IsHealthy}");      // True
Console.WriteLine($"FailureCount: {health.FailureCount}"); // 0
Console.WriteLine($"LastSuccessfulCheck: {health.LastSuccessfulCheck}");
```

### Example 2: Recording a failure followed by resource update

```csharp
var health = new ServiceHealth
{
    Id = 2,
    ServiceId = "database-primary",
    Status = HealthStatus.Healthy,
    CheckedAt = DateTime.UtcNow.AddMinutes(-1),
    LastSuccessfulCheck = DateTime.UtcNow.AddMinutes(-1)
};

// Connection timeout occurs
health.RecordFailure(reason: "Connection timed out after 30s");

// Update resource snapshot independently (e.g., from a sidecar agent)
health.UpdateResources(
    cpuUsagePercent: 12.7,
    memoryUsageMb: 1024.0,
    activeConnections: 3,
    errorRatePercent: 0.0
);

Console.WriteLine($"Status: {health.Status}");              // Unhealthy
Console.WriteLine($"RequiresAttention: {health.RequiresAttention}"); // True
Console.WriteLine($"FailureReason: {health.FailureReason}"); // "Connection timed out after 30s"
Console.WriteLine($"FailureCount: {health.FailureCount}");   // 1
Console.WriteLine($"MemoryUsageMb: {health.MemoryUsageMb}"); // 1024.0
```

## Notes

- **Thread safety:** This type is not thread-safe. Concurrent calls to `RecordSuccess`, `RecordFailure`, or `UpdateResources` from multiple threads will produce unpredictable results, including torn state and inconsistent derived properties. Synchronisation must be applied externally when shared across threads.
- **Derived property consistency:** `IsHealthy` and `RequiresAttention` are recalculated only by `RecordSuccess` and `RecordFailure`. Calling `UpdateResources` does not recompute them, even if resource values would normally trigger warnings. A subsequent call to `RecordSuccess` or `RecordFailure` will bring them up to date.
- **Clamping behaviour:** `UpdateResources` clamps negative inputs to zero and caps `errorRatePercent` at `100.0`. It does not throw on out-of-range values, unlike the recording methods which enforce stricter preconditions.
- **Null `LastSuccessfulCheck`:** A newly initialised record or one that has never succeeded will have `null` for `LastSuccessfulCheck`. Code consuming this property must handle the null case.
- **`FailureCount` semantics:** This counter tracks consecutive failures. A single success resets it to zero regardless of prior history. It is not a cumulative lifetime failure counter.
- **`Warnings` list:** This property is a `List<string>` reference. External code holding a reference to the list can mutate it outside the control of the `ServiceHealth` instance, potentially desynchronising `RequiresAttention`. Defensive copying is recommended when exposing the list beyond the owning component.
