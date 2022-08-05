# CoolifyApiClient

`CoolifyApiClient` is the primary HTTP client wrapper for interacting with a Coolify instance's REST API. It encapsulates authentication, serialization, and error handling, exposing typed asynchronous methods for the standard CRUD operations (`GET`, `POST`, `PUT`, `DELETE`) and a connectivity check. All responses are wrapped in a generic `ApiResponse<T>` envelope that carries the deserialized payload, status information, and any error context.

## API

### `CoolifyApiClient`

Constructor for the client. Initializes the underlying HTTP handler, configures base addressing, and sets up authentication headers or tokens required by the Coolify API. The exact parameters (e.g., base URL, API key) are implementation-specific and should be supplied according to the target Coolify instance configuration.

### `GetAsync<T>`

```csharp
public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
```

Performs an HTTP `GET` request against the specified relative `endpoint` and deserializes a successful JSON response body into an instance of `T`.

- **Parameters:**
  - `endpoint` (`string`): The relative URL path (e.g., `"projects"`, `"servers/1"`). Must not be `null` or empty.
- **Returns:** `Task<ApiResponse<T>>` — an `ApiResponse<T>` containing the deserialized data on success, or error details on failure.
- **Exceptions:** Throws `ArgumentNullException` when `endpoint` is `null`. Throws `HttpRequestException` for network-level failures. Throws `JsonException` when the response body cannot be deserialized to `T`.

### `PostAsync<T>`

```csharp
public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null)
```

Performs an HTTP `POST` request to the specified `endpoint`, optionally sending a JSON-serialized request `body`. Deserializes the response into `T`.

- **Parameters:**
  - `endpoint` (`string`): The relative URL path. Must not be `null` or empty.
  - `body` (`object?`): An optional object to serialize as the JSON request body. `null` sends an empty body.
- **Returns:** `Task<ApiResponse<T>>` — the wrapped response.
- **Exceptions:** Throws `ArgumentNullException` when `endpoint` is `null`. Throws `HttpRequestException` for transport errors. Throws `JsonException` on serialization or deserialization failures.

### `PutAsync<T>`

```csharp
public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? body = null)
```

Performs an HTTP `PUT` request to the specified `endpoint`, optionally sending a JSON-serialized request `body`. Deserializes the response into `T`.

- **Parameters:**
  - `endpoint` (`string`): The relative URL path. Must not be `null` or empty.
  - `body` (`object?`): An optional object to serialize as the JSON request body. `null` sends an empty body.
- **Returns:** `Task<ApiResponse<T>>` — the wrapped response.
- **Exceptions:** Throws `ArgumentNullException` when `endpoint` is `null`. Throws `HttpRequestException` for transport errors. Throws `JsonException` on serialization or deserialization failures.

### `DeleteAsync<T>`

```csharp
public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
```

Performs an HTTP `DELETE` request against the specified `endpoint`. Deserializes the response body into `T` (often an acknowledgment or status object).

- **Parameters:**
  - `endpoint` (`string`): The relative URL path. Must not be `null` or empty.
- **Returns:** `Task<ApiResponse<T>>` — the wrapped response.
- **Exceptions:** Throws `ArgumentNullException` when `endpoint` is `null`. Throws `HttpRequestException` for network-level failures. Throws `JsonException` when the response body cannot be deserialized to `T`.

### `TestConnectionAsync`

```csharp
public async Task<bool> TestConnectionAsync()
```

Sends a lightweight probe request to the Coolify API (typically a `GET` against a health-check or root endpoint) to verify that the configured base URL is reachable and the authentication credentials are valid.

- **Parameters:** None.
- **Returns:** `Task<bool>` — `true` if the API responds with a successful status code; `false` otherwise.
- **Exceptions:** Does not throw for standard HTTP error responses (returns `false`). May throw `HttpRequestException` for DNS resolution failures or network timeouts if the underlying handler is configured to do so.

## Usage

### Example 1: Fetching a list of projects

```csharp
using CoolifyCli;

var client = new CoolifyApiClient("https://coolify.example.com", "your-api-token");

ApiResponse<List<Project>> response = await client.GetAsync<List<Project>>("projects");

if (response.IsSuccess)
{
    foreach (var project in response.Data)
    {
        Console.WriteLine($"Project: {project.Name} (UUID: {project.Uuid})");
    }
}
else
{
    Console.WriteLine($"Failed to fetch projects: {response.ErrorMessage}");
}
```

### Example 2: Creating a new resource and checking connectivity

```csharp
using CoolifyCli;

var client = new CoolifyApiClient("https://coolify.example.com", "your-api-token");

// Verify connectivity before proceeding
bool isConnected = await client.TestConnectionAsync();
if (!isConnected)
{
    Console.WriteLine("Coolify instance is unreachable or credentials are invalid.");
    return;
}

var newServer = new { Name = "production-web", Description = "Main production server" };
ApiResponse<Server> createResponse = await client.PostAsync<Server>("servers", newServer);

if (createResponse.IsSuccess)
{
    Console.WriteLine($"Server created with UUID: {createResponse.Data.Uuid}");
}
else
{
    Console.WriteLine($"Creation failed: {createResponse.ErrorMessage}");
}
```

## Notes

- **Thread Safety:** The client is designed to be instantiated once and reused across multiple requests. Concurrent calls to its async methods are safe; however, the underlying `HttpClient` instance should not be disposed while operations are in flight. Avoid creating a new `CoolifyApiClient` per request in high-throughput scenarios to prevent socket exhaustion.
- **Serialization Behavior:** All methods that accept a `body` parameter perform JSON serialization using the client's configured serializer settings. Ensure that passed objects are composed of types and property shapes compatible with `System.Text.Json` (or the configured alternative) to avoid runtime `JsonException`.
- **Error Handling:** Non-success HTTP status codes (4xx, 5xx) populate the `ApiResponse<T>` error fields rather than throwing. Callers must inspect `IsSuccess` or equivalent properties before accessing `Data`. Network-level failures (DNS, timeout, connection reset) surface as `HttpRequestException`.
- **Endpoint Format:** The `endpoint` parameter is relative to the configured base URL. Leading slashes are normalized internally; supplying `"projects"` and `"/projects"` are typically equivalent. Query strings must be included in the endpoint string directly (e.g., `"projects?page=2"`).
- **`TestConnectionAsync` Semantics:** This method returns `false` for any non-success response, including `401 Unauthorized` and `403 Forbidden`. It is a best-effort probe and does not guarantee that subsequent authenticated calls will succeed if permissions change between calls.
