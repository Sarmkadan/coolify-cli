using Xunit;
using FluentAssertions;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

public class DatabaseConfigurationTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenNameIsMissing()
    {
        var config = new DatabaseConfiguration { Name = "" };
        var errors = config.Validate();
        errors.Should().Contain("Database name is required.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPortIsInvalid()
    {
        var config = new DatabaseConfiguration { Name = "db", Port = 0 };
        var errors = config.Validate();
        errors.Should().Contain("Invalid port: 0. Must be between 1 and 65535.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPasswordIsTooShort()
    {
        var config = new DatabaseConfiguration { Name = "db", Port = 5432, RootPassword = "123" };
        var errors = config.Validate();
        errors.Should().Contain("Root password must be at least 8 characters long.");
    }
}
