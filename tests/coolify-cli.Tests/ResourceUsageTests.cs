#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the <see cref="ResourceUsage"/> model.
/// </summary>
public class ResourceUsageTests
{
    /// <summary>
    /// Verifies that <see cref="ResourceUsage.MemoryPercent"/> returns 0 when <see cref="ResourceUsage.MemoryLimitMb"/> is zero.
    /// </summary>
    [Fact]
    public void MemoryPercent_WhenLimitIsZero_ReturnsZero()
    {
        var usage = new ResourceUsage { MemoryMb = 512, MemoryLimitMb = 0 };

        usage.MemoryPercent.Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.MemoryPercent"/> returns 50.0 when the memory usage is half of the limit.
    /// </summary>
    [Fact]
    public void MemoryPercent_WhenHalfOfLimit_ReturnsFiftyPercent()
    {
        var usage = new ResourceUsage { MemoryMb = 256, MemoryLimitMb = 512 };

        usage.MemoryPercent.Should().Be(50.0);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.GetAlertSeverity"/> returns <c>null</c> when all metrics are within normal ranges.
    /// </summary>
    [Fact]
    public void GetAlertSeverity_WhenAllMetricsNormal_ReturnsNull()
    {
        var usage = new ResourceUsage
        {
            CpuPercent     = 30,
            MemoryMb       = 256,
            MemoryLimitMb  = 1024,
        };

        usage.GetAlertSeverity().Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.GetAlertSeverity"/> returns <see cref="SeverityLevel.Warning"/> when CPU usage exceeds 80%.
    /// </summary>
    [Fact]
    public void GetAlertSeverity_WhenCpuAboveEighty_ReturnsWarning()
    {
        var usage = new ResourceUsage { CpuPercent = 85, MemoryMb = 100, MemoryLimitMb = 1024 };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Warning);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.GetAlertSeverity"/> returns <see cref="SeverityLevel.Critical"/> when CPU usage exceeds 95%.
    /// </summary>
    [Fact]
    public void GetAlertSeverity_WhenCpuAboveNinetyFive_ReturnsCritical()
    {
        var usage = new ResourceUsage { CpuPercent = 97, MemoryMb = 100, MemoryLimitMb = 1024 };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Critical);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.GetAlertSeverity"/> returns <see cref="SeverityLevel.Warning"/> when memory usage exceeds 85% of the limit.
    /// </summary>
    [Fact]
    public void GetAlertSeverity_WhenMemoryPercentAboveEightyFive_ReturnsWarning()
    {
        var usage = new ResourceUsage
        {
            CpuPercent    = 10,
            MemoryMb      = 900,
            MemoryLimitMb = 1024
        };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Warning);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.GetAlertSeverity"/> returns <see cref="SeverityLevel.Critical"/> when memory usage exceeds 95% of the limit.
    /// </summary>
    [Fact]
    public void GetAlertSeverity_WhenMemoryPercentAboveNinetyFive_ReturnsCritical()
    {
        var usage = new ResourceUsage
        {
            CpuPercent    = 10,
            MemoryMb      = 980,
            MemoryLimitMb = 1024
        };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Critical);
    }

    /// <summary>
    /// Verifies that <see cref="ResourceUsage.ToSummaryLine"/> includes the application ID, name, and CPU percentage in its output.
    /// </summary>
    [Fact]
    public void ToSummaryLine_IncludesApplicationIdAndName()
    {
        var usage = new ResourceUsage
        {
            ApplicationId   = 7,
            ApplicationName = "my-api",
            CpuPercent      = 42.5,
            MemoryMb        = 128,
            MemoryLimitMb   = 512
        };

        var line = usage.ToSummaryLine();

        line.Should().Contain("7");
        line.Should().Contain("my-api");
        line.Should().Contain("42.5");
    }
}
