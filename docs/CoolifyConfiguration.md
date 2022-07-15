# CoolifyConfiguration
The `CoolifyConfiguration` type is used to configure the behavior of the Coolify CLI, allowing users to customize settings such as API endpoints, authentication, logging, and retry policies. This configuration object is essential for establishing a connection to the Coolify API and controlling the flow of requests.

## API
### Properties
* `ApiUrl`: The URL of the Coolify API endpoint. This property is a string and does not throw any exceptions.
* `ApiKey`: The API key used for authentication. This property is a nullable string and does not throw any exceptions.
* `RequestTimeoutSeconds`: The timeout in seconds for API requests. This property is an integer and does not throw any exceptions.
* `VerboseLogging`: A boolean indicating whether to enable verbose logging. This property does not throw any exceptions.
* `DefaultEnvironment`: The default environment to use. This property is a string and does not throw any exceptions.
* `AutoRetry`: A boolean indicating whether to automatically retry failed requests. This property does not throw any exceptions.
* `MaxRetries`: The maximum number of retries for failed requests. This property is an integer and does not throw any exceptions.
* `TrustedHosts`: A list of trusted hosts. This property is a list of strings and does not throw any exceptions.
* `Validate`: A collection of validation rules. This property is an enumerable of strings and does not throw any exceptions.
### Static Properties
* `FromEnvironment`: A static property that creates a `CoolifyConfiguration` instance from environment variables. This property returns a `CoolifyConfiguration` object and may throw exceptions if the environment variables are not set correctly.

## Usage
The following examples demonstrate how to use the `CoolifyConfiguration` type:
```csharp
// Example 1: Creating a CoolifyConfiguration instance
var config = new CoolifyConfiguration
{
    ApiUrl = "https://api.coolify.io",
    ApiKey = "my_api_key",
    RequestTimeoutSeconds = 30,
    VerboseLogging = true,
    DefaultEnvironment = "dev",
    AutoRetry = true,
    MaxRetries = 3,
    TrustedHosts = new List<string> { "https://api.coolify.io" },
    Validate = new[] { "rule1", "rule2" }
};

// Example 2: Using the FromEnvironment static property
var configFromEnvironment = CoolifyConfiguration.FromEnvironment;
Console.WriteLine(configFromEnvironment.ApiUrl);
```

## Notes
When using the `CoolifyConfiguration` type, consider the following edge cases:
* If `ApiKey` is null, authentication may fail.
* If `RequestTimeoutSeconds` is set too low, requests may timeout prematurely.
* If `AutoRetry` is enabled, requests may be retried multiple times, potentially leading to increased latency.
* The `TrustedHosts` list should only contain hosts that are trusted to handle sensitive data.
* The `Validate` collection should only contain validation rules that are relevant to the specific use case.
Regarding thread-safety, the `CoolifyConfiguration` type is not thread-safe by default. If multiple threads need to access the same `CoolifyConfiguration` instance, synchronization mechanisms should be used to prevent concurrent modifications.
