using Xunit;
using FluentAssertions;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

public class ApplicationDeploymentTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenNameIsMissing()
    {
        var deployment = new ApplicationDeployment { Name = "" };
        var errors = deployment.Validate();
        errors.Should().Contain("Application name is required.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPortsAreEmpty()
    {
        var deployment = new ApplicationDeployment { Name = "app", Repository = "repo", EnvironmentId = "env", StartCommand = "start", Ports = new List<string>() };
        var errors = deployment.Validate();
        errors.Should().Contain("At least one port must be specified.");
    }
}
