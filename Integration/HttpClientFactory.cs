// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Services;

namespace CoolifiCli.Integration;

/// <summary>
/// Factory for creating configured HttpClient instances with authentication and interceptors.
/// Centralizes HTTP client configuration to ensure consistent behavior across API calls.
/// Implements retry policies, timeouts, and custom headers.
/// </summary>
public class HttpClientFactory
{
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly ILogger _logger;
    private readonly int _timeoutSeconds;

    public HttpClientFactory(string apiKey, string apiUrl, ILogger logger, int timeoutSeconds = 30)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));

        if (string.IsNullOrWhiteSpace(apiUrl))
            throw new ArgumentException("API URL cannot be empty", nameof(apiUrl));

        _apiKey = apiKey;
        _apiUrl = apiUrl;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// Creates a new HttpClient with default configuration.
    /// Includes authentication headers and timeout settings.
    /// </summary>
    public HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds),
            BaseAddress = new Uri(_apiUrl)
        };

        ConfigureDefaultHeaders(client);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient with a custom message handler (for interceptors, mocking, etc.).
    /// </summary>
    public HttpClient CreateClient(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds),
            BaseAddress = new Uri(_apiUrl)
        };

        ConfigureDefaultHeaders(client);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient with automatic retry policy.
    /// Retries on transient failures (5xx, 408, 429).
    /// </summary>
    public HttpClient CreateClientWithRetry(int maxRetries = 3)
    {
        var handler = new RetryHandler(maxRetries, _logger);
        return CreateClient(handler);
    }

    /// <summary>
    /// Creates an HttpClient with authentication interceptor.
    /// Automatically adds API key to requests.
    /// </summary>
    public HttpClient CreateClientWithInterceptor()
    {
        var handler = new AuthenticationInterceptor(_apiKey, _logger);
        return CreateClient(handler);
    }

    /// <summary>
    /// Creates an HttpClient with both retry and authentication.
    /// </summary>
    public HttpClient CreateClientWithFullConfiguration(int maxRetries = 3)
    {
        var handler = new AuthenticationInterceptor(_apiKey, _logger);
        var retryHandler = new RetryHandler(maxRetries, _logger)
        {
            InnerHandler = handler
        };

        return CreateClient(retryHandler);
    }

    /// <summary>
    /// Configures default HTTP headers on the client.
    /// </summary>
    private void ConfigureDefaultHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "CoolifyCliClient/1.0");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Content-Type", "application/json");
    }

    /// <summary>
    /// HTTP message handler that implements retry logic with exponential backoff.
    /// </summary>
    private class RetryHandler : DelegatingHandler
    {
        private readonly int _maxRetries;
        private readonly ILogger _logger;

        public RetryHandler(int maxRetries, ILogger logger)
        {
            _maxRetries = maxRetries;
            _logger = logger;
            InnerHandler = new HttpClientHandler();
        }

        /// <summary>
        /// Sends request with automatic retry on transient failures.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    var response = await base.SendAsync(request, cancellationToken);

                    // Retry on specific status codes
                    if (response.IsSuccessStatusCode || !IsTransientFailure(response.StatusCode))
                    {
                        return response;
                    }

                    if (attempt < _maxRetries)
                    {
                        var delay = (int)Math.Pow(2, attempt) * 1000; // Exponential backoff
                        _logger.Warn($"Transient failure ({(int)response.StatusCode}), retrying in {delay}ms (attempt {attempt + 1}/{_maxRetries})");
                        await Task.Delay(delay, cancellationToken);
                    }
                }
                catch (HttpRequestException ex) when (attempt < _maxRetries)
                {
                    _logger.Warn($"Request failed ({ex.Message}), retrying in 1000ms (attempt {attempt + 1}/{_maxRetries})");
                    await Task.Delay(1000, cancellationToken);
                }
            }

            // Final attempt without retry
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Determines if a status code represents a transient failure that should be retried.
        /// </summary>
        private bool IsTransientFailure(System.Net.HttpStatusCode statusCode)
        {
            return statusCode == System.Net.HttpStatusCode.RequestTimeout ||           // 408
                   statusCode == System.Net.HttpStatusCode.TooManyRequests ||          // 429
                   (int)statusCode >= 500;                                             // 5xx
        }
    }

    /// <summary>
    /// HTTP message handler that adds authentication and logging to requests.
    /// </summary>
    private class AuthenticationInterceptor : DelegatingHandler
    {
        private readonly string _apiKey;
        private readonly ILogger _logger;

        public AuthenticationInterceptor(string apiKey, ILogger logger)
        {
            _apiKey = apiKey;
            _logger = logger;
            InnerHandler = new HttpClientHandler();
        }

        /// <summary>
        /// Intercepts requests to add authentication headers and log activity.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            _logger.Debug($"API Request: {request.Method} {request.RequestUri}");

            var response = await base.SendAsync(request, cancellationToken);

            _logger.Debug($"API Response: {(int)response.StatusCode} {response.StatusCode}");

            return response;
        }
    }
}
