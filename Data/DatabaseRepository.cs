// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Data;

using CoolifiCli.Models;

/// <summary>
/// Repository for database configuration data access with specialized queries.
/// </summary>
public class DatabaseRepository : BaseRepository<DatabaseConfiguration>
{
    private readonly CoolifiCli.Services.CoolifyApiClient _apiClient;

    public DatabaseRepository(CoolifiCli.Services.CoolifyApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Loads all databases from the API.
    /// </summary>
    protected override async Task LoadAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync<List<DatabaseConfiguration>>("/api/v1/databases");
            if (response.Success && response.Data != null)
            {
                _all.Clear();
                _cache.Clear();

                foreach (var db in response.Data)
                {
                    _all.Add(db);
                    _cache[db.Id] = db;
                }

                _isLoaded = true;
            }
        }
        catch
        {
            _isLoaded = false;
        }
    }

    /// <summary>
    /// Gets the entity ID for caching.
    /// </summary>
    protected override int GetId(DatabaseConfiguration entity) => entity.Id;

    /// <summary>
    /// Finds databases by type.
    /// </summary>
    /// <param name="type">Database type to filter by.</param>
    /// <returns>Databases of the specified type.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> FindByTypeAsync(DatabaseType type)
    {
        var all = await GetAllAsync();
        return all.Where(d => d.Type == type);
    }

    /// <summary>
    /// Finds unhealthy databases.
    /// </summary>
    /// <returns>Databases that are not healthy.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> FindUnhealthyAsync()
    {
        var all = await GetAllAsync();
        return all.Where(d => !d.IsHealthy);
    }

    /// <summary>
    /// Finds databases by host.
    /// </summary>
    /// <param name="host">Host name or IP address.</param>
    /// <returns>Databases on the specified host.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> FindByHostAsync(string host)
    {
        var all = await GetAllAsync();
        return all.Where(d => d.Host == host);
    }

    /// <summary>
    /// Finds databases by environment.
    /// </summary>
    /// <param name="environmentId">Environment ID.</param>
    /// <returns>Databases in the specified environment.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> FindByEnvironmentAsync(string environmentId)
    {
        var all = await GetAllAsync();
        return all.Where(d => d.EnvironmentId == environmentId);
    }

    /// <summary>
    /// Gets databases that need backup rotation.
    /// </summary>
    /// <returns>Databases with old backups.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> GetBackupRotationCandidatesAsync()
    {
        var all = await GetAllAsync();
        return all.Where(d => d.EnableBackups && d.BackupRetentionDays < 7);
    }

    /// <summary>
    /// Gets databases not checked recently.
    /// </summary>
    /// <param name="withinMinutes">Minutes since last health check.</param>
    /// <returns>Databases needing health check.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> GetStaleHealthChecksAsync(int withinMinutes = 30)
    {
        var all = await GetAllAsync();
        var threshold = DateTime.UtcNow.AddMinutes(-withinMinutes);
        return all.Where(d => !d.LastHealthCheckAt.HasValue || d.LastHealthCheckAt < threshold);
    }

    /// <summary>
    /// Gets databases with high connection usage.
    /// </summary>
    /// <param name="usageThresholdPercent">Usage threshold percentage.</param>
    /// <returns>Databases with high connection usage.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> FindHighConnectionUsageAsync(double usageThresholdPercent = 80.0)
    {
        // This would require additional metrics data from the API
        var all = await GetAllAsync();
        return all.Take(0); // Placeholder - requires metrics integration
    }

    /// <summary>
    /// Gets count of databases by type.
    /// </summary>
    /// <returns>Dictionary of type counts.</returns>
    public async Task<Dictionary<DatabaseType, int>> GetTypeCountsAsync()
    {
        var all = await GetAllAsync();
        return all
            .GroupBy(d => d.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets total connections across all databases.
    /// </summary>
    /// <returns>Sum of max connections.</returns>
    public async Task<int> GetTotalMaxConnectionsAsync()
    {
        var all = await GetAllAsync();
        return all.Sum(d => d.MaxConnections);
    }

    /// <summary>
    /// Finds databases with backups enabled.
    /// </summary>
    /// <returns>Databases with backup enabled.</returns>
    public async Task<IEnumerable<DatabaseConfiguration>> GetBackupEnabledAsync()
    {
        var all = await GetAllAsync();
        return all.Where(d => d.EnableBackups);
    }
}
