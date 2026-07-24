using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CoolifyCli.Http;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Core HTTP client for Coolify API communication.
/// Handles authentication, request serialization, and error handling.
/// Every request is wrapped in a <see cref="ResiliencePolicy"/> that retries transient
/// failures (408/429/5xx and connection errors) with exponential backoff and jitter, and
/// trips a circuit breaker after repeated consecutive failures so a Coolify outage does
/// not turn into a pile of hanging requests.
/// </summary>
public class CoolifyApiClient
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly CoolifyApiClientOptions _options;
    private readonly ResiliencePolicy _resilience;

    public CoolifyApiClient(HttpClient httpClient, string baseUrl, string apiKey,
        CoolifyApiClientOptions? options = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? new CoolifyApiClientOptions();
        _resilience = new ResiliencePolicy(failureThreshold: 5, breakDuration: TimeSpan.FromSeconds(30));

        // Disable the global HttpClient timeout; per-method CancellationTokenSources control timing.
        _httpClient.Timeout = Timeout.InfiniteTimeSpan;
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CoolifyCli/1.0");
    }

    /// <summary>
    /// Performs a GET request to the specified endpoint.
    /// Uses <see cref="CoolifyApiClientOptions.GetTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <returns>API response with data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is null or empty.</exception>
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        return await ExecuteWithResilienceAsync<T>(
            TimeSpan.FromSeconds(_options.GetTimeoutSeconds),
            token => _httpClient.GetAsync(endpoint, token));
    }

    /// <summary>
    /// Performs a POST request with JSON body.
    /// Uses <see cref="CoolifyApiClientOptions.PostTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object content)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNull(content);

        return await ExecuteWithResilienceAsync<T>(
            TimeSpan.FromSeconds(_options.PostTimeoutSeconds),
            token => _httpClient.PostAsJsonAsync(endpoint, content, token));
    }

    /// <summary>
    /// Performs a PUT request with JSON body.
    /// Uses <see cref="CoolifyApiClientOptions.PutTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object content)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNull(content);

        return await ExecuteWithResilienceAsync<T>(
            TimeSpan.FromSeconds(_options.PutTimeoutSeconds),
            token => _httpClient.PutAsJsonAsync(endpoint, content, token));
    }

    /// <summary>
    /// Performs a DELETE request.
    /// Uses <see cref="CoolifyApiClientOptions.DeleteTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <returns>API response with data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is null or empty.</exception>
    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        return await ExecuteWithResilienceAsync<T>(
            TimeSpan.FromSeconds(_options.DeleteTimeoutSeconds),
            token => _httpClient.DeleteAsync(endpoint, token));
    }

    /// <summary>
    /// Runs a single HTTP call through the <see cref="ResiliencePolicy"/>: retries on
    /// 408/429/5xx responses and <see cref="HttpRequestException"/> with exponential
    /// backoff and jitter (honouring <c>Retry-After</c> on 429), applies
    /// <paramref name="perRequestTimeout"/> to each individual attempt, and enforces the
    /// circuit breaker across calls made through this client instance.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="perRequestTimeout">Timeout applied to each attempt.</param>
    /// <param name="send">Issues one HTTP attempt given the attempt's cancellation token.</param>
    /// <returns>The deserialized API response, or a clear unreachable error after retries/circuit-breaker rejection.</returns>
    private async Task<ApiResponse<T>> ExecuteWithResilienceAsync<T>(
        TimeSpan perRequestTimeout,
        Func<CancellationToken, Task<HttpResponseMessage>> send)
    {
        return await _resilience.ExecuteAsync(
            async token =>
            {
                using var response = await send(token);
                var processed = await ProcessResponse<T>(response);

                return ResiliencePolicy.IsRetryableStatusCode(response.StatusCode)
                    ? ResilientOutcome<ApiResponse<T>>.Retry(processed, GetRetryAfter(response))
                    : ResilientOutcome<ApiResponse<T>>.Final(processed);
            },
            perRequestTimeout,
            reason => ApiResponse<T>.ErrorResponse(reason, 503));
    }

    /// <summary>
    /// Extracts the server-requested delay from a <c>Retry-After</c> header, supporting
    /// both the delta-seconds and HTTP-date forms.
    /// </summary>
    /// <param name="response">The HTTP response to inspect.</param>
    /// <returns>The requested delay, or <c>null</c> when no valid <c>Retry-After</c> header is present.</returns>
    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta.HasValue)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter.Date.HasValue)
        {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return null;
    }

    /// <summary>
    /// Processes HTTP response and deserializes API response.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="response">HTTP response message.</param>
    /// <returns>Deserialized API response.</returns>
    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        try
        {
            var contentAsString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(contentAsString, DeserializeOptions);

                return ApiResponse<T>.SuccessResponse(data!);
            }
            else
            {
                return ApiResponse<T>.ErrorResponse(
                    $"API error: {contentAsString}",
                    (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse(
                $"Failed to process response: {ex.Message}",
                500);
        }
    }

    /// <summary>
    /// Tests the connection to the Coolify API.
    /// Returns false (rather than throwing) when the server is unreachable or the request times out.
    /// Uses <see cref="CoolifyApiClientOptions.GetTimeoutSeconds"/> as the timeout.
    /// </summary>
    /// <returns>True if connection is successful.</returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.GetTimeoutSeconds));
            using var response = await _httpClient.GetAsync("/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
            return false;
        }
    }
}