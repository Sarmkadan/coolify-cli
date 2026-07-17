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
    /// <returns>A connection string appropriate for the database type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if required connection properties are null or empty.</exception>
    public static string CreateConnectionString(this DatabaseConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.Type switch
        {
            DatabaseType.PostgreSQL => $"Host={configuration.Host};Port={configuration.Port};Username={configuration.RootUsername};Password={configuration.RootPassword};Database={configuration.DefaultDatabase}",
            DatabaseType.MySQL => $"server={configuration.Host};port={configuration.Port};uid={configuration.RootUsername};pwd={configuration.RootPassword};database={configuration.DefaultDatabase}",
            DatabaseType.MongoDB => $"mongodb://{configuration.RootUsername}:{configuration.RootPassword}@{configuration.Host}:{configuration.Port}/{configuration.DefaultDatabase}",
            DatabaseType.Redis => $"{configuration.Host}:{configuration.Port}",
            DatabaseType.MariaDB => $"Server={configuration.Host};Port={configuration.Port};Database={configuration.DefaultDatabase};User Id={configuration.RootUsername};Password={configuration.RootPassword};",
            DatabaseType.CouchDB => $"http://{configuration.RootUsername}:{configuration.RootPassword}@{configuration.Host}:{configuration.Port}",
            _ => throw new InvalidOperationException($"Unsupported database type: {configuration.Type}")
        };
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

        return !string.IsNullOrWhiteSpace(configuration.BackupSchedule) && configuration.BackupSchedule.Length <= 32;
    }
}
