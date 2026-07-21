# HealthSummary

Represents an aggregated health snapshot across all monitored services, providing counts, percentages, and a list of unhealthy service identifiers for quick assessment of overall system status.

## API

### TotalServices
**Type:** `int`  
Gets the total number of services included in the health summary. This value equals the sum of all health-state counts.

### HealthyCount
**Type:** `int`  
Gets the number of services currently reporting a healthy state.

### UnhealthyCount
**Type:** `int`  
Gets the number of services currently reporting an unhealthy state.

### DegradedCount
**Type:** `int`  
Gets the number of services currently reporting a degraded state.

### CriticalCount
**Type:** `int`  
Gets the number of services currently reporting a critical state.

### UnknownCount
**Type:** `int`  
Gets the number of services with an unknown or indeterminate health state.

### HealthyPercentage
**Type:** `double`  
Gets the percentage of services in a healthy state, calculated as `HealthyCount / TotalServices * 100`. Returns 0 when `TotalServices` is 0.

### UnhealthyPercentage
**Type:** `double`  
Gets the percentage of services in an unhealthy state, calculated as `UnhealthyCount / TotalServices * 100`. Returns 0 when `TotalServices` is 0.

### DegradedPercentage
**Type:** `double`  
Gets the percentage of services in a degraded state, calculated as `DegradedCount / TotalServices * 100`. Returns 0 when `TotalServices` is 0.

### CriticalPercentage
**Type:** `double`  
Gets the percentage of services in a critical state, calculated as `CriticalCount / TotalServices * 100`. Returns 0 when `TotalServices` is 0.

### UnknownPercentage
**Type:** `double`  
Gets the percentage of services in an unknown state, calculated as `UnknownCount / TotalServices * 100`. Returns 0 when `TotalServices` is 0.

### UnhealthyServiceNames
**Type:** `List<string>`  
Gets a list of service names that are not in a healthy state (i.e., unhealthy, degraded, critical, or unknown). The list is empty when all services are healthy or when no services exist.

### IsOverallHealthy
**Type:** `bool`  
Gets a value indicating whether the overall system is considered healthy. Returns `true` only when `UnhealthyCount`, `DegradedCount`, `CriticalCount`, and `UnknownCount` are all zero; otherwise `false`.

## Usage

```csharp
var summary = await healthClient.GetSummaryAsync();

Console.WriteLine($"System Health: {(summary.IsOverallHealthy ? "HEALTHY" : "DEGRADED")}");
Console.WriteLine($"Total Services: {summary.TotalServices}");
Console.WriteLine($"Healthy: {summary.HealthyCount} ({summary.HealthyPercentage:F1}%)");
Console.WriteLine($"Unhealthy: {summary.UnhealthyCount} ({summary.UnhealthyPercentage:F1}%)");
Console.WriteLine($"Degraded: {summary.DegradedCount} ({summary.DegradedPercentage:F1}%)");
Console.WriteLine($"Critical: {summary.CriticalCount} ({summary.CriticalPercentage:F1}%)");
Console.WriteLine($"Unknown: {summary.UnknownCount} ({summary.UnknownPercentage:F1}%)");

if (!summary.IsOverallHealthy)
{
    Console.WriteLine("Affected services:");
    foreach (var name in summary.UnhealthyServiceNames)
    {
        Console.WriteLine($"  - {name}");
    }
}
```

```csharp
var summary = await healthClient.GetSummaryAsync();

var alertThreshold = 95.0;
if (summary.HealthyPercentage < alertThreshold)
{
    var message = $"Health check alert: only {summary.HealthyPercentage:F1}% of services are healthy. " +
                  $"{summary.UnhealthyServiceNames.Count} service(s) affected: " +
                  $"{string.Join(", ", summary.UnhealthyServiceNames)}";
    await notificationService.SendAlertAsync(message, severity: AlertSeverity.Warning);
}

if (summary.CriticalCount > 0)
{
    await notificationService.SendAlertAsync(
        $"{summary.CriticalCount} service(s) in CRITICAL state",
        severity: AlertSeverity.Critical);
}
```

## Notes

- All percentage properties return `0` when `TotalServices` is `0` to avoid division-by-zero exceptions; no exceptions are thrown.
- `UnhealthyServiceNames` includes services in any non-healthy state (unhealthy, degraded, critical, unknown), not strictly those with an "unhealthy" status.
- `IsOverallHealthy` treats degraded, critical, and unknown states as unhealthy for the purpose of the overall determination.
- The type is a plain data container with no internal synchronization; it is not thread-safe for concurrent mutation. Treat instances as immutable after construction for safe multi-threaded read access.
- Percentage values are computed at property access time and reflect the current count values; they are not cached.
