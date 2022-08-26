#nullable enable

using CoolifyCli.Extensions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Extension methods for <see cref="StringExtensionsTests"/> to facilitate testing scenarios.
/// Provides assertion helpers for StringExtensions extension methods.
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a test instance of StringExtensionsTests for use in test scenarios.
    /// </summary>
    /// <param name="_">The test instance (discard parameter).</param>
    /// <returns>A new instance of StringExtensionsTests.</returns>
    public static StringExtensionsTests CreateTestInstance(this StringExtensionsTests _) => new();

    /// <summary>
    /// Asserts that ToPascalCase correctly converts hyphen-delimited words to PascalCase.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="input">The input string to test.</param>
    /// <param name="expected">The expected PascalCase result.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="expected"/> is <see langword="null"/>.</exception>
    public static void AssertToPascalCase(
        this StringExtensionsTests test,
        string input,
        string expected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expected);

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
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static void AssertTruncate(
        this StringExtensionsTests test,
        string input,
        int maxLength,
        string expected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

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
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="expected"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="showChars"/> is negative.</exception>
    public static void AssertMaskSensitive(
        this StringExtensionsTests test,
        string input,
        int showChars,
        string expected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentOutOfRangeException.ThrowIfNegative(showChars);

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
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="expected"/> is <see langword="null"/>.</exception>
    public static void AssertSplitTrimmed(
        this StringExtensionsTests test,
        string input,
        char separator,
        string[] expected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expected);

        var result = input.SplitTrimmed(separator);
        Assert.Equal(expected, result);
    }
}