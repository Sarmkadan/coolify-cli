#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Models;

/// <summary>
/// Represents the health status of an application or service.
/// Tracks response times, error rates, and overall system health metrics.
/// </summary>
public class ServiceHealth
{
    public int Id { get; set; }
    public string ServiceId { get; set; } = string.Empty;
    public HealthStatus Status { get; set; } = HealthStatus.Unknown;
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public double ResponseTimeMs { get; set; } = 0;
    public int HttpStatusCode { get; set; } = 0;
    public double CpuUsagePercent { get; set; } = 0;
    public double MemoryUsageMb { get; set; } = 0;
    public int ActiveConnections { get; set; } = 0;
    public double ErrorRatePercent { get; set; } = 0;
    public DateTime? LastSuccessfulCheck { get; set; }
    public int FailureCount { get; set; } = 0;
    public string? FailureReason { get; set; }
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Determines if the service is healthy based on status and metrics.
    /// </summary>
    /// <returns>True if service status is Healthy.</returns>
    public bool IsHealthy() => Status == HealthStatus.Healthy;

    /// <summary>
    /// Checks if the service requires immediate attention.
    /// </summary>
    /// <returns>True if status is Critical or failure count is high.</returns>
    public bool RequiresAttention() => Status == HealthStatus.Critical || FailureCount > 3;

    /// <summary>
    /// Records a successful health check with current metrics.
    /// </summary>
    /// <param name="responseTimeMs">Response time in milliseconds.</param>
    /// <param name="httpStatus">HTTP status code received.</param>
    public void RecordSuccess(double responseTimeMs, int httpStatus)
    {
        Status = DetermineHealthStatus(responseTimeMs, httpStatus);
        ResponseTimeMs = responseTimeMs;
        HttpStatusCode = httpStatus;
        CheckedAt = DateTime.UtcNow;
        LastSuccessfulCheck = DateTime.UtcNow;
        FailureCount = 0;
        FailureReason = null;
        Warnings.Clear();
    }

    /// <summary>
    /// Records a failed health check with error details.
    /// </summary>
    /// <param name="reason">Description of the failure.</param>
    public void RecordFailure(string reason)
    {
        Status = HealthStatus.Critical;
        FailureCount++;
        FailureReason = reason;
        CheckedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates system resource usage metrics.
    /// </summary>
    /// <param name="cpuPercent">CPU usage percentage.</param>
    /// <param name="memoryMb">Memory usage in megabytes.</param>
    public void UpdateResources(double cpuPercent, double memoryMb)
    {
        CpuUsagePercent = cpuPercent;
        MemoryUsageMb = memoryMb;

        Warnings.Clear();
        if (cpuPercent > 80)
            Warnings.Add("High CPU usage detected");
        if (memoryMb > 1024) // Assuming 1GB threshold
            Warnings.Add("High memory usage detected");
    }

    /// <summary>
    /// Determines health status based on response time and HTTP status.
    /// </summary>
    /// <param name="responseTimeMs">Response time in milliseconds.</param>
    /// <param name="httpStatus">HTTP status code.</param>
    /// <returns>Appropriate health status.</returns>
    private static HealthStatus DetermineHealthStatus(double responseTimeMs, int httpStatus)
    {
        if (httpStatus < 200 || httpStatus >= 500)
            return HealthStatus.Unhealthy;

        if (responseTimeMs > 5000)
            return HealthStatus.Degraded;

        return HealthStatus.Healthy;
    }
}

public enum HealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy,
    Critical
}
