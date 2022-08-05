#nullable enable
using CoolifyCli.Models;
using System.Globalization;

namespace CoolifyCli.Tests;

/// <summary>
/// Extension methods for <see cref="ResourceUsageTests"/> that provide additional functionality
/// for testing resource usage scenarios and assertions.
/// </summary>
public static class ResourceUsageTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="ResourceUsage"/> instance with default values suitable for testing.
    /// </summary>
    /// <param name="cpuPercent">The CPU percentage to set.</param>
    /// <param name="memoryMb">The memory usage in MB to set.</param>
    /// <param name="memoryLimitMb">The memory limit in MB to set.</param>
    /// <returns>A new <see cref="ResourceUsage"/> instance.</returns>
    public static ResourceUsage WithUsage(this ResourceUsageTests _, double cpuPercent, double memoryMb, double memoryLimitMb)
    {
        ArgumentNullException.ThrowIfNull(_);

        return new ResourceUsage
        {
            CpuPercent = cpuPercent,
            MemoryMb = memoryMb,
            MemoryLimitMb = memoryLimitMb
        };
    }

    /// <summary>
    /// Creates a new <see cref="ResourceUsage"/> instance with application context for testing.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="applicationName">The application name.</param>
    /// <param name="cpuPercent">The CPU percentage to set.</param>
    /// <param name="memoryMb">The memory usage in MB to set.</param>
    /// <param name="memoryLimitMb">The memory limit in MB to set.</param>
    /// <returns>A new <see cref="ResourceUsage"/> instance with application context.</returns>
    public static ResourceUsage WithApplication(this ResourceUsageTests _, int applicationId, string applicationName, double cpuPercent, double memoryMb, double memoryLimitMb)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentException.ThrowIfNullOrEmpty(applicationName);

        return new ResourceUsage
        {
            ApplicationId = applicationId,
            ApplicationName = applicationName,
            CpuPercent = cpuPercent,
            MemoryMb = memoryMb,
            MemoryLimitMb = memoryLimitMb
        };
    }

    /// <summary>
    /// Creates a collection of <see cref="ResourceUsage"/> instances representing different load scenarios.
    /// </summary>
    /// <param name="count">The number of usage instances to create.</param>
    /// <param name="scenario">The load scenario to generate.</param>
    /// <returns>A read-only list of <see cref="ResourceUsage"/> instances.</returns>
    public static IReadOnlyList<ResourceUsage> GenerateLoadScenario(this ResourceUsageTests _, int count, LoadScenario scenario = LoadScenario.Normal)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        return scenario switch
        {
            LoadScenario.Normal => Enumerable.Range(1, count)
                .Select(i => new ResourceUsage
                {
                    ApplicationId = i,
                    ApplicationName = $"app-{i}",
                    CpuPercent = 40 + (i % 30),
                    MemoryMb = 100 + (i * 50),
                    MemoryLimitMb = 1024
                })
                .ToList()
                .AsReadOnly(),

            LoadScenario.HighCpu => Enumerable.Range(1, count)
                .Select(i => new ResourceUsage
                {
                    ApplicationId = i,
                    ApplicationName = $"cpu-heavy-{i}",
                    CpuPercent = 85 + (i % 15),
                    MemoryMb = 200 + (i * 30),
                    MemoryLimitMb = 1024
                })
                .ToList()
                .AsReadOnly(),

            LoadScenario.MemoryPressure => Enumerable.Range(1, count)
                .Select(i => new ResourceUsage
                {
                    ApplicationId = i,
                    ApplicationName = $"memory-heavy-{i}",
                    CpuPercent = 20 + (i % 20),
                    MemoryMb = 900 + (i * 20),
                    MemoryLimitMb = 1024
                })
                .ToList()
                .AsReadOnly(),

            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown load scenario")
        };
    }

    /// <summary>
    /// Asserts that the resource usage has normal severity level (no alerts).
    /// </summary>
    /// <param name="usage">The resource usage to check.</param>
    /// <returns>True if severity is normal; otherwise false.</returns>
    public static bool HasNormalSeverity(this ResourceUsageTests _, ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentNullException.ThrowIfNull(usage);

        return usage.GetAlertSeverity() is null;
    }

    /// <summary>
    /// Asserts that the resource usage has warning severity level.
    /// </summary>
    /// <param name="usage">The resource usage to check.</param>
    /// <returns>True if severity is warning; otherwise false.</returns>
    public static bool HasWarningSeverity(this ResourceUsageTests _, ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentNullException.ThrowIfNull(usage);

        return usage.GetAlertSeverity() == SeverityLevel.Warning;
    }

    /// <summary>
    /// Asserts that the resource usage has critical severity level.
    /// </summary>
    /// <param name="usage">The resource usage to check.</param>
    /// <returns>True if severity is critical; otherwise false.</returns>
    public static bool HasCriticalSeverity(this ResourceUsageTests _, ResourceUsage usage)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentNullException.ThrowIfNull(usage);

        return usage.GetAlertSeverity() == SeverityLevel.Critical;
    }
}

/// <summary>
/// Represents different load scenarios for generating test data.
/// </summary>
public enum LoadScenario
{
    /// <summary>Normal load with moderate resource usage.</summary>
    Normal,

    /// <summary>High CPU load scenario.</summary>
    HighCpu,

    /// <summary>High memory pressure scenario.</summary>
    MemoryPressure
}