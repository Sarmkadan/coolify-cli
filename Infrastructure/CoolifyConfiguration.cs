#nullable enable
namespace CoolifyCli.Infrastructure;

/// <summary>
/// Configuration holder for Coolify API connection and CLI settings.
/// </summary>
public class CoolifyConfiguration
{
    /// <summary>
    /// Gets or sets the base URL for the Coolify API.
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.coolify.io";
    /// <summary>
    /// Gets or sets the API key for authenticating with the Coolify API.
    /// </summary>
    public string? ApiKey { get; set; }
    /// <summary>
    /// Gets or sets the request timeout in seconds for API calls.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
    /// <summary>
    /// Gets or sets whether verbose logging is enabled.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
    /// <summary>
    /// Gets or sets whether quiet logging is enabled (suppresses non-essential output).
    /// </summary>
public bool QuietLogging { get; set; } = false;
    /// <summary>
    /// Gets or sets the default environment name for deployments.
    /// </summary>
    public string DefaultEnvironment { get; set; } = "production";
    /// <summary>
    /// Gets or sets whether automatic retry of failed requests is enabled.
    /// </summary>
    public bool AutoRetry { get; set; } = true;
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    /// <summary>
    /// Gets or sets the list of trusted hosts for SSL certificate validation.
    /// </summary>
    public List<string> TrustedHosts { get; set; } = new();

    /// <summary>
    /// Validates the configuration for required fields.
    /// </summary>
    /// <returns>Collection of validation errors.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ApiUrl))
            errors.Add("API URL is required.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            errors.Add("API Key is required. Set via COOLIFY_API_KEY environment variable.");

        if (RequestTimeoutSeconds < 5 || RequestTimeoutSeconds > 300)
            errors.Add("Request timeout must be between 5 and 300 seconds.");

        if (MaxRetries < 0 || MaxRetries > 10)
            errors.Add("Max retries must be between 0 and 10.");

        try
        {
            new Uri(ApiUrl);
        }
        catch
        {
            errors.Add("API URL is not a valid URI.");
        }

        return errors;
    }

    /// <summary>
    /// Loads configuration from environment variables.
    /// </summary>
    /// <returns>Configured instance.</returns>
    public static CoolifyConfiguration FromEnvironment()
    {
        return new CoolifyConfiguration
        {
            ApiUrl = Environment.GetEnvironmentVariable("COOLIFY_API_URL") ?? "https://api.coolify.io",
            ApiKey = Environment.GetEnvironmentVariable("COOLIFY_API_KEY"),
            RequestTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("COOLIFY_TIMEOUT"), out var timeout) ? timeout : 30,
            VerboseLogging = bool.TryParse(Environment.GetEnvironmentVariable("COOLIFY_VERBOSE"), out var verbose) && verbose,
QuietLogging = bool.TryParse(Environment.GetEnvironmentVariable("COOLIFY_QUIET"), out var quiet) && quiet,
            DefaultEnvironment = Environment.GetEnvironmentVariable("COOLIFY_ENVIRONMENT") ?? "production",
            AutoRetry = !bool.TryParse(Environment.GetEnvironmentVariable("COOLIFY_NO_RETRY"), out var noRetry) || !noRetry,
            MaxRetries = int.TryParse(Environment.GetEnvironmentVariable("COOLIFY_MAX_RETRIES"), out var retries) ? retries : 3
        };
    }
}
