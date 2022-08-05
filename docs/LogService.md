# LogService

The `LogService` class provides asynchronous methods to retrieve, search, filter, stream, and export application and database logs from Coolify. It encapsulates HTTP-based interactions with the Coolify API to manage log data efficiently.

## API

### `GetApplicationLogsAsync`

Retrieves all logs for the current application from Coolify.

- **Parameters**: None
- **Return value**: `Task<ApiResponse<List<LogEntry>>>` containing a list of `LogEntry` objects or an error response.
- **Exceptions**: Throws if the underlying HTTP request fails or the API returns an error status.

### `SearchLogsAsync`

Searches application logs using a keyword query.

- **Parameters**: None
- **Return value**: `Task<ApiResponse<List<LogEntry>>>` containing filtered `LogEntry` objects matching the search term, or an error response.
- **Exceptions**: Throws if the search request fails or the API returns an error status.

### `GetLogsByLevelAsync`

Retrieves application logs filtered by severity level (e.g., INFO, ERROR).

- **Parameters**: None
- **Return value**: `Task<ApiResponse<List<LogEntry>>>` containing logs of the specified level, or an error response.
- **Exceptions**: Throws if the level-based query fails or the API returns an error status.

### `GetLogsByTimeRangeAsync`

Fetches application logs within a specified time range.

- **Parameters**: None
- **Return value**: `Task<ApiResponse<List<LogEntry>>>` containing logs within the time range, or an error response.
- **Exceptions**: Throws if the time range request fails or the API returns an error status.

### `StreamLogsAsync`

Streams application logs in real-time as they are generated.

- **Parameters**: None
- **Return value**: `IAsyncEnumerable<LogEntry>` that yields log entries as they arrive.
- **Exceptions**: Throws if the streaming connection cannot be established or the API returns an error status.

### `GetDatabaseLogsAsync`

Retrieves all logs for the current database from Coolify.

- **Parameters**: None
- **Return value**: `Task<ApiResponse<List<LogEntry>>>` containing a list of `LogEntry` objects or an error response.
- **Exceptions**: Throws if the database log request fails or the API returns an error status.

### `ExportLogsAsync`

Exports logs (application or database) in a structured format (e.g., JSON).

- **Parameters**: None
- **Return value**: `Task<ApiResponse<object>>` containing the exported data or an error response.
- **Exceptions**: Throws if the export request fails or the API returns an error status.

## Usage
