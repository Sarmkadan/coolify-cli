#nullable enable
using CoolifyCli.Utilities;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Test suite for the <see cref="ValidationHelper"/> utility class.
/// </summary>
public class ValidationHelperTests
{
    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidId(int)"/> correctly identifies valid and invalid integer IDs.
    /// </summary>
    /// <param name="id">The integer ID to validate.</param>
    /// <param name="expected">The expected boolean result.</param>
    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsValidId_WithVariousInputs_ReturnsExpectedResult(int id, bool expected)
    {
        ValidationHelper.IsValidId(id).Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidEmail(string)"/> correctly validates email addresses.
    /// </summary>
    /// <param name="email">The email address string to validate.</param>
    /// <param name="expected">The expected boolean result.</param>
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

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidPort(string)"/> correctly validates port numbers.
    /// </summary>
    /// <param name="port">The port string to validate.</param>
    /// <param name="expected">The expected boolean result.</param>
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

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidSemanticVersion(string)"/> correctly validates semantic version strings.
    /// </summary>
    /// <param name="version">The semantic version string to validate.</param>
    /// <param name="expected">The expected boolean result.</param>
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

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidCommitHash(string)"/> correctly validates a 40‑character lowercase hexadecimal commit hash.
    /// </summary>
    [Fact]
    public void IsValidCommitHash_WithFortyLowercaseHexCharacters_ReturnsTrue()
    {
        // Arrange — a realistic-looking commit hash
        var hash = "a3f1b8c2d4e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3";

        // Act & Assert
        ValidationHelper.IsValidCommitHash(hash).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidDatabaseName(string)"/> rejects database names that start with a digit.
    /// </summary>
    [Fact]
    public void IsValidDatabaseName_WithNameStartingWithDigit_ReturnsFalse()
    {
        ValidationHelper.IsValidDatabaseName("123_invalid_db").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidResourceName(string)"/> rejects resource names that end with a hyphen.
    /// </summary>
    [Fact]
    public void IsValidResourceName_WithTrailingHyphen_ReturnsFalse()
    {
        // Resource names must not end with a hyphen per naming conventions
        ValidationHelper.IsValidResourceName("my-app-").Should().BeFalse();
    }
}
