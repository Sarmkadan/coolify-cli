## DateTimeExtensions

The `DateTimeExtensions` class provides a comprehensive set of utility methods for date and time manipulation, formatting, and calculations. It includes helpers for converting dates to relative time strings, formatting durations, date arithmetic, Unix timestamp conversions, and business day calculations.

Here's an example of how to use some of the utility methods:

```csharp
// Convert a DateTime to a human-readable relative time
var deploymentTime = DateTime.UtcNow.AddHours(-2);
var relativeTime = deploymentTime.ToRelativeTime(); // "2 hours ago"

// Format a TimeSpan as a readable duration
var duration = new TimeSpan(3, 45, 30);
var formattedDuration = duration.ToReadableDuration(); // "3h 45m 30s"

// Get start and end of day
var now = DateTime.UtcNow;
var startOfDay = now.StartOfDay(); // Midnight of current day
var endOfDay = now.EndOfDay(); // Just before midnight of current day

// Get start of week (Monday by default)
var monday = now.StartOfWeek(); // Start of current week (Monday)
var mondayWithCustomStart = now.StartOfWeek(DayOfWeek.Sunday); // Start of week on Sunday

// Get start and end of month
var startOfMonth = now.StartOfMonth(); // First day of current month
var endOfMonth = now.EndOfMonth(); // Last day of current month

// Check if a date is in the past, future, or today
var pastDate = DateTime.UtcNow.AddDays(-1);
pastDate.IsPast().Should().BeTrue(); // true

var futureDate = DateTime.UtcNow.AddDays(1);
futureDate.IsFuture().Should().BeTrue(); // true

var today = DateTime.UtcNow;
today.IsToday().Should().BeTrue(); // true

// Calculate business days between two dates
var businessDays = DateTime.UtcNow.StartOfWeek().BusinessDaysBetween(DateTime.UtcNow.EndOfWeek());

// Round a DateTime to the nearest minute
var preciseTime = DateTime.UtcNow.AddSeconds(30);
var roundedTime = preciseTime.RoundToMinute(); // Seconds set to 0

// Convert between DateTime and Unix timestamp
var timestamp = DateTime.UtcNow.ToUnixTimestamp(); // Seconds since epoch
var dateFromTimestamp = timestamp.FromUnixTimestamp(); // Convert back to DateTime

// Format DateTime as ISO 8601 string for API calls
var isoDate = DateTime.UtcNow.ToIso8601String(); // "2024-06-15T14:30:45.1234567Z"

// Convert milliseconds to readable duration
var msDuration = 15000L.MillisecondsToReadable(); // "15s"
```

## StringExtensions

The `StringExtensions` class provides a set of utility methods for string manipulation and validation. It includes helpers for formatting text into various naming conventions, truncating text, masking sensitive information, and validating common data formats like emails, URLs, and IP addresses.

Here's an example of how to use some of the utility methods:

```csharp
// Convert strings to various naming conventions
var pascal = "deploy_my_service".ToPascalCase(); // "DeployMyService"
var camel = "Deploy-My-Service".ToCamelCase();   // "deployMyService"
var snake = "DeployMyService".ToSnakeCase();     // "deploy_my_service"
var kebab = "DeployMyService".ToKebabCase();     // "deploy-my-service"

// Truncate text with optional ellipsis
var longText = "This is a very long text that needs to be shortened";
var truncated = longText.Truncate(15);           // "This is a very ..."

// Validate data formats
var isEmailValid = "user@example.com".IsValidEmail(); // True
var isUrlValid = "https://coolify.io".IsValidUrl();     // True
var isIpValid = "192.168.1.1".IsValidIpAddress();    // True

// Mask sensitive information
var apiKey = "sk_live_1234567890abcdef";
var maskedKey = apiKey.MaskSensitive(showChars: 4);   // "sk_l_************def"

// Split and trim strings
var items = " item1 , item2 , item3 ".SplitTrimmed(','); // ["item1", "item2", "item3"]

// Colorize strings for CLI output
var colored = "Warning!".WithColor(ConsoleColor.Yellow);

// Pad string
var padded = "ID".PadTo(10, '-'); // "ID--------"
```


## IntegrationTests

The `IntegrationTests` class provides a set of integration tests that exercise multiple components together, verifying end-to-end workflows, concurrency safety, and configuration combinations described in the project README.

Here's an example of how to use some of the tested methods:

```csharp
var tests = new IntegrationTests();

// Test deployment lifecycle
tests.DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect();

// Test validation pipeline
tests.ValidationPipeline_AllHelperMethods_WorkTogether();

// Test cache workflow
tests.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();

// Test collection and string pipeline
tests.CollectionAndStringPipeline_BatchAndFormatDeploymentNames_ProducesExpectedOutput();

// Test concurrent cache access
tests.ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions();

// Test concurrent deployment state updates
await tests.ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent();

// Test validation with invalid field combinations
tests.Validate_AllInvalidFieldCombinations_ReturnsAllExpectedErrors();

// Test validation with only start command
tests.Validate_WithOnlyStartCommand_PassesBuildCommandCheck();

// Test validation with invalid port in list
tests.Validate_WithInvalidPortInList_ReportsSpecificPort();

// Test datetime and enum pipeline
tests.DateTimeAndEnumPipeline_FormatDeploymentTimestamp_ProducesHumanReadableOutput();

## DeploymentDiffTests

The `DeploymentDiffTests` class validates the behavior of the `DeploymentDiff` class, which compares two `ApplicationDeployment` configurations and produces a detailed diff of changes. It detects property differences, flags high-risk changes (like repository URL changes), and tracks environment variable additions/removals. The tests also verify that the `DeploymentDiffEntry` class correctly identifies when values have changed.

Here's an example of how to use the deployment diff functionality:

```csharp
// Create two deployment configurations
var current = new ApplicationDeployment
{
    Id = 1,
    Name = "my-service",
    Repository = "https://github.com/org/my-service",
    Branch = "main",
    EnvironmentId = "env-prod",
    BuildCommand = "dotnet publish",
    StartCommand = "dotnet run",
    Ports = new List<string> { "8080" },
    HealthCheckIntervalSeconds = 30,
    EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "info" }
};

var proposed = new ApplicationDeployment
{
    Id = 1,
    Name = "my-service",
    Repository = "https://github.com/org/my-service",
    Branch = "release/v2",  // Changed from "main"
    EnvironmentId = "env-prod",
    BuildCommand = "dotnet publish",
    StartCommand = "dotnet run",
    Ports = new List<string> { "8080" },
    HealthCheckIntervalSeconds = 30,
    EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "info" }
};

// Compute the deployment diff
var diff = DeploymentDiff.Compute(current, proposed);

// Verify changes were detected
diff.HasChanges.Should().BeTrue();
diff.IsHighRisk.Should().BeFalse(); // Branch change is not high risk

// Check specific changes
diff.Changes.Should().ContainSingle(e => e.Property == "Branch");
var branchChange = diff.Changes.Single(e => e.Property == "Branch");
branchChange.CurrentValue.Should().Be("main");
branchChange.ProposedValue.Should().Be("release/v2");

// Check application metadata
diff.ApplicationId.Should().Be(1);
diff.ApplicationName.Should().Be("my-service");
```

## DatabaseManagementCommands

The `DatabaseManagementCommands` class provides database lifecycle management commands for backup, restore, optimization, and credential management operations. It handles critical database operations with safety validations and confirmation prompts to prevent accidental data loss.

Here's a realistic example of using the database management commands:

```csharp
// Create database management commands instance
var dbCommands = new DatabaseManagementCommands(apiClient, logger, config);

// Create and execute backup command
var backupCommand = dbCommands.CreateBackupCommand();
// backupCommand can be added to a CLI root command with arguments:
// backupCommand.AddArgument(new Argument<int>("id"));
// backupCommand.AddOption(new Option<string>("--type"));
// backupCommand.AddOption(new Option<string>("--destination"));

// Create and execute restore command
var restoreCommand = dbCommands.CreateRestoreCommand();
// restoreCommand.AddArgument(new Argument<int>("id"));
// restoreCommand.AddOption(new Option<string>("--backup"));
// restoreCommand.AddOption(new Option<bool>("--force"));

// Create and execute optimize command
var optimizeCommand = dbCommands.CreateOptimizeCommand();
// optimizeCommand.AddArgument(new Argument<int>("id"));
// optimizeCommand.AddOption(new Option<string>("--mode"));

// Create and execute credentials command
var credentialsCommand = dbCommands.CreateCredentialsCommand();
// credentialsCommand.AddArgument(new Argument<int>("id"));
// credentialsCommand.AddOption(new Option<bool>("--reset"));
```

## DeploymentDiffEntry

The `DeploymentDiffEntry` class represents a single property change between the current and proposed deployment configuration. It tracks the property name, current value, proposed value, category for display purposes, and provides a helper method to determine if the values actually differ. This model is used by the `DeploymentDiff` class to summarize all changes between two deployment configurations.

Here's a realistic example of creating and using a `DeploymentDiffEntry`:

```csharp
// Create a deployment diff entry for a changed environment variable
var envVarChange = new DeploymentDiffEntry
{
    Property = "LOG_LEVEL",
    CurrentValue = "info",
    ProposedValue = "debug",
    Category = "EnvVars"
};

// Check if the values differ
if (envVarChange.HasChange)
{
    Console.WriteLine($"Environment variable {envVarChange.Property} will change from '{envVarChange.CurrentValue}' to '{envVarChange.ProposedValue}'");
}

// Create a deployment diff entry for a core configuration change
var branchChange = new DeploymentDiffEntry
{
    Property = "Branch",
    CurrentValue = "main",
    ProposedValue = "release/v2.1",
    Category = "Core"
};

// Access the property values
Console.WriteLine($"Property: {branchChange.Property}");
Console.WriteLine($"Current: {branchChange.CurrentValue}");
Console.WriteLine($"Proposed: {branchChange.ProposedValue}");
Console.WriteLine($"Category: {branchChange.Category}");
```

## LogEntry

The `LogEntry` class represents a single log entry from an application or system component. It supports structured logging with levels (Debug, Info, Warning, Error, Fatal), timestamps, source tracking, and metadata storage. This model is used throughout the application for logging deployment events, errors, warnings, and informational messages with rich context.

Here's a realistic example of creating and using a `LogEntry`:

```csharp
// Create a log entry for a successful deployment event
var successLog = new LogEntry
{
ApplicationId = "web-storefront",
Message = "Deployment completed successfully",
Level = LogLevel.Info,
Source = "DeploymentCoordinator",
Timestamp = DateTime.UtcNow,
Metadata = new Dictionary<string, string>
{
["DurationMs"] = "12500",
["BuildId"] = "build-42",
["Environment"] = "production"
}
};

// Log an informational message
Console.WriteLine(successLog.ToString());
// Output: [2024-06-15 14:30:45] INFO [DeploymentCoordinator] Deployment completed successfully

// Create a warning log entry for high resource usage
var warningLog = new LogEntry
{
ApplicationId = "api-service",
Message = "High CPU usage detected",
Level = LogLevel.Warning,
Source = "ResourceMonitor",
Metadata = new Dictionary<string, string>
{
["CpuPercent"] = "92.5",
["MemoryMb"] = "1850",
["Threshold"] = "80"
}
};

// Log a warning
Console.WriteLine(warningLog.ToString());
// Output: [2024-06-15 14:31:12] WARN [ResourceMonitor] High CPU usage detected

// Create a log entry from an exception
try
{
// Some operation that might fail
}
catch (Exception ex)
{
var errorLog = LogEntry.FromException("web-storefront", ex, "DatabaseService");
Console.WriteLine(errorLog.ToString());
// Output: [2024-06-15 14:32:05] ERROR [DatabaseService] Connection timeout
// Metadata includes ExceptionType and InnerException

// Check if a log entry is critical
if (errorLog.IsCritical())
{
Console.WriteLine("Critical error detected!");
}

// Add additional metadata to an existing log entry
successLog.AddMetadata("Artifacts", "web-storefront-v1.2.3.tar.gz");
var artifactPath = successLog.GetMetadata("Artifacts");
// artifactPath is "web-storefront-v1.2.3.tar.gz"
```

## LogService

The `LogService` class provides comprehensive log retrieval and management capabilities for applications and databases managed by Coolify. It supports retrieving recent logs, searching by content, filtering by log level, querying by time ranges, real-time streaming, and exporting logs to various formats. This service integrates with the Coolify API to provide centralized log access and management.

Here's a realistic example of using the `LogService` to manage and retrieve logs:

```csharp
// Initialize required services
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var logService = new LogService(apiClient, logger);

// Example 1: Get recent application logs
var recentLogsResponse = await logService.GetApplicationLogsAsync("web-storefront", lines: 200);
if (recentLogsResponse.Success && recentLogsResponse.Data is not null)
{
  Console.WriteLine($"Retrieved {recentLogsResponse.Data.Count} log entries");
  foreach (var log in recentLogsResponse.Data.Take(5))
  {
    Console.WriteLine($"[{log.Timestamp:HH:mm:ss}] {log.Level} {log.Message}");
  }
}

// Example 2: Search logs for specific error messages
var searchResponse = await logService.SearchLogsAsync("api-service", "timeout", limit: 50);
if (searchResponse.Success)
{
  Console.WriteLine($"Found {searchResponse.Data?.Count ?? 0} matching log entries");
}

// Example 3: Filter logs by severity level
var errorLogsResponse = await logService.GetLogsByLevelAsync("web-storefront", LogLevel.Error);
if (errorLogsResponse.Success)
{
  Console.WriteLine($"Found {errorLogsResponse.Data?.Count ?? 0} error entries");
}

// Example 4: Get logs within a specific time range
var startTime = DateTime.UtcNow.AddHours(-1);
var endTime = DateTime.UtcNow;
var timeRangeResponse = await logService.GetLogsByTimeRangeAsync("web-storefront", startTime, endTime);
if (timeRangeResponse.Success)
{
  Console.WriteLine($"Logs between {startTime:HH:mm:ss} and {endTime:HH:mm:ss}");
}

// Example 5: Stream logs in real-time with cancellation
using var cts = new CancellationTokenSource();

// Start streaming logs in background
var streamingTask = Task.Run(async () =>
{
  await foreach (var logEntry in logService.StreamLogsAsync("web-storefront", cts.Token))
  {
    Console.WriteLine($"[{logEntry.Timestamp:HH:mm:ss}] {logEntry.Level} {logEntry.Message}");
  }
});

// Run for 30 seconds then stop
await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
cts.Cancel();

try
{
  await streamingTask;
}
catch (OperationCanceledException)
{
  Console.WriteLine("Log streaming stopped");
}

// Example 6: Get database logs
var dbLogsResponse = await logService.GetDatabaseLogsAsync(42, lines: 100);
if (dbLogsResponse.Success)
{
  Console.WriteLine($"Retrieved {dbLogsResponse.Data?.Count ?? 0} database log entries");
}

// Example 7: Export logs to JSON format
var exportResponse = await logService.ExportLogsAsync("web-storefront", "json");
if (exportResponse.Success)
{
  Console.WriteLine("Log export initiated successfully");
}
```

## DeploymentTests

The `DeploymentTests` class provides unit tests that verify the behavior of the `ApplicationDeployment` class, focusing on validation, deployment state management, failure tracking, and caching functionality. These tests ensure that deployment configurations are properly validated, state transitions work correctly, failure states are tracked accurately, and cached deployments are retrieved and updated as expected.

Here's an example of how to use the deployment tests to verify common scenarios:

```csharp
// Create a deployment configuration
var deployment = new ApplicationDeployment
{
Name = "my-service",
Repository = "https://github.com/user/repo",
EnvironmentId = "env-prod",
BuildCommand = "npm run build",
Ports = new List<string> { "3000" }
};

// Test 1: Validate that a complete deployment configuration passes validation
var errors = deployment.Validate().ToList();
errors.Should().BeEmpty(); // No validation errors for complete configuration

// Test 2: Mark a deployment as deployed after previous failures
// This resets the failure state and sets the deployment timestamp
deployment.MarkAsFailed("build timeout");
deployment.MarkAsFailed("health check failed");
deployment.MarkAsDeployed();

// Verify state was reset
deployment.Status.Should().Be(DeploymentStatus.Deployed);
deployment.FailureCount.Should().Be(0);
deployment.LastErrorMessage.Should().BeNull();
deployment.LastDeployedAt.Should().NotBeNull();

// Test 3: Track failure accumulation
// Each failure increases the failure count and updates the error message
deployment.MarkAsFailed("timeout on step 1");
deployment.MarkAsFailed("timeout on step 2");

// Verify failure tracking
deployment.FailureCount.Should().Be(2);
deployment.LastErrorMessage.Should().Be("timeout on step 2");
deployment.Status.Should().Be(DeploymentStatus.Failed);

// Test 4: Check when attention is required
// When failure count reaches a threshold, the deployment requires attention
deployment.MarkAsFailed("error");
deployment.RequiresAttention().Should().BeTrue();

// Test 5: Verify cache provider behavior
// When a deployment is not in cache, the factory method is called to create it
var mockCache = new Mock<ICacheProvider>();
mockCache
.Setup(c => c.GetOrAdd<ApplicationDeployment>(
"deployment:42",
It.IsAny<Func<ApplicationDeployment>>(),
It.IsAny<TimeSpan?>()))
.Returns<string, Func<ApplicationDeployment>, TimeSpan?>((_, factory, __) => factory());

var cachedDeployment = mockCache.Object.GetOrAdd<ApplicationDeployment>(
"deployment:42",
() => new ApplicationDeployment { Id = 42, Name = "cached-service" });

// Verify the cached deployment
cachedDeployment.Id.Should().Be(42);
cachedDeployment.Name.Should().Be("cached-service");
```

## TuiState

The `TuiState` class manages the state of a terminal user interface for navigating and selecting applications and databases. It tracks the active view, selected items, scroll position, status messages, and provides navigation methods for interactive terminal applications. The state also maintains timestamps for refresh operations and can signal when the application should exit.

Here's a realistic example of creating and using a `TuiState` for a terminal UI application:

```csharp
// Create a TUI state with applications and databases
var state = new TuiState
{
    ActiveView = TuiView.Applications,
    Applications = new List<ApplicationDeployment>
    {
        new() { Id = 1, Name = "web-storefront", Description = "Production web application" },
        new() { Id = 2, Name = "api-gateway", Description = "API gateway service" },
        new() { Id = 3, Name = "worker-queue", Description = "Background worker service" },
        new() { Id = 4, Name = "cache-service", Description = "Redis cache layer" },
        new() { Id = 5, Name = "auth-service", Description = "Authentication service" }
    },
    Databases = new List<DatabaseConfiguration>
    {
        new() { Id = 1, Name = "production-postgres", Type = DatabaseType.PostgreSQL },
        new() { Id = 2, Name = "redis-cache", Type = DatabaseType.Redis },
        new() { Id = 3, Name = "analytics-mongo", Type = DatabaseType.MongoDB }
    },
    SelectedIndex = 0,
    ScrollOffset = 0,
    StatusMessage = "Ready",
    IsRefreshing = false,
    LastRefreshedAt = DateTime.UtcNow,
    ShouldExit = false
};

// Navigate down through the applications list
state.MoveDown(state.Applications.Count);
Console.WriteLine($"Selected index: {state.SelectedIndex}"); // Output: Selected index: 1

state.MoveDown(state.Applications.Count);
Console.WriteLine($"Selected index: {state.SelectedIndex}"); // Output: Selected index: 2

// Navigate up
state.MoveUp();
Console.WriteLine($"Selected index: {state.SelectedIndex}"); // Output: Selected index: 1

// Get the currently selected application
var selectedApp = state.GetSelectedApp();
Console.WriteLine($"Selected app: {selectedApp?.Name}"); // Output: Selected app: api-gateway

// Reset selection to initial state
state.ResetSelection();
Console.WriteLine($"Reset - Selected index: {state.SelectedIndex}, Scroll offset: {state.ScrollOffset}");
// Output: Reset - Selected index: 0, Scroll offset: 0

// Get visible apps for rendering (with a window size of 3)
var visibleApps = state.GetVisibleApps(3);
Console.WriteLine($"Visible apps count: {visibleApps.Count}");
foreach (var app in visibleApps)
{
    Console.WriteLine($"  - {app.Name}");
}
/* Output:
  - web-storefront
  - api-gateway
  - worker-queue
*/

// Set a selected app ID
state.SelectedAppId = 3;
Console.WriteLine($"Selected app ID: {state.SelectedAppId}"); // Output: Selected app ID: 3

// Update status message
state.StatusMessage = "Loading applications...";
state.IsRefreshing = true;
Console.WriteLine($"Status: {state.StatusMessage}, IsRefreshing: {state.IsRefreshing}");
// Output: Status: Loading applications..., IsRefreshing: True

// Mark as refreshed
state.IsRefreshing = false;
state.LastRefreshedAt = DateTime.UtcNow;
Console.WriteLine($"Refreshed at: {state.LastRefreshedAt}");

// Signal application to exit
state.ShouldExit = true;
Console.WriteLine($"Should exit: {state.ShouldExit}"); // Output: Should exit: True
```

## MemoryCacheProviderTests

The `MemoryCacheProviderTests` class provides unit tests for the `MemoryCacheProvider` class, which handles in-memory caching with support for expiration, atomic operations, and factory-based value creation. These tests verify core CRUD operations, expiration handling, concurrency, and cache management to ensure that cached data is reliably stored, retrieved, and cleaned up as expected.

Here's an example of how to use the tested methods to manage cached data:

```csharp
// Initialize the memory cache provider
// Use a reasonable cleanup interval for the test
using var cache = new MemoryCacheProvider(cleanupInterval: TimeSpan.FromMinutes(1));

// Store a value
cache.Set("key1", "value1");

// Verify existence and retrieve
if (cache.Exists("key1"))
{
    var value = cache.Get<string>("key1");
    // value is "value1"
}

// Try to retrieve a value safely
if (cache.TryGet("key1", out string? retrievedValue))
{
    // retrievedValue is "value1"
}

// Remove an entry
cache.Remove("key1");

// Clear all entries
cache.Clear();

// Efficiently get or add a value (invokes factory if key is missing)
var result = cache.GetOrAdd("key2", () => "computedValue");
// result is "computedValue"

// Verify count
var count = cache.Count;
// count is 1
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides unit tests for the DateTime extension methods in the `CoolifyCli.Extensions` namespace. It tests relative time formatting, duration formatting, date manipulation, and business day calculations to ensure these utility methods work correctly across different time ranges and scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Format a recent timestamp as relative time
var justNow = DateTime.UtcNow.AddMilliseconds(-500).ToRelativeTime();
justNow.Should().Be("just now");

// Format a timestamp from 30 seconds ago
var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30).ToRelativeTime();
thirtySecondsAgo.Should().Be("30 seconds ago");

// Format a timestamp from 3 hours ago
var threeHoursAgo = DateTime.UtcNow.AddHours(-3).ToRelativeTime();
threeHoursAgo.Should().Be("3 hours ago");

// Format a duration with hours, minutes, and seconds
var duration = new TimeSpan(2, 30, 15).ToReadableDuration();
duration.Should().Be("2h 30m 15s");

// Get the start of the current day
dateTime.StartOfDay().Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));

// Get the end of the current day
dateTime.EndOfDay().Should().Be(new DateTime(2024, 6, 15, 23, 59, 59, 999));

// Get the start of the current week (Monday)
var monday = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday);

// Get the start of the current month
var monthStart = DateTime.UtcNow.StartOfMonth();

// Get the end of the current month
var monthEnd = DateTime.UtcNow.EndOfMonth();

// Calculate business days between two dates
var businessDays = DateTime.UtcNow.StartOfWeek().BusinessDaysBetween(DateTime.UtcNow.EndOfWeek());
```

## StringExtensionsTests

The `StringExtensionsTests` class provides unit tests for string extension methods in the `CoolifyCli.Extensions` namespace. It tests text transformation utilities including Pascal case conversion, string truncation with ellipsis, sensitive data masking, and whitespace handling to ensure these common string operations work correctly across different input scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Convert a hyphen-delimited string to PascalCase
var deploymentName = "deploy-my-service".ToPascalCase();
deploymentName.Should().Be("DeployMyService");

// Truncate a long string with ellipsis when exceeding max length
var longText = "This is a very long deployment description that needs to be shortened";
var truncated = longText.Truncate(20);
truncated.Should().Be("This is a very lon...");

// Truncate preserves the original when within max length
var shortText = "Hi";
var preserved = shortText.Truncate(10);
preserved.Should().Be("Hi");

// Mask sensitive data, showing only edge characters
var apiKey = "sk_live_1234567890abcdef";
var maskedKey = apiKey.MaskSensitive(showChars: 4);
maskedKey.Should().Be("sk_l_************def");

// Split and trim whitespace-padded strings
var services = " api , web , worker ".SplitTrimmed(',');
services.Should().Equal("api", "web", "worker");
```

## DeploymentContext

The `DeploymentContext` class encapsulates the deployment context with application, environment, and configuration details. It serves as a coordination container for multi-step deployment operations, tracking the entire deployment lifecycle from initialization through completion. The context maintains references to the application being deployed, environment variables, linked databases, deployment status, logs, artifacts, and approval workflows.

Here's a realistic example of creating and using a `DeploymentContext` for a production deployment:

```csharp
// Create a deployment context for a production web service
var context = new DeploymentContext
{
    DeploymentId = Guid.NewGuid().ToString(),
    Application = new ApplicationDeployment
    {
        Id = 42,
        Name = "web-storefront",
        Description = "Production e-commerce storefront application",
        Repository = "https://github.com/myorg/web-storefront.git",
        Branch = "release/v2.1",
        EnvironmentId = "env-prod-01",
        Status = DeploymentStatus.Deployed,
        BuildCommand = "npm run build",
        StartCommand = "npm start",
        Ports = new List<string> { "3000", "8080" },
        EnvironmentVariables = new Dictionary<string, string>
        {
            ["NODE_ENV"] = "production",
            ["DATABASE_URL"] = "postgresql://prod-db:5432/storefront",
            ["REDIS_URL"] = "redis://cache-service:6379",
            ["API_BASE_URL"] = "https://api.myorg.com/v1"
        },
        HealthCheckUrl = "/health",
        HealthCheckIntervalSeconds = 30,
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow,
        LastDeployedAt = DateTime.UtcNow.AddHours(-2)
    },
    TargetStatus = DeploymentStatus.Deployed,
    StartedAt = DateTime.UtcNow,
    EnvironmentVariables = new List<EnvironmentVariable>(),
    LinkedDatabases = new List<DatabaseConfiguration>(),
    DeploymentLogs = new List<LogEntry>(),
    Artifacts = new Dictionary<string, string>(),
    RequiresApproval = false
};

// Load environment variables from configuration
var envVars = new List<EnvironmentVariable>
{
    new EnvironmentVariable
    {
        ApplicationId = "web-storefront",
        Key = "DATABASE_URL",
        Value = "postgresql://prod-db:5432/storefront",
        IsSecret = true,
        EnvironmentScope = "production"
    },
    new EnvironmentVariable
    {
        ApplicationId = "web-storefront",
        Key = "REDIS_URL",
        Value = "redis://cache-service:6379",
        IsSecret = true,
        EnvironmentScope = "production"
    }
};
context.LoadEnvironmentVariables(envVars);

// Log deployment events
context.LogEvent("Deployment initialized", LogLevel.Info, "DeploymentCoordinator");
context.LogEvent("Environment variables loaded", LogLevel.Debug, "ConfigLoader");

// Validate deployment context
var validationErrors = context.Validate().ToList();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Deployment context is valid!");
}

// Track deployment progress
context.LogEvent("Starting build process", LogLevel.Info);

// Simulate deployment completion
context.MarkAsCompleted();
context.LogEvent("Deployment completed successfully", LogLevel.Info);

Console.WriteLine($"Deployment duration: {context.GetDuration().ToReadableDuration()}");
Console.WriteLine($"Total log entries: {context.DeploymentLogs.Count}");
Console.WriteLine($"Artifacts produced: {context.Artifacts.Count}");
```

## ApplicationDeployment

The `ApplicationDeployment` class represents a deployed application instance in the Coolify infrastructure. It encapsulates all configuration and runtime state for an application, including repository information, build and start commands, environment variables, port mappings, health checks, and deployment status tracking. This model is used throughout the application for deployment management, validation, and state transitions.

Here's a realistic example of creating and using an `ApplicationDeployment`:

```csharp
// Create a production web service deployment
var deployment = new ApplicationDeployment
{
    Id = 42,
    Name = "web-storefront",
    Description = "Production e-commerce storefront application",
    Repository = "https://github.com/myorg/web-storefront.git",
    Branch = "release/v2.1",
    EnvironmentId = "env-prod-01",
    Status = DeploymentStatus.Deployed,
    BuildCommand = "npm run build",
    StartCommand = "npm start",
    Ports = new List<string> { "3000", "8080" },
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["NODE_ENV"] = "production",
        ["DATABASE_URL"] = "postgresql://prod-db:5432/storefront",
        ["REDIS_URL"] = "redis://cache-service:6379",
        ["API_BASE_URL"] = "https://api.myorg.com/v1"
    },
    HealthCheckUrl = "/health",
    HealthCheckIntervalSeconds = 30,
    IsActive = true,
    CreatedAt = DateTime.UtcNow.AddDays(-1),
    UpdatedAt = DateTime.UtcNow,
    LastDeployedAt = DateTime.UtcNow.AddHours(-2)
};

// Validate the deployment configuration
var validationErrors = deployment.Validate().ToList();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Deployment configuration is valid!");
}

// Track deployment state transitions
if (deployment.Status == DeploymentStatus.Failed && deployment.RequiresAttention())
{
    Console.WriteLine($"Deployment {deployment.Name} requires attention!");
    Console.WriteLine($"Failure count: {deployment.FailureCount}");
    Console.WriteLine($"Last error: {deployment.LastErrorMessage}");
}

// Update deployment after successful deployment
if (deployment.Status == DeploymentStatus.Deploying)
{
    deployment.MarkAsDeployed();
    Console.WriteLine($"Deployment successful at {deployment.LastDeployedAt}");
}
```

## DatabaseConfiguration

The `DatabaseConfiguration` class represents a database instance managed by Coolify. It supports multiple database engines (PostgreSQL, MySQL, MongoDB, Redis) with connection pooling, health monitoring, and backup configuration. This model is used for database provisioning, connection management, and infrastructure-as-code templates.

Here's a realistic example of creating and using a `DatabaseConfiguration` for a production PostgreSQL database:

```csharp
// Create a production PostgreSQL database configuration
var postgresConfig = new DatabaseConfiguration
{
  Id = 1,
  Name = "production-postgres",
  Type = DatabaseType.PostgreSQL,
  Version = "15",
  Host = "prod-db.internal",
  Port = 5432,
  RootUsername = "admin",
  RootPassword = "SecurePassword123!",
  DefaultDatabase = "web-storefront",
  CreatedAt = DateTime.UtcNow,
  MaxConnections = 200,
  ConnectionTimeoutSeconds = 30,
  EnableBackups = true,
  BackupRetentionDays = 30,
  BackupSchedule = "0 2 * * *",
  IsHealthy = true,
  LastHealthCheckAt = DateTime.UtcNow.AddMinutes(-5),
  EnvironmentId = "env-prod-01",
  AllowedHostPatterns = new List<string> { "10.0.0.0/8", "192.168.0.0/16" }
};

// Validate the database configuration
var validationErrors = postgresConfig.Validate().ToList();
if (validationErrors.Count > 0)
{
  Console.WriteLine("Database configuration validation failed:");
  foreach (var error in validationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}
else
{
  Console.WriteLine("Database configuration is valid!");
}

// Build a connection string for application use
var connectionString = postgresConfig.BuildConnectionString();
Console.WriteLine($"Connection string: {connectionString}");

// Mark database as healthy after successful health check
postgresConfig.MarkAsHealthy();
Console.WriteLine($"Database health status: {(postgresConfig.IsHealthy ? "Healthy" : "Unhealthy")}");
Console.WriteLine($"Last health check: {postgresConfig.LastHealthCheckAt}");

// Get default port for a specific database type
var defaultPort = DatabaseConfiguration.GetDefaultPort(DatabaseType.PostgreSQL);
Console.WriteLine($"Default PostgreSQL port: {defaultPort}");
```

## InfrastructureTemplateEngine

The `InfrastructureTemplateEngine` class provides infrastructure-as-code capabilities for managing Coolify resources declaratively. It reads YAML templates from disk, validates their structure, computes a live-state diff via the Coolify API, and reconciles resources (applications and databases) to match the desired state defined in the template. The engine supports dry-run mode for previewing changes, fail-fast execution, and comprehensive logging for audit trails.

Here's a realistic example of using the `InfrastructureTemplateEngine` to manage infrastructure:

```csharp
// Initialize required services
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var appService = new ApplicationService(apiClient, logger, config);
var dbService = new DatabaseService(apiClient, logger, config);

// Create the template engine
var templateEngine = new InfrastructureTemplateEngine(appService, dbService, logger);

// Load a template from disk (YAML file)
var loadResult = await templateEngine.LoadTemplateAsync("production-stack.yaml");
if (!loadResult.Success)
{
    Console.WriteLine($"Failed to load template: {loadResult.Message}");
    return;
}

var template = loadResult.Data!;

// Validate the template structure
var validationResult = await templateEngine.ValidateTemplateAsync(template);
if (!validationResult.Success)
{
    Console.WriteLine("Template validation failed:");
    foreach (var error in validationResult.Data!.Errors)
    {
        Console.WriteLine($"- {error}");
    }
    return;
}

// Compute the diff between template and live environment
var diffResult = await templateEngine.ComputeDiffAsync(template);
if (!diffResult.Success)
{
    Console.WriteLine($"Failed to compute diff: {diffResult.Message}");
    return;
}

var diff = diffResult.Data!;

Console.WriteLine($"Template diff summary:");
Console.WriteLine($"  Added: {diff.Added.Count}");
Console.WriteLine($"  Modified: {diff.Modified.Count}");
Console.WriteLine($"  Removed: {diff.Removed.Count}");
Console.WriteLine($"  Unchanged: {diff.Unchanged.Count}");

// Apply the template to reconcile the live environment
var applyOptions = new IacTemplateOptions
{
    DryRun = false,      // Set to true to preview changes without applying
    FailFast = true,       // Stop on first failure
    SkipValidation = false // Validate before applying
};

var applyResult = await templateEngine.ApplyTemplateAsync(template, applyOptions);
if (!applyResult.Success)
{
    Console.WriteLine($"Failed to apply template: {applyResult.Message}");
    return;
}

var result = applyResult.Data!;
Console.WriteLine($"Apply completed in {result.Duration.TotalSeconds:F1}s");
Console.WriteLine($"Succeeded: {result.SucceededCount}, Failed: {result.FailedCount}");

// Export the current live state as a template
var exportResult = await templateEngine.ExportCurrentStateAsync();
if (exportResult.Success)
{
    var exportedTemplate = exportResult.Data!;
    
    // Serialize to YAML for saving to disk
    var yaml = InfrastructureTemplateEngine.SerializeToYaml(exportedTemplate);
    await File.WriteAllTextAsync("exported-infrastructure.yaml", yaml);
    
    Console.WriteLine("Current state exported successfully!");
}
```

## ApiResponse

The `ApiResponse<T>` class is a generic wrapper for standardized API responses from Coolify. It provides consistent error handling, data serialization, and status tracking across all endpoints. The response includes success status, data payload, error messages, HTTP status codes, pagination metadata, and timestamps.

Here's an example of how to use the `ApiResponse<T>` class:

```csharp
// Create a successful API response with data
var deployment = new ApplicationDeployment
{
    Id = 1,
    Name = "web-app",
    Repository = "https://github.com/org/web-app",
    Branch = "main"
};

var successResponse = ApiResponse<ApplicationDeployment>.SuccessResponse(
    deployment,
    "Deployment configuration retrieved successfully"
);

Console.WriteLine($"Success: {successResponse.Success}");
Console.WriteLine($"Status: {successResponse.StatusCode}");
Console.WriteLine($"Message: {successResponse.Message}");
Console.WriteLine($"Data: {successResponse.Data?.Name}");

// Create a failed API response with error details
var errorResponse = ApiResponse<string>.ErrorResponse(
    new List<string> { "Repository URL is invalid", "Branch not found" },
    400
);

Console.WriteLine($"Success: {errorResponse.Success}");
Console.WriteLine($"Status: {errorResponse.StatusCode}");
Console.WriteLine($"Errors: {string.Join(", ", errorResponse.Errors)}");

// Add an error to an existing response
var response = ApiResponse<ApplicationDeployment>.SuccessResponse(deployment);
response.AddError("Validation failed: missing build command");
Console.WriteLine($"Has errors: {response.HasErrors()}");
Console.WriteLine($"First error: {response.GetFirstError()}");

// Check response status
if (response.Success)
{
    Console.WriteLine("Operation completed successfully");
}
else
{
    Console.WriteLine($"Failed: {response.GetFirstError()}");
}
```

## InfrastructureTemplate

The `InfrastructureTemplate` record represents a declarative infrastructure-as-code YAML template that describes the desired state of applications and databases to reconcile with Coolify. It serves as the root document model for defining infrastructure stacks with metadata, applications, and databases in a structured format.

Here's an example of how to create and use an `InfrastructureTemplate`:

```csharp
// Create a production web application infrastructure template
var template = new InfrastructureTemplate
{
    ApiVersion = "v2",
    Kind = "CoolifyInfrastructure",
    Metadata = new IacTemplateMetadata
    {
        Name = "production-web-stack",
        Description = "Production web application with database and cache",
        Environment = "production",
        Version = "1.0.0",
        Labels = new Dictionary<string, string>
        {
            { "team", "platform" },
            { "cost-center", "engineering" }
        }
    },
    Applications = new List<IacTemplateApplication>
    {
        new IacTemplateApplication
        {
            Name = "web-app",
            Repository = "https://github.com/myorg/web-app.git",
            Branch = "main",
            Runtime = RuntimeEnvironment.Docker,
            EnvironmentId = "env-prod-001",
            BuildCommand = "npm run build",
            StartCommand = "npm start",
            Ports = new List<int> { 3000, 8080 },
            HealthCheck = new IacHealthCheckSpec
            {
                Url = "/health",
                IntervalSeconds = 30,
                FailureThreshold = 3
            },
            Environment = new Dictionary<string, string>
            {
                { "NODE_ENV", "production" },
                { "DATABASE_URL", "${{db-connection-string}}" }
            },
            Resources = new IacResourceLimits
            {
                CpuLimit = "500m",
                MemoryLimit = "512Mi"
            },
            Scaling = new IacScalingSpec
            {
                Instances = 3,
                Policy = ScalingPolicy.Auto
            }
        }
    },
    Databases = new List<IacTemplateDatabase>
    {
        new IacTemplateDatabase
        {
            Name = "main-db",
            Type = DatabaseType.PostgreSql,
            Version = "15",
            MaxConnections = 100,
            ConnectionTimeoutSeconds = 30,
            Backup = new IacBackupSpec
            {
                Enabled = true,
                Strategy = BackupStrategy.Snapshot,
                RetentionDays = 30,
                Schedule = "0 2 * * *"
            }
        }
    }
};

// Validate the template structure
var validationErrors = template.Validate().ToList();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Template validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Template is valid!");
}
```

## ServiceHealth

The `ServiceHealth` class represents the health status and resource metrics of a service instance in the Coolify infrastructure. It tracks service identifiers, health check results, response times, resource utilization (CPU, memory), connection metrics, error rates, and provides methods to record successful and failed health checks. This model is used throughout the application for monitoring, alerting, and service discovery.

Here's a realistic example of creating and using a `ServiceHealth` instance:

```csharp
// Create a service health record for a web service
var serviceHealth = new ServiceHealth
{
    Id = 1,
    ServiceId = "web-app-001",
    Status = HealthStatus.Healthy,
    CheckedAt = DateTime.UtcNow,
    ResponseTimeMs = 45.2,
    HttpStatusCode = 200,
    CpuUsagePercent = 12.5,
    MemoryUsageMb = 185.7,
    ActiveConnections = 42,
    ErrorRatePercent = 0.0,
    LastSuccessfulCheck = DateTime.UtcNow.AddMinutes(-2),
    FailureCount = 0,
    FailureReason = null,
    Warnings = new List<string>(),
    IsHealthy = true,
    RequiresAttention = false
};

// Record a successful health check
serviceHealth.RecordSuccess(38.7, 200, 8.3, 156.2, 25);

// Check if the service requires attention
if (serviceHealth.RequiresAttention)
{
    Console.WriteLine($"Service {serviceHealth.ServiceId} requires attention!");
    Console.WriteLine($"Status: {serviceHealth.Status}");
    Console.WriteLine($"Failure count: {serviceHealth.FailureCount}");
    Console.WriteLine($"Failure reason: {serviceHealth.FailureReason}");
}

// Add a warning for resource thresholds
if (serviceHealth.CpuUsagePercent > 80.0)
{
    serviceHealth.Warnings.Add("High CPU usage detected");
}

if (serviceHealth.MemoryUsageMb > 500.0)
{
    serviceHealth.Warnings.Add("High memory usage detected");
}

// Check if the service is healthy
if (serviceHealth.IsHealthy)
{
    Console.WriteLine($"Service {serviceHealth.ServiceId} is healthy");
}
```

## ResourceMonitorService

The `ResourceMonitorService` class provides real-time resource monitoring capabilities for applications managed by Coolify. It polls the Coolify API for per-application resource usage metrics including CPU, memory, network I/O, and other system resources. The service supports both one-time snapshots and continuous monitoring streams, making it ideal for CLI monitoring commands and automated alerting systems.

Here's a realistic example of using the ResourceMonitorService to monitor application resources:

```csharp
// Initialize required services
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var resourceMonitor = new ResourceMonitorService(apiClient, logger);

// Example 1: Get a single resource usage snapshot
var snapshotResponse = await resourceMonitor.GetResourceUsageAsync(42);
if (snapshotResponse.Success && snapshotResponse.Data is not null)
{
  var usage = snapshotResponse.Data;
  Console.WriteLine($"CPU: {usage.CpuPercent:F1}%");
  Console.WriteLine($"Memory: {usage.MemoryMb} MB");
  Console.WriteLine($"Memory usage: {usage.MemoryPercent:F1}%");
  Console.WriteLine($"Network RX: {usage.NetworkRxBytes:N0} bytes");
  Console.WriteLine($"Network TX: {usage.NetworkTxBytes:N0} bytes");
}

// Example 2: Get bulk resource usage for multiple applications
var appIds = new List<int> { 42, 43, 44 };
var bulkUsage = await resourceMonitor.GetBulkResourceUsageAsync(appIds);
Console.WriteLine($"Retrieved resource data for {bulkUsage.Count} applications");

// Example 3: Render resource usage in a table format
ResourceMonitorService.RenderHeader();
foreach (var usage in bulkUsage)
{
  ResourceMonitorService.RenderUsageLine(usage);
}

// Example 4: Continuously monitor an application with cancellation
using var cts = new CancellationTokenSource();

// Start monitoring in background
var monitoringTask = Task.Run(async () =>
{
  await foreach (var usage in resourceMonitor.MonitorAsync(42, intervalSeconds: 10, cts.Token))
  {
    Console.WriteLine($"[{usage.CapturedAt:HH:mm:ss}] CPU: {usage.CpuPercent:F1}%, Memory: {usage.MemoryMb} MB");
  }
});

// Run for 1 minute then stop
await Task.Delay(TimeSpan.FromMinutes(1));
cts.Cancel();

try
{
  await monitoringTask;
}
catch (OperationCanceledException)
{
  Console.WriteLine("Monitoring stopped");
}
```

## ResourceUsage

The `ResourceUsage` class represents a point-in-time snapshot of resource consumption for a single application instance. It tracks CPU utilisation, memory usage, network I/O, file handles, and thread counts to provide comprehensive monitoring data. The class includes methods for calculating memory percentage, determining alert severity based on resource thresholds, and generating human-readable summary lines for tabular display.

Here's a realistic example of creating and using a `ResourceUsage` instance:

```csharp
// Create a resource usage snapshot for a production web service
var usage = new ResourceUsage
{
    ApplicationId = 42,
    ApplicationName = "web-storefront",
    CapturedAt = DateTime.UtcNow,
    CpuPercent = 68.5,
    MemoryMb = 1024,
    MemoryLimitMb = 2048,
    NetworkRxBytes = 1548723456,
    NetworkTxBytes = 87654321,
    OpenFileHandles = 42,
    ThreadCount = 25
};

// Calculate memory percentage (should be ~50.0%)
var memoryPercent = usage.MemoryPercent;
Console.WriteLine($"Memory usage: {memoryPercent}%"); // Output: Memory usage: 50.0%

// Get alert severity based on resource thresholds
var severity = usage.GetAlertSeverity();
Console.WriteLine($"Alert severity: {severity}"); // Output: Alert severity: null (within normal ranges)

// Update CPU to trigger warning threshold
usage.CpuPercent = 85;
severity = usage.GetAlertSeverity();
Console.WriteLine($"Alert severity: {severity}"); // Output: Alert severity: Warning

// Update memory to trigger critical threshold
usage.MemoryMb = 1950;
severity = usage.GetAlertSeverity();
Console.WriteLine($"Alert severity: {severity}"); // Output: Alert severity: Critical

// Generate a summary line for monitoring display
var summary = usage.ToSummaryLine();
Console.WriteLine(summary);
// Output: "    42 web-storefront                 85.0%    1950 MB   95.2%    1.5 GB      83 MB"
```

## ResourceMonitorService

The `ResourceMonitorService` class provides real-time resource monitoring capabilities for applications managed by Coolify. It polls the Coolify API for per-application resource usage metrics including CPU, memory, network I/O, and other system resources. The service supports both one-time snapshots and continuous monitoring streams, making it ideal for CLI monitoring commands and automated alerting systems.

Here's a realistic example of using the ResourceMonitorService to monitor application resources:

```csharp
// Initialize required services
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var resourceMonitor = new ResourceMonitorService(apiClient, logger);

// Example 1: Get a single resource usage snapshot
var snapshotResponse = await resourceMonitor.GetResourceUsageAsync(42);
if (snapshotResponse.Success && snapshotResponse.Data is not null)
{
  var usage = snapshotResponse.Data;
  Console.WriteLine($"CPU: {usage.CpuPercent:F1}%");
  Console.WriteLine($"Memory: {usage.MemoryMb} MB");
  Console.WriteLine($"Memory usage: {usage.MemoryPercent:F1}%");
  Console.WriteLine($"Network RX: {usage.NetworkRxBytes:N0} bytes");
  Console.WriteLine($"Network TX: {usage.NetworkTxBytes:N0} bytes");
}

// Example 2: Get bulk resource usage for multiple applications
var appIds = new List<int> { 42, 43, 44 };
var bulkUsage = await resourceMonitor.GetBulkResourceUsageAsync(appIds);
Console.WriteLine($"Retrieved resource data for {bulkUsage.Count} applications");

// Example 3: Render resource usage in a table format
ResourceMonitorService.RenderHeader();
foreach (var usage in bulkUsage)
{
  ResourceMonitorService.RenderUsageLine(usage);
}

// Example 4: Continuously monitor an application with cancellation
using var cts = new CancellationTokenSource();

// Start monitoring in background
var monitoringTask = Task.Run(async () =>
{
  await foreach (var usage in resourceMonitor.MonitorAsync(42, intervalSeconds: 10, cts.Token))
  {
    Console.WriteLine($"[{usage.CapturedAt:HH:mm:ss}] CPU: {usage.CpuPercent:F1}%, Memory: {usage.MemoryMb} MB");
  }
});

// Run for 1 minute then stop
await Task.Delay(TimeSpan.FromMinutes(1));
cts.Cancel();

try
{
  await monitoringTask;
}
catch (OperationCanceledException)
{
  Console.WriteLine("Monitoring stopped");
}
```

## ResourceUsageTests

The `ResourceUsageTests` class provides unit tests for the `ResourceUsage` model, which tracks and analyzes resource consumption metrics such as CPU percentage and memory usage. These tests verify the calculation of memory percentage, alert severity determination based on resource thresholds, and summary line generation for monitoring purposes.

Here's an example of how to use the tested methods:

```csharp
// Create a resource usage with memory limit
var usage = new ResourceUsage
{
    ApplicationId = 42,
    ApplicationName = "api-service",
    CpuPercent = 75.5,
    MemoryMb = 850,
    MemoryLimitMb = 1024
};

// Calculate memory percentage (should be ~83.0%)
var memoryPercent = usage.MemoryPercent;
// memoryPercent is 83.0

// Get alert severity based on resource thresholds
var severity = usage.GetAlertSeverity();
// severity is null (within normal ranges)

// Update CPU to trigger warning
usage.CpuPercent = 85;
severity = usage.GetAlertSeverity();
// severity is SeverityLevel.Warning

// Update memory to trigger critical
usage.MemoryMb = 980;
severity = usage.GetAlertSeverity();
// severity is SeverityLevel.Critical

// Generate a summary line for monitoring
var summary = usage.ToSummaryLine();
// summary contains "42", "api-service", and "75.5"
```

## CollectionExtensions

The `CollectionExtensions` class provides a comprehensive set of extension methods for working with collections in C#. These utilities simplify common operations like filtering, batching, transformation, and dictionary manipulation, making collection processing more readable and concise.

Here's an example of how to use some of the most useful methods:

```csharp
// Check if a collection is null or empty
var emptyList = new List<string>();
if (emptyList.IsNullOrEmpty())
{
    Console.WriteLine("Collection is null or empty");
}

var populatedList = new[] { "app1", "app2", "app3" };
if (!populatedList.IsNullOrEmpty())
{
    Console.WriteLine($"Collection has {populatedList.Count} items");
}

// Split a collection into batches of 3 items each
var deploymentIds = Enumerable.Range(1, 10).ToList();
var batches = deploymentIds.Batch(3).ToList();
// batches contains: [[1,2,3], [4,5,6], [7,8,9], [10]]

// Filter out null values from a collection
var servicesWithNulls = new string?[] { "web", null, "api", null, "worker" };
var validServices = servicesWithNulls.WhereNotNull().ToList();
// validServices: ["web", "api", "worker"]

// Split a collection into two based on a predicate
var (evenDeployments, oddDeployments) = deploymentIds.Split(n => n % 2 == 0);
// evenDeployments: [2,4,6,8,10]
// oddDeployments: [1,3,5,7,9]

// Flatten nested collections
var nestedDeploymentGroups = new List<List<string>>
{
    new() { "web-app", "web-worker" },
    new() { "api-service" },
    new() { "cache-redis", "cache-memcached" }
};
var allDeployments = nestedDeploymentGroups.Flatten().ToList();
// allDeployments: ["web-app", "web-worker", "api-service", "cache-redis", "cache-memcached"]

// Find items with max/min values based on a key selector
var applications = new[] 
{
    new { Name = "web", Memory = 512 },
    new { Name = "api", Memory = 1024 },
    new { Name = "worker", Memory = 256 }
};

var maxMemoryApp = applications.MaxBy(a => a.Memory);
// maxMemoryApp.Name is "api"

var minMemoryApp = applications.MinBy(a => a.Memory);
// minMemoryApp.Name is "worker"

// Merge two dictionaries, with second dictionary values overwriting first
var defaultConfig = new Dictionary<string, string>
{
    { "timeout", "30" },
    { "retries", "3" }
};

var customConfig = new Dictionary<string, string>
{
    { "retries", "5" },
    { "region", "us-east-1" }
};

var mergedConfig = defaultConfig.Merge(customConfig);
// mergedConfig["timeout"] is "30"
// mergedConfig["retries"] is "5"
// mergedConfig["region"] is "us-east-1"

// Convert a dictionary to a query string
var queryParams = new Dictionary<string, string>
{
    { "env", "production" },
    { "region", "us-east-1" },
    { "version", "v2" }
};
var queryString = queryParams.ToQueryString();
// queryString is "env=production&region=us-east-1&version=v2"

// Group consecutive items based on a condition
var deploymentNumbers = new[] { 1, 2, 3, 10, 11, 20, 21, 22 };
var groups = deploymentNumbers.GroupConsecutive((a, b) => b - a <= 1).ToList();
// groups[0]: [1,2,3]
// groups[1]: [10,11]
// groups[2]: [20,21,22]

// Safely get an item at index
var deploymentList = new[] { "web", "api", "worker", "cache" };
var thirdItem = deploymentList.GetAtIndexOrDefault(2); // "worker"
var outOfRangeItem = deploymentList.GetAtIndexOrDefault(10); // null

// Shuffle a collection randomly
var shuffledDeployments = deploymentList.Shuffle().ToList();
// shuffledDeployments contains the same items in random order

// Order by descending using a key selector
var sortedApps = applications.OrderByDescending(a => a.Memory).ToList();
// sortedApps[0] has the highest memory

// Partition a collection into groups of specified size
var partitions = deploymentIds.Partition(4);
// partitions contains: [[1,2,3,4], [5,6,7,8], [9,10]]
```

## EnvironmentVariableService

The `EnvironmentVariableService` class provides comprehensive management of environment variables for applications and services within Coolify. It handles CRUD operations, bulk updates, secret rotation, validation, and change tracking across different environment scopes. The service integrates with the API client to ensure consistent environment variable management across the deployment lifecycle.

Here's a realistic example of using the EnvironmentVariableService to manage application environment variables:

```csharp
// Initialize the environment variable service with API client and logger
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var envVarService = new EnvironmentVariableService(apiClient, logger, config);

// Retrieve all environment variables for a specific application
var appVarsResponse = await envVarService.GetApplicationVariablesAsync("web-storefront");
if (appVarsResponse.Success)
{
    var environmentVariables = appVarsResponse.Data;
    Console.WriteLine($"Found {environmentVariables.Count} environment variables");
}

// Create a new environment variable
var createResponse = await envVarService.CreateVariableAsync(new EnvironmentVariable
{
    ApplicationId = "web-storefront",
    Key = "DATABASE_URL",
    Value = "postgresql://user:pass@localhost:5432/mydb",
    IsSecret = true,
    Description = "Database connection string",
    EnvironmentScope = "production"
});

if (createResponse.Success)
{
    Console.WriteLine($"Created variable: {createResponse.Data.Key}");
}

// Update an existing environment variable
var updateResponse = await envVarService.UpdateVariableAsync(new EnvironmentVariable
{
    Id = "var-123",
    ApplicationId = "web-storefront",
    Key = "DATABASE_URL",
    Value = "postgresql://user:newpass@localhost:5432/mydb",
    IsSecret = true,
    EnvironmentScope = "production"
});

// Get a specific environment variable by ID
var getResponse = await envVarService.GetVariableAsync("var-123");
if (getResponse.Success)
{
    var variable = getResponse.Data;
    Console.WriteLine($"Variable {variable.Key} = {variable.GetDisplayValue(maskSecrets: true)}");
}

// Bulk update multiple environment variables
var bulkUpdateResponse = await envVarService.BulkUpdateVariablesAsync(new List<EnvironmentVariable>
{
    new EnvironmentVariable { Id = "var-123", Value = "postgresql://updated:pass@localhost:5432/mydb" },
    new EnvironmentVariable { Id = "var-456", Value = "redis://cache:6379" }
});

// Get variables by scope (e.g., all production variables)
var scopeResponse = await envVarService.GetVariablesByScopeAsync("web-storefront", "production");

// Rotate secrets for an application (generates new values for all secret variables)
var rotateResponse = await envVarService.RotateSecretsAsync("web-storefront", "production");

// Validate environment variables before deployment
var validationResponse = await envVarService.ValidateVariablesAsync("web-storefront", new Dictionary<string, string>
{
    ["DATABASE_URL"] = "postgresql://user:pass@localhost:5432/mydb",
    ["REDIS_URL"] = "redis://cache:6379"
});

if (!validationResponse.Success)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationResponse.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Get change history for audit purposes
var historyResponse = await envVarService.GetChangeHistoryAsync("web-storefront", "production");
if (historyResponse.Success)
{
    var changes = historyResponse.Data;
    Console.WriteLine($"Found {changes.Count} change records");
}

// Delete an environment variable
var deleteResponse = await envVarService.DeleteVariableAsync("var-123");
```

## EnumExtensions

The `EnumExtensions` class provides a comprehensive set of utility methods for working with enums in C#. It includes helpers for getting enum descriptions, parsing strings to enums, converting enums to display strings, checking flags, converting to numeric values, and generating CLI-friendly formats. These utilities are particularly useful for CLI applications, configuration options, and user-friendly enum displays.

## AdvancedAppCommands

The `AdvancedAppCommands` class provides advanced application lifecycle management commands for deployment configuration, environment variables, scaling, and rollback operations. It offers fine-grained control over application management through commands like restart, set-env, scale, and rollback.

Here's an example of how to use the `AdvancedAppCommands` class to manage application lifecycle:

```csharp
// Create an instance of AdvancedAppCommands with required dependencies
var apiClient = new CoolifyApiClient("https://api.coolify.io", "your-api-token");
var logger = new Logger();
var config = new CoolifyConfiguration { /* your configuration */ };

var advancedCommands = new AdvancedAppCommands(apiClient, logger, config);

// Create and execute restart command
var restartCommand = advancedCommands.CreateRestartCommand();
// restartCommand can be added to a CLI root command and executed with appropriate arguments

// Create and execute set-env command to update environment variables
var setEnvCommand = advancedCommands.CreateSetEnvCommand();
// setEnvCommand can be configured with --file or --var options for environment variable updates

// Create and execute scale command to adjust application resources
var scaleCommand = advancedCommands.CreateScaleCommand();
// scaleCommand accepts --instances, --cpu, and --memory options for scaling

// Create and execute rollback command to revert to a previous deployment
var rollbackCommand = advancedCommands.CreateRollbackCommand();
// rollbackCommand accepts --deployment option to specify a specific deployment ID
```

## EnvironmentVariable

The `EnvironmentVariable` class represents configuration variables for applications and services, supporting secret management, environment scoping, and change tracking. It provides validation, value masking for display, and cloning capabilities for auditing purposes.

Here's an example of how to create and use an environment variable:

```csharp
// Create a new environment variable for a web application
var envVar = new EnvironmentVariable
{
    ApplicationId = "web-app-001",
    Key = "DATABASE_URL",
    Value = "postgresql://user:pass@localhost:5432/mydb",
    IsSecret = true,
    Description = "Database connection string for production environment",
    EnvironmentScope = "production",
    CreatedBy = "system"
};

// Validate the environment variable configuration
var validationErrors = envVar.Validate().ToList();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Environment variable configuration is valid!");
}

// Get a masked display value for logging (shows last 4 chars of secret)
var displayValue = envVar.GetDisplayValue(maskSecrets: true);
// displayValue is "***d/mydb"

// Clone the variable for auditing before making changes
var auditCopy = envVar.Clone();

// Update the variable and mark it as changed
envVar.Value = "postgresql://user:newpass@localhost:5432/mydb";
envVar.MarkAsUpdated("admin-user");

// Verify the update
Console.WriteLine($"Updated at: {envVar.UpdatedAt}");
Console.WriteLine($"Updated by: {envVar.UpdatedBy}");
```

Here's an example of how to use some of the utility methods:

```csharp
// Define an enum with Description attributes
public enum DeploymentStatus
{
    [Description("Not yet deployed")]
    NotDeployed,
    
    [Description("Currently deploying")]
    Deploying,
    
    [Description("Successfully deployed")]
    Deployed,
    
    [Description("Deployment failed")]
    Failed
}

// Get the description from an enum value
var status = DeploymentStatus.Deploying;
var description = status.GetDescription(); // "Currently deploying"

// Convert an enum to a human-readable display string
var displayString = status.ToDisplayString(); // "Deploying"

// Parse a string to an enum
var parsedStatus = "deployed".ParseEnum<DeploymentStatus>(); // DeploymentStatus.Deployed

// Try to parse a string to an enum (returns null if invalid)
var invalidStatus = "invalid".TryParseEnum<DeploymentStatus>(); // null

// Get all enum values
var allStatuses = EnumExtensions.GetAllValues<DeploymentStatus>();
// [NotDeployed, Deploying, Deployed, Failed]

// Get all enum values with their descriptions
var statusMap = EnumExtensions.GetValueDescriptionMap<DeploymentStatus>();
// {NotDeployed: "Not yet deployed", Deploying: "Currently deploying", ...}

// Get all display strings
var displayStrings = EnumExtensions.GetDisplayStrings<DeploymentStatus>();
// ["Not deployed", "Deploying", "Deployed", "Failed"]

// Check if an enum has a specific flag (works with [Flags] enums)
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    All = Read | Write | Execute
}

var userPermissions = Permissions.Read | Permissions.Write;
userPermissions.HasFlag(Permissions.Read).Should().BeTrue(); // true
userPermissions.HasFlag(Permissions.Execute).Should().BeFalse(); // false

// Convert enum to numeric values
var statusAsLong = status.ToLong<DeploymentStatus>(); // 1L
var statusAsInt = status.ToInt<DeploymentStatus>(); // 1

// Convert enum to CLI format (kebab-case)
var cliArg = DeploymentStatus.Deployed.ToCliFormat(); // "deployed"

// Get a random enum value
var randomStatus = EnumExtensions.GetRandomValue<DeploymentStatus>();

// Check if enum equals a name (case-insensitive)
status.EqualsIgnoreCase("DEPLOYING").Should().BeTrue(); // true

// Get custom attributes from enum values
var descriptionAttribute = status.GetAttribute<DescriptionAttribute>();
// Returns the DescriptionAttribute instance
```

## CollectionExtensionsTests

The `CollectionExtensionsTests` class provides unit tests for collection extension methods in the `CoolifyCli.Extensions` namespace. It tests various utility methods for working with collections including batching operations, filtering, transformation, and dictionary manipulation to ensure these extension methods work correctly across different scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Test if a collection is null or empty
var emptyList = new List<string>();
emptyList.IsNullOrEmpty().Should().BeTrue();

var populatedList = new[] { "item1", "item2", "item3" };
populatedList.IsNullOrEmpty().Should().BeFalse();

// Split a collection into batches of 2 items each
var numbers = Enumerable.Range(1, 6).ToList();
var batches = numbers.Batch(2).ToList();
batches.Should().HaveCount(3);
batches[0].Should().BeEquivalentTo(new[] { 1, 2 });
batches[1].Should().BeEquivalentTo(new[] { 3, 4 });
batches[2].Should().BeEquivalentTo(new[] { 5, 6 });

// Filter out null values from a collection
var itemsWithNulls = new string?[] { "a", null, "b", null, "c" };
var nonNullItems = itemsWithNulls.WhereNotNull().ToList();
nonNullItems.Should().BeEquivalentTo(new[] { "a", "b", "c" });

// Split a collection into two based on a predicate
var (evens, odds) = Enumerable.Range(1, 6).Split(n => n % 2 == 0);
evens.Should().BeEquivalentTo(new[] { 2, 4, 6 });
odds.Should().BeEquivalentTo(new[] { 1, 3, 5 });

// Flatten nested collections
var nested = new List<List<int>>
{
    new() { 1, 2 },
    new() { 3 },
    new() { 4, 5, 6 }
};
var flattened = nested.Flatten().ToList();
flattened.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6 });

// Find items with max/min values based on a key selector
var words = new[] { "cat", "elephant", "dog" };
var longest = words.MaxBy(w => w.Length);
longest.Should().Be("elephant");

var shortest = words.MinBy(w => w.Length);
shortest.Should().Be("cat");

// Merge two dictionaries, with second dictionary values overwriting first
var first = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
var second = new Dictionary<string, int> { { "b", 99 }, { "c", 3 } };
var merged = first.Merge(second);
merged["a"].Should().Be(1);
merged["b"].Should().Be(99);
merged["c"].Should().Be(3);

// Convert a dictionary to a query string
var queryParams = new Dictionary<string, string>
{
    { "env", "prod" },
    { "region", "us-east-1" }
};
var queryString = queryParams.ToQueryString();
queryString.Should().Contain("env=prod");
queryString.Should().Contain("region=us-east-1");

// Group consecutive items based on a condition
var numbersForGrouping = new[] { 1, 2, 3, 10, 11, 20 };
var groups = numbersForGrouping.GroupConsecutive((a, b) => b - a <= 1).ToList();
groups.Should().HaveCount(3);
groups[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
groups[1].Should().BeEquivalentTo(new[] { 10, 11 });
groups[2].Should().BeEquivalentTo(new[] { 20 });
```
