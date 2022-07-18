# CoolifyException

The `CoolifyException` class is a base exception type used throughout the `coolify-cli` project to standardize error handling. It provides structured error information through contextual data, error codes, and HTTP status codes, enabling consistent error reporting and processing in CLI operations.

## API

### `public string? ErrorCode`
A machine-readable error identifier used to categorize exceptions. Typically a short uppercase string (e.g., `"CONFIG_ERROR"`, `"API_ERROR"`). This value is automatically set by derived exception constructors and can be used for error handling logic.

### `public Dictionary<string, string> ContextData`
A dictionary containing additional context about the error. Use `AddContextData` to populate this with key-value pairs relevant to debugging or user feedback. The keys should be descriptive of the data being stored (e.g., `"filePath"`, `"apiEndpoint"`).

### `public CoolifyException()`
Default constructor. Initializes a new instance of the `CoolifyException` class with a default error message and no context data.

### `public CoolifyException(string message)`
Constructs a new `CoolifyException` with the specified error message. The `ErrorCode` is set to `null` unless overridden by a derived class.

### `public void AddContextData(string key, string value)`
Adds a key-value pair to the `ContextData` dictionary. If the key already exists, its value is overwritten. This method is thread-safe for concurrent calls.

### `public ConfigurationException(string message) : base(message, "CONFIG_ERROR")`
Constructs a `ConfigurationException` with the specified message. Sets the `ErrorCode` to `"CONFIG_ERROR"` to indicate a configuration-related failure.

### `public ConfigurationException`
Default constructor for `ConfigurationException`. Initializes a new instance with a default message and `"CONFIG_ERROR"` error code.

### `public int? HttpStatusCode`
An optional HTTP status code associated with the exception, typically used for API-related errors. Derived exceptions may set this value to indicate the HTTP response status (e.g., `404`, `500`).

### `public ApiCommunicationException`
Default constructor for `ApiCommunicationException`. Initializes a new instance with a default message and `"API_ERROR"` error code.

### `public ApiCommunicationException(string message)`
Constructs an `ApiCommunicationException` with the specified message. Sets the `ErrorCode` to `"API_ERROR"` to indicate an API communication failure.

### `public int StatusCode`
An integer status code associated with the exception, often used to mirror HTTP or application-specific status values. Derived exceptions may populate this field to provide structured error details.

### `public string? ApiErrorCode`
A machine-readable error code returned by an external API. Used in `ApiException` and derived types to propagate API-specific error identifiers for further processing.

### `public ApiException`
Default constructor for `ApiException`. Initializes a new instance with a default message and `"API_ERROR"` error code.

### `public ApplicationNotFoundException`
Default constructor for `ApplicationNotFoundException`. Initializes a new instance with a default message and `"APP_NOT_FOUND"` error code.

### `public DatabaseNotFoundException`
Default constructor for `DatabaseNotFoundException`. Initializes a new instance with a default message and `"DB_NOT_FOUND"` error code.

### `public string? DeploymentId`
A unique identifier for a deployment, typically set by exceptions related to deployment operations (e.g., `DeploymentException`). Used to correlate errors with specific deployments in logs or monitoring systems.

### `public DeploymentException`
Default constructor for `DeploymentException`. Initializes a new instance with a default message and `"DEPLOYMENT_ERROR"` error code.

### `public TimeSpan Timeout`
A time duration indicating how long an operation was allowed to run before timing out. Used in `OperationTimeoutException` to specify the timeout period that was exceeded.

### `public OperationTimeoutException`
Default constructor for `OperationTimeoutException`. Initializes a new instance with a default message and `"TIMEOUT_ERROR"` error code.

### `public List<string> ValidationErrors`
A list of validation error messages. Used in exceptions related to input or configuration validation to collect multiple error messages in a single exception instance.

## Usage
