#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

using System.Text.RegularExpressions;
using CoolifiCli.Infrastructure;

/// <summary>
/// Resolves <c>${VAR_NAME}</c> placeholder tokens embedded in raw YAML template text by
/// substituting values sourced first from caller-supplied overrides, then from the process
/// environment.  Any token that cannot be resolved is collected and returned so the caller
/// can surface a targeted error rather than producing a silently-incomplete template.
/// </summary>
public sealed class TemplateVariableResolver
{
    // Compiled once; shared across all instances for efficiency.
    private static readonly Regex PlaceholderPattern =
        new(Constants.Iac.TemplateVariablePattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly Dictionary<string, string> _overrides;
    private readonly ILogger _logger;

    /// <summary>
    /// Initialises a resolver with no additional overrides; the process environment is the
    /// sole resolution source.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public TemplateVariableResolver(ILogger logger)
        : this(logger, new Dictionary<string, string>()) { }

    /// <summary>
    /// Initialises a resolver with caller-supplied variable overrides that take precedence
    /// over process environment variables.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="overrides">
    /// Variables to substitute before consulting the process environment.
    /// </param>
    public TemplateVariableResolver(
        ILogger logger,
        IReadOnlyDictionary<string, string> overrides)
    {
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
        _overrides = new Dictionary<string, string>(
            overrides ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds or replaces a single variable override. Overrides always take precedence over
    /// the process environment.
    /// </summary>
    /// <param name="key">The variable name (case-insensitive).</param>
    /// <param name="value">The substitution value.</param>
    public void SetOverride(string key, string value) =>
        _overrides[key] = value;

    /// <summary>
    /// Parses a <c>.env</c>-format file (<c>KEY=VALUE</c> lines) and merges the entries
    /// into this resolver's override table.  Blank lines and lines beginning with <c>#</c>
    /// are ignored.  Existing overrides are not replaced.
    /// </summary>
    /// <param name="dotEnvPath">Absolute or relative path to the <c>.env</c> file.</param>
    /// <returns>
    /// The number of new variables loaded; zero when the file does not exist or is
    /// unreadable.
    /// </returns>
    public int LoadDotEnvFile(string dotEnvPath)
    {
        if (!File.Exists(dotEnvPath))
            return 0;

        int loaded = 0;
        foreach (var line in File.ReadAllLines(dotEnvPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var eq = trimmed.IndexOf('=');
            if (eq <= 0) continue;

            var key   = trimmed[..eq].Trim();
            var value = trimmed[(eq + 1)..].Trim().Trim('"').Trim('\'');

            if (!_overrides.ContainsKey(key))
            {
                _overrides[key] = value;
                loaded++;
            }
        }

        _logger.Debug($"Loaded {loaded} variable(s) from '{dotEnvPath}'");
        return loaded;
    }

    /// <summary>
    /// Scans <paramref name="rawYaml"/> for <c>${VAR}</c> tokens, resolves each against
    /// the override table then against the process environment, and returns the fully
    /// expanded text alongside a list of any variable names that could not be resolved.
    /// </summary>
    /// <param name="rawYaml">The raw YAML content to expand.</param>
    /// <returns>
    /// A value tuple of (<c>ExpandedYaml</c>, <c>Unresolved</c>).
    /// <c>Unresolved</c> is empty when every placeholder was resolved successfully.
    /// </returns>
    public (string ExpandedYaml, List<string> Unresolved) Expand(string rawYaml)
    {
        if (string.IsNullOrEmpty(rawYaml))
            return (rawYaml, []);

        var unresolved = new List<string>();

        var result = PlaceholderPattern.Replace(rawYaml, match =>
        {
            var name = match.Groups[1].Value;

            if (_overrides.TryGetValue(name, out var fromOverride))
                return fromOverride;

            var fromEnv = System.Environment.GetEnvironmentVariable(name);
            if (fromEnv is not null)
                return fromEnv;

            unresolved.Add(name);
            return match.Value; // preserve the token so the caller can report it precisely
        });

        if (unresolved.Count > 0)
            _logger.Warn(
                $"Unresolved template variable(s): {string.Join(", ", unresolved.Select(n => "${" + n + "}"))}");

        return (result, unresolved);
    }

    /// <summary>
    /// Returns the set of distinct <c>${VAR}</c> placeholder names found in
    /// <paramref name="rawYaml"/> without attempting to resolve any of them.
    /// Useful for pre-flight checks that enumerate all required variables before invoking
    /// <see cref="Expand"/>.
    /// </summary>
    /// <param name="rawYaml">The raw YAML text to scan.</param>
    /// <returns>
    /// A read-only set of unique variable names referenced by placeholders in the document.
    /// </returns>
    public static IReadOnlySet<string> CollectPlaceholders(string rawYaml)
    {
        if (string.IsNullOrEmpty(rawYaml))
            return new HashSet<string>();

        return PlaceholderPattern
            .Matches(rawYaml)
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }
}
