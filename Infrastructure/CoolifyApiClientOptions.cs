#nullable enable
namespace CoolifyCli.Infrastructure;

/// <summary>
/// Per-HTTP-method timeout configuration for <c>CoolifyApiClient</c>.
/// Use this to tune timeouts independently for quick operations (GET health checks)
/// vs. long-running ones (POST deployments).
/// </summary>
public class CoolifyApiClientOptions
{
    /// <summary>Timeout in seconds applied to GET requests. Default: 30.</summary>
    public int GetTimeoutSeconds { get; set; } = 30;

    /// <summary>Timeout in seconds applied to POST requests. Default: 120.</summary>
    public int PostTimeoutSeconds { get; set; } = 120;

    /// <summary>Timeout in seconds applied to PUT requests. Default: 120.</summary>
    public int PutTimeoutSeconds { get; set; } = 120;

    /// <summary>Timeout in seconds applied to DELETE requests. Default: 30.</summary>
    public int DeleteTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Builds a <see cref="CoolifyApiClientOptions"/> from a <see cref="CoolifyConfiguration"/>,
    /// using the global <see cref="CoolifyConfiguration.RequestTimeoutSeconds"/> as the baseline
    /// for GET and DELETE, and doubling it for POST/PUT operations.
    /// </summary>
    public static CoolifyApiClientOptions FromConfiguration(CoolifyConfiguration config)
    {
        var baseline = config.RequestTimeoutSeconds;
        return new CoolifyApiClientOptions
        {
            GetTimeoutSeconds    = baseline,
            DeleteTimeoutSeconds = baseline,
            PostTimeoutSeconds   = Math.Max(baseline * 2, 60),
            PutTimeoutSeconds    = Math.Max(baseline * 2, 60)
        };
    }
}
