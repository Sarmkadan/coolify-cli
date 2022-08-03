# LogEntry
Represents a single log record captured by the CLI, containing identifying information, the log message, severity level, timestamps, and optional diagnostic data such as source, trace identifier, exit code, stack trace, and extensible metadata.

## API
### Id
**Purpose:** Unique identifier for the log entry.  
**Type:** `int`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### ApplicationId
**Purpose:** Identifier of the application that produced the log.  
**Type:** `string`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### Message
**Purpose:** The log message text.  
**Type:** `string`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### Level
**Purpose:** Severity level of the log entry.  
**Type:** `LogLevel`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### Timestamp
**Purpose:** Date and time when the log entry was created.  
**Type:** `DateTime`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### Source
**Purpose:** Optional component or module that originated the log.  
**Type:** `string?`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### TraceId
**Purpose:** Optional identifier for correlating logs across distributed traces.  
**Type:** `string?`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### Metadata
**Purpose:** Extensible key‑value store for additional contextual information.  
**Type:** `Dictionary<string, string>`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### ExitCode
**Purpose:** Optional process exit code associated with the log entry (e.g., from a command execution).  
**Type:** `int?`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### StackTrace
**Purpose:** Optional stack trace string, typically populated when the log originates from an exception.  
**Type:** `string?`.  
**Parameters:** None.  
**Return value:** N/A (field).  
**Throws:** None.

### FromException
**Purpose:** Creates a `LogEntry` instance initialized with data extracted from an `Exception`.  
**Parameters:**  
- `Exception ex` – The exception to source the log entry from.  
**Return value:** A new `LogEntry` with `Message` set to `ex.Message`, `StackTrace` set to `ex.StackTrace`, `Level` set to `LogLevel.Error`, and `Timestamp` set to `DateTime.UtcNow`. Other fields remain at their default values unless subsequently assigned.  
**Throws:**  
- `ArgumentNullException` if `ex` is `null`.

### IsCritical
**Purpose:** Indicates whether the log entry represents a critical severity level.  
**Parameters:** None.  
**Return value:** `true` if `Level` equals `LogLevel.Critical`; otherwise `false`.  
**Throws:** None.

### ToString
**Purpose:** Provides a human‑readable string representation of the log entry, useful for debugging or logging to a text sink.  
**Parameters:** None.  
**Return value:** A formatted string containing all non‑null fields (e.g., `Id`, `Timestamp`, `Level`, `Message`, etc.).  
**Throws:** None.

### AddMetadata
**Purpose:** Inserts a key‑value pair into the `Metadata` dictionary.  
**Parameters:**  
- `string key` – The metadata key.  
- `string value` – The metadata value.  
**Return value:** `void`.  
**Throws:**  
- `ArgumentNullException` if `key` or `value` is `null`.  
- `ArgumentException` if the dictionary implementation rejects duplicate keys (though `Dictionary<string, string>` permits overwriting; behavior depends on the underlying implementation).

### GetMetadata
**Purpose:** Retrieves the value associated with a specified key from the `Metadata` dictionary.  
**Parameters:**  
- `string key` – The metadata key to look up.  
**Return value:** The corresponding `string` value, or `null` if the key is not present.  
**Throws:**  
- `ArgumentNullException` if `key` is `null`.

## Usage
```csharp
// Example 1: Manual creation of a log entry
var entry = new LogEntry
{
    Id = 12345,
    ApplicationId = "my-app",
    Message = "Service started successfully.",
    Level = LogLevel.Information,
    Timestamp = DateTime.UtcNow,
    Source = "Startup",
    TraceId = "abcdef123456",
    ExitCode = 0
};

entry.AddMetadata("Environment", "Production");
entry.AddMetadata("Version", "2.3.1");

Console.WriteLine(entry.ToString());
// Output similar to:
// Id: 12345 | Timestamp: 2025-11-02 14:05:00Z | Level: Information | ApplicationId: my-app | Message: Service started successfully. | Source: Startup | TraceId: abcdef123456 | ExitCode: 0 | Metadata: [Environment=Production, Version=2.3.1]
```

```csharp
// Example 2: Creating a log entry from an exception and enriching it
try
{
    // Some operation that may fail
    throw new InvalidOperationException("Configuration missing.");
}
catch (Exception ex)
{
    var log = LogEntry.FromException(ex);
    log.ApplicationId = "config-loader";
    log.Source = "ConfigReader";
    log.AddMetadata("ConfigKey", "connectionString");
    log.IsCritical // true because Level is set to Error by FromException; adjust if needed

    // Persist or transmit the log entry
    Logger.Write(log);
}
```

## Notes
- The `Metadata` field is a mutable `Dictionary<string, string>`. Concurrent modifications from multiple threads without external synchronization can lead to undefined behavior or exceptions. Consumers should synchronize access if the `LogEntry` instance may be accessed concurrently.
- Nullable string fields (`Source`, `TraceId`, `StackTrace`) may be `null`; code that reads these members should handle the null case appropriately.
- `ExitCode` is nullable to allow scenarios where no exit code is relevant (e.g., logs not tied to a process execution).
- `IsCritical` is a read‑only derived property; it reflects the current value of `Level` and does not have a setter.
- `FromException` populates only `Message`, `StackTrace`, `Level` (set to `Error`), and `Timestamp`. All other fields retain their default values and must be set explicitly if needed.
- The `ToString` implementation does not guarantee a specific format beyond including all non‑null fields; consumers should not parse its output for machine‑consumption. Use the individual fields or `Metadata` for structured data.
