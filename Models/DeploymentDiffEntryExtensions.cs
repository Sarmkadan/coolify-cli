#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Provides useful extension methods for <see cref="DeploymentDiffEntry"/> to facilitate
/// comparison, formatting, and working with deployment differences.
/// </summary>
public static class DeploymentDiffEntryExtensions
{
    /// <summary>
    /// Determines whether this entry represents a critical change that should be reviewed carefully.
    /// Critical changes include repository URL, environment ID, port changes, or sensitive environment variables.
    /// </summary>
    /// <param name="entry">The deployment diff entry to check.</param>
    /// <returns>True if this is a critical change; otherwise, false.</returns>
    public static bool IsCriticalChange(this DeploymentDiffEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        return entry.Property switch
        {
            "Repository" or "EnvironmentId" or "Ports" => true,
            _ when entry.Property.StartsWith("env:", StringComparison.Ordinal) &&
                 (entry.Property.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) ||
                  entry.Property.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
                  entry.Property.Contains("TOKEN", StringComparison.OrdinalIgnoreCase)) => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets a formatted display string showing the before and after values for this change.
    /// </summary>
    /// <param name="entry">The deployment diff entry.</param>
    /// <param name="includeCategory">Whether to include the category in the output.</param>
    /// <returns>A formatted string representation of the change.</returns>
    public static string FormatChange(this DeploymentDiffEntry entry, bool includeCategory = true)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var categoryPrefix = includeCategory && !string.IsNullOrEmpty(entry.Category) && entry.Category != "General"
            ? $"[{entry.Category}] "
            : string.Empty;

        return entry.HasChange
            ? $"{categoryPrefix}{entry.Property}: '{entry.CurrentValue}' → '{entry.ProposedValue}'"
            : $"{categoryPrefix}{entry.Property}: no change (both '{entry.CurrentValue}')";
    }

    /// <summary>
    /// Determines whether this entry represents a change to a sensitive value.
    /// </summary>
    /// <param name="entry">The deployment diff entry to check.</param>
    /// <returns>True if this entry contains sensitive data; otherwise, false.</returns>
    public static bool IsSensitiveChange(this DeploymentDiffEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        return entry.Property.StartsWith("env:", StringComparison.Ordinal) &&
               (entry.Property.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) ||
                entry.Property.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
                entry.Property.Contains("TOKEN", StringComparison.OrdinalIgnoreCase) ||
                entry.Property.Contains("KEY", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates a deep copy of this deployment diff entry.
    /// </summary>
    /// <param name="entry">The deployment diff entry to copy.</param>
    /// <returns>A new <see cref="DeploymentDiffEntry"/> instance with the same values.</returns>
    public static DeploymentDiffEntry DeepCopy(this DeploymentDiffEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        return new DeploymentDiffEntry
        {
            Property = entry.Property,
            CurrentValue = entry.CurrentValue,
            ProposedValue = entry.ProposedValue,
            Category = entry.Category
        };
    }

    /// <summary>
    /// Determines whether this entry represents a change in a resource-related property.
    /// </summary>
    /// <param name="entry">The deployment diff entry to check.</param>
    /// <returns>True if this is a resource-related change; otherwise, false.</returns>
    public static bool IsResourceChange(this DeploymentDiffEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        return entry.Category.Equals("Resources", StringComparison.OrdinalIgnoreCase);
    }
}
