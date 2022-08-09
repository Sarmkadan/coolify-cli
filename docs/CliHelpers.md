# CliHelpers

The `CliHelpers` static class provides a set of utility methods for building a consistent command-line interface in the `coolify-cli` application. It centralizes common tasks such as printing formatted messages, prompting for user input, formatting data for display, and rendering tables and progress indicators. All members are static and designed to be used directly without instantiation.

## API

### `PrintHeader`
Prints the application header to the console. This method takes no parameters and outputs a predefined banner (e.g., application name, version, or ASCII art). It does not throw exceptions under normal operation.

### `PrintSubheader`
Prints a subheader line to the console. The subheader text is provided as a string argument. This method is typically used to separate sections within the output. No return value.

### `PrintSuccess`
Prints a success message to the console. Accepts a string message that is displayed with a success indicator (e.g., a green checkmark). No return value.

### `PrintError`
Prints an error message to the console. Accepts a string message that is displayed with an error indicator (e.g., a red cross). No return value.

### `PrintWarning`
Prints a warning message to the console. Accepts a string message that is displayed with a warning indicator (e.g., a yellow triangle). No return value.

### `PrintInfo`
Prints an informational message to the console. Accepts a string message that is displayed with an info indicator (e.g., a blue “i”). No return value.

### `PromptConfirmation`
Prompts the user for a yes/no confirmation. Accepts a string prompt message. Returns `true` if the user confirms (e.g., types “y” or “yes”), otherwise `false`. The comparison is case-insensitive. Throws no exceptions; invalid input is handled gracefully by re-prompting or returning `false`.

### `PromptInput`
Prompts the user for a text input. Accepts a string prompt message. Returns the entered text as a `string?` (nullable). Returns `null` if the user cancels or provides an empty input (depending on implementation). Does not throw.

### `PromptSecretInput`
Prompts the user for a secret input (e.g., password) without echoing the typed characters. Accepts a string prompt message. Returns the entered text as a non-nullable `string`. Throws no exceptions.

### `FormatBytes`
Formats a byte count into a human-readable string (e.g., “1.23 MB”). Accepts a `long` value representing the number of bytes. Returns a `string`. Throws no exceptions; negative values are handled gracefully (e.g., “-1 B”).

### `FormatTimeSpan`
Formats a `TimeSpan` into a concise, human-readable string (e.g., “2d 3h 15m”). Accepts a `TimeSpan` value. Returns a `string`. Throws no exceptions.

### `GetStatusIndicator`
Returns a string representing a status indicator (e.g., a colored symbol or emoji) based on a boolean or status value. Accepts a `bool` indicating success (`true`) or failure (`false`). Returns a `string` (e.g., “✔” or “✘”). Does not throw.

### `GetHealthIndicator`
Returns a string representing a health indicator (e.g., a colored dot or icon) based on a health status. Accepts a `bool` indicating healthy (`true`) or unhealthy (`false`). Returns a `string` (e.g., “●” or “○”). Does not throw.

### `PrintTable`
Prints a formatted table to the console. Accepts a collection of rows, where each row is a collection of strings (e.g., `IEnumerable<IEnumerable<string>>`). The first row is treated as the header. Columns are automatically sized to fit content. No return value. Throws no exceptions; an empty collection results in no output.

### `GetProgressBar`
Returns a string representation of a progress bar. Accepts a `double` value between 0.0 and 1.0 representing the progress fraction, and optionally an integer width (default is 20 characters). Returns a `string` (e.g., “[████████░░░░] 50%”). Throws `ArgumentOutOfRangeException` if the progress value is outside [0,1] or width is less than 1.

## Usage

### Example 1: User interaction and status display

```csharp
using static CliHelpers;

PrintHeader();
PrintSubheader("Deployment Setup");

string projectName = PromptInput("Enter project name:") ?? "default";
if (PromptConfirmation($"Deploy project '{projectName}'?"))
{
    PrintInfo("Starting deployment...");
    // Simulate deployment
    bool success = true;
    PrintSuccess(success ? "Deployment completed." : "Deployment failed.");
    Console.WriteLine(GetStatusIndicator(success));
}
else
{
    PrintWarning("Deployment cancelled.");
}
```

### Example 2: Formatting and table output

```csharp
using static CliHelpers;

long fileSize = 1_500_000;
TimeSpan elapsed = new TimeSpan(0, 2, 35, 10);

Console.WriteLine($"File size: {FormatBytes(fileSize)}");
Console.WriteLine($"Elapsed: {FormatTimeSpan(elapsed)}");

var rows = new List<string[]>
{
    new[] { "Name", "Status", "Health" },
    new[] { "web", GetStatusIndicator(true), GetHealthIndicator(true) },
    new[] { "db", GetStatusIndicator(false), GetHealthIndicator(false) }
};
PrintTable(rows);

double progress = 0.73;
Console.WriteLine(GetProgressBar(progress, 30));
```

## Notes

- **Thread safety**: All methods in `CliHelpers` are static and rely on `System.Console` for output and input. `System.Console` is not thread-safe; concurrent calls from multiple threads may produce interleaved output or cause exceptions. If used in a multi-threaded context, external synchronization (e.g., a lock) is required.
- **Edge cases**:
  - `PromptInput` may return `null` when the user cancels (e.g., Ctrl+C). Callers should handle the nullable return.
  - `PromptSecretInput` does not echo input; on some platforms, the underlying console implementation may not support hiding input, and the method may fall back to normal input.
  - `FormatBytes` and `FormatTimeSpan` handle zero and negative values gracefully, but negative values are not semantically meaningful.
  - `GetProgressBar` throws `ArgumentOutOfRangeException` for invalid progress values. Ensure the argument is clamped to [0,1] before calling.
  - `PrintTable` with a very large number of rows or long strings may cause console wrapping; it does not truncate content.
- **Console encoding**: Output methods assume the console supports Unicode characters (e.g., checkmarks, progress bar blocks). On legacy consoles, these characters may appear as question marks or boxes. Set `Console.OutputEncoding` to `Encoding.UTF8` if needed.
