#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class ResourceUsageTests
{
    [Fact]
    public void MemoryPercent_WhenLimitIsZero_ReturnsZero()
    {
        var usage = new ResourceUsage { MemoryMb = 512, MemoryLimitMb = 0 };

        usage.MemoryPercent.Should().Be(0);
    }

    [Fact]
    public void MemoryPercent_WhenHalfOfLimit_ReturnsFiftyPercent()
    {
        var usage = new ResourceUsage { MemoryMb = 256, MemoryLimitMb = 512 };

        usage.MemoryPercent.Should().Be(50.0);
    }

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

    [Fact]
    public void GetAlertSeverity_WhenCpuAboveEighty_ReturnsWarning()
    {
        var usage = new ResourceUsage { CpuPercent = 85, MemoryMb = 100, MemoryLimitMb = 1024 };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Warning);
    }

    [Fact]
    public void GetAlertSeverity_WhenCpuAboveNinetyFive_ReturnsCritical()
    {
        var usage = new ResourceUsage { CpuPercent = 97, MemoryMb = 100, MemoryLimitMb = 1024 };

        usage.GetAlertSeverity().Should().Be(SeverityLevel.Critical);
    }

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
