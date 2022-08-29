#nullable enable

using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace CoolifyCli.Extensions;

/// <summary>
/// Extension methods for string manipulation and validation.
/// Provides utilities for formatting, validation, and text processing.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to PascalCase format.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The PascalCase formatted string, or the original string if it's null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToPascalCase(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return input;

        return string.Concat(input.Split([' ', '-', '_'])
            .Select(word => word.Length > 0
                ? char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()
                : string.Empty));
    }

    /// <summary>
    /// Converts a string to camelCase format.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The camelCase formatted string, or the original string if it's null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToCamelCase(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return input;

        var pascalCase = input.ToPascalCase();
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>
    /// Converts a string to snake_case format.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The snake_case formatted string, or the original string if it's null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToSnakeCase(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return input;

        var regex = new System.Text.RegularExpressions.Regex("([a-z0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))");
        return regex.Replace(input, "$1_").ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to kebab-case format.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The kebab-case formatted string, or the original string if it's null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToKebabCase(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Truncates a string to the specified maximum length, optionally adding ellipsis.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="addEllipsis">Whether to append ellipsis when truncating.</param>
    /// <returns>The truncated string, or the original string if it's null, empty, or shorter than maxLength.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string? input, int maxLength, bool addEllipsis = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        var truncated = input[..maxLength];
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>
    /// Repeats a string the specified number of times.
    /// </summary>
    /// <param name="input">The string to repeat.</param>
    /// <param name="times">The number of times to repeat the string.</param>
    /// <returns>A new string containing the input repeated the specified number of times, or empty string if times is zero or negative.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="times"/> is negative.</exception>
    public static string Repeat(this string? input, int times)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(times);

        return times == 0 ? string.Empty : string.Concat(Enumerable.Repeat(input, times));
    }

    /// <summary>
    /// Checks if a string is a valid email format.
    /// </summary>
    /// <param name="input">The email address to validate.</param>
    /// <returns>True if the string is a valid email format; otherwise, false.</returns>
    public static bool IsValidEmail(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            var addr = new MailAddress(input);
            return addr.Address == input.Trim();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string is a valid URL format.
    /// </summary>
    /// <param name="input">The URL to validate.</param>
    /// <returns>True if the string is a valid URL format; otherwise, false.</returns>
    public static bool IsValidUrl(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Checks if a string is a valid IPv4 address.
    /// </summary>
    /// <param name="input">The IP address to validate.</param>
    /// <returns>True if the string is a valid IPv4 address; otherwise, false.</returns>
    public static bool IsValidIpAddress(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return IPAddress.TryParse(input, out _);
    }

    /// <summary>
    /// Masks sensitive information in a string (e.g., API key), showing only first and last characters.
    /// </summary>
    /// <param name="input">The string containing sensitive information to mask.</param>
    /// <param name="showChars">The number of characters to show at the beginning and end.</param>
    /// <returns>The masked string, or the original string if it's null, empty, or too short to mask.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="showChars"/> is negative.</exception>
    public static string MaskSensitive(this string? input, int showChars = 4)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(showChars);

        if (string.IsNullOrEmpty(input) || input.Length <= showChars * 2)
            return input;

        var first = input[..showChars];
        var last = input[^showChars..];
        var masked = new string('*', input.Length - (showChars * 2));

        return $"{first}{masked}{last}";
    }

    /// <summary>
    /// Converts a string to a query parameter-safe format by URL encoding.
    /// </summary>
    /// <param name="input">The string to URL encode.</param>
    /// <returns>The URL-encoded string, or null if the input is null.</returns>
    public static string? ToQueryParameter(this string? input)
    {
        return input == null ? null : WebUtility.UrlEncode(input);
    }

    /// <summary>
    /// Decodes a URL-encoded query parameter back to readable text.
    /// </summary>
    /// <param name="input">The URL-encoded string to decode.</param>
    /// <returns>The decoded string, or null if the input is null.</returns>
    public static string? FromQueryParameter(this string? input)
    {
        return input == null ? null : WebUtility.UrlDecode(input);
    }

    /// <summary>
    /// Splits a string by a delimiter and returns non-empty trimmed parts.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <param name="delimiters">The delimiter characters.</param>
    /// <returns>An array of non-empty, trimmed strings.</returns>
    public static string[] SplitTrimmed(this string input, params char[] delimiters)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return input
            .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();
    }

    /// <summary>
    /// Converts a delimited string to a camelCase identifier.
    /// Used for converting CLI arguments to property names.
    /// </summary>
    /// <param name="input">The delimited string to convert.</param>
    /// <returns>The camelCase identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToIdentifier(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .ToCamelCase();
    }

    /// <summary>
    /// Adds ANSI color codes to string for console output.
    /// </summary>
    /// <param name="input">The string to colorize.</param>
    /// <param name="color">The console color to apply.</param>
    /// <returns>The ANSI-colored string, or null if the input is null.</returns>
    public static string? WithColor(this string input, ConsoleColor color)
    {
        if (input == null)
            return null;

        var colorCode = (int)color;
        return $"\x1b[38;5;{colorCode}m{input}\x1b[0m";
    }

    /// <summary>
    /// Pads a string with a character to match a target width, left-aligned.
    /// </summary>
    /// <param name="input">The string to pad.</param>
    /// <param name="width">The target width of the resulting string.</param>
    /// <param name="padChar">The character to use for padding.</param>
    /// <returns>The padded string, or the original string if it's already at or exceeds the target width.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> is negative.</exception>
    public static string PadTo(this string? input, int width, char padChar = ' ')
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(width);

        if (input.Length >= width)
            return input;

        return input + new string(padChar, width - input.Length);
    }
}