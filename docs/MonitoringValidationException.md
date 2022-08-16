# MonitoringValidationException

Exception thrown when monitoring resource validation fails, typically during CLI command execution. Provides standardized error handling for resource identifier validation and command-line option configuration.

## API

### `MonitoringValidationException(string message) : base(message)`

Constructs a new `MonitoringValidationException` with the specified error message.

- **Parameters**
  - `message` (string): The validation failure reason to be reported.
- **Remarks**
  Inherits from `Exception` and sets the exception message directly.

---

### `public static Command WithStandardOptions(Command command)`

Adds standard monitoring-related options to a CLI command.

- **Parameters**
  - `command` (Command): The base command to which options should be added.
- **Returns**
  - Command: The modified command with standard options (`--resource-id`, `--timeout`, `--output-format`).
- **Remarks**
  Options are added in a non-destructive manner; existing options are preserved.

---

### `public static Command WithTimeoutOption(Command command)`

Adds a `--timeout` option to a CLI command for monitoring operations.

- **Parameters**
  - `command` (Command): The command to extend with the timeout option.
- **Returns**
  - Command: The command with the timeout option added.
- **Remarks**
  The option expects an integer value representing seconds. Defaults to `30` if not specified.

---
### `public static void ValidateResourceId(string resourceId)`

Validates a resource identifier for monitoring operations.

- **Parameters**
  - `resourceId` (string): The resource identifier to validate.
- **Exceptions**
  - Throws `MonitoringValidationException` if the identifier is null, empty, or malformed.
- **Remarks**
  Validation rules include non-null/empty checks and format compliance (e.g., `res_*` prefix).

---
### `public static Command WithOutputFormatting(Command command)`

Configures output formatting options for monitoring results.

- **Parameters**
  - `command` (Command): The command to extend with output formatting options.
- **Returns**
  - Command: The command with `--output-format` (json|plaintext) and `--no-color` options.
- **Remarks**
  Defaults to `plaintext` if not specified. Color output is enabled by default.

## Usage

```csharp
// Example 1: Validating a resource ID before command execution
try
{
    MonitoringValidationException.ValidateResourceId(resourceId);
    var command = MonitoringValidationException.WithStandardOptions(baseCommand);
    // Execute command...
}
catch (MonitoringValidationException ex)
{
    Console.Error.WriteLine($"Validation failed: {ex.Message}");
    return ExitCodes.InvalidInput;
}

// Example 2: Adding timeout and output options to a monitoring command
var monitoringCommand = new Command("monitor")
    .WithDescription("Monitor a resource");

var extendedCommand = MonitoringValidationException
    .WithTimeoutOption(monitoringCommand)
    .WithOutputFormatting(monitoringCommand);

return extendedCommand;
```

## Notes

- **Thread Safety**: All members are stateless and thread-safe. No shared mutable state is accessed.
- **Validation Edge Cases**: `ValidateResourceId` throws if `resourceId` is `null`, whitespace-only, or lacks a required prefix (e.g., `res_`). Malformed identifiers (e.g., `res-123`) are rejected.
- **Option Conflicts**: `WithStandardOptions` does not overwrite existing options; conflicts are resolved by preserving the first-added option.
