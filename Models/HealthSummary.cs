#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Represents a comprehensive health summary for all applications/services.
/// Contains aggregate statistics and list of unhealthy services.
/// </summary>
public class HealthSummary
{
    public int TotalServices { get; set; } = 0;
    public int HealthyCount { get; set; } = 0;
    public int UnhealthyCount { get; set; } = 0;
    public int DegradedCount { get; set; } = 0;
    public int CriticalCount { get; set; } = 0;
    public int UnknownCount { get; set; } = 0;
    public double HealthyPercentage { get; set; } = 0;
    public double UnhealthyPercentage { get; set; } = 0;
    public double DegradedPercentage { get; set; } = 0;
    public double CriticalPercentage { get; set; } = 0;
    public double UnknownPercentage { get; set; } = 0;
    public List<string> UnhealthyServiceNames { get; set; } = new();

    /// <summary>
    /// Determines if the overall health is good (80%+ healthy).
    /// </summary>
    /// <returns>True if overall health is good.</returns>
    public bool IsOverallHealthy() => HealthyPercentage >= 80;

    /// <summary>
    /// Gets the count of unhealthy services (Unhealthy + Critical + Degraded).
    /// </summary>
    public int TotalUnhealthyCount => UnhealthyCount + CriticalCount + DegradedCount;

    /// <summary>
    /// Gets the percentage of unhealthy services.
    /// </summary>
    public double TotalUnhealthyPercentage => UnhealthyPercentage + CriticalPercentage + DegradedPercentage;
}