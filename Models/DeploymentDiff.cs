#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Describes a single property change between the current and proposed deployment configuration.
/// </summary>
public class DeploymentDiffEntry
{
    /// <summary>Gets or sets the name of the property that changed.</summary>
    public string Property { get; set; } = string.Empty;

    /// <summary>Gets or sets the value that is currently live.</summary>
    public string CurrentValue { get; set; } = string.Empty;

    /// <summary>Gets or sets the value that the deployment will apply.</summary>
    public string ProposedValue { get; set; } = string.Empty;

    /// <summary>Gets or sets the category group for rendering (e.g. "Core", "Resources", "EnvVars").</summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Returns true when the current and proposed values differ (non-null-safe string comparison).
    /// </summary>
    public bool HasChange => !string.Equals(CurrentValue, ProposedValue, StringComparison.Ordinal);
}

/// <summary>
/// Aggregates all detected differences between a live deployment and a proposed configuration.
/// Provides helper methods for categorised display and risk assessment.
/// </summary>
public class DeploymentDiff
{
    /// <summary>Gets the application ID this diff applies to.</summary>
    public int ApplicationId { get; init; }

    /// <summary>Gets the application name for display purposes.</summary>
    public string ApplicationName { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp at which the diff was computed.</summary>
    public DateTime ComputedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the list of individual property changes.</summary>
    public List<DeploymentDiffEntry> Entries { get; init; } = new();

    /// <summary>
    /// Returns only the entries that contain an actual value change.
    /// </summary>
    public IReadOnlyList<DeploymentDiffEntry> Changes =>
        Entries.Where(e => e.HasChange).ToList();

    /// <summary>
    /// Returns true when at least one property has a detected change.
    /// </summary>
    public bool HasChanges => Entries.Any(e => e.HasChange);

    /// <summary>
    /// Returns true when any changed property is considered high-risk
    /// (repository URL, environment ID, or port changes).
    /// </summary>
    public bool IsHighRisk => Changes.Any(c =>
        c.Property is "Repository" or "EnvironmentId" or "Ports");

    /// <summary>
    /// Computes the diff between a current deployment and a proposed one,
    /// returning a populated <see cref="DeploymentDiff"/> instance.
    /// </summary>
    /// <param name="current">The live deployment fetched from the API.</param>
    /// <param name="proposed">The deployment configuration to be applied.</param>
    /// <returns>A diff summarising all detected changes.</returns>
    public static DeploymentDiff Compute(ApplicationDeployment current, ApplicationDeployment proposed)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(proposed);

        var entries = new List<DeploymentDiffEntry>
        {
            Entry("Name",         current.Name,           proposed.Name,           "Core"),
            Entry("Repository",   current.Repository,     proposed.Repository,     "Core"),
            Entry("Branch",       current.Branch,         proposed.Branch,         "Core"),
            Entry("EnvironmentId",current.EnvironmentId,  proposed.EnvironmentId,  "Core"),
            Entry("BuildCommand", current.BuildCommand,   proposed.BuildCommand,   "Build"),
            Entry("StartCommand", current.StartCommand,   proposed.StartCommand,   "Build"),
            Entry("Ports",        FormatList(current.Ports),  FormatList(proposed.Ports),  "Resources"),
            Entry("HealthCheckUrl",
                  current.HealthCheckUrl ?? string.Empty,
                  proposed.HealthCheckUrl ?? string.Empty,
                  "Resources"),
            Entry("HealthCheckIntervalSeconds",
                  current.HealthCheckIntervalSeconds.ToString(),
                  proposed.HealthCheckIntervalSeconds.ToString(),
                  "Resources"),
        };

        // Environment variable diff
        var currentKeys = current.EnvironmentVariables.Keys.ToHashSet();
        var proposedKeys = proposed.EnvironmentVariables.Keys.ToHashSet();
        var allKeys = currentKeys.Union(proposedKeys).OrderBy(k => k);

        foreach (var key in allKeys)
        {
            var cur = current.EnvironmentVariables.TryGetValue(key, out var cv) ? cv : "(not set)";
            var prop = proposed.EnvironmentVariables.TryGetValue(key, out var pv) ? pv : "(not set)";
            entries.Add(Entry($"env:{key}", cur, prop, "EnvVars"));
        }

        return new DeploymentDiff
        {
            ApplicationId = current.Id,
            ApplicationName = current.Name,
            ComputedAt = DateTime.UtcNow,
            Entries = entries
        };
    }

    private static DeploymentDiffEntry Entry(string prop, string cur, string prop2, string cat) =>
        new() { Property = prop, CurrentValue = cur, ProposedValue = prop2, Category = cat };

    private static string FormatList(IEnumerable<string> items) =>
        string.Join(", ", items.OrderBy(x => x));
}
