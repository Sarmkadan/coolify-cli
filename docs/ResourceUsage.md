# ResourceUsage

Represents runtime resource consumption metrics for a single application instance, including CPU, memory, network, and file handle usage, along with contextual metadata and alerting capabilities.

## API

### `public int ApplicationId`
Unique identifier for the application being monitored. Used to correlate metrics with application configuration and identity.

### `public string ApplicationName`
Human-readable name of the application. Provided for display purposes and to aid in identifying metrics in logs or dashboards.

### `public DateTime CapturedAt`
Timestamp indicating when the resource metrics were collected. Useful for time-series analysis and trend detection.

### `public double CpuPercent`
Current CPU usage percentage of the application process. Value is between 0.0 and 100.0, representing the proportion of available CPU time consumed.

### `public double MemoryMb`
Current memory usage of the application process in megabytes. Represents the resident set size (RSS) or equivalent platform-specific metric.

### `public double MemoryLimitMb`
Configured memory limit for the application in megabytes. Used to determine if the application is approaching or exceeding its allocated memory.

### `public long NetworkRxBytes`
Total number of bytes received by the application over the network since startup or last reset. Useful for monitoring data ingestion patterns.

### `public long NetworkTxBytes`
Total number of bytes transmitted by the application over the network since startup or last reset. Useful for monitoring data output patterns.

### `public int OpenFileHandles`
Number of open file descriptors currently held by the application process. High values may indicate resource leaks or improper cleanup.

### `public int ThreadCount`
Number of active threads in the application process. Elevated thread counts may indicate performance issues or inefficient concurrency patterns.

### `public SeverityLevel? GetAlertSeverity()`
Determines the appropriate alert severity based on current resource usage thresholds.

- **Return value**: A `SeverityLevel` value indicating the highest severity level triggered by the current metrics, or `null` if no thresholds are exceeded.
- **Behavior**: Evaluates CPU, memory, file handles, and thread count against configurable thresholds to determine alert state.

### `public string ToSummaryLine()`
Generates a concise, human-readable summary of the resource usage.

- **Return value**: A single-line string containing key metrics (e.g., "App: webapp | CPU: 45.2% | Mem: 128/256 MB | Threads: 12 | Files: 8").
- **Format**: Designed for logging or console output where brevity is preferred over detail.

## Usage
