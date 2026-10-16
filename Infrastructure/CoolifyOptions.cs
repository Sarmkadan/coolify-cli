#nullable enable
namespace CoolifyCli.Infrastructure;

/// <summary>
/// Startup options for the Coolify API connection, bound through <c>IOptions&lt;CoolifyOptions&gt;</c>
/// and checked by <see cref="CoolifyOptionsValidator"/> before any HTTP call is made.
/// </summary>
/// <remarks>
/// Precedence, highest first: CLI flags (<c>--coolify-*</c>), environment variables, the JSON config file,
/// then the defaults declared here. See <c>docs/CoolifyOptions.md</c> for the full list of names.
/// </remarks>
public sealed class CoolifyOptions
{
    /// <summary>The configuration section (<c>Coolify</c>) that every source writes into.</summary>
    public const string SectionName = "Coolify";

    /// <summary>
    /// Base URL of the Coolify instance, for example <c>https://coolify.example.com</c>.
    /// Must be absolute http or https with no path, query, fragment, or credentials.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.coolify.io";

    /// <summary>
    /// API token sent with every request. Required and never printed in validation messages.
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds, between 1 and 600 inclusive. Default: 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Optional project UUID used by commands that are not given one explicitly.
    /// </summary>
    public string? DefaultProjectUuid { get; set; }
}
