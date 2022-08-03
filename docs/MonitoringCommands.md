# MonitoringCommands

`MonitoringCommands` provides CLI entry points for real-time observability features in the Coolify platform. It exposes commands that allow users to retrieve system metrics, stream live logs, and manage alerting configurations from the command line.

## API

### MonitoringCommands

The constructor for the type. Initializes a new instance of `MonitoringCommands`, preparing the underlying command infrastructure for registration with a command-line application.

- **Parameters:** None
- **Returns:** A new `MonitoringCommands` instance
- **Throws:** No exceptions are thrown by this constructor

### Command CreateMetricsCommand

Creates a command that fetches and displays current monitoring metrics for a specified resource or scope.

- **Parameters:** None (the returned `Command` encapsulates its own argument definitions)
- **Returns:** A configured `Command` object ready for execution
- **Throws:** Does not throw during creation; the returned command may throw at runtime if the metrics endpoint is unreachable or authentication fails

### Command CreateLogStreamCommand

Creates a command that opens a continuous log stream, printing incoming log entries to the console until the user terminates the session.

- **Parameters:** None (the returned `Command` encapsulates its own argument definitions)
- **Returns:** A configured `Command` object ready for execution
- **Throws:** Does not throw during creation; the returned command may throw at runtime if the log stream cannot be established or the connection is interrupted

### Command CreateAlertsCommand

Creates a command for managing alert definitions, including listing, creating, updating, and deleting alert rules.

- **Parameters:** None (the returned `Command` encapsulates its own argument definitions)
- **Returns:** A configured `Command` object ready for execution
- **Throws:** Does not throw during creation; the returned command may throw at runtime if alert configuration operations fail due to validation errors or server-side issues

## Usage

```csharp
// Register monitoring commands with a root command
var rootCommand = new RootCommand("Coolify CLI");
var monitoring = new MonitoringCommands();

rootCommand.AddCommand(monitoring.CreateMetricsCommand());
rootCommand.AddCommand(monitoring.CreateLogStreamCommand());
rootCommand.AddCommand(monitoring.CreateAlertsCommand());

await rootCommand.InvokeAsync(args);
```

```csharp
// Execute only the metrics command programmatically
var monitoring = new MonitoringCommands();
var metricsCommand = monitoring.CreateMetricsCommand();

// Simulate CLI arguments for a specific resource
await metricsCommand.InvokeAsync("--resource my-service --interval 30s");
```

## Notes

- Each `Create*Command` method returns a new `Command` instance on every call; callers may register the same command type multiple times if distinct configurations are required, though this is atypical.
- The returned `Command` objects are not thread-safe by default. Registration and invocation should occur on a single thread or be synchronized externally if concurrent access is necessary.
- The `MonitoringCommands` constructor performs no I/O or network operations; it is safe to instantiate in any context without risk of blocking or failure.
- Runtime exceptions from the returned commands are surfaced through the standard command execution pipeline. Callers should handle `OperationCanceledException` for user-initiated interruptions of long-running streams such as log streaming.
