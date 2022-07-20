#nullable enable

using CoolifyCli.Extensions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Extension methods for <see cref="StringExtensionsTests"/> to facilitate testing scenarios.
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a test instance of StringExtensionsTests for use in test scenarios.
    /// </summary>
    /// <returns>A new instance of StringExtensionsTests.</returns>
    public static StringExtensionsTests CreateTestInstance(this StringExtensionsTests _) => new();

    /// <summary>
    /// Asserts that ToPascalCase correctly converts hyphen-delimited words to PascalCase.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="input">The input string to test.</param>
    /// <param name="expected">The expected PascalCase result.</param>
    public static void AssertToPascalCase(
        this StringExtensionsTests test,
        string input,
        string expected)
    {
        var result = input.ToPascalCase();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Asserts that Truncate behaves correctly based on max length.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="input">The input string to test.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="expected">The expected truncated result.</param>
    public static void AssertTruncate(
        this StringExtensionsTests test,
        string input,
        int maxLength,
        string expected)
    {
        var result = input.Truncate(maxLength);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Asserts that MaskSensitive correctly masks sensitive data.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="input">The input string to mask.</param>
    /// <param name="showChars">Number of characters to show at start and end.</param>
    /// <param name="expected">The expected masked result.</param>
    public static void AssertMaskSensitive(
        this StringExtensionsTests test,
        string input,
        int showChars,
        string expected)
    {
        var result = input.MaskSensitive(showChars);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Asserts that SplitTrimmed correctly splits and trims strings.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="input">The input string to split.</param>
    /// <param name="separator">The separator character.</param>
    /// <param name="expected">The expected split and trimmed result.</param>
    public static void AssertSplitTrimmed(
        this StringExtensionsTests test,
        string input,
        char separator,
        string[] expected)
    {
        var result = input.SplitTrimmed(separator);
        Assert.Equal(expected, result);
    }
}