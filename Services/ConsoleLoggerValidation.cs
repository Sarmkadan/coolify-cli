#nullable enable

namespace CoolifyCli.Services;

/// <summary>
/// Provides validation helpers for <see cref="ConsoleLogger"/> instances.
/// </summary>
public static class ConsoleLoggerValidation
{
    /// <summary>
    /// Validates a <see cref="ConsoleLogger"/> instance.
    /// </summary>
    /// <param name="value">The logger instance to validate.</param>
    /// <returns>A list of validation problems; empty if the logger is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConsoleLogger? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // ConsoleLogger has no state to validate - it's always valid as long as it's not null
        // The constructor parameters are validated by the constructor itself
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="ConsoleLogger"/> instance is valid.
    /// </summary>
    /// <param name="value">The logger instance to check.</param>
    /// <returns>True if the logger is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConsoleLogger? value)
    {
        return value is not null;
    }

    /// <summary>
    /// Ensures that a <see cref="ConsoleLogger"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The logger instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ConsoleLogger? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}