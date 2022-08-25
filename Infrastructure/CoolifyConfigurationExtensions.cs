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
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static string GetApiUrlWithTrailingSlash(this CoolifyConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.ApiUrl.TrimEnd('/') + '/';
    }

    /// <summary>
    /// Determines if verbose logging is enabled and should output detailed information.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>True if verbose logging is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static bool ShouldLogVerbose(this CoolifyConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.VerboseLogging;
    }

    /// <summary>
    /// Gets the effective request timeout in milliseconds for HTTP client operations.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>Timeout in milliseconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static int GetRequestTimeoutMilliseconds(this CoolifyConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.RequestTimeoutSeconds * 1000;
    }

    /// <summary>
    /// Creates a deep copy of the configuration to prevent mutation of the original.
    /// </summary>
    /// <param name="configuration">The configuration instance to copy.</param>
    /// <returns>A new instance with the same values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static CoolifyConfiguration Clone(this CoolifyConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new CoolifyConfiguration
        {
            ApiUrl = configuration.ApiUrl ?? string.Empty,
            ApiKey = configuration.ApiKey,
            RequestTimeoutSeconds = configuration.RequestTimeoutSeconds,
            VerboseLogging = configuration.VerboseLogging,
            DefaultEnvironment = configuration.DefaultEnvironment ?? string.Empty,
            AutoRetry = configuration.AutoRetry,
            MaxRetries = configuration.MaxRetries,
            TrustedHosts = new List<string>(configuration.TrustedHosts ?? [])
        };
    }
}