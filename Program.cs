#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Commands;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;

var config = CoolifyConfiguration.FromEnvironment();
var validationErrors = config.Validate().ToList();

if (validationErrors.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Configuration Error:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
    Console.ResetColor();
    return Constants.ExitCodes.ConfigurationError;
}

// Disable color when stdout is redirected/piped, NO_COLOR env var is set, or --no-color is passed
bool colorOutput = !Console.IsOutputRedirected
    && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"))
    && !args.Contains("--no-color");

var logger = new ConsoleLogger(config.VerboseLogging, colorOutput);
var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds) };
var apiClient = new CoolifyApiClient(httpClient, config.ApiUrl, config.ApiKey!);

logger.Info($"Coolify CLI v{Constants.ApplicationVersion}");
logger.Debug($"API URL: {config.ApiUrl}");

// Root command
var rootCommand = new RootCommand("Coolify CLI - Manage Coolify infrastructure from the terminal");
rootCommand.Add(new Option<bool>("--verbose", ["-v"]) { Description = "Enable verbose logging" });
rootCommand.Add(new Option<bool>("--no-color") { Description = "Disable color output (also auto-disabled when stdout is not a TTY)" });

// Application commands
var appListCommand = new Command("list", "List all applications");
appListCommand.SetAction(async (parseResult, ct) =>
{
    try
    {
        var appService = new ApplicationService(apiClient, logger);
        var result = await appService.GetAllApplicationsAsync();

        if (result.Success && result.Data is not null)
        {
            if (result.Data.Count == 0)
            {
                Console.WriteLine("No applications found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Status",-12} {"Deployed At",-20}");
            Console.WriteLine(new string('-', 62));

            foreach (var app in result.Data)
            {
                var deployedAt = app.LastDeployedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";
                Console.WriteLine($"{app.Id,-5} {app.Name,-25} {app.Status,-12} {deployedAt,-20}");
            }
        }
        else
        {
            logger.Error($"Failed to list applications: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to list applications");
    }
});

var appGetCommand = new Command("get", "Get application details");
var appIdArg = new Argument<int>("id") { Description = "Application ID" };
appGetCommand.Add(appIdArg);
appGetCommand.SetAction(async (parseResult, ct) =>
{
    var id = parseResult.GetValue(appIdArg);
    try
    {
        var appService = new ApplicationService(apiClient, logger);
        var result = await appService.GetApplicationAsync(id);

        if (result.Success && result.Data is not null)
        {
            var app = result.Data;
            Console.WriteLine($"\nApplication: {app.Name}");
            Console.WriteLine($"ID: {app.Id}");
            Console.WriteLine($"Repository: {app.Repository}");
            Console.WriteLine($"Branch: {app.Branch}");
            Console.WriteLine($"Status: {app.Status}");
            Console.WriteLine($"Environment: {app.EnvironmentId}");
            Console.WriteLine($"Created: {app.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Last Deployed: {app.LastDeployedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
            Console.WriteLine($"Ports: {string.Join(", ", app.Ports)}");
            Console.WriteLine($"Health Check: {app.HealthCheckUrl ?? "Not configured"}");
        }
        else
        {
            logger.Error($"Failed to get application: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to get application");
    }
});

var appDeployCommand = new Command("deploy", "Deploy an application");
var deployAppIdArg = new Argument<int>("id") { Description = "Application ID to deploy" };
appDeployCommand.Add(deployAppIdArg);
appDeployCommand.SetAction(async (parseResult, ct) =>
{
    var id = parseResult.GetValue(deployAppIdArg);
    try
    {
        var appService = new ApplicationService(apiClient, logger);

        // Pre-flight: verify server is reachable before attempting deployment
        logger.Info("Checking server connectivity...");
        bool connected;
        try
        {
            connected = await apiClient.TestConnectionAsync();
        }
        catch (TaskCanceledException)
        {
            connected = false;
        }
        catch (HttpRequestException)
        {
            connected = false;
        }

        if (!connected)
        {
            logger.Error(
                $"Cannot reach Coolify server at {config.ApiUrl}. " +
                $"Verify COOLIFY_API_URL is correct and the server is running. " +
                $"Current timeout: {config.RequestTimeoutSeconds}s (override via COOLIFY_TIMEOUT).");
            Environment.ExitCode = 1;
            return;
        }

        // Get application details
        var appResult = await appService.GetApplicationAsync(id);
        if (!appResult.Success || appResult.Data is null)
        {
            logger.Error($"Failed to get application: {appResult.Message}");
            Environment.ExitCode = 1;
            return;
        }

        var context = new DeploymentContext { Application = appResult.Data };
        logger.Info($"Starting deployment of {appResult.Data.Name}");

        var deployResult = await appService.DeployApplicationAsync(id, context);
        if (deployResult.Success)
        {
            logger.Info("Deployment initiated successfully");
            Console.WriteLine($"Deployment ID: {deployResult.Data?.DeploymentId}");
        }
        else
        {
            logger.Error($"Deployment failed: {deployResult.Message}");
            Environment.ExitCode = 1;
        }
    }
    catch (TaskCanceledException)
    {
        logger.Error(
            $"Deployment timed out after {config.RequestTimeoutSeconds}s. " +
            $"The Coolify server at {config.ApiUrl} did not respond in time. " +
            $"Increase the timeout via COOLIFY_TIMEOUT (current: {config.RequestTimeoutSeconds}s).");
        Environment.ExitCode = 1;
    }
    catch (HttpRequestException ex)
    {
        logger.Error(
            $"Network error connecting to {config.ApiUrl}: {ex.Message}. " +
            $"Ensure the server is reachable and COOLIFY_API_URL is set correctly.");
        Environment.ExitCode = 1;
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Deployment error");
        Environment.ExitCode = 1;
    }
});

// Database commands
var dbListCommand = new Command("list", "List all databases");
dbListCommand.SetAction(async (parseResult, ct) =>
{
    try
    {
        var dbService = new DatabaseService(apiClient, logger);
        var result = await dbService.GetAllDatabasesAsync();

        if (result.Success && result.Data is not null)
        {
            if (result.Data.Count == 0)
            {
                Console.WriteLine("No databases found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Name",-20} {"Type",-15} {"Host",-20} {"Status",-10}");
            Console.WriteLine(new string('-', 70));

            foreach (var db in result.Data)
            {
                var status = db.IsHealthy ? "Healthy" : "Unhealthy";
                Console.WriteLine($"{db.Id,-5} {db.Name,-20} {db.Type,-15} {db.Host,-20} {status,-10}");
            }
        }
        else
        {
            logger.Error($"Failed to list databases: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to list databases");
    }
});

var dbHealthCommand = new Command("health", "Check database health");
var dbIdArg = new Argument<int>("id") { Description = "Database ID" };
dbHealthCommand.Add(dbIdArg);
dbHealthCommand.SetAction(async (parseResult, ct) =>
{
    var id = parseResult.GetValue(dbIdArg);
    try
    {
        var dbService = new DatabaseService(apiClient, logger);
        var result = await dbService.CheckDatabaseHealthAsync(id);

        if (result.Success && result.Data is not null)
        {
            var health = result.Data;
            Console.WriteLine($"\nDatabase Health Check:");
            Console.WriteLine($"Status: {health.Status}");
            Console.WriteLine($"Response Time: {health.ResponseTimeMs}ms");
            Console.WriteLine($"CPU Usage: {health.CpuUsagePercent:F2}%");
            Console.WriteLine($"Memory: {health.MemoryUsageMb:F2}MB");
            Console.WriteLine($"Active Connections: {health.ActiveConnections}");
            Console.WriteLine($"Error Rate: {health.ErrorRatePercent:F2}%");
        }
        else
        {
            logger.Error($"Health check failed: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Health check error");
    }
});

// Log commands
var logsCommand = new Command("logs", "View application logs");
var logAppIdArg = new Argument<int>("id") { Description = "Application ID" };
var linesOption = new Option<int>("--lines", ["-n"]) { Description = "Number of log lines to display", DefaultValueFactory = _ => 100 };
var watchOption = new Option<bool>("--watch", ["-f", "--follow"]) { Description = "Stream logs in real-time (follow mode). Reconnects automatically if the stream is interrupted." };
logsCommand.Add(logAppIdArg);
logsCommand.Add(linesOption);
logsCommand.Add(watchOption);
logsCommand.SetAction(async (parseResult, ct) =>
{
    var appId = parseResult.GetValue(logAppIdArg);
    var lines = parseResult.GetValue(linesOption);
    var watch = parseResult.GetValue(watchOption);
    try
    {
        var logService = new LogService(apiClient, logger);

        if (watch)
        {
            Console.WriteLine($"Streaming logs for application {appId} (press Ctrl+C to stop)...\n");
            using var cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            int reconnectDelayMs = 2000;
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await foreach (var log in logService.StreamLogsAsync(appId.ToString(), cts.Token))
                    {
                        if (colorOutput)
                        {
                            var color = log.Level switch
                            {
                                LogLevel.Error => ConsoleColor.Red,
                                LogLevel.Warning => ConsoleColor.Yellow,
                                LogLevel.Fatal => ConsoleColor.DarkRed,
                                _ => ConsoleColor.Gray
                            };
                            Console.ForegroundColor = color;
                            Console.WriteLine(log.ToString());
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine(log.ToString());
                        }
                        reconnectDelayMs = 2000; // reset backoff on successful receive
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (cts.Token.IsCancellationRequested) break;
                    logger.Warn($"Log stream interrupted ({ex.Message}). Reconnecting in {reconnectDelayMs / 1000}s...");
                    try { await Task.Delay(reconnectDelayMs, cts.Token); } catch (TaskCanceledException) { break; }
                    reconnectDelayMs = Math.Min(reconnectDelayMs * 2, 30000); // exponential backoff, max 30s
                }
            }
        }
        else
        {
            var result = await logService.GetApplicationLogsAsync(appId.ToString(), lines);

            if (result.Success && result.Data is not null)
            {
                Console.WriteLine($"\nLogs for application {appId} (showing {result.Data.Count} lines):\n");
                foreach (var log in result.Data.OrderBy(l => l.Timestamp))
                {
                    if (colorOutput)
                    {
                        var color = log.Level switch
                        {
                            LogLevel.Error => ConsoleColor.Red,
                            LogLevel.Warning => ConsoleColor.Yellow,
                            LogLevel.Fatal => ConsoleColor.DarkRed,
                            _ => ConsoleColor.Gray
                        };
                        Console.ForegroundColor = color;
                        Console.WriteLine(log.ToString());
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(log.ToString());
                    }
                }
            }
            else
            {
                logger.Error($"Failed to retrieve logs: {result.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to retrieve logs");
    }
});

// Build command hierarchy
var appCommand = new Command("app", "Manage applications") { appListCommand, appGetCommand, appDeployCommand };
var dbCommand = new Command("db", "Manage databases") { dbListCommand, dbHealthCommand };

rootCommand.Add(appCommand);
rootCommand.Add(dbCommand);
rootCommand.Add(logsCommand);

// Health command
var healthCommand = new Command("health", "Check system health");
healthCommand.SetAction(async (parseResult, ct) =>
{
    try
    {
        if (await apiClient.TestConnectionAsync())
        {
            Console.WriteLine("✓ Connected to Coolify API");
            var healthService = new HealthCheckService(apiClient, logger);
            var result = await healthService.GetSystemHealthAsync();

            if (result.Success)
            {
                Console.WriteLine("✓ System health check passed");
            }
            else
            {
                Console.WriteLine("✗ System health check failed");
            }
        }
        else
        {
            Console.WriteLine("✗ Failed to connect to Coolify API");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Health check failed");
    }
});

rootCommand.Add(healthCommand);

// Infrastructure-as-code commands (iac apply | validate | diff | export | init)
var iacCommand = InfrastructureCommands.CreateIacCommand(
    new ApplicationService(apiClient, logger),
    new DatabaseService(apiClient, logger),
    logger);
rootCommand.Add(iacCommand);

// Version command
var versionCommand = new Command("version", "Display version information");
versionCommand.SetAction((parseResult, ct) =>
{
    Console.WriteLine($"Coolify CLI v{Constants.ApplicationVersion}");
    Console.WriteLine($"Author: {Constants.Author}");
    Console.WriteLine($"Website: {Constants.AuthorUrl}");
    return Task.CompletedTask;
});

rootCommand.Add(versionCommand);

return await rootCommand.Parse(args).InvokeAsync();
