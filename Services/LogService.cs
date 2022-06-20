#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

using CoolifiCli.Models;

/// <summary>
/// Service for retrieving and managing application logs from Coolify.
/// Supports filtering, searching, and real-time log streaming.
/// </summary>
public class LogService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public LogService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves recent logs for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="lines">Number of log lines to retrieve.</param>
    /// <returns>List of log entries.</returns>
    public async Task<ApiResponse<List<LogEntry>>> GetApplicationLogsAsync(string applicationId, int lines = 100)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            return ApiResponse<List<LogEntry>>.ErrorResponse("Application ID is required.", 400);
        }

        _logger.Info($"Fetching {lines} log lines for application {applicationId}");
        var response = await _apiClient.GetAsync<List<LogEntry>>($"/api/v1/applications/{applicationId}/logs?lines={lines}");

        if (response.Success)
        {
            _logger.Info($"Retrieved {response.Data?.Count ?? 0} log entries");
        }
        else
        {
            _logger.Error($"Failed to fetch logs: {response.Message}");
        }

        return response;
    }

    /// <summary>
    /// Searches logs by message content.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="searchTerm">Search query term.</param>
    /// <param name="limit">Maximum results to return.</param>
    /// <returns>Matching log entries.</returns>
    public async Task<ApiResponse<List<LogEntry>>> SearchLogsAsync(string applicationId, string searchTerm, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<LogEntry>>.ErrorResponse("Application ID is required.", 400);

        if (string.IsNullOrWhiteSpace(searchTerm))
            return ApiResponse<List<LogEntry>>.ErrorResponse("Search term is required.", 400);

        _logger.Info($"Searching logs for application {applicationId} with term: {searchTerm}");
        var response = await _apiClient.GetAsync<List<LogEntry>>(
            $"/api/v1/applications/{applicationId}/logs/search?q={Uri.EscapeDataString(searchTerm)}&limit={limit}");

        return response;
    }

    /// <summary>
    /// Retrieves logs filtered by severity level.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="level">Log level filter.</param>
    /// <param name="limit">Maximum results.</param>
    /// <returns>Filtered log entries.</returns>
    public async Task<ApiResponse<List<LogEntry>>> GetLogsByLevelAsync(string applicationId, LogLevel level, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<LogEntry>>.ErrorResponse("Application ID is required.", 400);

        _logger.Info($"Fetching {level} logs for application {applicationId}");
        var response = await _apiClient.GetAsync<List<LogEntry>>(
            $"/api/v1/applications/{applicationId}/logs?level={level}&limit={limit}");

        return response;
    }

    /// <summary>
    /// Retrieves logs within a time range.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="startTime">Start of time range.</param>
    /// <param name="endTime">End of time range.</param>
    /// <returns>Logs within the time range.</returns>
    public async Task<ApiResponse<List<LogEntry>>> GetLogsByTimeRangeAsync(string applicationId, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<LogEntry>>.ErrorResponse("Application ID is required.", 400);

        if (startTime >= endTime)
            return ApiResponse<List<LogEntry>>.ErrorResponse("Start time must be before end time.", 400);

        _logger.Info($"Fetching logs for application {applicationId} from {startTime:O} to {endTime:O}");

        var startStr = Uri.EscapeDataString(startTime.ToString("O"));
        var endStr = Uri.EscapeDataString(endTime.ToString("O"));
        var response = await _apiClient.GetAsync<List<LogEntry>>(
            $"/api/v1/applications/{applicationId}/logs?from={startStr}&to={endStr}");

        return response;
    }

    /// <summary>
    /// Streams logs in real-time for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="cancellationToken">Cancellation token for stopping the stream.</param>
    /// <returns>Async enumerable of log entries as they arrive.</returns>
    public async IAsyncEnumerable<LogEntry> StreamLogsAsync(string applicationId, System.Threading.CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            yield break;

        _logger.Info($"Starting log stream for application {applicationId}");
        var lastTimestamp = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            var logsToYield = new List<LogEntry>();
            bool shouldBreak = false;

            try
            {
                var response = await GetApplicationLogsAsync(applicationId, 20);

                if (response.Success && response.Data is not null)
                {
                    var newLogs = response.Data
                        .Where(l => l.Timestamp > lastTimestamp)
                        .OrderBy(l => l.Timestamp)
                        .ToList();

                    logsToYield.AddRange(newLogs);
                    if (newLogs.Count > 0)
                        lastTimestamp = newLogs[^1].Timestamp;
                }

                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                shouldBreak = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error streaming logs: {ex.Message}");
                await Task.Delay(5000, cancellationToken);
            }

            foreach (var logEntry in logsToYield)
            {
                yield return logEntry;
            }

            if (shouldBreak)
                break;
        }

        _logger.Info($"Log stream ended for application {applicationId}");
    }

    /// <summary>
    /// Gets database logs.
    /// </summary>
    /// <param name="databaseId">The database ID.</param>
    /// <param name="lines">Number of lines to retrieve.</param>
    /// <returns>Database log entries.</returns>
    public async Task<ApiResponse<List<LogEntry>>> GetDatabaseLogsAsync(int databaseId, int lines = 100)
    {
        _logger.Info($"Fetching {lines} log lines for database {databaseId}");
        var response = await _apiClient.GetAsync<List<LogEntry>>($"/api/v1/databases/{databaseId}/logs?lines={lines}");

        if (!response.Success)
            _logger.Error($"Failed to fetch database logs: {response.Message}");

        return response;
    }

    /// <summary>
    /// Exports logs to a file format.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="format">Export format (json, csv, etc).</param>
    /// <returns>Export status.</returns>
    public async Task<ApiResponse<object>> ExportLogsAsync(string applicationId, string format = "json")
    {
        _logger.Info($"Exporting logs for application {applicationId} as {format}");
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/logs/export",
            new { Format = format });

        if (response.Success)
            _logger.Info($"Log export initiated");
        else
            _logger.Error($"Failed to export logs: {response.Message}");

        return response;
    }
}
