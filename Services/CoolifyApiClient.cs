// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CoolifiCli.Models;

namespace CoolifiCli.Services;

/// <summary>
/// Core HTTP client for Coolify API communication.
/// Handles authentication, request serialization, and error handling.
/// </summary>
public class CoolifyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public CoolifyApiClient(HttpClient httpClient, string baseUrl, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CoolifiCli/1.0");
    }

    /// <summary>
    /// Performs a GET request to the specified endpoint.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            using var response = await _httpClient.GetAsync(endpoint);
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
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object content)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, content);
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
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="content">Request body content.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object content)
    {
        try
        {
            using var response = await _httpClient.PutAsJsonAsync(endpoint, content);
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
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    /// <param name="endpoint">API endpoint path.</param>
    /// <returns>API response with data.</returns>
    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(endpoint);
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

                return ApiResponse<T>.SuccessResponse(data);
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
    /// </summary>
    /// <returns>True if connection is successful.</returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            // In a real implementation, you might want to use a logger here
            System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
            return false;
        }
    }
}