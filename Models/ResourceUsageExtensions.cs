#nullable enable

using System.Globalization;

namespace CoolifyCli.Models;

/// <summary>
/// Provides extension methods for <see cref="ResourceUsage"/> that offer additional resource monitoring and analysis capabilities.
/// </summary>
public static class ResourceUsageExtensions
{
    /// <summary>
    /// Determines whether the resource usage indicates a critical state requiring immediate attention.
    /// </summary>
    /// <param name="usage">The resource usage data to evaluate.</param>
    /// <returns>True if the resource usage is in a critical state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="usage"/> is null.</exception>
    public static bool IsCritical(this ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        return usage.GetAlertSeverity() == SeverityLevel.Critical;
    }

    /// <summary>
    /// Determines whether the resource usage indicates a warning state that should be monitored.
    /// </summary>
    /// <param name="usage">The resource usage data to evaluate.</param>
    /// <returns>True if the resource usage is in a warning state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="usage"/> is null.</exception>
    public static bool IsWarning(this ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        return usage.GetAlertSeverity() == SeverityLevel.Warning;
    }

    /// <summary>
    /// Calculates the network throughput in bytes per second based on the time elapsed since capture.
    /// </summary>
    /// <param name="usage">The resource usage data containing network statistics.</param>
    /// <param name="timeElapsedSeconds">The time elapsed in seconds between captures.</param>
    /// <returns>A tuple containing (RxBytesPerSecond, TxBytesPerSecond).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="usage"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeElapsedSeconds"/> is zero or negative.</exception>
    public static (double RxBytesPerSecond, double TxBytesPerSecond) GetNetworkThroughput(this ResourceUsage usage, double timeElapsedSeconds)
    {
        ArgumentNullException.ThrowIfNull(usage);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeElapsedSeconds, 0);

        double factor = 1.0 / timeElapsedSeconds;
        return (usage.NetworkRxBytes * factor, usage.NetworkTxBytes * factor);
    }

    /// <summary>
    /// Formats the resource usage as a detailed diagnostic string suitable for logging or debugging.
    /// </summary>
    /// <param name="usage">The resource usage data to format.</param>
    /// <returns>A formatted string with all resource metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="usage"/> is null.</exception>
    public static string ToDiagnosticString(this ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        var alertSeverity = usage.GetAlertSeverity();
        var memoryPercent = usage.MemoryLimitMb > 0
            ? Math.Round(usage.MemoryMb / usage.MemoryLimitMb * 100.0, 1)
            : 0;

        var alertSeverityText = alertSeverity?.ToString() ?? "None";
        return string.Create(CultureInfo.InvariantCulture, $$"""
Resource Usage Diagnostic - {{usage.ApplicationName}} (ID: {{usage.ApplicationId}})
Captured: {{usage.CapturedAt:u}}
Alert Severity: {{alertSeverityText}}

Metrics:
  CPU: {{usage.CpuPercent:F1}}%
  Memory: {{usage.MemoryMb:F2}} MB / {{usage.MemoryLimitMb:F2}} MB ({{memoryPercent:F1}}%)
  Threads: {{usage.ThreadCount}}
  Open File Handles: {{usage.OpenFileHandles}}
  Network:
    RX: {{FormatBytes(usage.NetworkRxBytes)}}
    TX: {{FormatBytes(usage.NetworkTxBytes)}}
""");
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1_073_741_824 => string.Create(CultureInfo.InvariantCulture, $"{(double)bytes / 1_073_741_824:F2} GB"),
            >= 1_048_576 => string.Create(CultureInfo.InvariantCulture, $"{(double)bytes / 1_048_576:F2} MB"),
            >= 1_024 => string.Create(CultureInfo.InvariantCulture, $"{(double)bytes / 1_024:F2} KB"),
            _ => string.Create(CultureInfo.InvariantCulture, $"{bytes} B")
        };
    }
}