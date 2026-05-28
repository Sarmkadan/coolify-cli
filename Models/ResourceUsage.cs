#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// A point-in-time snapshot of resource consumption for a single application instance.
/// </summary>
public class ResourceUsage
{
    /// <summary>Gets or sets the application ID this snapshot belongs to.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Gets or sets the human-readable application name.</summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC time this snapshot was captured.</summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets CPU utilisation as a percentage (0–100).</summary>
    public double CpuPercent { get; set; }

    /// <summary>Gets or sets the amount of RAM in use, in megabytes.</summary>
    public double MemoryMb { get; set; }

    /// <summary>Gets or sets the total memory available to the container, in megabytes.</summary>
    public double MemoryLimitMb { get; set; }

    /// <summary>Gets or sets cumulative network bytes received since container start.</summary>
    public long NetworkRxBytes { get; set; }

    /// <summary>Gets or sets cumulative network bytes transmitted since container start.</summary>
    public long NetworkTxBytes { get; set; }

    /// <summary>Gets or sets the number of open file descriptors / handles.</summary>
    public int OpenFileHandles { get; set; }

    /// <summary>Gets or sets the number of running process threads.</summary>
    public int ThreadCount { get; set; }

    /// <summary>
    /// Gets memory utilisation as a percentage of the configured limit.
    /// Returns 0 when <see cref="MemoryLimitMb"/> is zero to avoid division by zero.
    /// </summary>
    public double MemoryPercent =>
        MemoryLimitMb > 0 ? Math.Round(MemoryMb / MemoryLimitMb * 100.0, 1) : 0;

    /// <summary>
    /// Evaluates resource pressure and returns the appropriate alert severity level,
    /// or null when all metrics are within acceptable bounds.
    /// </summary>
    public SeverityLevel? GetAlertSeverity()
    {
        if (CpuPercent >= 95 || MemoryPercent >= 95)
            return SeverityLevel.Critical;

        if (CpuPercent >= 80 || MemoryPercent >= 85)
            return SeverityLevel.Warning;

        return null;
    }

    /// <summary>
    /// Returns a short human-readable summary line suitable for tabular display.
    /// </summary>
    public string ToSummaryLine() =>
        $"{ApplicationId,-6} {ApplicationName,-28} {CpuPercent,6:F1}%  {MemoryMb,8:F0} MB  {MemoryPercent,5:F1}%  {FormatBytes(NetworkRxBytes),10}  {FormatBytes(NetworkTxBytes),10}";

    private static string FormatBytes(long bytes) =>
        bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
            >= 1_048_576     => $"{bytes / 1_048_576.0:F1} MB",
            >= 1_024         => $"{bytes / 1_024.0:F1} KB",
            _                => $"{bytes} B"
        };
}
