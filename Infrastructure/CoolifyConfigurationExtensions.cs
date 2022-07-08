#nullable enable

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Extension methods for <see cref="CoolifyConfiguration"/> to provide additional functionality
/// for configuration management and API client operations.
/// </summary>
public static class CoolifyConfigurationExtensions
{
    /// <summary>
    /// Creates a sanitized API URL by ensuring it ends with a trailing slash.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>API URL with trailing slash.</returns>
    public static string GetApiUrlWithTrailingSlash(this CoolifyConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.ApiUrl.TrimEnd('/') + '/';
    }

    /// <summary>
    /// Determines if verbose logging is enabled and should output detailed information.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>True if verbose logging is enabled; otherwise false.</returns>
    public static bool ShouldLogVerbose(this CoolifyConfiguration configuration)
    {
        return configuration?.VerboseLogging == true;
    }

    /// <summary>
    /// Gets the effective request timeout in milliseconds for HTTP client operations.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>Timeout in milliseconds.</returns>
    public static int GetRequestTimeoutMilliseconds(this CoolifyConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.RequestTimeoutSeconds * 1000;
    }

    /// <summary>
    /// Creates a deep copy of the configuration to prevent mutation of the original.
    /// </summary>
    /// <param name="configuration">The configuration instance to copy.</param>
    /// <returns>A new instance with the same values.</returns>
    public static CoolifyConfiguration Clone(this CoolifyConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return new CoolifyConfiguration
        {
            ApiUrl = configuration.ApiUrl,
            ApiKey = configuration.ApiKey,
            RequestTimeoutSeconds = configuration.RequestTimeoutSeconds,
            VerboseLogging = configuration.VerboseLogging,
            DefaultEnvironment = configuration.DefaultEnvironment,
            AutoRetry = configuration.AutoRetry,
            MaxRetries = configuration.MaxRetries,
            TrustedHosts = new List<string>(configuration.TrustedHosts)
        };
    }
}