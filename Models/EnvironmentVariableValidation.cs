#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Provides validation helpers for <see cref="EnvironmentVariable"/> instances.
/// </summary>
public static class EnvironmentVariableValidation
{
    /// <summary>
    /// Validates the environment variable and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The environment variable to validate.</param>
    /// <returns>Read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EnvironmentVariable value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(value.Key))
            errors.Add("Environment variable key is required.");
        else if (!IsValidKeyFormat(value.Key))
            errors.Add("Environment variable key must contain only alphanumeric characters and underscores, and cannot start with a digit.");

        if (string.IsNullOrWhiteSpace(value.ApplicationId))
            errors.Add("Application ID is required.");

        if (string.IsNullOrWhiteSpace(value.EnvironmentScope))
            errors.Add("Environment scope is required.");

        // Validate value based on secret status
        if (string.IsNullOrEmpty(value.Value))
        {
            if (!value.IsSecret)
                errors.Add("Environment variable value cannot be empty for non-secret variables.");
        }
        else
        {
            if (value.IsSecret && value.Value.Length > 0 && value.Value.Length < 8)
                errors.Add("Secret values should be at least 8 characters long.");
        }

        // Validate timestamps
        if (value.CreatedAt == default)
            errors.Add("CreatedAt timestamp must be set.");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("CreatedAt timestamp cannot be in the future.");

        if (value.UpdatedAt == default)
            errors.Add("UpdatedAt timestamp must be set.");
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("UpdatedAt timestamp cannot be in the future.");
        else if (value.CreatedAt > value.UpdatedAt)
            errors.Add("UpdatedAt timestamp cannot be earlier than CreatedAt timestamp.");

        // Validate active status
        if (!value.IsActive)
            errors.Add("Environment variable must be active.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the environment variable is valid.
    /// </summary>
    /// <param name="value">The environment variable to check.</param>
    /// <returns>True if the environment variable is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this EnvironmentVariable value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the environment variable is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The environment variable to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the environment variable is invalid, containing a list of validation errors.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this EnvironmentVariable value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Environment variable validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)
                }");
        }
    }

    /// <summary>
    /// Checks if the environment variable key follows naming conventions.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <returns>True if key format is valid.</returns>
    private static bool IsValidKeyFormat(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        if (char.IsDigit(key[0]))
            return false;

        return key.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}