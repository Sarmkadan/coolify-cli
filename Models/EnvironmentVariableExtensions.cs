namespace CoolifyCli.Models;

/// <summary>
/// Provides extension methods for <see cref="EnvironmentVariable"/> to support common operations
/// such as display formatting, sensitivity checking, and cloning.
/// </summary>
public static class EnvironmentVariableExtensions
{
    /// <summary>
    /// Returns a human-readable string representation of the environment variable.
    /// </summary>
    /// <param name="environmentVariable">The environment variable.</param>
    /// <returns>A string representation of the environment variable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="environmentVariable"/> is <see langword="null"/>.</exception>
    public static string ToDisplayString(this EnvironmentVariable environmentVariable)
    {
        ArgumentNullException.ThrowIfNull(environmentVariable);

        return $"Key: {environmentVariable.Key}, Value: {environmentVariable.GetDisplayValue()}";
    }

    /// <summary>
    /// Checks if the environment variable is sensitive (i.e., its value should not be logged).
    /// </summary>
    /// <param name="environmentVariable">The environment variable.</param>
    /// <returns>True if the environment variable is sensitive; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="environmentVariable"/> is <see langword="null"/>.</exception>
    public static bool IsSensitive(this EnvironmentVariable environmentVariable)
    {
        ArgumentNullException.ThrowIfNull(environmentVariable);

        return environmentVariable.IsSecret;
    }

    /// <summary>
    /// Creates a new environment variable with the same properties as the original.
    /// </summary>
    /// <param name="environmentVariable">The environment variable to clone.</param>
    /// <returns>A new environment variable with the same properties as the original.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="environmentVariable"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the environment variable's <see cref="EnvironmentVariable.Clone"/> method returns null.</exception>
    public static EnvironmentVariable CreateCopy(this EnvironmentVariable environmentVariable)
    {
        ArgumentNullException.ThrowIfNull(environmentVariable);

        var copy = environmentVariable.Clone();
        return copy ?? throw new InvalidOperationException("Failed to create a copy of the environment variable.");
    }
}
