#nullable enable

namespace CoolifyCli.Services;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="ApplicationService"/> instances.
/// Validates service initialization and configuration state.
/// </summary>
public static class ApplicationServiceValidation
{
    /// <summary>
    /// Validates the application service instance for proper initialization and configuration.
    /// Since <see cref="ApplicationService"/> constructor validates its dependencies,
    /// this method always returns an empty collection for valid instances.
    /// </summary>
    /// <param name="value">The application service instance to validate.</param>
    /// <returns>Collection of validation error messages, empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApplicationService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the application service instance is valid.
    /// </summary>
    /// <param name="value">The application service instance to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    public static bool IsValid(this ApplicationService value)
        => value is not null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the application service instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The application service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ApplicationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}