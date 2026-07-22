#nullable enable
using System.Text.RegularExpressions;

namespace CoolifyCli.Utilities;

/// <summary>
/// Helper class for input validation. Provides reusable validation methods for
/// common patterns like IDs, URLs, emails, and resource names.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that a value is a positive integer ID.
    /// </summary>
    public static bool IsValidId(int? value)
    {
        return value > 0;
    }

    /// <summary>
    /// Validates that a string is not null or whitespace.
    /// </summary>
    public static bool IsValidString(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validates that a string has a minimum length.
    /// </summary>
    public static bool HasMinLength(string? value, int minLength)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= minLength;
    }

    /// <summary>
    /// Validates that a string has a maximum length.
    /// </summary>
    public static bool HasMaxLength(string? value, int maxLength)
    {
        return string.IsNullOrEmpty(value) || value.Length <= maxLength;
    }

    /// <summary>
    /// Validates that a string length is within a range.
    /// </summary>
    public static bool HasLengthBetween(string? value, int minLength, int maxLength)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates that a string matches a regex pattern.
    /// </summary>
    public static bool MatchesPattern(string? value, string pattern)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Validates that a string is a valid email address.
    /// </summary>
    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        return Regex.IsMatch(value, emailPattern);
    }

    /// <summary>
    /// Validates that a string is a valid URL.
    /// </summary>
    public static bool IsValidUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates that a string is a valid IPv4 address.
    /// </summary>
    public static bool IsValidIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return System.Net.IPAddress.TryParse(value, out _);
    }

    /// <summary>
    /// Validates that a string is a valid hostname.
    /// </summary>
    public static bool IsValidHostname(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Hostname can contain letters, digits, hyphens, and dots
        const string hostnamePattern = @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z]{2,}$";
        return Regex.IsMatch(value, hostnamePattern);
    }

    /// <summary>
    /// Validates that a string is a valid port number (1-65535).
    /// </summary>
    public static bool IsValidPort(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!int.TryParse(value, out var port))
            return false;

        return port >= 1 && port <= 65535;
    }

    /// <summary>
    /// Validates that a string is a valid database name (alphanumeric with underscore).
    /// </summary>
    public static bool IsValidDatabaseName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string dbNamePattern = @"^[a-zA-Z][a-zA-Z0-9_]{0,63}$";
        return Regex.IsMatch(value, dbNamePattern);
    }

    /// <summary>
    /// Validates that a string is a valid username (alphanumeric, hyphen, underscore).
    /// </summary>
    public static bool IsValidUsername(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 255)
            return false;

        const string usernamePattern = @"^[a-zA-Z0-9._-]+$";
        return Regex.IsMatch(value, usernamePattern);
    }

    /// <summary>
    /// Validates that a string is in a list of allowed values.
    /// </summary>
    public static bool IsOneOf(string? value, params string[] allowedValues)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that a number is within a range.
    /// </summary>
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a number is within a range.
    /// </summary>
    public static bool IsInRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a collection is not null and not empty.
    /// </summary>
    public static bool IsNotEmpty<T>(IEnumerable<T>? collection)
    {
        return collection is not null && collection.Any();
    }

    /// <summary>
    /// Validates a collection has a minimum count.
    /// </summary>
    public static bool HasMinimumCount<T>(IEnumerable<T>? collection, int minimum)
    {
        return collection is not null && collection.Count() >= minimum;
    }

    /// <summary>
    /// Validates a collection has a maximum count.
    /// </summary>
    public static bool HasMaximumCount<T>(IEnumerable<T>? collection, int maximum)
    {
        return collection is null || collection.Count() <= maximum;
    }

    /// <summary>
    /// Validates that a string is a valid semantic version (major.minor.patch).
    /// </summary>
    public static bool IsValidSemanticVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string versionPattern = @"^\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?$";
        return Regex.IsMatch(value, versionPattern);
    }

    /// <summary>
    /// Validates that a string is a valid Git commit hash (40 hex characters).
    /// </summary>
    public static bool IsValidCommitHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-f0-9]{40}$");
    }

    /// <summary>
    /// Validates that a string is a valid Git branch name.
    /// </summary>
    public static bool IsValidBranchName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Git branch names cannot start with hyphen, contain consecutive dots, or end with .lock
        const string branchPattern = @"^(?!-)[a-zA-Z0-9._/-]{1,255}(?<!\.lock)$";
        return Regex.IsMatch(value, branchPattern);
    }

    /// <summary>
    /// Validates that a DateTime is in the future.
    /// </summary>
    public static bool IsFutureDate(DateTime value)
    {
        return value > DateTime.UtcNow;
    }

    /// <summary>
    /// Validates that a DateTime is in the past.
    /// </summary>
    public static bool IsPastDate(DateTime value)
    {
        return value < DateTime.UtcNow;
    }

    /// <summary>
    /// Validates that a DateTime is today.
    /// </summary>
    public static bool IsToday(DateTime value)
    {
        return value.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Validates that a resource name follows naming conventions (alphanumeric, hyphen, no spaces).
    /// </summary>
    public static bool IsValidResourceName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 255)
            return false;

        const string resourceNamePattern = @"^[a-z0-9][a-z0-9-]*[a-z0-9]$";
        return Regex.IsMatch(value, resourceNamePattern);
    }
}
