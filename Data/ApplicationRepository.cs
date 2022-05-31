// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Data;

using CoolifiCli.Models;

/// <summary>
/// Repository for application deployment data access with query capabilities.
/// </summary>
public class ApplicationRepository : BaseRepository<ApplicationDeployment>
{
    private readonly CoolifiCli.Services.CoolifyApiClient _apiClient;

    public ApplicationRepository(CoolifiCli.Services.CoolifyApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Loads all applications from the API.
    /// </summary>
    protected override async Task LoadAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync<List<ApplicationDeployment>>("/api/v1/applications");
            if (response.Success && response.Data != null)
            {
                _all.Clear();
                _cache.Clear();

                foreach (var app in response.Data)
                {
                    _all.Add(app);
                    _cache[app.Id] = app;
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
    protected override int GetId(ApplicationDeployment entity) => entity.Id;

    /// <summary>
    /// Finds applications by status.
    /// </summary>
    /// <param name="status">Deployment status to filter by.</param>
    /// <returns>Applications matching the status.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> FindByStatusAsync(DeploymentStatus status)
    {
        var all = await GetAllAsync();
        return all.Where(a => a.Status == status);
    }

    /// <summary>
    /// Finds applications by environment ID.
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <returns>Applications in the specified environment.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> FindByEnvironmentAsync(string environmentId)
    {
        var all = await GetAllAsync();
        return all.Where(a => a.EnvironmentId == environmentId);
    }

    /// <summary>
    /// Finds applications with health issues.
    /// </summary>
    /// <returns>Applications requiring attention.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> FindWithHealthIssuesAsync()
    {
        var all = await GetAllAsync();
        return all.Where(a => a.RequiresAttention());
    }

    /// <summary>
    /// Finds applications that have never been deployed.
    /// </summary>
    /// <returns>Applications without deployment history.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> FindUndeployedAsync()
    {
        var all = await GetAllAsync();
        return all.Where(a => a.LastDeployedAt == null);
    }

    /// <summary>
    /// Finds recently deployed applications.
    /// </summary>
    /// <param name="within">Time period to check.</param>
    /// <returns>Applications deployed within the period.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> FindRecentlyDeployedAsync(TimeSpan within)
    {
        var all = await GetAllAsync();
        var threshold = DateTime.UtcNow.Subtract(within);
        return all.Where(a => a.LastDeployedAt.HasValue && a.LastDeployedAt > threshold);
    }

    /// <summary>
    /// Gets applications with the most recent deployments.
    /// </summary>
    /// <param name="limit">Number of applications to return.</param>
    /// <returns>Most recently deployed applications.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> GetRecentDeploymentsAsync(int limit = 10)
    {
        var all = await GetAllAsync();
        return all
            .Where(a => a.LastDeployedAt.HasValue)
            .OrderByDescending(a => a.LastDeployedAt)
            .Take(limit);
    }

    /// <summary>
    /// Gets applications with failed deployments.
    /// </summary>
    /// <returns>Applications in failed state.</returns>
    public async Task<IEnumerable<ApplicationDeployment>> GetFailedDeploymentsAsync()
    {
        var all = await GetAllAsync();
        return all.Where(a => a.Status == DeploymentStatus.Failed).OrderByDescending(a => a.UpdatedAt);
    }

    /// <summary>
    /// Counts applications by status.
    /// </summary>
    /// <returns>Dictionary of status counts.</returns>
    public async Task<Dictionary<DeploymentStatus, int>> GetStatusCountsAsync()
    {
        var all = await GetAllAsync();
        return all
            .GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
