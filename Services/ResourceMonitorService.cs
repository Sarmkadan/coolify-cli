#nullable enable
using System.Runtime.CompilerServices;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Polls the Coolify API for per-application resource usage metrics and exposes
/// both one-shot snapshots and continuous monitoring streams.
/// </summary>
public class ResourceMonitorService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public ResourceMonitorService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches a single resource-usage snapshot for the specified application.
    /// </summary>
    /// <param name="applicationId">The application to query.</param>
    /// <returns>An <see cref="ApiResponse{T}"/> containing the <see cref="ResourceUsage"/> snapshot.</returns>
    public async Task<ApiResponse<ResourceUsage>> GetResourceUsageAsync(int applicationId)
    {
        _logger.Info($"Fetching resource usage for application {applicationId}");
        var response = await _apiClient.GetAsync<ResourceUsage>(
            $"/api/v1/applications/{applicationId}/resources");

        if (response.Success && response.Data is not null)
            _logger.Info($"Resource snapshot: cpu={response.Data.CpuPercent:F1}% mem={response.Data.MemoryMb:F0}MB");

        return response;
    }

    /// <summary>
    /// Fetches resource-usage snapshots for all listed applications in parallel,
    /// returning only the successful results.
    /// </summary>
    /// <param name="applicationIds">The application IDs to query.</param>
    /// <returns>List of <see cref="ResourceUsage"/> snapshots (failed queries are silently skipped).</returns>
    public async Task<List<ResourceUsage>> GetBulkResourceUsageAsync(IEnumerable<int> applicationIds)
    {
        var ids = applicationIds.ToList();
        _logger.Info($"Fetching resource usage for {ids.Count} applications");

        var tasks = ids.Select(id => GetResourceUsageAsync(id));
        var results = await Task.WhenAll(tasks);

        return results
            .Where(r => r.Success && r.Data is not null)
            .Select(r => r.Data!)
            .ToList();
    }

    /// <summary>
    /// Continuously polls resource usage for an application at the specified interval,
    /// yielding each snapshot until the cancellation token is cancelled.
    /// </summary>
    /// <param name="applicationId">The application to monitor.</param>
    /// <param name="intervalSeconds">Polling interval in seconds (minimum 1, maximum 300).</param>
    /// <param name="cancellationToken">Token used to stop the monitor loop.</param>
    /// <returns>Async stream of <see cref="ResourceUsage"/> snapshots.</returns>
    public async IAsyncEnumerable<ResourceUsage> MonitorAsync(
        int applicationId,
        int intervalSeconds = 5,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        intervalSeconds = Math.Clamp(intervalSeconds, 1, 300);
        _logger.Info($"Starting resource monitor for application {applicationId} ({intervalSeconds}s interval)");

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await GetResourceUsageAsync(applicationId);
            if (result.Success && result.Data is not null)
                yield return result.Data;

            try
            {
                await Task.Delay(intervalSeconds * 1_000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.Info($"Resource monitor stopped for application {applicationId}");
    }

    /// <summary>
    /// Renders a resource-usage snapshot table row to the console with
    /// color-coded severity indicators.
    /// </summary>
    /// <param name="usage">The snapshot to render.</param>
    public static void RenderUsageLine(ResourceUsage usage)
    {
        var severity = usage.GetAlertSeverity();
        Console.ForegroundColor = severity switch
        {
            SeverityLevel.Critical => ConsoleColor.Red,
            SeverityLevel.Warning  => ConsoleColor.Yellow,
            _                      => ConsoleColor.Green
        };

        Console.WriteLine(usage.ToSummaryLine());
        Console.ResetColor();
    }

    /// <summary>
    /// Prints the table header for the resource-usage display.
    /// </summary>
    public static void RenderHeader()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            $"{"ID",-6} {"Name",-28} {"CPU %",7}  {"Memory",9}  {"Mem %",6}  {"Net RX",10}  {"Net TX",10}");
        Console.WriteLine(new string('-', 82));
        Console.ResetColor();
    }
}
