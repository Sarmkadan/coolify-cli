#nullable enable

namespace CoolifyCli.Services;

using CoolifyCli.Models;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseService"/> to ensure method parameters are valid
/// before database operations are performed.
/// </summary>
public static class DatabaseServiceValidation
{
    /// <summary>
    /// Validates a database ID parameter to ensure it is a positive integer.
    /// </summary>
    /// <param name="databaseId">The database ID to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when databaseId is not positive.</exception>
    public static void ValidateDatabaseId(this int databaseId, string paramName = "databaseId")
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(databaseId, 0, paramName);
    }

    /// <summary>
    /// Validates a backup ID parameter to ensure it is not null, empty, or whitespace.
    /// </summary>
    /// <param name="backupId">The backup ID to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when backupId is null or whitespace.</exception>
    public static void ValidateBackupId(this string? backupId, string paramName = "backupId")
    {
        ArgumentException.ThrowIfNullOrEmpty(backupId, paramName);
        if (string.IsNullOrWhiteSpace(backupId))
        {
            throw new ArgumentException("Backup ID cannot be whitespace.", paramName);
        }
    }

    /// <summary>
    /// Validates that a database service instance is not null.
    /// </summary>
    /// <param name="service">The database service instance to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
    public static void ValidateDatabaseService(this DatabaseService? service, string paramName = "service")
    {
        ArgumentNullException.ThrowIfNull(service, paramName);
    }

    /// <summary>
    /// Validates that a database configuration is not null and contains valid data.
    /// </summary>
    /// <param name="database">The database configuration to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>Collection of validation error messages, or empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this DatabaseConfiguration? database, string paramName = "database")
    {
        ArgumentNullException.ThrowIfNull(database, paramName);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(database.Name))
        {
            errors.Add($"Database name is required.");
        }

        if (database.Type is not (Models.DatabaseType.PostgreSQL or Models.DatabaseType.MySQL or Models.DatabaseType.MongoDB or Models.DatabaseType.Redis or Models.DatabaseType.MariaDB or Models.DatabaseType.CouchDB))
        {
            errors.Add($"Invalid database type: {database.Type}.");
        }

        if (string.IsNullOrWhiteSpace(database.Host))
        {
            errors.Add("Database host is required.");
        }

        if (database.Port < 1 || database.Port > 65535)
        {
            errors.Add(string.Format(CultureInfo.InvariantCulture, "Invalid port: {0}. Must be between 1 and 65535.", database.Port));
        }

        if (string.IsNullOrWhiteSpace(database.RootUsername))
        {
            errors.Add("Root username is required.");
        }

        if (string.IsNullOrWhiteSpace(database.RootPassword) || database.RootPassword.Length < 8)
        {
            errors.Add("Root password must be at least 8 characters long.");
        }

        if (database.MaxConnections < 1 || database.MaxConnections > 1000)
        {
            errors.Add("Max connections must be between 1 and 1000.");
        }

        if (database.ConnectionTimeoutSeconds < 5 || database.ConnectionTimeoutSeconds > 300)
        {
            errors.Add("Connection timeout must be between 5 and 300 seconds.");
        }

        if (database.BackupRetentionDays < 1 || database.BackupRetentionDays > 365)
        {
            errors.Add("Backup retention days must be between 1 and 365.");
        }

        if (string.IsNullOrWhiteSpace(database.EnvironmentId))
        {
            errors.Add("Environment ID is required.");
        }

        if (database.CreatedAt == default)
        {
            errors.Add("CreatedAt date must be set.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a database configuration is valid.
    /// </summary>
    /// <param name="database">The database configuration to check.</param>
    /// <returns>True if the database configuration is valid; otherwise, false.</returns>
    public static bool IsValid(this DatabaseConfiguration? database)
    {
        return Validate(database).Count == 0;
    }

    /// <summary>
    /// Ensures that a database configuration is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="database">The database configuration to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the database configuration is invalid.</exception>
    public static void EnsureValid(this DatabaseConfiguration? database, string paramName = "database")
    {
        var errors = Validate(database, paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Database configuration is invalid:\n{string.Join("\n", errors)}",
                paramName);
        }
    }

    /// <summary>
    /// Validates database ID parameters for operations that require multiple IDs.
    /// </summary>
    /// <param name="databaseId">The database ID to validate.</param>
    /// <param name="backupId">The backup ID to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when databaseId is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when backupId is null or whitespace.</exception>
    public static void ValidateDatabaseAndBackupIds(this int databaseId, string backupId, string paramName = "databaseId")
    {
        databaseId.ValidateDatabaseId(paramName);
        backupId.ValidateBackupId("backupId");
    }
}