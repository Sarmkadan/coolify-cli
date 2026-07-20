#nullable enable
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

/// <summary>
/// Monitoring and observability commands for real-time metrics, alerts, and system status.
/// Provides comprehensive visibility into application and infrastructure health.
/// </summary>
public class MonitoringCommands : CommandBase
{
    private readonly HealthCheckService _healthService;
    private readonly LogService _logService;

    public MonitoringCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _healthService = new HealthCheckService(apiClient, logger);
        _logService = new LogService(apiClient, logger);
    }

    /// <summary>
    /// Creates command to display real-time metrics dashboard. Streams CPU, memory, disk usage
    /// and request metrics with automatic refresh intervals.
    /// </summary>
    public Command CreateMetricsCommand()
    {
        var metricsCmd = new Command("metrics", "View system metrics");
        var resourceOption = new Option<string>("--resource", ["-r"])
        {
            Description = "Resource type: all, cpu, memory, disk, network",
            DefaultValueFactory = _ => "all"
        };
        var intervalOption = new Option<int>("--interval", ["-i"])
        {
            Description = "Refresh interval in seconds",
            DefaultValueFactory = _ => 5
        };

        metricsCmd.Add(resourceOption);
        metricsCmd.Add(intervalOption);

        metricsCmd.SetAction(async (parseResult, ct) =>
        {
            var resource = parseResult.GetValue(resourceOption);
            var interval = parseResult.GetValue(intervalOption);
            try
            {
                if (interval < 1)
                {
                    throw new ValidationException("Refresh interval must be at least 1 second");
                }

                Logger.Info($"Fetching {resource} metrics with {interval}s refresh interval");

                var result = await _healthService.GetSystemHealthAsync();

                if (result.Success && result.Data is ServiceHealth health)
                {
                    if (resource == "all" || resource == "cpu")
                    {
                        Console.WriteLine("\n--- CPU Metrics ---");
                        Console.WriteLine($"Usage: {health.CpuUsagePercent:F1}%");
                    }

                    if (resource == "all" || resource == "memory")
                    {
                        Console.WriteLine("\n--- Memory Metrics ---");
                        Console.WriteLine($"Used: {health.MemoryUsageMb:F1}MB");
                    }

                    if (resource == "all" || resource == "disk")
                    {
                        Console.WriteLine("\n--- Disk Metrics ---");
                        Console.WriteLine($"Response Time: {health.ResponseTimeMs:F1}ms");
                    }
                }
                else
                {
                    WriteError("Failed to retrieve metrics");
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return metricsCmd;
    }

    /// <summary>
    /// Creates command to stream live application logs with filtering by level, service, or text pattern.
    /// Supports both real-time streaming and batch retrieval with pagination.
    /// </summary>
    public Command CreateLogStreamCommand()
    {
        var logCmd = new Command("stream", "Stream live logs from an application");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var levelOption = new Option<string>("--level", ["-l"]) { Description = "Log level filter: all, info, warning, error, fatal" };
        var tailOption = new Option<bool>("--tail", ["-f"]) { Description = "Follow log stream (live mode)" };
        var filterOption = new Option<string>("--filter") { Description = "Text pattern to filter logs" };

        logCmd.Add(appIdArg);
        logCmd.Add(levelOption);
        logCmd.Add(tailOption);
        logCmd.Add(filterOption);

        logCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var level = parseResult.GetValue(levelOption);
            var tail = parseResult.GetValue(tailOption);
            var filter = parseResult.GetValue(filterOption);
            try
            {
                ValidatePositiveId(appId);

                Logger.Info($"Streaming logs from application {appId} (level={level}, tail={tail})");

                if (tail)
                {
                    Console.WriteLine($"Following logs from application {appId} (press Ctrl+C to stop)...\n");

                    // Simulate live streaming by polling periodically
                    var lastTimestamp = DateTime.UtcNow;
                    while (true)
                    {
                        var result = await _logService.GetApplicationLogsAsync(appId.ToString(), 50);

                        if (result.Success && result.Data is not null)
                        {
                            var filteredLogs = result.Data
                                .Where(l => l.Timestamp > lastTimestamp)
                                .OrderBy(l => l.Timestamp)
                                .ToList();

                            if (level != "all")
                            {
                                filteredLogs = filteredLogs
                                    .Where(l => l.Level.ToString().Equals(level, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                            }

                            if (!string.IsNullOrWhiteSpace(filter))
                            {
                                filteredLogs = filteredLogs
                                    .Where(l => l.Message.Contains(filter, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                            }

                            foreach (var log in filteredLogs)
                            {
                                if (!Console.IsOutputRedirected)
                                {
                                    var color = log.Level switch
                                    {
                                        LogLevel.Error => ConsoleColor.Red,
                                        LogLevel.Warning => ConsoleColor.Yellow,
                                        LogLevel.Fatal => ConsoleColor.DarkRed,
                                        _ => ConsoleColor.Gray
                                    };
                                    Console.ForegroundColor = color;
                                    Console.WriteLine($"[{log.Timestamp:HH:mm:ss}] {log.Level}: {log.Message}");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    Console.WriteLine($"[{log.Timestamp:HH:mm:ss}] {log.Level}: {log.Message}");
                                }

                                lastTimestamp = log.Timestamp;
                            }
                        }

                        await Task.Delay(2000);
                    }
                }
                else
                {
                    var result = await _logService.GetApplicationLogsAsync(appId.ToString(), 100);

                    if (result.Success && result.Data is not null)
                    {
                        IEnumerable<LogEntry> logs = result.Data.OrderBy(l => l.Timestamp);

                        if (level != "all")
                        {
                            logs = logs.Where(l => l.Level.ToString().Equals(level, StringComparison.OrdinalIgnoreCase));
                        }

                        if (!string.IsNullOrWhiteSpace(filter))
                        {
                            logs = logs.Where(l => l.Message.Contains(filter, StringComparison.OrdinalIgnoreCase));
                        }

                        foreach (var log in logs)
                        {
                            Console.WriteLine($"[{log.Timestamp:HH:mm:ss}] {log.Level}: {log.Message}");
                        }
                    }
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return logCmd;
    }

    /// <summary>
    /// Creates command to get resource usage and alerts summary for infrastructure.
    /// Highlights warning/critical conditions that require attention.
    /// </summary>
    public Command CreateAlertsCommand()
    {
        var alertsCmd = new Command("alerts", "View active alerts and issues");
        var severityOption = new Option<string>("--severity", ["-s"])
        {
            Description = "Alert severity: all, info, warning, critical",
            DefaultValueFactory = _ => "all"
        };

        alertsCmd.Add(severityOption);

        alertsCmd.SetAction(async (parseResult, ct) =>
        {
            var severity = parseResult.GetValue(severityOption);
            try
            {
                Logger.Info($"Fetching alerts with severity={severity}");

                var result = await _healthService.GetSystemHealthAsync();

                if (result.Success && result.Data is ServiceHealth health)
                {
                    var alerts = new List<(string Type, string Message, string Severity)>();

                    // CPU alerts
                    if (health.CpuUsagePercent > 90)
                        alerts.Add(("CPU", $"CPU usage critical: {health.CpuUsagePercent:F1}%", "critical"));
                    else if (health.CpuUsagePercent > 75)
                        alerts.Add(("CPU", $"CPU usage high: {health.CpuUsagePercent:F1}%", "warning"));

                    // Memory alerts
                    if (health.MemoryUsageMb > 900)
                        alerts.Add(("Memory", $"Memory usage critical: {health.MemoryUsageMb:F1}MB", "critical"));
                    else if (health.MemoryUsageMb > 768)
                        alerts.Add(("Memory", $"Memory usage high: {health.MemoryUsageMb:F1}MB", "warning"));

                    // Error rate alerts
                    if (health.ErrorRatePercent > 10)
                        alerts.Add(("ErrorRate", $"Error rate critical: {health.ErrorRatePercent:F1}%", "critical"));
                    else if (health.ErrorRatePercent > 5)
                        alerts.Add(("ErrorRate", $"Error rate high: {health.ErrorRatePercent:F1}%", "warning"));

                    if (alerts.Count == 0)
                    {
                        WriteSuccess("No active alerts");
                        return;
                    }

                    var filteredAlerts = severity == "all"
                        ? alerts
                        : alerts.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (filteredAlerts.Count == 0)
                    {
                        Console.WriteLine($"No {severity} alerts found");
                        return;
                    }

                    Console.WriteLine($"\n{filteredAlerts.Count} Active Alerts:\n");
                    foreach (var alert in filteredAlerts)
                    {
                        if (!Console.IsOutputRedirected)
                        {
                            var color = alert.Severity switch
                            {
                                "critical" => ConsoleColor.Red,
                                "warning" => ConsoleColor.Yellow,
                                _ => ConsoleColor.Cyan
                            };
                            Console.ForegroundColor = color;
                            Console.WriteLine($"[{alert.Severity.ToUpperInvariant()}] {alert.Type}: {alert.Message}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"[{alert.Severity.ToUpperInvariant()}] {alert.Type}: {alert.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError($"Failed to retrieve alerts: {ex.Message}");
            }
        });

        return alertsCmd;
    }

    /// <summary>
    /// Creates command to watch deployment status in real-time until terminal state is reached.
    /// Polls the API at specified intervals and displays status transitions with timestamps.
    /// </summary>
    public Command CreateDeployWatchCommand()
    {
        var deployCmd = new Command("watch", "Watch deployment status by deployment ID");
        var deploymentIdArg = new Argument<string>("id")
        {
            Description = "Deployment ID to monitor",
            Arity = ArgumentArity.ExactlyOne
        };
        var intervalOption = new Option<int>("--interval", ["-i"])
        {
            Description = "Polling interval in seconds",
            DefaultValueFactory = _ => 2
        };
        var timeoutOption = new Option<int>("--timeout", ["-t"])
        {
            Description = "Maximum watch duration in seconds",
            DefaultValueFactory = _ => 300
        };

        deployCmd.Add(deploymentIdArg);
        deployCmd.Add(intervalOption);
        deployCmd.Add(timeoutOption);

        deployCmd.SetAction(async (parseResult, ct) =>
        {
            var deploymentId = parseResult.GetValue(deploymentIdArg);
            var interval = parseResult.GetValue(intervalOption);
            var timeout = parseResult.GetValue(timeoutOption);

            try
            {
                if (string.IsNullOrWhiteSpace(deploymentId))
                {
                    throw new ValidationException("Deployment ID is required");
                }

                if (interval < 1)
                {
                    throw new ValidationException("Interval must be at least 1 second");
                }

                if (timeout < 1)
                {
                    throw new ValidationException("Timeout must be at least 1 second");
                }

                Logger.Info($"Starting deployment watch for deployment {deploymentId} (interval={interval}s, timeout={timeout}s)");

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
                var cancellationToken = cts.Token;

                Console.WriteLine($"\n👀 Monitoring deployment {deploymentId}...\n");
                Console.WriteLine($"[Status] [Timestamp] [Details]");
                Console.WriteLine(new string('-', 80));

                var lastStatus = DeploymentStatus.Pending;
                var startTime = DateTime.UtcNow;
                var deploymentCompleted = false;

                while (!cancellationToken.IsCancellationRequested && !deploymentCompleted)
                {
                    try
                    {
                        var service = new ApplicationService(ApiClient, Logger);
                        var response = await service.GetDeploymentStatusAsync(deploymentId);

                        if (response.Success && response.Data is not null)
                        {
                            var deployment = response.Data;
                            var currentStatus = deployment.TargetStatus;

                            if (currentStatus != lastStatus)
                            {
                                var elapsed = DateTime.UtcNow - startTime;
                                var statusSymbol = currentStatus switch
                                {
                                    DeploymentStatus.Deployed => "✅",
                                    DeploymentStatus.Failed => "❌",
                                    DeploymentStatus.Rollback => "🔄",
                                    DeploymentStatus.Maintenance => "⚠️",
                                    DeploymentStatus.Stopped => "⏹️",
                                    _ => "📊"
                                };

                                var statusColor = currentStatus switch
                                {
                                    DeploymentStatus.Deployed => "green",
                                    DeploymentStatus.Failed => "red",
                                    DeploymentStatus.Rollback => "yellow",
                                    DeploymentStatus.Maintenance => "cyan",
                                    DeploymentStatus.Stopped => "gray",
                                    _ => "white"
                                };

                                Console.WriteLine($"[{statusSymbol}] [{DateTime.UtcNow:HH:mm:ss}] Status: {currentStatus}");

                                if (!string.IsNullOrEmpty(deployment.Application.Name))
                                {
                                    Console.WriteLine($"      Application: {deployment.Application.Name}");
                                }

                                if (!string.IsNullOrEmpty(deployment.Application.Description))
                                {
                                    Console.WriteLine($"      Description: {deployment.Application.Description}");
                                }

                                if (deployment.StartedAt != default)
                                {
                                    Console.WriteLine($"      Started: {deployment.StartedAt:yyyy-MM-dd HH:mm:ss} UTC");
                                }

                                if (deployment.CompletedAt.HasValue)
                                {
                                    Console.WriteLine($"      Completed: {deployment.CompletedAt.Value:yyyy-MM-dd HH:mm:ss} UTC");
                                    Console.WriteLine($"      Duration: {deployment.GetDuration().TotalSeconds:F1}s");
                                }

					if (deployment.DeploymentLogs.Count > 0 && deployment.DeploymentLogs.Any(l => l.Level == LogLevel.Error || l.Level == LogLevel.Fatal))
					{
						var errorLogs = deployment.DeploymentLogs.Where(l => l.Level == LogLevel.Error || l.Level == LogLevel.Fatal).ToList();
						if (errorLogs.Count == 1)
						{
								Console.WriteLine($"      Error: {errorLogs[0].Message}");
						}
						else
						{
								Console.WriteLine($"      Errors: {errorLogs.Count} error(s) occurred");
						}
					}

                                Console.WriteLine();
                                lastStatus = currentStatus;

                                if (currentStatus is DeploymentStatus.Deployed or DeploymentStatus.Failed or DeploymentStatus.Rollback or DeploymentStatus.Stopped)
                                {
                                    deploymentCompleted = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            WriteError($"Failed to retrieve deployment status: {response.Message}");
                            break;
                        }
                    }
                    catch (Exception ex) when (ex is not ValidationException)
                    {
                        WriteError($"Error polling deployment status: {ex.Message}");
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
                }

                if (!deploymentCompleted)
                {
                    WriteError($"Deployment watch timed out after {timeout} seconds");
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
            catch (OperationCanceledException)
            {
                WriteError($"Deployment watch cancelled");
            }
            catch (Exception ex)
            {
                WriteError($"Unexpected error: {ex.Message}");
            }
        });

        return deployCmd;
    }
}
