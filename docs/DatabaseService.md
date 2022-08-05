# DatabaseService

The `DatabaseService` class provides a client interface for managing databases through the Coolify API. It encapsulates all CRUD operations, backup management, health checks, and connection testing, returning standardized `ApiResponse<T>` wrappers that include success status, data payloads, and error information.

## API

### `public DatabaseService`

Initializes a new instance of the `DatabaseService`. The constructor typically requires authentication credentials and a base URL for the Coolify API, though the exact parameters are implementation‑specific.

### `public async Task<ApiResponse<List<DatabaseConfiguration>>> GetAllDatabasesAsync()`

Retrieves all database configurations accessible to the authenticated user.

- **Parameters**: None.
- **Returns**: An `ApiResponse` containing a list of `DatabaseConfiguration` objects on success, or error details on failure.
- **Throws**: May throw `HttpRequestException` on network failures, or `AuthenticationException` if credentials are invalid.

### `public async Task<ApiResponse<DatabaseConfiguration>> GetDatabaseAsync(string id)`

Retrieves a single database configuration by its unique identifier.

- **Parameters**:
  - `id` (string): The identifier of the database.
- **Returns**: An `ApiResponse` containing the matching `DatabaseConfiguration`, or error details if not found.
- **Throws**: `ArgumentException` if `id` is null or empty; `HttpRequestException` on network issues.

### `public async Task<ApiResponse<DatabaseConfiguration>> CreateDatabaseAsync(DatabaseConfiguration configuration)`

Creates a new database configuration on the server.

- **Parameters**:
  - `configuration` (DatabaseConfiguration): The configuration object describing the database to create.
- **Returns**: An `ApiResponse` containing the created `DatabaseConfiguration` (including server‑assigned identifiers).
- **Throws**: `ArgumentNullException` if `configuration` is null; `ValidationException` if the configuration is invalid; `HttpRequestException` on network errors.

### `public async Task<ApiResponse<DatabaseConfiguration>> UpdateDatabaseAsync(string id, DatabaseConfiguration configuration)`

Updates an existing database configuration.

- **Parameters**:
  - `id` (string): The identifier of the database to update.
  - `configuration` (DatabaseConfiguration): The updated configuration values.
- **Returns**: An `ApiResponse` containing the updated `DatabaseConfiguration`.
- **Throws**: `ArgumentException` if `id` is null or empty; `ArgumentNullException` if `configuration` is null; `HttpRequestException` on network failures.

### `public async Task<ApiResponse<ServiceHealth>> CheckDatabaseHealthAsync(string id)`

Checks the current health status of a database.

- **Parameters**:
  - `id` (string): The identifier of the database.
- **Returns**: An `ApiResponse` containing a `ServiceHealth` object with status details (e.g., healthy, degraded, unreachable).
- **Throws**: `ArgumentException` if `id` is null or empty; `HttpRequestException` on network issues.

### `public async Task<ApiResponse<object>> BackupDatabaseAsync(string id)`

Triggers an immediate backup of the specified database.

- **Parameters**:
  - `id` (string): The identifier of the database to back up.
- **Returns**: An `ApiResponse` containing an `object` with backup metadata (e.g., backup ID, start time).
- **Throws**: `ArgumentException` if `id` is null or empty; `InvalidOperationException` if the database is not in a state that allows backups; `HttpRequestException` on network errors.

### `public async Task<ApiResponse<List<object>>> GetBackupHistoryAsync(string id)`

Retrieves the history of backups for a given database.

- **Parameters**:
  - `id` (string): The identifier of the database.
- **Returns**: An `ApiResponse` containing a list of backup records (each as an `object` with relevant properties).
- **Throws**: `ArgumentException` if `id` is null or empty; `HttpRequestException` on network failures.

### `public async Task<ApiResponse<object>> RestoreDatabaseAsync(string databaseId, string backupId)`

Restores a database from a specific backup.

- **Parameters**:
  - `databaseId` (string): The identifier of the database to restore.
  - `backupId` (string): The identifier of the backup to use.
- **Returns**: An `ApiResponse` containing an `object` with restoration status details.
- **Throws**: `ArgumentException` if either parameter is null or empty; `InvalidOperationException` if the database is currently in use; `HttpRequestException` on network issues.

### `public async Task<ApiResponse<bool>> TestConnectionAsync(string id)`

Tests the network connection to a database.

- **Parameters**:
  - `id` (string): The identifier of the database.
- **Returns**: An `ApiResponse` containing `true` if the connection succeeded, `false` otherwise.
- **Throws**: `ArgumentException` if `id` is null or empty; `HttpRequestException` on network failures.

### `public async Task<ApiResponse<object>> DeleteDatabaseAsync(string id)`

Deletes a database configuration and its associated resources.

- **Parameters**:
  - `id` (string): The identifier of the database to delete.
- **Returns**: An `ApiResponse` containing an `object` with deletion confirmation details.
- **Throws**: `ArgumentException` if `id` is null or empty; `InvalidOperationException` if the database cannot be deleted (e.g., it is still in use); `HttpRequestException` on network errors.

### `public async Task<ApiResponse<List<object>>> GetAvailableBackupsAsync(string id)`

Retrieves the list of backups that are currently available for restoration.

- **Parameters**:
  - `id` (string): The identifier of the database.
- **Returns**: An `ApiResponse` containing a list of available backup records (each as an `object`).
- **Throws**: `ArgumentException` if `id` is null or empty; `HttpRequestException` on network failures.

## Usage

### Example 1: List all databases and display their names

```csharp
var service = new DatabaseService(); // assumes configuration is provided via constructor
var response = await service.GetAllDatabasesAsync();

if (response.Success)
{
    foreach (var db in response.Data)
    {
        Console.WriteLine($"Database: {db.Name} (ID: {db.Id})");
    }
}
else
{
    Console.WriteLine($"Error: {response.ErrorMessage}");
}
```

### Example 2: Create a database, then check its health

```csharp
var service = new DatabaseService();
var newDb = new DatabaseConfiguration
{
    Name = "my-app-db",
    Type = "postgresql",
    Version = "15"
};

var createResponse = await service.CreateDatabaseAsync(newDb);
if (!createResponse.Success)
{
    Console.WriteLine($"Creation failed: {createResponse.ErrorMessage}");
    return;
}

var dbId = createResponse.Data.Id;
var healthResponse = await service.CheckDatabaseHealthAsync(dbId);
if (healthResponse.Success)
{
    Console.WriteLine($"Health status: {healthResponse.Data.Status}");
}
```

## Notes

- All methods are asynchronous and should be awaited. They are safe to call concurrently from multiple threads as long as the `DatabaseService` instance is not modified (e.g., by changing its internal configuration) during those calls.
- The `ApiResponse<T>` object always contains a `Success` boolean and an `ErrorMessage` string when `Success` is `false`. The `Data` property is `null` on failure.
- When a method accepts an identifier (`id`), passing `null` or an empty string will throw an `ArgumentException`. Always validate identifiers before calling.
- Backup‑related methods (`BackupDatabaseAsync`, `RestoreDatabaseAsync`, `GetBackupHistoryAsync`, `GetAvailableBackupsAsync`) return `object` for the data payload. The actual shape of these objects depends on the Coolify API version; cast or deserialize them to a known type when needed.
- Network errors, authentication failures, and server‑side validation errors are surfaced as exceptions or as `ApiResponse` failures depending on the implementation. Always check `Success` before accessing `Data`.
- The `TestConnectionAsync` method returns `bool` inside the response; a `true` value indicates a successful connection test, while `false` indicates a failure (the `ErrorMessage` may provide details).
