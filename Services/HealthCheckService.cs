#nullable enable
namespace CoolifyCli.Services;

using System.Runtime.CompilerServices;
using CoolifyCli.Models;

/// <summary>
/// Service for monitoring health and performance of applications and databases.
/// Performs health checks, tracks metrics, and manages alerts.
/// </summary>
public class HealthCheckService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public HealthCheckService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs a health check on an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Health status information.</returns>
    public async Task<ApiResponse<ServiceHealth>> CheckApplicationHealthAsync(int applicationId)
    {
        if (applicationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(applicationId), "Application ID must be positive.");

        _logger.Info($"Performing health check for application {applicationId}");

        try
        {
            var response = await _apiClient.GetAsync<ServiceHealth>($"/api/v1/applications/{applicationId}/health");

            if (response.Success && response.Data is not null)
            {
                _logger.Info($"Application {applicationId} health: {response.Data.Status} (Response: {response.Data.ResponseTimeMs}ms)");
            }
            else
            {
                _logger.Error($"Health check failed: {response.Message}");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception during health check: {ex.Message}");
            return ApiResponse<ServiceHealth>.ErrorResponse($"Health check error: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Gets the health status of multiple applications at once.
    /// </summary>
    /// <param name="applicationIds">List of application IDs.</param>
    /// <returns>Health statuses for all applications.</returns>
    public async Task<ApiResponse<Dictionary<int, ServiceHealth>>> CheckBulkHealthAsync(List<int> applicationIds)
    {
        if (applicationIds is null || applicationIds.Count == 0)
            throw new ArgumentException("Application IDs are required.", nameof(applicationIds));

        foreach (var id in applicationIds)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(applicationIds), "All application IDs must be positive.");
        }

        _logger.Info($"Performing bulk health check for {applicationIds.Count} applications");
        var idsStr = string.Join(",", applicationIds);
        var response = await _apiClient.GetAsync<Dictionary<int, ServiceHealth>>($"/api/v1/applications/health/bulk?ids={idsStr}");

        return response;
    }

    /// <summary>
    /// Retrieves health history for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="days">Number of days of history to retrieve.</param>
    /// <returns>Historical health data.</returns>
    public async Task<ApiResponse<List<ServiceHealth>>> GetHealthHistoryAsync(int applicationId, int days = 7)
    {
        if (applicationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(applicationId), "Application ID must be positive.");

        if (days < 1 || days > 90)
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 90.");

        _logger.Info($"Fetching {days} days of health history for application {applicationId}");
        var response = await _apiClient.GetAsync<List<ServiceHealth>>(
            $"/api/v1/applications/{applicationId}/health/history?days={days}");

        return response;
    }

    /// <summary>
    /// Retrieves metrics for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="metricType">Type of metric (cpu, memory, requests, etc).</param>
    /// <returns>Metrics data.</returns>
    public async Task<ApiResponse<object>> GetMetricsAsync(int applicationId, string metricType = "all")
    {
        if (applicationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(applicationId), "Application ID must be positive.");

        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type is required.", nameof(metricType));

        _logger.Info($"Fetching {metricType} metrics for application {applicationId}");
        var response = await _apiClient.GetAsync<object>(
            $"/api/v1/applications/{applicationId}/metrics?type={metricType}");

        return response;
    }

    /// <summary>
    /// Gets real-time metrics for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Current metrics snapshot.</returns>
    public async Task<ApiResponse<object>> GetRealtimeMetricsAsync(int applicationId)
    {
        _logger.Info($"Fetching real-time metrics for application {applicationId}");
        var response = await _apiClient.GetAsync<object>($"/api/v1/applications/{applicationId}/metrics/realtime");

        return response;
    }

    /// <summary>
    /// Retrieves alerts for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="severity">Filter by severity level (optional).</param>
    /// <returns>List of active alerts.</returns>
    public async Task<ApiResponse<List<object>>> GetApplicationAlertsAsync(int applicationId, string? severity = null)
    {
        _logger.Info($"Fetching alerts for application {applicationId}");

        var endpoint = $"/api/v1/applications/{applicationId}/alerts";
        if (!string.IsNullOrEmpty(severity))
            endpoint += $"?severity={severity}";

        var response = await _apiClient.GetAsync<List<object>>(endpoint);
        return response;
    }

    /// <summary>
    /// Acknowledges an alert.
    /// </summary>
    /// <param name="alertId">The alert ID.</param>
    /// <param name="acknowledgedBy">User acknowledging the alert.</param>
    /// <returns>Updated alert status.</returns>
    public async Task<ApiResponse<object>> AcknowledgeAlertAsync(int alertId, string acknowledgedBy)
    {
        if (alertId <= 0)
            throw new ArgumentOutOfRangeException(nameof(alertId), "Alert ID must be positive.");

        if (string.IsNullOrWhiteSpace(acknowledgedBy))
            throw new ArgumentException("Acknowledged by is required.", nameof(acknowledgedBy));

        _logger.Info($"Acknowledging alert {alertId} by {acknowledgedBy}");

        var acknowledgeRequest = new { AcknowledgedBy = acknowledgedBy, AcknowledgedAt = DateTime.UtcNow };
        var response = await _apiClient.PostAsync<object>($"/api/v1/alerts/{alertId}/acknowledge", acknowledgeRequest);

        return response;
    }

    /// <summary>
    /// Gets system-wide health summary.
    /// </summary>
    /// <returns>Overall system health status.</returns>
    public async Task<ApiResponse<object>> GetSystemHealthAsync()
    {
        _logger.Info("Fetching system health summary");
        var response = await _apiClient.GetAsync<object>("/api/v1/health/system");

        return response;
    }

    /// <summary>
    /// Monitors an application continuously and logs health changes.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="intervalSeconds">Check interval in seconds.</param>
    /// <param name="cancellationToken">Token to stop monitoring.</param>
    /// <returns>Async enumerable of health snapshots.</returns>
    public async IAsyncEnumerable<ServiceHealth> MonitorHealthAsync(int applicationId, int intervalSeconds = 30, [EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
    {
        if (intervalSeconds < 5 || intervalSeconds > 300)
            yield break;

        _logger.Info($"Starting health monitoring for application {applicationId} with {intervalSeconds}s interval");

        while (!cancellationToken.IsCancellationRequested)
        {
            var healthResponse = await CheckApplicationHealthAsync(applicationId);

            if (healthResponse.Success && healthResponse.Data is not null)
            {
                yield return healthResponse.Data;
            }

            try
            {
                await Task.Delay(intervalSeconds * 1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.Info($"Health monitoring stopped for application {applicationId}");
    }
}
