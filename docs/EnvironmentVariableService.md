# EnvironmentVariableService

The `EnvironmentVariableService` provides methods to manage environment variables for applications, including retrieval, creation, updating, deletion, and validation. It interacts with an API to perform these operations and returns structured responses that include success status and payloads.

## API

### `public EnvironmentVariableService`

Constructor for the service. Initializes the client or dependencies required to communicate with the environment variable management API.

### `public async Task<ApiResponse<List<EnvironmentVariable>>> GetApplicationVariablesAsync`

Retrieves all environment variables associated with a specific application. The variables are returned as a list in the response payload.

- **Parameters**: None.
- **Return value**: An `ApiResponse<List<EnvironmentVariable>>` containing the list of variables if successful.
- **Exceptions**: May throw if the API request fails or the application identifier is invalid.

### `public async Task<ApiResponse<EnvironmentVariable>> GetVariableAsync`

Fetches a single environment variable by its unique identifier.

- **Parameters**: None (relies on internal context or identifier resolution).
- **Return value**: An `ApiResponse<EnvironmentVariable>>` containing the variable if found.
- **Exceptions**: May throw if the variable does not exist or the request fails.

### `public async Task<ApiResponse<EnvironmentVariable>> CreateVariableAsync`

Creates a new environment variable for an application.

- **Parameters**: None (relies on internal context or DTOs passed via service setup).
- **Return value**: An `ApiResponse<EnvironmentVariable>>` containing the created variable.
- **Exceptions**: May throw if validation fails or the variable already exists.

### `public async Task<ApiResponse<EnvironmentVariable>> UpdateVariableAsync`

Updates an existing environment variable.

- **Parameters**: None (relies on internal context or identifier resolution).
- **Return value**: An `ApiResponse<EnvironmentVariable>>` containing the updated variable.
- **Exceptions**: May throw if the variable does not exist or validation fails.

### `public async Task<ApiResponse<object>> DeleteVariableAsync`

Removes an environment variable by its identifier.

- **Parameters**: None (relies on internal context or identifier resolution).
- **Return value**: An `ApiResponse<object>>` indicating success or failure.
- **Exceptions**: May throw if the variable does not exist or deletion is not permitted.

### `public async Task<ApiResponse<object>> BulkUpdateVariablesAsync`

Applies multiple updates to environment variables in a single operation.

- **Parameters**: None (relies on internal context or batch DTOs).
- **Return value**: An `ApiResponse<object>>` indicating success or failure of the batch.
- **Exceptions**: May throw if any update fails or validation is violated.

### `public async Task<ApiResponse<List<EnvironmentVariable>>> GetVariablesByScopeAsync`

Retrieves environment variables filtered by a specific scope (e.g., application, team, global).

- **Parameters**: None (relies on internal scope resolution).
- **Return value**: An `ApiResponse<List<EnvironmentVariable>>>` containing the filtered variables.
- **Exceptions**: May throw if the scope is invalid or the request fails.

### `public async Task<ApiResponse<object>> RotateSecretsAsync`

Triggers a rotation of all secret environment variables for enhanced security.

- **Parameters**: None.
- **Return value**: An `ApiResponse<object>>` indicating success or failure.
- **Exceptions**: May throw if rotation is not supported or fails.

### `public async Task<ApiResponse<List<object>>> GetChangeHistoryAsync`

Retrieves a chronological history of changes made to environment variables.

- **Parameters**: None.
- **Return value**: An `ApiResponse<List<object>>>` containing change records.
- **Exceptions**: May throw if the history is unavailable or the request fails.

### `public async Task<ApiResponse<object>> ValidateVariablesAsync`

Validates the current set of environment variables for correctness and compliance.

- **Parameters**: None.
- **Return value**: An `ApiResponse<object>>` indicating validation success or failure.
- **Exceptions**: May throw if validation logic encounters an unrecoverable error.

## Usage
