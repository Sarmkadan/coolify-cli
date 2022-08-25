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

        var problems = new List<string>();

        // ConsoleLogger has no public properties to validate
        // The validation is primarily about the instance itself being non-null
        // and the constructor parameters being valid (handled by the constructor)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ConsoleLogger"/> instance is valid.
    /// </summary>
    /// <param name="value">The logger instance to check.</param>
    /// <returns>True if the logger is valid; otherwise, false.</returns>
    public static bool IsValid(this ConsoleLogger? value)
    {
        try
        {
            _ = value.Validate();
            return true;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that a <see cref="ConsoleLogger"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The logger instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ConsoleLogger? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ConsoleLogger is not valid. Problems: {string.Join(", ", problems)}");
        }
    }
}