#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Represents the deployment status of an application.
/// </summary>
public enum DeploymentStatus
{
    /// <summary>
    /// Deployment is pending and waiting to start.
    /// </summary>
    Pending,
    /// <summary>
    /// Deployment is currently in progress.
    /// </summary>
    InProgress,
    /// <summary>
    /// Deployment has been successfully completed.
    /// </summary>
    Deployed,
    /// <summary>
    /// Deployment has failed and requires attention.
    /// </summary>
    Failed,
    /// <summary>
    /// Deployment is being rolled back to a previous state.
    /// </summary>
    Rollback,
    /// <summary>
    /// Application is under maintenance and temporarily unavailable.
    /// </summary>
    Maintenance,
    /// <summary>
    /// Application is stopped and not running.
    /// </summary>
    Stopped
}

/// <summary>
/// Supported database management systems.
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// PostgreSQL relational database.
    /// </summary>
    PostgreSQL,
    /// <summary>
    /// MySQL relational database.
    /// </summary>
    MySQL,
    /// <summary>
    /// MongoDB NoSQL document database.
    /// </summary>
    MongoDB,
    /// <summary>
    /// Redis in-memory data structure store.
    /// </summary>
    Redis,
    /// <summary>
    /// MariaDB relational database.
    /// </summary>
    MariaDB,
    /// <summary>
    /// CouchDB NoSQL document database.
    /// </summary>
    CouchDB
}

/// <summary>
/// Application runtime environments.
/// </summary>
public enum RuntimeEnvironment
{
    /// <summary>
    /// Node.js JavaScript runtime.
    /// </summary>
    NodeJs,
    /// <summary>
    /// Python programming language runtime.
    /// </summary>
    Python,
    /// <summary>
    /// Java Virtual Machine runtime.
    /// </summary>
    Java,
    /// <summary>
    /// .NET framework runtime.
    /// </summary>
    DotNet,
    /// <summary>
    /// Go programming language runtime.
    /// </summary>
    Go,
    /// <summary>
    /// Ruby programming language runtime.
    /// </summary>
    Ruby,
    /// <summary>
    /// PHP programming language runtime.
    /// </summary>
    PHP,
    /// <summary>
    /// Docker container runtime.
    /// </summary>
    Docker
}

/// <summary>
/// Backup strategies for databases.
/// </summary>
public enum BackupStrategy
{
    /// <summary>
    /// Full backup of all data.
    /// </summary>
    Full,
    /// <summary>
    /// Incremental backup of changes since last backup.
    /// </summary>
    Incremental,
    /// <summary>
    /// Differential backup of changes since last full backup.
    /// </summary>
    Differential,
    /// <summary>
    /// Snapshot-based backup at a point in time.
    /// </summary>
    Snapshot
}

/// <summary>
/// Severity levels for alerts and incidents.
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// Informational message requiring no action.
    /// </summary>
    Info,
    /// <summary>
    /// Warning indicating potential issue.
    /// </summary>
    Warning,
    /// <summary>
    /// Error indicating a problem that needs attention.
    /// </summary>
    Error,
    /// <summary>
    /// Critical issue requiring immediate action.
    /// </summary>
    Critical,
    /// <summary>
    /// Fatal error causing system failure.
    /// </summary>
    Fatal
}

/// <summary>
/// Resource scaling policies.
/// </summary>
public enum ScalingPolicy
{
    /// <summary>
    /// Manual scaling by user intervention.
    /// </summary>
    Manual,
    /// <summary>
    /// Automatic scaling up based on demand.
    /// </summary>
    AutoScaleUp,
    /// <summary>
    /// Automatic scaling down based on demand.
    /// </summary>
    AutoScaleDown,
    /// <summary>
    /// Automatic scaling both up and down based on demand.
    /// </summary>
    AutoScaleBoth,
    /// <summary>
    /// Custom scaling policy defined by user.
    /// </summary>
    Custom
}