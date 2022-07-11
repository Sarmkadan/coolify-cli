# CoolifyApiClientOptions

Configuration class for the Coolify API client, providing timeout settings for different HTTP operations and a factory method to construct instances from application configuration.

## API

### `public int GetTimeoutSeconds`

Gets or sets the timeout in seconds for HTTP GET requests to the Coolify API. Defaults to 30 seconds if not explicitly configured.

### `public int PostTimeoutSeconds`

Gets or sets the timeout in seconds for HTTP POST requests to the Coolify API. Defaults to 30 seconds if not explicitly configured.

### `public int PutTimeoutSeconds`

Gets or sets the timeout in seconds for HTTP PUT requests to the Coolify API. Defaults to 30 seconds if not explicitly configured.

### `public int DeleteTimeoutSeconds`

Gets or sets the timeout in seconds for HTTP DELETE requests to the Coolify API. Defaults to 30 seconds if not explicitly configured.

### `public static CoolifyApiClientOptions FromConfiguration`

Constructs a `CoolifyApiClientOptions` instance by reading timeout values from the application configuration. The configuration keys expected are `Coolify:ApiClient:GetTimeoutSeconds`, `Coolify:ApiClient:PostTimeoutSeconds`, `Coolify:ApiClient:PutTimeoutSeconds`, and `Coolify:ApiClient:DeleteTimeoutSeconds`. If a key is missing, the corresponding timeout defaults to 30 seconds.

## Usage
