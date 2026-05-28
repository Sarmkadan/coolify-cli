#nullable enable
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
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return string.Concat(input.Split(' ', '-', '_')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Converts a string to camelCase format.
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var pascalCase = input.ToPascalCase();
        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    /// Converts a string to snake_case format.
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var regex = new System.Text.RegularExpressions.Regex("([a-z0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))");
        return regex.Replace(input, "$1_").ToLower();
    }

    /// <summary>
    /// Converts a string to kebab-case format.
    /// </summary>
    public static string ToKebabCase(this string input)
    {
        return input.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Truncates a string to the specified maximum length, optionally adding ellipsis.
    /// </summary>
    public static string Truncate(this string input, int maxLength, bool addEllipsis = true)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        var truncated = input.Substring(0, maxLength);
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>
    /// Repeats a string the specified number of times.
    /// </summary>
    public static string Repeat(this string input, int times)
    {
        if (times <= 0)
            return string.Empty;

        return string.Concat(Enumerable.Repeat(input, times));
    }

    /// <summary>
    /// Checks if a string is a valid email format using basic regex.
    /// </summary>
    public static bool IsValidEmail(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address == input;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string is a valid URL format.
    /// </summary>
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
    public static bool IsValidIpAddress(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return System.Net.IPAddress.TryParse(input, out _);
    }

    /// <summary>
    /// Masks sensitive information in a string (e.g., API key), showing only first and last characters.
    /// </summary>
    public static string MaskSensitive(this string input, int showChars = 4)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= showChars * 2)
            return input;

        var first = input.Substring(0, showChars);
        var last = input.Substring(input.Length - showChars);
        var masked = new string('*', input.Length - (showChars * 2));

        return $"{first}{masked}{last}";
    }

    /// <summary>
    /// Converts a string to a query parameter-safe format by URL encoding.
    /// </summary>
    public static string ToQueryParameter(this string input)
    {
        return System.Net.WebUtility.UrlEncode(input);
    }

    /// <summary>
    /// Decodes a URL-encoded query parameter back to readable text.
    /// </summary>
    public static string FromQueryParameter(this string input)
    {
        return System.Net.WebUtility.UrlDecode(input);
    }

    /// <summary>
    /// Splits a string by a delimiter and returns non-empty trimmed parts.
    /// </summary>
    public static string[] SplitTrimmed(this string input, params char[] delimiters)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new string[0];

        return input
            .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();
    }

    /// <summary>
    /// Converts a delimited string to a camelCase identifier.
    /// Used for converting CLI arguments to property names.
    /// </summary>
    public static string ToIdentifier(this string input)
    {
        return input.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-")
            .ToCamelCase();
    }

    /// <summary>
    /// Adds color codes to string for console output.
    /// </summary>
    public static string WithColor(this string input, ConsoleColor color)
    {
        var colorCode = (int)color;
        return $"\x1b[38;5;{colorCode}m{input}\x1b[0m";
    }

    /// <summary>
    /// Pads a string with a character to match a target width, left-aligned.
    /// </summary>
    public static string PadTo(this string input, int width, char padChar = ' ')
    {
        if (input.Length >= width)
            return input;

        return input + new string(padChar, width - input.Length);
    }
}
