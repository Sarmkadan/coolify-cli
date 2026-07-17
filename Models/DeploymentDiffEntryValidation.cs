#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Provides validation helpers for <see cref="DeploymentDiffEntry"/> instances.
/// Validates that all required fields are populated and contain valid values.
/// </summary>
public static class DeploymentDiffEntryValidation
{
    /// <summary>
    /// Validates the specified deployment diff entry.
    /// </summary>
    /// <param name="value">The deployment diff entry to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DeploymentDiffEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Property))
        {
            problems.Add("Property cannot be null or whitespace.");
        }
        else if (value.Property.Length > 200)
        {
            problems.Add("Property exceeds maximum length of 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.CurrentValue))
        {
            problems.Add("CurrentValue cannot be null or whitespace.");
        }
        else if (value.CurrentValue.Length > 4000)
        {
            problems.Add("CurrentValue exceeds maximum length of 4000 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.ProposedValue))
        {
            problems.Add("ProposedValue cannot be null or whitespace.");
        }
        else if (value.ProposedValue.Length > 4000)
        {
            problems.Add("ProposedValue exceeds maximum length of 4000 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Category))
        {
            problems.Add("Category cannot be null or whitespace.");
        }
        else if (value.Category.Length > 100)
        {
            problems.Add("Category exceeds maximum length of 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified deployment diff entry is valid.
    /// </summary>
    /// <param name="value">The deployment diff entry to check.</param>
    /// <returns>True if the entry is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DeploymentDiffEntry value)
        => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified deployment diff entry is valid.
    /// </summary>
    /// <param name="value">The deployment diff entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the entry contains validation problems.</exception>
    public static void EnsureValid(this DeploymentDiffEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DeploymentDiffEntry is invalid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}",
                nameof(value));
        }
    }
}