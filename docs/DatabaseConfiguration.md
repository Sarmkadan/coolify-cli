# DatabaseConfiguration

Represents the configuration settings for a database instance, including connection details, health status, and backup policies. This type is used to manage and validate database configurations within the coolify-cli system.

## API

### `public int Id`
Unique identifier for the database configuration. Used internally for persistence and reference.

### `public string Name`
Human-readable name of the database instance. Must be unique within the system.

### `public DatabaseType Type`
The type of database (e.g., PostgreSQL, MySQL, MongoDB). Determines connection behavior and supported features.

### `public string Version`
Version of the database software. Used for compatibility checks and feature validation.

### `public string Host`
Network address or hostname of the database server. May include IPv4, IPv6, or domain names.

### `public int Port`
Network port on which the database server listens. Must be a valid port number (1–65535).

### `public string RootUsername`
Username with administrative privileges for the database. Used for initial setup and maintenance.

### `public string RootPassword`
Password for the `RootUsername`. Stored securely; avoid logging or exposing in plaintext.

### `public string DefaultDatabase`
Name of the default database to connect to when no specific database is provided.

### `public DateTime CreatedAt`
Timestamp indicating when the configuration was created. Set automatically on creation.

### `public int MaxConnections`
Maximum number of concurrent connections allowed. Used to enforce resource limits.

### `public int ConnectionTimeoutSeconds`
Duration (in seconds) to wait for a connection before timing out. Must be non-negative.

### `public bool EnableBackups`
Flag indicating whether automated backups are enabled for this database.

### `public int BackupRetentionDays`
Number of days to retain backup snapshots. Must be non-negative; zero implies no retention.

### `public string BackupSchedule`
Cron expression defining the backup schedule (e.g., `"0 2 * * *"` for daily at 2 AM). Null or empty disables scheduling.

### `public bool IsHealthy`
Indicates whether the database is currently healthy and responsive. Updated via health checks.

### `public DateTime? LastHealthCheckAt`
Timestamp of the last health check. Null if never checked or unavailable.

### `public string EnvironmentId`
Identifier for the environment (e.g., staging, production) where the database resides. Used for scoping and access control.

### `public List<string> AllowedHostPatterns`
List of glob patterns (e.g., `"192.168.*.*"`) restricting which hosts can connect. Empty list allows all hosts.

### `public IEnumerable<string> Validate()`
Validates the configuration and returns a sequence of error messages. Returns empty if valid.

## Usage

### Example 1: Creating and Validating a Configuration
