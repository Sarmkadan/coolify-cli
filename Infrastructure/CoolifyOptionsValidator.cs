#nullable enable
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Validates <see cref="CoolifyOptions"/> at startup. Every failing rule is reported, not just the first,
/// and no message contains the token value.
/// </summary>
public sealed partial class CoolifyOptionsValidator : IValidateOptions<CoolifyOptions>
{
    /// <summary>The smallest accepted <see cref="CoolifyOptions.TimeoutSeconds"/> value.</summary>
    public const int MinTimeoutSeconds = 1;

    /// <summary>The largest accepted <see cref="CoolifyOptions.TimeoutSeconds"/> value.</summary>
    public const int MaxTimeoutSeconds = 600;

    /// <summary>Matches common placeholder words such as <c>changeme</c>, <c>your-token-here</c>, or <c>xxx</c>.</summary>
    [GeneratedRegex(
        @"^(changeme|change[-_ ]?me|placeholder|replace[-_ ]?me|todo|tbd|x{3,}|none|null|undefined|secret|(your[-_ ]?)?(api[-_ ]?)?(token|key|secret)([-_ ]?here)?)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderWordPattern();

    /// <summary>Matches unexpanded template syntax such as <c>&lt;token&gt;</c>, <c>${COOLIFY_TOKEN}</c>, or <c>$COOLIFY_TOKEN</c>.</summary>
    [GeneratedRegex(@"^(<.*>|\$\{.*\}|\$[A-Za-z_][A-Za-z0-9_]*)$")]
    private static partial Regex PlaceholderSyntaxPattern();

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CoolifyOptions options)
    {
        var failures = new List<string>();

        ValidateBaseUrl(options.BaseUrl, failures);
        ValidateApiToken(options.ApiToken, failures);
        ValidateTimeout(options.TimeoutSeconds, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateBaseUrl(string? baseUrl, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            failures.Add("BaseUrl is required. Set COOLIFY_URL, --coolify-url, or Coolify:BaseUrl in the config file.");
            return;
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri.Host))
        {
            failures.Add("BaseUrl must be an absolute URL such as https://coolify.example.com.");
            return;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add("BaseUrl must use the http or https scheme.");
            return;
        }

        if (uri.UserInfo.Length > 0)
        {
            failures.Add("BaseUrl must not contain credentials; use ApiToken instead.");
        }

        if (uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            failures.Add("BaseUrl must not contain a query string or fragment.");
        }

        // The client sends requests to "/api/v1/..." with a leading slash, so any path on the base URL
        // (for example a trailing /api/v1) is discarded. Reject it here rather than let it be ignored silently.
        if (uri.AbsolutePath != "/")
        {
            failures.Add("BaseUrl must not contain a path such as /api/v1; the CLI adds its own API paths to the host root.");
        }
    }

    private static void ValidateApiToken(string? apiToken, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            failures.Add("ApiToken is required. Set COOLIFY_TOKEN, --coolify-token, or Coolify:ApiToken in the config file.");
            return;
        }

        var value = apiToken.Trim();
        if (PlaceholderWordPattern().IsMatch(value) || PlaceholderSyntaxPattern().IsMatch(value))
        {
            failures.Add("ApiToken looks like a placeholder value. Replace it with a real token from your Coolify instance.");
        }
    }

    private static void ValidateTimeout(int timeoutSeconds, List<string> failures)
    {
        if (timeoutSeconds < MinTimeoutSeconds || timeoutSeconds > MaxTimeoutSeconds)
        {
            failures.Add(
                $"TimeoutSeconds must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds} (got {timeoutSeconds}). " +
                "Set COOLIFY_TIMEOUT, --coolify-timeout, or Coolify:TimeoutSeconds in the config file.");
        }
    }
}
