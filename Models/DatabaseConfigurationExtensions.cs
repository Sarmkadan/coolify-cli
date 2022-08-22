namespace CoolifyCli.Models;

/// <summary>
/// Provides extension methods for <see cref="DatabaseConfiguration"/>.
/// </summary>
public static class DatabaseConfigurationExtensions
{
    /// <summary>
    /// Determines whether a database configuration is valid for a specific database type.
    /// </summary>
    /// <param name="configuration">The database configuration to validate.</param>
    /// <param name="databaseType">The database type to check against.</param>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is <c>null</c>.</exception>
    public static bool IsValidForDatabaseType(this DatabaseConfiguration configuration, DatabaseType databaseType)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.Type == databaseType;
    }

    /// <summary>
    /// Creates a connection string based on the database configuration.
    /// </summary>
    /// <param name="configuration">The database configuration to create a connection string for.</param>
    /// <returns>A connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is <c>null</c>.</exception>
    public static string CreateConnectionString(this DatabaseConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return $"Server={configuration.Host}:{configuration.Port};Database={configuration.DefaultDatabase};Username={configuration.RootUsername};Password={configuration.RootPassword};";
    }

    /// <summary>
    /// Checks if the database configuration has a valid backup schedule.
    /// </summary>
    /// <param name="configuration">The database configuration to check.</param>
    /// <returns><c>true</c> if the backup schedule is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is <c>null</c>.</exception>
    public static bool HasValidBackupSchedule(this DatabaseConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // For simplicity, assume a valid schedule is not empty and does not exceed 32 characters
        return !string.IsNullOrEmpty(configuration.BackupSchedule) && configuration.BackupSchedule.Length <= 32;
    }
}
