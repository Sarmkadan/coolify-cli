using Xunit;
using FluentAssertions;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

public class EnvironmentVariableTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenKeyIsInvalidFormat()
    {
        var variable = new EnvironmentVariable { Key = "invalid-key", Value = "val", ApplicationId = "app" };
        var errors = variable.Validate();
        errors.Should().Contain("Environment variable key must contain only alphanumeric characters and underscores.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenSecretValueIsTooShort()
    {
        var variable = new EnvironmentVariable { Key = "MY_SECRET", Value = "123", IsSecret = true, ApplicationId = "app" };
        var errors = variable.Validate();
        errors.Should().Contain("Secret values should be at least 8 characters long.");
    }
}
