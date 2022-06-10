#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Infrastructure;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

/// <summary>
/// Provides the <c>resources</c> top-level command for querying and continuously
/// monitoring per-application CPU and memory consumption.
/// </summary>
public class ResourceMonitorCommands : CommandBase
{
    private readonly ApplicationService _appService;

    public ResourceMonitorCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _appService = new ApplicationService(apiClient, logger);
    }

    /// <summary>
    /// Creates the <c>resources</c> top-level command that groups sub-commands
    /// for resource-usage inspection.
    /// </summary>
    public Command CreateResourcesCommand()
    {
        var resourcesCmd = new Command("resources", "Inspect and monitor application resource usage");
        resourcesCmd.Add(CreateShowCommand());
        resourcesCmd.Add(CreateWatchCommand());
        return resourcesCmd;
    }

    /// <summary>
    /// Creates the <c>resources show</c> sub-command for a one-shot snapshot.
    /// </summary>
    private Command CreateShowCommand()
    {
        var showCmd = new Command("show", "Display a one-time resource usage snapshot");
        var appIdArg = new Argument<int?>("id")
        {
            Description = "Application ID (omit to show all applications)",
            Arity = ArgumentArity.ZeroOrOne
        };
        showCmd.Add(appIdArg);

        showCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var monitor = new ResourceMonitorService(ApiClient, Logger);

            try
            {
                if (appId.HasValue)
                {
                    ValidatePositiveId(appId.Value);
                    var result = await monitor.GetResourceUsageAsync(appId.Value);
                    if (!result.Success || result.Data is null)
                    {
                        WriteError($"Failed to fetch resource usage: {result.Message}");
                        Environment.ExitCode = 1;
                        return;
                    }

                    Console.WriteLine();
                    ResourceMonitorService.RenderHeader();
                    ResourceMonitorService.RenderUsageLine(result.Data);
                }
                else
                {
                    var appsResult = await _appService.GetAllApplicationsAsync();
                    if (!appsResult.Success || appsResult.Data is null)
                    {
                        WriteError($"Failed to list applications: {appsResult.Message}");
                        Environment.ExitCode = 1;
                        return;
                    }

                    var ids = appsResult.Data.Select(a => a.Id).ToList();
                    var usages = await monitor.GetBulkResourceUsageAsync(ids);

                    Console.WriteLine();
                    ResourceMonitorService.RenderHeader();

                    foreach (var u in usages.OrderBy(u => u.ApplicationId))
                        ResourceMonitorService.RenderUsageLine(u);

                    Console.WriteLine($"\n  {usages.Count} application(s) shown.");
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return showCmd;
    }

    /// <summary>
    /// Creates the <c>resources watch</c> sub-command for continuous monitoring.
    /// </summary>
    private Command CreateWatchCommand()
    {
        var watchCmd = new Command("watch", "Continuously monitor resource usage (press Ctrl+C to stop)");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var intervalOption = new Option<int>("--interval", ["-i"])
        {
            Description = "Poll interval in seconds",
            DefaultValueFactory = _ => 5
        };

        watchCmd.Add(appIdArg);
        watchCmd.Add(intervalOption);

        watchCmd.SetAction(async (parseResult, ct) =>
        {
            var appId    = parseResult.GetValue(appIdArg);
            var interval = parseResult.GetValue(intervalOption);

            try
            {
                ValidatePositiveId(appId);

                if (interval < 1)
                    throw new ValidationException("Interval must be at least 1 second");

                var monitor = new ResourceMonitorService(ApiClient, Logger);

                Console.WriteLine($"Watching resource usage for application {appId} every {interval}s (Ctrl+C to stop)…\n");
                ResourceMonitorService.RenderHeader();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

                await foreach (var usage in monitor.MonitorAsync(appId, interval, cts.Token))
                {
                    // Re-print header every 20 rows to keep context visible.
                    ResourceMonitorService.RenderUsageLine(usage);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nMonitoring stopped.");
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return watchCmd;
    }
}
