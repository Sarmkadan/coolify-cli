#nullable enable

using System.Text.RegularExpressions;

namespace CoolifyCli.Utilities;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
/// <param name="isValid">Indicates whether the validation succeeded.</param>
/// <param name="error">The error message describing why validation failed (null if valid).</param>
/// <param name="paramName">The name of the parameter being validated (optional).</param>
public sealed record ValidationResult(bool IsValid, string? Error = null, string? ParamName = null)
{
    /// <summary>
    /// Gets a validation result indicating success.
    /// </summary>
    public static ValidationResult Success => new(true);

    /// <summary>
    /// Creates a validation result indicating failure with the specified error message.
    /// </summary>
    /// <param name="error">The error message describing why validation failed.</param>
    /// <param name="paramName">The name of the parameter being validated (optional).</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(string error, string? paramName = null) => new(false, error, paramName);
}

/// <summary>
/// Helper class for input validation. Provides reusable validation methods for
/// common patterns like IDs, URLs, emails, and resource names.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that a value is a positive integer ID.
    /// </summary>
    /// <param name="value">The ID to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static ValidationResult IsValidId(int? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value > 0
            ? ValidationResult.Success
            : ValidationResult.Failure($"ID '{value}' must be a positive integer.", nameof(value));
    }

    /// <summary>
    /// Validates that a string is not null or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidString(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? ValidationResult.Success
            : ValidationResult.Failure("String cannot be null or whitespace.", nameof(value));
    }

    /// <summary>
    /// Validates that a string has a minimum length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLength">The minimum required length.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minLength"/> is less than 0.</exception>
    public static ValidationResult HasMinLength(string? value, int minLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minLength);

        return !string.IsNullOrEmpty(value) && value.Length >= minLength
            ? ValidationResult.Success
            : ValidationResult.Failure(
                value is null
                    ? $"String cannot be null."
                    : $"String '{value}' must have a minimum length of {minLength} characters (has {value.Length}).",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string has a maximum length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is less than 0.</exception>
    public static ValidationResult HasMaxLength(string? value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"String '{value}' must have a maximum length of {maxLength} characters (has {value.Length}).",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string length is within a range.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLength">The minimum required length.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minLength"/> or <paramref name="maxLength"/> is less than 0.</exception>
    public static ValidationResult HasLengthBetween(string? value, int minLength, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minLength);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return !string.IsNullOrEmpty(value) && value.Length >= minLength && value.Length <= maxLength
            ? ValidationResult.Success
            : ValidationResult.Failure(
                value is null
                    ? $"String cannot be null."
                    : $"String '{value}' must have a length between {minLength} and {maxLength} characters (has {value.Length}).",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string matches a regex pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is null or empty.</exception>
    public static ValidationResult MatchesPattern(string? value, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        if (string.IsNullOrEmpty(value))
            return ValidationResult.Failure("String cannot be null or empty.", nameof(value));

        return Regex.IsMatch(value, pattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"String '{value}' does not match the required pattern: {pattern}",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid email address.
    /// </summary>
    /// <param name="value">The email address to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Email cannot be null or whitespace.", nameof(value));

        const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        return Regex.IsMatch(value, emailPattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Email '{value}' is not valid. Expected format: user@domain.com",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid URL.
    /// Supports HTTP/HTTPS URLs with ports, IPv4/IPv6 addresses, and paths.
    /// Examples: http://localhost:8000, https://example.com, http://[::1]:8080
    /// </summary>
    /// <param name="value">The URL to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("URL cannot be null or whitespace.", nameof(value));

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                return ValidationResult.Success;
            }

            return ValidationResult.Failure(
                $"URL '{value}' is not valid. Scheme must be HTTP or HTTPS, got '{uri.Scheme}'.",
                nameof(value));
        }

        return ValidationResult.Failure(
            $"URL '{value}' is not a valid absolute HTTP/HTTPS URL. Examples: http://localhost:8000, https://example.com",
            nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid IPv4 address.
    /// </summary>
    /// <param name="value">The IP address to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("IP address cannot be null or whitespace.", nameof(value));

        if (System.Net.IPAddress.TryParse(value, out var ip))
        {
            return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                ? ValidationResult.Success
                : ValidationResult.Failure(
                    $"IP address '{value}' is not a valid IPv4 address. Use IPv4 format (e.g., 192.168.1.1).",
                    nameof(value));
        }

        return ValidationResult.Failure(
            $"IP address '{value}' is not valid. Expected format: 192.168.1.1",
            nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid hostname.
    /// Supports single-label hostnames (e.g., "localhost", "myhost"), domain names (e.g., "example.com"),
    /// and IPv4/IPv6 addresses.
    /// </summary>
    /// <param name="value">The hostname to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidHostname(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Hostname cannot be null or whitespace.", nameof(value));

        // Hostname can contain letters, digits, hyphens, and dots
        // Supports single-label hostnames for self-hosted setups
        const string hostnamePattern = @"^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        if (!Regex.IsMatch(value, hostnamePattern))
        {
            return ValidationResult.Failure(
                $"Hostname '{value}' is not valid. Must contain only letters, digits, hyphens, and dots, start and end with alphanumeric characters.",
                nameof(value));
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Validates that a string is a valid port number (1-65535).
    /// </summary>
    /// <param name="value">The port to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidPort(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Port cannot be null or whitespace.", nameof(value));

        if (!int.TryParse(value, out var port))
            return ValidationResult.Failure(
                $"Port '{value}' is not a valid integer.",
                nameof(value));

        return port >= 1 && port <= 65535
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Port '{value}' must be between 1 and 65535.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid database name (alphanumeric with underscore).
    /// </summary>
    /// <param name="value">The database name to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidDatabaseName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Database name cannot be null or whitespace.", nameof(value));

        const string dbNamePattern = @"^[a-zA-Z][a-zA-Z0-9_]{0,63}$";

        return Regex.IsMatch(value, dbNamePattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Database name '{value}' is not valid. Must start with a letter and contain only alphanumeric characters and underscores (max 63 chars).",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid username (alphanumeric, hyphen, underscore).
    /// </summary>
    /// <param name="value">The username to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidUsername(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Username cannot be null or whitespace.", nameof(value));

        if (value.Length > 255)
            return ValidationResult.Failure("Username cannot exceed 255 characters.", nameof(value));

        const string usernamePattern = @"^[a-zA-Z0-9._-]+$";

        return Regex.IsMatch(value, usernamePattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Username '{value}' can only contain alphanumeric characters, dots, hyphens, and underscores.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is in a list of allowed values.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="allowedValues">The allowed values.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="allowedValues"/> is <see langword="null"/>.</exception>
    public static ValidationResult IsOneOf(string? value, params string[] allowedValues)
    {
        ArgumentNullException.ThrowIfNull(allowedValues);

        if (string.IsNullOrEmpty(value))
            return ValidationResult.Failure("Value cannot be null or empty.", nameof(value));

        return allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Value '{value}' is not valid. Must be one of: {string.Join(", ", allowedValues)}",
                nameof(value));
    }

    /// <summary>
    /// Validates that a number is within a range.
    /// </summary>
    /// <param name="value">The number to validate.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Value {value} must be between {min} and {max}.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a number is within a range.
    /// </summary>
    /// <param name="value">The number to validate.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsInRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Value {value} must be between {min} and {max}.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a collection is not null and not empty.
    /// </summary>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public static ValidationResult IsNotEmpty<T>(IEnumerable<T>? collection)
    {
        return collection is not null && collection.Any()
            ? ValidationResult.Success
            : ValidationResult.Failure("Collection cannot be null or empty.", nameof(collection));
    }

    /// <summary>
    /// Validates a collection has a minimum count.
    /// </summary>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="minimum">The minimum required count.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimum"/> is less than 0.</exception>
    public static ValidationResult HasMinimumCount<T>(IEnumerable<T>? collection, int minimum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimum);

        return collection is not null && collection.Count() >= minimum
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Collection must have at least {minimum} items.",
                nameof(collection));
    }

    /// <summary>
    /// Validates a collection has a maximum count.
    /// </summary>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="maximum">The maximum allowed count.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximum"/> is less than 0.</exception>
    public static ValidationResult HasMaximumCount<T>(IEnumerable<T>? collection, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximum);

        return collection is null || collection.Count() <= maximum
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Collection cannot have more than {maximum} items.",
                nameof(collection));
    }

    /// <summary>
    /// Validates that a string is a valid semantic version (major.minor.patch).
    /// </summary>
    /// <param name="value">The version string to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidSemanticVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Version cannot be null or whitespace.", nameof(value));

        const string versionPattern = @"^\d+\.\d+\.\d+(?:-[a-zA-Z0-9]+)?$";

        return Regex.IsMatch(value, versionPattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Version '{value}' is not valid. Expected format: major.minor.patch or major.minor.patch-prerelease",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid Git commit hash (40 hex characters).
    /// </summary>
    /// <param name="value">The commit hash to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidCommitHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Commit hash cannot be null or whitespace.", nameof(value));

        return Regex.IsMatch(value, @"^[a-f0-9]{40}$")
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Commit hash '{value}' is not valid. Must be a 40-character hexadecimal string.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a string is a valid Git branch name.
    /// </summary>
    /// <param name="value">The branch name to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidBranchName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Branch name cannot be null or whitespace.", nameof(value));

        // Git branch names cannot start with hyphen, contain consecutive dots, or end with .lock
        const string branchPattern = @"^(?!-)[a-zA-Z0-9._/-]{1,255}(?<!\.lock)$";

        return Regex.IsMatch(value, branchPattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Branch name '{value}' contains invalid characters or format.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a DateTime is in the future.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsFutureDate(DateTime value)
    {
        return value > DateTime.UtcNow
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"DateTime '{value:O}' must be in the future.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a DateTime is in the past.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsPastDate(DateTime value)
    {
        return value < DateTime.UtcNow
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"DateTime '{value:O}' must be in the past.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a DateTime is today.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsToday(DateTime value)
    {
        return value.Date == DateTime.UtcNow.Date
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"DateTime '{value:O}' must be today.",
                nameof(value));
    }

    /// <summary>
    /// Validates that a resource name follows naming conventions (alphanumeric, hyphen, no spaces).
    /// </summary>
    /// <param name="value">The resource name to validate.</param>
    /// <returns>A validation result indicating success or failure with an error message.</returns>
    public static ValidationResult IsValidResourceName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure("Resource name cannot be null or whitespace.", nameof(value));

        if (value.Length > 255)
            return ValidationResult.Failure("Resource name cannot exceed 255 characters.", nameof(value));

        const string resourceNamePattern = @"^[a-z0-9][a-z0-9-]*[a-z0-9]$";

        return Regex.IsMatch(value, resourceNamePattern)
            ? ValidationResult.Success
            : ValidationResult.Failure(
                $"Resource name '{value}' is not valid. Must contain only lowercase letters, digits, and hyphens, start and end with alphanumeric characters, and be 3-255 characters long.",
                nameof(value));
    }
}