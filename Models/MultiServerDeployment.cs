#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Models;

/// <summary>
/// Represents a Coolify server instance targeted for a multi-server deployment operation.
/// </summary>
public class ServerTarget
{
    /// <summary>Unique identifier for this server within the Coolify fleet.</summary>
    public string ServerId { get; set; } = string.Empty;

    /// <summary>Human-readable display name used in logs and summaries.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Base URL of the Coolify API running on this server.</summary>
    public string ApiUrl { get; set; } = string.Empty;

    /// <summary>API key used to authenticate with this server's Coolify instance.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Coolify application ID to deploy on this server.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Traffic routing weight for load-balanced environments (1–100). Defaults to 1.</summary>
    public int Weight { get; set; } = 1;

    /// <summary>When false the server is silently skipped during multi-server operations.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Logical environment tag used for filtering, e.g. "production" or "staging".</summary>
    public string Environment { get; set; } = "production";

    /// <summary>
    /// Validates this server target and returns any configuration errors.
    /// </summary>
    /// <returns>Collection of human-readable validation error messages.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ServerId))
            errors.Add($"ServerId is required for server '{Name}'.");

        if (string.IsNullOrWhiteSpace(ApiUrl))
            errors.Add($"ApiUrl is required for server '{Name}'.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            errors.Add($"ApiKey is required for server '{Name}'.");

        if (ApplicationId <= 0)
            errors.Add($"ApplicationId must be a positive integer for server '{Name}'.");

        if (Weight < 1 || Weight > 100)
            errors.Add($"Weight must be between 1 and 100 for server '{Name}'.");

        return errors;
    }
}

/// <summary>
/// Captures the outcome of a deployment operation targeting a single server.
/// </summary>
public class ServerDeploymentResult
{
    /// <summary>The server this result corresponds to.</summary>
    public ServerTarget Server { get; set; } = new();

    /// <summary>Whether the deployment was accepted by the server's Coolify instance.</summary>
    public bool Success { get; set; }

    /// <summary>Deployment identifier returned by Coolify, when available.</summary>
    public string? DeploymentId { get; set; }

    /// <summary>Descriptive status message from the API.</summary>
    public string? Message { get; set; }

    /// <summary>Error description when <see cref="Success"/> is false.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Timestamped event log collected during this server's deployment.</summary>
    public List<string> Events { get; set; } = new();

    /// <summary>UTC timestamp when this server's deployment started.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when this server's deployment completed or failed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Returns the elapsed time for this server's deployment.
    /// </summary>
    /// <returns>Duration from start to completion (or now if still running).</returns>
    public TimeSpan GetDuration() => (CompletedAt ?? DateTime.UtcNow) - StartedAt;

    /// <summary>
    /// Appends a timestamped entry to <see cref="Events"/>.
    /// </summary>
    /// <param name="message">Event description to record.</param>
    public void LogEvent(string message) =>
        Events.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
}

/// <summary>
/// Aggregates the outcomes of a parallel multi-server deployment run.
/// </summary>
public class MultiServerDeploymentResult
{
    /// <summary>Unique identifier for this deployment run, used for correlation in logs.</summary>
    public string DeploymentRunId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Per-server results ordered by start time.</summary>
    public List<ServerDeploymentResult> Results { get; set; } = new();

    /// <summary>UTC timestamp when the parallel run was initiated.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the last server result was collected.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Total number of servers targeted in this run.</summary>
    public int TotalServers => Results.Count;

    /// <summary>Number of servers where deployment succeeded.</summary>
    public int SucceededCount => Results.Count(r => r.Success);

    /// <summary>Number of servers where deployment failed or was skipped.</summary>
    public int FailedCount => Results.Count(r => !r.Success);

    /// <summary>True only when every targeted server deployment succeeded.</summary>
    public bool AllSucceeded => TotalServers > 0 && FailedCount == 0;

    /// <summary>True when at least one server deployment succeeded.</summary>
    public bool AnySucceeded => SucceededCount > 0;

    /// <summary>
    /// Returns the wall-clock duration of the entire multi-server operation.
    /// </summary>
    /// <returns>Total elapsed time from start to the last server completing.</returns>
    public TimeSpan GetTotalDuration() => (CompletedAt ?? DateTime.UtcNow) - StartedAt;

    /// <summary>
    /// Returns only the results from servers where deployment did not succeed.
    /// </summary>
    /// <returns>Enumerable of failed server results.</returns>
    public IEnumerable<ServerDeploymentResult> GetFailures() =>
        Results.Where(r => !r.Success);
}
