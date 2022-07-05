#nullable enable
namespace CoolifyCli.Services;

using CoolifyCli.Infrastructure;
using CoolifyCli.Models;

/// <summary>
/// Service for managing database configurations and operations.
/// Handles provisioning, backups, health checks, and connection management.
/// </summary>
public sealed class DatabaseService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public DatabaseService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all database configurations.
    /// </summary>
    /// <returns>List of databases.</returns>
    public async Task<ApiResponse<List<DatabaseConfiguration>>> GetAllDatabasesAsync()
    {
        _logger.Info("Fetching all databases");
        var response = await _apiClient.GetAsync<List<DatabaseConfiguration>>(Constants.Api.DatabasesEndpoint);
        return response;
    }

    /// <summary>
    /// Retrieves a specific database by ID.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>Database configuration.</returns>
    public async Task<ApiResponse<DatabaseConfiguration>> GetDatabaseAsync(int databaseId)
    {
        _logger.Info($"Fetching database {databaseId}");
        var response = await _apiClient.GetAsync<DatabaseConfiguration>($"{Constants.Api.DatabasesEndpoint}/{databaseId}");
        return response;
    }

    /// <summary>
    /// Creates a new database instance.
    /// </summary>
    /// <param name="database">Database configuration.</param>
    /// <returns>Created database with assigned ID.</returns>
    public async Task<ApiResponse<DatabaseConfiguration>> CreateDatabaseAsync(DatabaseConfiguration database)
    {
        var validationErrors = database.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            _logger.Error($"Database validation failed: {string.Join(", ", validationErrors)}");
            return ApiResponse<DatabaseConfiguration>.ErrorResponse(validationErrors, 400);
        }

        _logger.Info($"Creating new {database.Type} database: {database.Name}");
        var response = await _apiClient.PostAsync<DatabaseConfiguration>(Constants.Api.DatabasesEndpoint, database);

        if (response.Success)
        {
            _logger.Info($"Database created successfully with ID: {response.Data?.Id}");
        }
        else
        {
            _logger.Error($"Failed to create database: {response.Message}");
        }

        return response;
    }

    /// <summary>
    /// Updates an existing database configuration.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <param name="database">Updated configuration.</param>
    /// <returns>Updated database.</returns>
    public async Task<ApiResponse<DatabaseConfiguration>> UpdateDatabaseAsync(int databaseId, DatabaseConfiguration database)
    {
        _logger.Info($"Updating database {databaseId}");
        database.Id = databaseId;

        var validationErrors = database.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            return ApiResponse<DatabaseConfiguration>.ErrorResponse(validationErrors, 400);
        }

        var response = await _apiClient.PutAsync<DatabaseConfiguration>($"/api/v1/databases/{databaseId}", database);

        if (response.Success)
            _logger.Info($"Database {databaseId} updated successfully");

        return response;
    }

    /// <summary>
    /// Performs a health check on the database.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>Health status of the database.</returns>
    public async Task<ApiResponse<ServiceHealth>> CheckDatabaseHealthAsync(int databaseId)
    {
        if (databaseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(databaseId), "Database ID must be positive.");

        _logger.Info($"Checking health of database {databaseId}");
        var response = await _apiClient.GetAsync<ServiceHealth>($"/api/v1/databases/{databaseId}/health");

        if (response.Success && response.Data is not null)
        {
            _logger.Info($"Database {databaseId} health status: {response.Data.Status}");
        }
        else
        {
            _logger.Error($"Failed to check database health: {response.Message}");
        }

        return response;
    }

    /// <summary>
    /// Backs up a database.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>Backup job status.</returns>
    public async Task<ApiResponse<object>> BackupDatabaseAsync(int databaseId)
    {
        _logger.Info($"Initiating backup for database {databaseId}");
        var response = await _apiClient.PostAsync<object>($"/api/v1/databases/{databaseId}/backup", new { });

        if (response.Success)
            _logger.Info($"Backup initiated successfully for database {databaseId}");
        else
            _logger.Error($"Failed to initiate backup: {response.Message}");

        return response;
    }

    /// <summary>
    /// Retrieves backup history for a database.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>List of backups.</returns>
    public async Task<ApiResponse<List<object>>> GetBackupHistoryAsync(int databaseId)
    {
        _logger.Info($"Fetching backup history for database {databaseId}");
        var response = await _apiClient.GetAsync<List<object>>($"/api/v1/databases/{databaseId}/backups");
        return response;
    }

    /// <summary>
    /// Restores a database from a backup.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <param name="backupId">The backup ID to restore from.</param>
    /// <returns>Restore operation status.</returns>
    public async Task<ApiResponse<object>> RestoreDatabaseAsync(int databaseId, string backupId)
    {
        if (databaseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(databaseId), "Database ID must be positive.");

        if (string.IsNullOrWhiteSpace(backupId))
            throw new ArgumentException("Backup ID is required for restore.", nameof(backupId));

        _logger.Info($"Restoring database {databaseId} from backup {backupId}");

        var restoreRequest = new { BackupId = backupId };
        var response = await _apiClient.PostAsync<object>($"/api/v1/databases/{databaseId}/restore", restoreRequest);

        if (response.Success)
            _logger.Info($"Database restore initiated for {databaseId}");
        else
            _logger.Error($"Failed to restore database: {response.Message}");

        return response;
    }

    /// <summary>
    /// Tests the connection to a database.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>True if connection successful.</returns>
    public async Task<ApiResponse<bool>> TestConnectionAsync(int databaseId)
    {
        if (databaseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(databaseId), "Database ID must be positive.");

        _logger.Info($"Testing connection to database {databaseId}");
        var response = await _apiClient.GetAsync<bool>($"/api/v1/databases/{databaseId}/test-connection");

        if (response.Success)
            _logger.Info($"Database {databaseId} connection test passed");
        else
            _logger.Error($"Database {databaseId} connection test failed: {response.Message}");

        return response;
    }

    /// <summary>
    /// Deletes a database instance.
    /// </summary>
    /// <param name="databaseId">The database ID to delete.</param>
    /// <returns>Deletion status.</returns>
    public async Task<ApiResponse<object>> DeleteDatabaseAsync(int databaseId)
    {
        _logger.Warn($"Deleting database {databaseId}");
        var response = await _apiClient.DeleteAsync<object>($"/api/v1/databases/{databaseId}");

        if (response.Success)
            _logger.Info($"Database {databaseId} deleted successfully");
        else
            _logger.Error($"Failed to delete database: {response.Message}");

        return response;
    }

    /// <summary>
    /// Retrieves all backups for a database.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <returns>List of available backups.</returns>
    public async Task<ApiResponse<List<object>>> GetAvailableBackupsAsync(int databaseId)
    {
        _logger.Info($"Fetching available backups for database {databaseId}");
        var response = await _apiClient.GetAsync<List<object>>($"/api/v1/databases/{databaseId}/available-backups");
        return response;
    }
}
