# ResourceMonitorService

Provides asynchronous monitoring and retrieval of system resource usage metrics such as CPU, memory, and disk usage for targets managed by the `coolify-cli` tool.

## API

### `ResourceMonitorService`

Public constructor for the resource monitoring service. Initializes a new instance configured to monitor system resources.

### `GetResourceUsageAsync`

Asynchronously retrieves the current resource usage for the target system.

- **Returns**: `Task<ApiResponse<ResourceUsage>>`
  A task that represents the asynchronous operation. The task result contains an `ApiResponse` wrapping the `ResourceUsage` object with the current system metrics.

- **Exceptions**:
  Throws if the underlying monitoring system is unavailable or if the request to the resource provider fails.

### `GetBulkResourceUsageAsync`

Asynchronously retrieves resource usage metrics for multiple targets in a single batch.

- **Returns**: `Task<List<ResourceUsage>>`
  A task that represents the asynchronous operation. The task result contains a list of `ResourceUsage` objects, one for each monitored target.

- **Exceptions**:
  Throws if any target is unreachable or if the batch request fails.

### `MonitorAsync`

Asynchronously streams real-time resource usage metrics for the target system.

- **Returns**: `IAsyncEnumerable<ResourceUsage>`
  An asynchronous enumerable that yields `ResourceUsage` objects at regular intervals.

- **Exceptions**:
  Throws if the monitoring stream cannot be established or if the underlying system fails during monitoring.

### `RenderUsageLine`

Renders a single line of resource usage information to the console.

- **Parameters**:
  - `usage`: `ResourceUsage` – The resource usage data to render.

- **Exceptions**:
  Throws `ArgumentNullException` if `usage` is `null`.

### `RenderHeader`

Renders a header line for resource usage output to the console.

## Usage
