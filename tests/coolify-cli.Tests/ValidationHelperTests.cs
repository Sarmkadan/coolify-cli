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
    /// <param name="expected">The expected validation result.</param>
    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsValidId_WithVariousInputs_ReturnsExpectedResult(int id, bool expected)
    {
        var result = ValidationHelper.IsValidId(id);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidEmail(string)"/> correctly validates email addresses.
    /// </summary>
    /// <param name="email">The email address string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name+tag@sub.domain.org", true)]
    [InlineData("notanemail", false)]
    [InlineData("@missing-local.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_WithVariousAddresses_ReturnsExpectedResult(string email, bool expected)
    {
        var result = ValidationHelper.IsValidEmail(email);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidPort(string)"/> correctly validates port numbers.
    /// </summary>
    /// <param name="port">The port string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    [Theory]
    [InlineData("1", true)]
    [InlineData("8080", true)]
    [InlineData("65535", true)]
    [InlineData("0", false)]
    [InlineData("65536", false)]
    [InlineData("abc", false)]
    public void IsValidPort_WithBoundaryAndInvalidValues_ReturnsExpectedResult(string port, bool expected)
    {
        var result = ValidationHelper.IsValidPort(port);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidSemanticVersion(string)"/> correctly validates semantic version strings.
    /// </summary>
    /// <param name="version">The semantic version string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
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
        var result = ValidationHelper.IsValidSemanticVersion(version);
        result.IsValid.Should().Be(expected);
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
        var result = ValidationHelper.IsValidCommitHash(hash);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidDatabaseName(string)"/> rejects database names that start with a digit.
    /// </summary>
    [Fact]
    public void IsValidDatabaseName_WithNameStartingWithDigit_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidDatabaseName("123_invalid_db");
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidResourceName(string)"/> rejects resource names that end with a hyphen.
    /// </summary>
    [Fact]
    public void IsValidResourceName_WithTrailingHyphen_ReturnsFalse()
    {
        // Resource names must not end with a hyphen per naming conventions
        var result = ValidationHelper.IsValidResourceName("my-app-");
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidUrl(string)"/> correctly validates various URL formats.
    /// </summary>
    [Theory]
    [InlineData("http://localhost:8000", true)]
    [InlineData("https://example.com", true)]
    [InlineData("http://192.168.1.1:8080", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("example.com", false)]
    [InlineData("", false)]
    public void IsValidUrl_WithVariousFormats_ReturnsExpectedResult(string url, bool expected)
    {
        var result = ValidationHelper.IsValidUrl(url);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidHostname(string)"/> supports single-label hostnames.
    /// </summary>
    [Theory]
    [InlineData("localhost", true)]
    [InlineData("myhost", true)]
    [InlineData("example.com", true)]
    [InlineData("sub.example.com", true)]
    [InlineData("invalid_host", false)]
    public void IsValidHostname_WithSingleLabelAndDomainNames_ReturnsExpectedResult(string hostname, bool expected)
    {
        var result = ValidationHelper.IsValidHostname(hostname);
        result.IsValid.Should().Be(expected);
    }
}