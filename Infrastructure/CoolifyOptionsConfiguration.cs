#nullable enable
using Microsoft.Extensions.Configuration;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Builds the layered <see cref="IConfiguration"/> that feeds <see cref="CoolifyOptions"/>.
/// Sources, lowest to highest precedence: JSON config file, environment variables, CLI flags.
/// </summary>
public static class CoolifyOptionsConfiguration
{
    /// <summary>The CLI flag prefix that this class reads. Other arguments are ignored.</summary>
    public const string FlagPrefix = "--coolify-";

    /// <summary>The CLI flag that selects the config file path.</summary>
    public const string ConfigFileFlag = "--coolify-config";

    /// <summary>
    /// The config file used when neither <c>--coolify-config</c> nor <c>COOLIFY_CONFIG_FILE</c> is set:
    /// <c>~/.config/coolify/config.json</c>.
    /// </summary>
    public static string DefaultConfigFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "coolify", "config.json");

    private static readonly Dictionary<string, string> SwitchMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["--coolify-url"] = $"{CoolifyOptions.SectionName}:{nameof(CoolifyOptions.BaseUrl)}",
        ["--coolify-token"] = $"{CoolifyOptions.SectionName}:{nameof(CoolifyOptions.ApiToken)}",
        ["--coolify-timeout"] = $"{CoolifyOptions.SectionName}:{nameof(CoolifyOptions.TimeoutSeconds)}",
        ["--coolify-project"] = $"{CoolifyOptions.SectionName}:{nameof(CoolifyOptions.DefaultProjectUuid)}",
    };

    /// <summary>
    /// Builds the configuration from the config file, environment variables, and <c>--coolify-*</c> flags.
    /// </summary>
    /// <param name="args">Process arguments. Only <c>--coolify-*</c> flags are read; all others are ignored.</param>
    /// <param name="getEnvironmentVariable">Environment lookup. Defaults to the process environment; injectable for tests.</param>
    /// <returns>The layered configuration. Bind <c>Coolify</c> to <see cref="CoolifyOptions"/>.</returns>
    /// <exception cref="System.IO.InvalidDataException">The config file exists but cannot be parsed as JSON.</exception>
    public static IConfiguration Build(IEnumerable<string> args, Func<string, string?>? getEnvironmentVariable = null)
    {
        var getEnv = getEnvironmentVariable ?? Environment.GetEnvironmentVariable;
        var flags = ExtractCoolifyFlags(args);
        var configFilePath = ResolveConfigFilePath(flags, getEnv);

        return new ConfigurationBuilder()
            .AddJsonFile(configFilePath, optional: true, reloadOnChange: false)
            .AddInMemoryCollection(ReadEnvironmentLayer(getEnv))
            .AddCommandLine(flags.ToArray(), SwitchMappings)
            .Build();
    }

    /// <summary>
    /// Returns the <c>--coolify-*</c> flags from <paramref name="args"/>, each with its value when given
    /// as a separate argument, so that unrelated arguments never reach the command-line provider.
    /// </summary>
    private static List<string> ExtractCoolifyFlags(IEnumerable<string> args)
    {
        var list = args.ToList();
        var flags = new List<string>();

        for (var i = 0; i < list.Count; i++)
        {
            var arg = list[i];
            if (!arg.StartsWith(FlagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            flags.Add(arg);
            if (!arg.Contains('=') && i + 1 < list.Count && !list[i + 1].StartsWith('-'))
            {
                flags.Add(list[++i]);
            }
        }

        return flags;
    }

    private static string ResolveConfigFilePath(IReadOnlyList<string> flags, Func<string, string?> getEnv)
    {
        for (var i = 0; i < flags.Count; i++)
        {
            if (flags[i].Equals(ConfigFileFlag, StringComparison.OrdinalIgnoreCase) && i + 1 < flags.Count)
            {
                return flags[i + 1];
            }

            if (flags[i].StartsWith(ConfigFileFlag + "=", StringComparison.OrdinalIgnoreCase))
            {
                return flags[i][(ConfigFileFlag.Length + 1)..];
            }
        }

        var fromEnvironment = getEnv(Constants.Environment.ConfigFileVariableName);
        return string.IsNullOrWhiteSpace(fromEnvironment) ? DefaultConfigFilePath : fromEnvironment;
    }

    /// <summary>
    /// Maps environment variables onto configuration keys. The first non-empty name in each list wins,
    /// so the current <c>COOLIFY_URL</c>/<c>COOLIFY_TOKEN</c> names take precedence over the legacy ones.
    /// </summary>
    private static Dictionary<string, string?> ReadEnvironmentLayer(Func<string, string?> getEnv)
    {
        var layer = new Dictionary<string, string?>();

        AddFirstNonEmpty(layer, nameof(CoolifyOptions.BaseUrl), getEnv,
            Constants.Environment.BaseUrlVariableName, Constants.Environment.ApiUrlVariableName);
        AddFirstNonEmpty(layer, nameof(CoolifyOptions.ApiToken), getEnv,
            Constants.Environment.ApiTokenVariableName, Constants.Environment.ApiKeyVariableName);
        AddFirstNonEmpty(layer, nameof(CoolifyOptions.TimeoutSeconds), getEnv,
            Constants.Environment.TimeoutVariableName);
        AddFirstNonEmpty(layer, nameof(CoolifyOptions.DefaultProjectUuid), getEnv,
            Constants.Environment.ProjectUuidVariableName);

        return layer;
    }

    private static void AddFirstNonEmpty(
        Dictionary<string, string?> layer,
        string property,
        Func<string, string?> getEnv,
        params string[] variableNames)
    {
        foreach (var variableName in variableNames)
        {
            var value = getEnv(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                layer[$"{CoolifyOptions.SectionName}:{property}"] = value;
                return;
            }
        }
    }
}
