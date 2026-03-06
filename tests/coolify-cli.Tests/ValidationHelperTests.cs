#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Utilities;
using FluentAssertions;
using Xunit;

namespace CoolifiCli.Tests;

public class ValidationHelperTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsValidId_WithVariousInputs_ReturnsExpectedResult(int id, bool expected)
    {
        ValidationHelper.IsValidId(id).Should().Be(expected);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name+tag@sub.domain.org", true)]
    [InlineData("notanemail", false)]
    [InlineData("@missing-local.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_WithVariousAddresses_ReturnsExpectedResult(string email, bool expected)
    {
        ValidationHelper.IsValidEmail(email).Should().Be(expected);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("8080", true)]
    [InlineData("65535", true)]
    [InlineData("0", false)]
    [InlineData("65536", false)]
    [InlineData("abc", false)]
    public void IsValidPort_WithBoundaryAndInvalidValues_ReturnsExpectedResult(string port, bool expected)
    {
        ValidationHelper.IsValidPort(port).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.0", true)]
    [InlineData("2.1.0-beta", true)]
    [InlineData("10.20.30", true)]
    [InlineData("1.2", false)]
    [InlineData("1.2.3.4", false)]
    [InlineData("v1.0.0", false)]
    public void IsValidSemanticVersion_WithVariousVersionStrings_ReturnsExpectedResult(
        string version, bool expected)
    {
        ValidationHelper.IsValidSemanticVersion(version).Should().Be(expected);
    }

    [Fact]
    public void IsValidCommitHash_WithFortyLowercaseHexCharacters_ReturnsTrue()
    {
        // Arrange — a realistic-looking commit hash
        var hash = "a3f1b8c2d4e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3";

        // Act & Assert
        ValidationHelper.IsValidCommitHash(hash).Should().BeTrue();
    }

    [Fact]
    public void IsValidDatabaseName_WithNameStartingWithDigit_ReturnsFalse()
    {
        ValidationHelper.IsValidDatabaseName("123_invalid_db").Should().BeFalse();
    }

    [Fact]
    public void IsValidResourceName_WithTrailingHyphen_ReturnsFalse()
    {
        // Resource names must not end with a hyphen per naming conventions
        ValidationHelper.IsValidResourceName("my-app-").Should().BeFalse();
    }
}
