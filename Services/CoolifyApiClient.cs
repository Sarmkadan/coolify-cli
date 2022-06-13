using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Core HTTP client for Coolify API communication.
/// Handles authentication, request serialization, and error handling.
/// </summary>
public class CoolifyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly CoolifyApiClientOptions _options;

    public CoolifyApiClient(HttpClient httpClient, string baseUrl, string apiKey,
        CoolifyApiClientOptions? options = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? new CoolifyApiClientOptions();

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
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.GetTimeoutSeconds));
        try
        {
            using var response = await _httpClient.GetAsync(endpoint, cts.Token);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.ErrorResponse($"HTTP request failed: {ex.Message}", 500);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<T>.ErrorResponse("Request timeout exceeded.", 408);
        }
    }

    /// <summary>
    /// Performs a POST request with JSON body.
    /// Uses <see cref="CoolifyApiClientOptions.PostTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object content)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.PostTimeoutSeconds));
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, content, cts.Token);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.ErrorResponse($"HTTP request failed: {ex.Message}", 500);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<T>.ErrorResponse("Request timeout exceeded.", 408);
        }
    }

    /// <summary>
    /// Performs a PUT request with JSON body.
    /// Uses <see cref="CoolifyApiClientOptions.PutTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object content)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.PutTimeoutSeconds));
        try
        {
            using var response = await _httpClient.PutAsJsonAsync(endpoint, content, cts.Token);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.ErrorResponse($"HTTP request failed: {ex.Message}", 500);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<T>.ErrorResponse("Request timeout exceeded.", 408);
        }
    }

    /// <summary>
    /// Performs a DELETE request.
    /// Uses <see cref="CoolifyApiClientOptions.DeleteTimeoutSeconds"/> as the per-request timeout.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.DeleteTimeoutSeconds));
        try
        {
            using var response = await _httpClient.DeleteAsync(endpoint, cts.Token);
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.ErrorResponse($"HTTP request failed: {ex.Message}", 500);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<T>.ErrorResponse("Request timeout exceeded.", 408);
        }
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
                var data = JsonSerializer.Deserialize<T>(
                    contentAsString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
            var response = await _httpClient.GetAsync("/health", cts.Token);
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