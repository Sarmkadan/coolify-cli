#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Models;

/// <summary>
/// Represents a database instance managed by Coolify.
/// Supports multiple database engines with connection pooling and backup configuration.
/// </summary>
public class DatabaseConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DatabaseType Type { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; }
    public string RootUsername { get; set; } = "root";
    public string RootPassword { get; set; } = string.Empty;
    public string DefaultDatabase { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int MaxConnections { get; set; } = 100;
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public bool EnableBackups { get; set; } = true;
    public int BackupRetentionDays { get; set; } = 30;
    public string BackupSchedule { get; set; } = "0 2 * * *"; // 2 AM daily
    public bool IsHealthy { get; set; } = true;
    public DateTime? LastHealthCheckAt { get; set; }
    public string EnvironmentId { get; set; } = string.Empty;
    public List<string> AllowedHostPatterns { get; set; } = new();

    /// <summary>
    /// Validates database configuration for required fields and valid values.
    /// </summary>
    /// <returns>Collection of validation error messages.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Database name is required.");

        if (string.IsNullOrWhiteSpace(Host))
            errors.Add("Database host is required.");

        if (Port < 1 || Port > 65535)
            errors.Add($"Invalid port: {Port}. Must be between 1 and 65535.");

        if (string.IsNullOrWhiteSpace(RootUsername))
            errors.Add("Root username is required.");

        if (string.IsNullOrWhiteSpace(RootPassword) || RootPassword.Length < 8)
            errors.Add("Root password must be at least 8 characters long.");

        if (MaxConnections < 1 || MaxConnections > 1000)
            errors.Add("Max connections must be between 1 and 1000.");

        if (ConnectionTimeoutSeconds < 5 || ConnectionTimeoutSeconds > 300)
            errors.Add("Connection timeout must be between 5 and 300 seconds.");

        if (BackupRetentionDays < 1 || BackupRetentionDays > 365)
            errors.Add("Backup retention days must be between 1 and 365.");

        return errors;
    }

    /// <summary>
    /// Returns the default port for the specified database type.
    /// </summary>
    /// <param name="dbType">The database type.</param>
    /// <returns>Default port number for the database type.</returns>
    public static int GetDefaultPort(DatabaseType dbType) => dbType switch
    {
        DatabaseType.PostgreSQL => 5432,
        DatabaseType.MySQL => 3306,
        DatabaseType.MongoDB => 27017,
        DatabaseType.Redis => 6379,
        _ => 3306
    };

    /// <summary>
    /// Builds a connection string appropriate for the database type.
    /// </summary>
    /// <returns>Connection string ready for driver use.</returns>
    public string BuildConnectionString()
    {
        return Type switch
        {
            DatabaseType.PostgreSQL => $"Host={Host};Port={Port};Username={RootUsername};Password={RootPassword};Database={DefaultDatabase}",
            DatabaseType.MySQL => $"server={Host};port={Port};uid={RootUsername};pwd={RootPassword};database={DefaultDatabase}",
            DatabaseType.MongoDB => $"mongodb://{RootUsername}:{RootPassword}@{Host}:{Port}/{DefaultDatabase}",
            DatabaseType.Redis => $"{Host}:{Port}",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Marks the database as healthy after successful health check.
    /// </summary>
    public void MarkAsHealthy()
    {
        IsHealthy = true;
        LastHealthCheckAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the database as unhealthy after failed health check.
    /// </summary>
    public void MarkAsUnhealthy()
    {
        IsHealthy = false;
        LastHealthCheckAt = DateTime.UtcNow;
    }
}
