#nullable enable

namespace CoolifyCli.Services;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="ApplicationService"/> instances.
/// Validates service initialization, dependencies, and configuration state.
/// </summary>
public static class ApplicationServiceValidation
{
    /// <summary>
    /// Validates the application service instance for proper initialization and configuration.
    /// </summary>
    /// <param name="value">The application service instance to validate.</param>
    /// <returns>Collection of validation error messages, empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ApplicationService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Service dependencies validation
        // Note: These would be validated in the constructor, but we check here for completeness

        // Validate logger is not null (though constructor ensures this)
        if (value.GetType().GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            errors.Add("Logger dependency is not initialized.");
        }

        // Validate API client is not null
        if (value.GetType().GetField("_apiClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            errors.Add("API client dependency is not initialized.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the application service instance is valid.
    /// </summary>
    /// <param name="value">The application service instance to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    public static bool IsValid(this ApplicationService value)
    {
        try
        {
            return Validate(value).Count == 0;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the application service instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The application service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the service is not properly initialized.</exception>
    public static void EnsureValid(this ApplicationService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ApplicationService is not valid. Validation errors: {string.Join("; ", errors)}");
        }
    }
}