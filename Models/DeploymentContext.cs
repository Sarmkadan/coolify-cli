#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Encapsulates the deployment context with application, environment, and configuration details.
/// Used for coordinating multi-step deployment operations.
/// </summary>
public class DeploymentContext
{
    /// <summary>Gets or sets the unique identifier for this deployment.</summary>
    public string DeploymentId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the application being deployed.</summary>
    public ApplicationDeployment Application { get; set; } = new();

    /// <summary>Gets or sets the environment variables applied to the deployment.</summary>
    public List<EnvironmentVariable> EnvironmentVariables { get; set; } = new();

    /// <summary>Gets or sets the databases linked to this deployment.</summary>
    public List<DatabaseConfiguration> LinkedDatabases { get; set; } = new();

    /// <summary>Gets or sets the target status the deployment should reach.</summary>
    public DeploymentStatus TargetStatus { get; set; } = DeploymentStatus.Deployed;

    /// <summary>Gets or sets the UTC timestamp when the deployment started.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the deployment completed, or null if not yet completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets the log entries recorded during the deployment.</summary>
    public List<LogEntry> DeploymentLogs { get; set; } = new();

    /// <summary>Gets or sets the artifacts produced during the deployment, keyed by identifier.</summary>
    public Dictionary<string, string> Artifacts { get; set; } = new();

    /// <summary>Gets or sets a value indicating whether the deployment requires manual approval.</summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>Gets or sets the identifier of the user who approved the deployment, or null if not approved.</summary>
    public string? ApprovedBy { get; set; }

    /// <summary>Gets or sets the version to roll back to, or null if no rollback is pending.</summary>
    public string? RollbackToVersion { get; set; }

    /// <summary>
    /// Logs an event during the deployment process.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="level">Log level.</param>
    /// <param name="source">Source of the log.</param>
    public void LogEvent(string message, LogLevel level = LogLevel.Info, string source = "Deployment")
    {
        var logEntry = new LogEntry
        {
            ApplicationId = Application.Id.ToString(),
            Message = message,
            Level = level,
            Source = source,
            TraceId = DeploymentId,
            Timestamp = DateTime.UtcNow
        };
        DeploymentLogs.Add(logEntry);
    }

    /// <summary>
    /// Marks the deployment as completed with final status.
    /// </summary>
    public void MarkAsCompleted()
    {
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the deployment duration if completed.
    /// </summary>
    /// <returns>TimeSpan representing deployment duration.</returns>
    public TimeSpan GetDuration() => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : DateTime.UtcNow - StartedAt;

    /// <summary>
    /// Adds an artifact produced during deployment.
    /// </summary>
    /// <param name="key">Artifact identifier.</param>
    /// <param name="value">Artifact path or reference.</param>
    public void AddArtifact(string key, string value)
    {
        Artifacts[key] = value;
    }

    /// <summary>
    /// Adds environment variables from the application to the context.
    /// </summary>
    public void LoadEnvironmentVariables(IEnumerable<EnvironmentVariable> vars)
    {
        EnvironmentVariables.Clear();
        EnvironmentVariables.AddRange(vars);
    }

    /// <summary>
    /// Checks if all required information is present for deployment.
    /// </summary>
    /// <returns>Collection of validation errors.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (Application is null)
            errors.Add("Application is required.");

        var appErrors = Application?.Validate() ?? new List<string>();
        errors.AddRange(appErrors);

        if (EnvironmentVariables.Count == 0)
            errors.Add("Environment variables are not loaded.");

        return errors;
    }
}
