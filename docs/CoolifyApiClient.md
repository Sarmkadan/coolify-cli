# CoolifyApiClient

Core HTTP client for Coolify API communication. Handles authentication, request serialization, and error handling.

## Overview

Every request is wrapped in a `ResiliencePolicy` that retries transient failures (408/429/5xx and connection errors) with exponential backoff and jitter, and trips a circuit breaker after repeated consecutive failures so a Coolify outage does not turn into a pile of hanging requests.

## Constructor

```csharp
public CoolifyApiClient(HttpClient httpClient, string baseUrl, string apiKey, CoolifyApiClientOptions? options = null)
```

### Parameters

- `httpClient`: HTTP client used to send API requests.
- `baseUrl`: Absolute HTTP or HTTPS URL of the Coolify API.
- `apiKey`: API key used to authenticate requests.
- `options`: Optional API client settings (defaults to new `CoolifyApiClientOptions()` if null).

### Exceptions

- `ArgumentNullException`: Thrown when `httpClient` is null.
- `ArgumentException`: Thrown when `baseUrl` or `apiKey` is null or white space, or when `baseUrl` is not an absolute HTTP or HTTPS URI.

## Configuration

The client configures the provided `HttpClient` as follows:
- Sets `Timeout` to `InfiniteTimeSpan` (per-method `CancellationTokenSource` controls timing).
- Sets `BaseAddress` to the provided `baseUrl` (ensuring trailing slash).
- Adds default headers:
  - `X-API-Key`: the provided `apiKey`
  - `User-Agent`: "CoolifyCli/1.0"

## Public Methods

### GetAsync<T>

```csharp
public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
```

Performs a GET request to the specified endpoint.

#### Parameters

- `endpoint`: API endpoint path.

#### Returns

- `ApiResponse<T>`: API response with data.

#### Exceptions

- `ArgumentException`: Thrown when `endpoint` is null or empty.

#### Timeout

Uses `CoolifyApiClientOptions.GetTimeoutSeconds` as the per-request timeout.

### PostAsync<T>

```csharp
public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object content)
```

Performs a POST request with JSON body.

#### Parameters

- `endpoint`: API endpoint path.
- `content`: Request body content.

#### Returns

- `ApiResponse<T>`: API response with data.

#### Exceptions

- `ArgumentException`: Thrown when `endpoint` is null or empty.
- `ArgumentNullException`: Thrown when `content` is null.

#### Timeout

Uses `CoolifyApiClientOptions.PostTimeoutSeconds` as the per-request timeout.

### PutAsync<T>

```csharp
public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object content)
```

Performs a PUT request with JSON body.

#### Parameters

- `endpoint`: API endpoint path.
- `content`: Request body content.

#### Returns

- `ApiResponse<T>`: API response with data.

#### Exceptions

- `ArgumentException`: Thrown when `endpoint` is null or empty.
- `ArgumentNullException`: Thrown when `content` is null.

#### Timeout

Uses `CoolifyApiClientOptions.PutTimeoutSeconds` as the per-request timeout.

### DeleteAsync<T>

```csharp
public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
```

Performs a DELETE request.

#### Parameters

- `endpoint`: API endpoint path.

#### Returns

- `ApiResponse<T>`: API response with data.

#### Exceptions

- `ArgumentException`: Thrown when `endpoint` is null or empty.

#### Timeout

Uses `CoolifyApiClientOptions.DeleteTimeoutSeconds` as the per-request timeout.

### TestConnectionAsync

```csharp
public async Task<bool> TestConnectionAsync()
```

Tests the connection to the Coolify API.

#### Returns

- `True` if connection is successful; `false` if the server is unreachable or the request times out.

#### Notes

- Returns false (rather than throwing) when the server is unreachable or the request times out.
- Uses `CoolifyApiClientOptions.GetTimeoutSeconds` as the timeout.
- Endpoint: `/health`

## Resilience Behavior

### Retry Policy

- Retries on HTTP status codes: 408, 429, and 5xx.
- Retries on `HttpRequestException` (connection errors).
- Uses exponential backoff with jitter.
- Honors `Retry-After` header (both delta-seconds and HTTP-date forms) on 429 responses.

### Circuit Breaker

- Trips after `DefaultFailureThreshold` (5) consecutive failures.
- Remains open for `DefaultBreakDuration` (30 seconds).
- While open, returns a 503 error without attempting the request.
- After the break duration, allows a single trial request to test if the service has recovered.

## Options

The client behavior can be customized via `CoolifyApiClientOptions` (not shown in this file, but referenced):

- `GetTimeoutSeconds`: Timeout for GET requests (used by `GetAsync<T>` and `TestConnectionAsync`).
- `PostTimeoutSeconds`: Timeout for POST requests.
- `PutTimeoutSeconds`: Timeout for PUT requests.
- `DeleteTimeoutSeconds`: Timeout for DELETE requests.

## Response Handling

All methods return an `ApiResponse<T>` object which indicates success or failure:

- On success: Contains deserialized response data of type `T`.
- On failure: Contains an error message and HTTP status code (or 503 for circuit breaker/open, 500 for processing failures).

JSON deserialization uses case-insensitive property matching.

## Thread Safety

The client is thread-safe for concurrent use by multiple threads.