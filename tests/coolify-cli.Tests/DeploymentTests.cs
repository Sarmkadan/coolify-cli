#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Caching;
using CoolifiCli.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoolifiCli.Tests;

public class DeploymentTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_IncludesNameRequiredError()
    {
        // Arrange — all fields provided except Name
        var deployment = new ApplicationDeployment
        {
            Repository = "https://github.com/user/repo",
            EnvironmentId = "env-prod",
            BuildCommand = "npm run build",
            Ports = ["3000"]
        };

        // Act
        var errors = deployment.Validate().ToList();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithCompleteValidConfiguration_ReturnsNoErrors()
    {
        // Arrange
        var deployment = new ApplicationDeployment
        {
            Name = "my-service",
            Repository = "https://github.com/user/my-service",
            EnvironmentId = "env-production",
            BuildCommand = "dotnet publish -c Release",
            Ports = ["8080"],
            HealthCheckIntervalSeconds = 30
        };

        // Act
        var errors = deployment.Validate().ToList();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsDeployed_AfterPreviousFailures_ResetsFailureStateAndSetsTimestamp()
    {
        // Arrange
        var deployment = new ApplicationDeployment { Name = "my-service" };
        deployment.MarkAsFailed("build timeout");
        deployment.MarkAsFailed("health check failed");

        // Act
        deployment.MarkAsDeployed();

        // Assert
        deployment.Status.Should().Be(DeploymentStatus.Deployed);
        deployment.FailureCount.Should().Be(0);
        deployment.LastErrorMessage.Should().BeNull();
        deployment.LastDeployedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage()
    {
        // Arrange
        var deployment = new ApplicationDeployment { Name = "my-service" };

        // Act
        deployment.MarkAsFailed("timeout on step 1");
        deployment.MarkAsFailed("timeout on step 2");

        // Assert
        deployment.FailureCount.Should().Be(2);
        deployment.LastErrorMessage.Should().Be("timeout on step 2");
        deployment.Status.Should().Be(DeploymentStatus.Failed);
    }

    [Fact]
    public void RequiresAttention_WhenFailureCountReachesThreshold_ReturnsTrue()
    {
        // Arrange
        var deployment = new ApplicationDeployment { Name = "my-service" };
        deployment.MarkAsFailed("error");
        deployment.MarkAsFailed("error");
        deployment.MarkAsFailed("error");

        // Act & Assert
        deployment.RequiresAttention().Should().BeTrue();
    }

    [Fact]
    public void CacheProvider_GetOrAdd_WhenKeyAbsent_DelegatesValueCreationToFactory()
    {
        // Arrange
        var mockCache = new Mock<ICacheProvider>();
        mockCache
            .Setup(c => c.GetOrAdd<ApplicationDeployment>(
                "deployment:42",
                It.IsAny<Func<ApplicationDeployment>>(),
                It.IsAny<TimeSpan?>()))
            .Returns<string, Func<ApplicationDeployment>, TimeSpan?>((_, factory, __) => factory());

        // Act
        var result = mockCache.Object.GetOrAdd<ApplicationDeployment>(
            "deployment:42",
            () => new ApplicationDeployment { Id = 42, Name = "cached-service" });

        // Assert
        result.Id.Should().Be(42);
        result.Name.Should().Be("cached-service");
        mockCache.Verify(
            c => c.GetOrAdd<ApplicationDeployment>(
                "deployment:42",
                It.IsAny<Func<ApplicationDeployment>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }
}
