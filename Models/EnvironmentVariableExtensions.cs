namespace CoolifyCli.Models;

public static class EnvironmentVariableExtensions
{
    /// <summary>
    /// Returns a human-readable string representation of the environment variable.
    /// </summary>
    /// <param name="environmentVariable">The environment variable.</param>
    /// <returns>A string representation of the environment variable.</returns>
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
    public static bool IsSensitive(this EnvironmentVariable environmentVariable)
    {
        ArgumentNullException.ThrowIfNull(environmentVariable);

        return environmentVariable.IsSecret;
    }

    /// <summary>
    /// Creates a new environment variable with the same properties as the original, but with a new <see cref="EnvironmentVariable.Id"/>.
    /// </summary>
    /// <param name="environmentVariable">The environment variable to clone.</param>
    /// <returns>A new environment variable with the same properties as the original.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the environment variable's <see cref="EnvironmentVariable.Clone"/> method returns null.</exception>
    public static EnvironmentVariable CreateCopy(this EnvironmentVariable environmentVariable)
    {
        ArgumentNullException.ThrowIfNull(environmentVariable);

        var copy = environmentVariable.Clone();
        if (copy == null)
        {
            throw new InvalidOperationException("Failed to create a copy of the environment variable.");
        }

        return copy;
    }
}
