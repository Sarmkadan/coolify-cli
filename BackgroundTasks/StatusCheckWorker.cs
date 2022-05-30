#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Events;
using CoolifiCli.Services;

namespace CoolifiCli.BackgroundTasks;

/// <summary>
/// Background worker for periodic health status checks of applications and databases.
/// Runs on a configurable interval and publishes events when status changes occur.
/// Enables proactive monitoring without blocking CLI operations.
/// </summary>
public class StatusCheckWorker : IDisposable
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly HealthCheckService _healthService;
    private Timer? _timer;
    private bool _isRunning;
    private readonly TimeSpan _checkInterval;
    private Dictionary<string, string> _previousStatuses = new();

    public StatusCheckWorker(CoolifyApiClient apiClient, ILogger logger, IEventPublisher eventPublisher, TimeSpan? checkInterval = null)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _healthService = new HealthCheckService(apiClient, logger);
        _checkInterval = checkInterval ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Starts the background health check worker.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _timer = new Timer(_ => PerformHealthCheck(), null, TimeSpan.Zero, _checkInterval);
        _logger.Info($"Status check worker started (interval: {_checkInterval.TotalSeconds}s)");
    }

    /// <summary>
    /// Stops the background health check worker.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.Info("Status check worker stopped");
    }

    /// <summary>
    /// Performs a complete health check of all system components.
    /// Compares current status with previous status and publishes change events.
    /// </summary>
    private async void PerformHealthCheck()
    {
        try
        {
            _logger.Debug("Performing periodic health check");

            var result = await _healthService.GetSystemHealthAsync();

            if (result.Success && result.Data is CoolifiCli.Models.ServiceHealth health)
            {
                await CheckComponentHealth("system", health.IsHealthy() ? "healthy" : "unhealthy");
                await CheckComponentHealth("api", "healthy"); // API is up if we got a response
                await CheckComponentHealth("cpu", GetCpuStatus(health.CpuUsagePercent));
                await CheckComponentHealth("memory", GetMemoryStatus(health.MemoryUsageMb));
                await CheckComponentHealth("error-rate", GetErrorRateStatus(health.ErrorRatePercent));
            }
            else
            {
                await CheckComponentHealth("api", "critical");
                _logger.Warn("Health check failed: " + result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during periodic health check");
            await CheckComponentHealth("api", "critical");
        }
    }

    /// <summary>
    /// Checks the status of a single component and publishes change event if status changed.
    /// </summary>
    private async Task CheckComponentHealth(string componentName, string currentStatus)
    {
        var statusKey = $"component:{componentName}";

        if (!_previousStatuses.TryGetValue(statusKey, out var previousStatus))
        {
            previousStatus = "unknown";
        }

        // Only publish event if status changed
        if (previousStatus != currentStatus)
        {
            _logger.Info($"Health status changed: {componentName} {previousStatus} -> {currentStatus}");

            var healthEvent = new HealthStatusChangedEvent
            {
                ComponentName = componentName,
                PreviousStatus = previousStatus,
                NewStatus = currentStatus
            };

            await _eventPublisher.PublishAsync(healthEvent);
        }

        _previousStatuses[statusKey] = currentStatus;
    }

    /// <summary>
    /// Determines CPU health status based on usage percentage.
    /// </summary>
    private string GetCpuStatus(double cpuUsage)
    {
        if (cpuUsage > 90)
            return "critical";

        if (cpuUsage > 75)
            return "warning";

        return "healthy";
    }

    /// <summary>
    /// Determines memory health status based on usage in MB.
    /// </summary>
    private string GetMemoryStatus(double usedMb)
    {
        if (usedMb > 900)
            return "critical";

        if (usedMb > 768)
            return "warning";

        return "healthy";
    }

    /// <summary>
    /// Determines health status based on error rate percentage.
    /// </summary>
    private string GetErrorRateStatus(double errorRatePercent)
    {
        if (errorRatePercent > 10)
            return "critical";

        if (errorRatePercent > 5)
            return "warning";

        return "healthy";
    }

    /// <summary>
    /// Gets the current check interval.
    /// </summary>
    public TimeSpan CheckInterval => _checkInterval;

    /// <summary>
    /// Gets whether the worker is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Disposes the timer and resources.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _timer?.Dispose();
    }
}
