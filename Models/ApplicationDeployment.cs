#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Models;

/// <summary>
/// Represents a deployed application in the Coolify infrastructure.
/// Contains all metadata and state information for an application instance.
/// </summary>
public class ApplicationDeployment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string EnvironmentId { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastDeployedAt { get; set; }
    public int FailureCount { get; set; } = 0;
    public string? LastErrorMessage { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public List<string> Ports { get; set; } = new();
    public string BuildCommand { get; set; } = string.Empty;
    public string StartCommand { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? HealthCheckUrl { get; set; }
    public int HealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the application deployment configuration for required fields and consistency.
    /// </summary>
    /// <returns>Collection of validation error messages, empty if valid.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Application name is required.");

        if (string.IsNullOrWhiteSpace(Repository))
            errors.Add("Repository URL is required.");

        if (string.IsNullOrWhiteSpace(EnvironmentId))
            errors.Add("Environment ID is required.");

        if (string.IsNullOrWhiteSpace(BuildCommand) && string.IsNullOrWhiteSpace(StartCommand))
            errors.Add("Either build command or start command is required.");

        if (Ports.Count == 0)
            errors.Add("At least one port must be specified.");

        foreach (var port in Ports)
        {
            if (!int.TryParse(port, out var portNum) || portNum < 1 || portNum > 65535)
                errors.Add($"Invalid port number: {port}");
        }

        if (HealthCheckIntervalSeconds < 5 || HealthCheckIntervalSeconds > 3600)
            errors.Add("Health check interval must be between 5 and 3600 seconds.");

        return errors;
    }

    /// <summary>
    /// Marks the deployment as successfully deployed with current timestamp.
    /// </summary>
    public void MarkAsDeployed()
    {
        Status = DeploymentStatus.Deployed;
        LastDeployedAt = DateTime.UtcNow;
        FailureCount = 0;
        LastErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the deployment as failed with error details.
    /// </summary>
    /// <param name="errorMessage">Description of the deployment failure.</param>
    public void MarkAsFailed(string errorMessage)
    {
        Status = DeploymentStatus.Failed;
        FailureCount++;
        LastErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the deployment requires attention due to repeated failures.
    /// </summary>
    /// <returns>True if failure count exceeds threshold.</returns>
    public bool RequiresAttention() => FailureCount >= 3;
}
