#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Represents the deployment status of an application.
/// </summary>
public enum DeploymentStatus
{
    Pending,
    InProgress,
    Deployed,
    Failed,
    Rollback,
    Maintenance,
    Stopped
}

/// <summary>
/// Supported database management systems.
/// </summary>
public enum DatabaseType
{
    PostgreSQL,
    MySQL,
    MongoDB,
    Redis,
    MariaDB,
    CouchDB
}

/// <summary>
/// Application runtime environments.
/// </summary>
public enum RuntimeEnvironment
{
    NodeJs,
    Python,
    Java,
    DotNet,
    Go,
    Ruby,
    PHP,
    Docker
}

/// <summary>
/// Backup strategies for databases.
/// </summary>
public enum BackupStrategy
{
    Full,
    Incremental,
    Differential,
    Snapshot
}

/// <summary>
/// Severity levels for alerts and incidents.
/// </summary>
public enum SeverityLevel
{
    Info,
    Warning,
    Error,
    Critical,
    Fatal
}

/// <summary>
/// Resource scaling policies.
/// </summary>
public enum ScalingPolicy
{
    Manual,
    AutoScaleUp,
    AutoScaleDown,
    AutoScaleBoth,
    Custom
}
