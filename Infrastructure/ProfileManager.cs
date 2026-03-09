#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a single named profile storing connection settings for a Coolify instance.
/// </summary>
public class ProfileEntry
{
    [JsonPropertyName("apiUrl")]
    public string ApiUrl { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Manages named connection profiles stored in <c>~/.coolify-cli/profiles.json</c>.
/// Profiles allow switching between multiple Coolify instances (e.g. staging, production)
/// without changing environment variables.
/// </summary>
public static class ProfileManager
{
    private static readonly string ProfilesFile =
        Path.Combine(Constants.Paths.ConfigDirectory, "profiles.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Loads all stored profiles from disk.</summary>
    public static Dictionary<string, ProfileEntry> LoadAll()
    {
        try
        {
            if (!File.Exists(ProfilesFile))
                return new();

            var json = File.ReadAllText(ProfilesFile);
            return JsonSerializer.Deserialize<Dictionary<string, ProfileEntry>>(json, JsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    /// <summary>Returns the named profile, or <c>null</c> if it does not exist.</summary>
    public static ProfileEntry? Get(string name)
    {
        var all = LoadAll();
        return all.TryGetValue(name, out var entry) ? entry : null;
    }

    /// <summary>Saves or overwrites a named profile.</summary>
    public static void Save(string name, string apiUrl, string apiKey, int timeoutSeconds = 30)
    {
        var dir = Constants.Paths.ConfigDirectory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var all = LoadAll();
        all[name] = new ProfileEntry
        {
            ApiUrl = apiUrl,
            ApiKey = apiKey,
            TimeoutSeconds = timeoutSeconds
        };
        File.WriteAllText(ProfilesFile, JsonSerializer.Serialize(all, JsonOptions));
    }

    /// <summary>Removes a named profile. Returns <c>false</c> when the profile did not exist.</summary>
    public static bool Remove(string name)
    {
        var all = LoadAll();
        if (!all.Remove(name))
            return false;

        File.WriteAllText(ProfilesFile, JsonSerializer.Serialize(all, JsonOptions));
        return true;
    }

    /// <summary>
    /// Loads a <see cref="CoolifyConfiguration"/> for the given profile name.
    /// Falls back to environment variables when <paramref name="profileName"/> is <c>null</c> or empty.
    /// Throws <see cref="InvalidOperationException"/> when the named profile is not found.
    /// </summary>
    public static CoolifyConfiguration LoadConfiguration(string? profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            return CoolifyConfiguration.FromEnvironment();

        var entry = Get(profileName)
            ?? throw new InvalidOperationException(
                $"Profile '{profileName}' not found. " +
                $"Use 'coolify profile set {profileName} <url> <key>' to create it.");

        var config = CoolifyConfiguration.FromEnvironment();
        config.ApiUrl = entry.ApiUrl;
        config.ApiKey = entry.ApiKey;
        if (entry.TimeoutSeconds > 0)
            config.RequestTimeoutSeconds = entry.TimeoutSeconds;

        return config;
    }
}
