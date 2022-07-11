using Xunit;
using FluentAssertions;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the ApplicationDeployment class.
/// </summary>
public class ApplicationDeploymentTests
{
    /// <summary>
    /// Verifies that the Validate method returns an error when the name is missing.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnError_WhenNameIsMissing()
    {
        var deployment = new ApplicationDeployment { Name = "" };
        var errors = deployment.Validate();
        errors.Should().Contain("Application name is required.");
    }

    /// <summary>
    /// Verifies that the Validate method returns an error when the ports are empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnError_WhenPortsAreEmpty()
    {
        var deployment = new ApplicationDeployment { Name = "app", Repository = "repo", EnvironmentId = "env", StartCommand = "start", Ports = new List<string>() };
        var errors = deployment.Validate();
        errors.Should().Contain("At least one port must be specified.");
    }
}
