#nullable enable

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="CoolifyApiClientOptions"/> instances.
/// Validates timeout values to ensure they are positive, non-zero, and within reasonable bounds.
/// </summary>
public static class CoolifyApiClientOptionsValidation
{
    private const int MinimumTimeoutSeconds = 1;
    private const int MaximumTimeoutSeconds = 3600; // 1 hour

    /// <summary>
    /// Validates the specified <see cref="CoolifyApiClientOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CoolifyApiClientOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        ValidateTimeout(value.GetTimeoutSeconds, nameof(value.GetTimeoutSeconds), errors);
        ValidateTimeout(value.PostTimeoutSeconds, nameof(value.PostTimeoutSeconds), errors);
        ValidateTimeout(value.PutTimeoutSeconds, nameof(value.PutTimeoutSeconds), errors);
        ValidateTimeout(value.DeleteTimeoutSeconds, nameof(value.DeleteTimeoutSeconds), errors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CoolifyApiClientOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CoolifyApiClientOptions value)
    {
        try
        {
            _ = Validate(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="CoolifyApiClientOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this CoolifyApiClientOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "CoolifyApiClientOptions validation failed. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }

    private static void ValidateTimeout(int timeoutSeconds, string propertyName, List<string> errors)
    {
        if (timeoutSeconds < MinimumTimeoutSeconds)
        {
            errors.Add($"{propertyName} must be at least {MinimumTimeoutSeconds} second(s), but was {timeoutSeconds}.");
        }
        else if (timeoutSeconds > MaximumTimeoutSeconds)
        {
            errors.Add($"{propertyName} must be at most {MaximumTimeoutSeconds} seconds (1 hour), but was {timeoutSeconds}.");
        }
    }
}
