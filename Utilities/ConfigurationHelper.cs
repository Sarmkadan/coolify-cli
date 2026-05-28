#nullable enable
namespace CoolifyCli.Utilities;

using System.Text.Json;
using CoolifyCli.Infrastructure;

/// <summary>
/// Helper for managing CLI configuration files and settings persistence.
/// </summary>
public static class ConfigurationHelper
{
    private static readonly string ConfigPath = Constants.Paths.ConfigFile;
    private static readonly string ConfigDir = Constants.Paths.ConfigDirectory;

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads configuration from file if it exists, otherwise returns empty config.
    /// </summary>
    /// <returns>Loaded or default configuration.</returns>
    public static Dictionary<string, object> LoadConfiguration()
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            if (!File.Exists(ConfigPath))
                return new Dictionary<string, object>();

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            return config ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load configuration: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Saves configuration to file.
    /// </summary>
    /// <param name="config">Configuration dictionary to save.</param>
    public static void SaveConfiguration(Dictionary<string, object> config)
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            var json = JsonSerializer.Serialize(config, WriteOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to save configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="defaultValue">Default value if key not found.</param>
    /// <returns>Configuration value or default.</returns>
    public static object? GetConfigValue(string key, object? defaultValue = null)
    {
        var config = LoadConfiguration();
        return config.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="value">Configuration value.</param>
    public static void SetConfigValue(string key, object value)
    {
        var config = LoadConfiguration();
        config[key] = value;
        SaveConfiguration(config);
    }

    /// <summary>
    /// Deletes a configuration value.
    /// </summary>
    /// <param name="key">Configuration key to delete.</param>
    public static void DeleteConfigValue(string key)
    {
        var config = LoadConfiguration();
        if (config.ContainsKey(key))
        {
            config.Remove(key);
            SaveConfiguration(config);
        }
    }

    /// <summary>
    /// Initializes the configuration directory and default files.
    /// </summary>
    public static void InitializeConfigDirectory()
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            var logsDir = Path.Combine(ConfigDir, "logs");
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);

            // Create default config if it doesn't exist
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new Dictionary<string, object>
                {
                    { "DefaultEnvironment", "production" },
                    { "LastUsedApplication", -1 },
                    { "LastUsedDatabase", -1 },
                    { "VerboseMode", false }
                };

                SaveConfiguration(defaultConfig);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to initialize config directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays current configuration (non-sensitive values only).
    /// </summary>
    public static void DisplayConfiguration()
    {
        var config = LoadConfiguration();

        Console.WriteLine("\nCurrent Configuration:");
        Console.WriteLine(new string('-', 40));

        foreach (var kvp in config)
        {
            // Hide sensitive values
            var displayValue = kvp.Key.Contains("key", StringComparison.OrdinalIgnoreCase) ||
                             kvp.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                             kvp.Key.Contains("password", StringComparison.OrdinalIgnoreCase)
                ? "***"
                : kvp.Value?.ToString() ?? "null";

            Console.WriteLine($"{kvp.Key}: {displayValue}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Resets configuration to defaults.
    /// </summary>
    public static void ResetConfiguration()
    {
        try
        {
            if (File.Exists(ConfigPath))
                File.Delete(ConfigPath);

            var defaultConfig = new Dictionary<string, object>();
            SaveConfiguration(defaultConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to reset configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates configuration has required fields.
    /// </summary>
    /// <returns>List of validation errors.</returns>
    public static List<string> ValidateConfiguration()
    {
        var errors = new List<string>();
        var apiKey = Environment.GetEnvironmentVariable(Constants.Environment.ApiKeyVariableName);

        if (string.IsNullOrWhiteSpace(apiKey))
            errors.Add($"API key must be set via {Constants.Environment.ApiKeyVariableName} environment variable");

        var apiUrl = Environment.GetEnvironmentVariable(Constants.Environment.ApiUrlVariableName);
        if (!string.IsNullOrWhiteSpace(apiUrl))
        {
            if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out _))
                errors.Add("API URL must be a valid URI");
        }

        return errors;
    }

    /// <summary>
    /// Exports configuration to a JSON file.
    /// </summary>
    /// <param name="filePath">Path to export to.</param>
    public static void ExportConfiguration(string filePath)
    {
        try
        {
            var config = LoadConfiguration();
            var json = JsonSerializer.Serialize(config, WriteOptions);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Configuration exported to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Imports configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to import from.</param>
    public static void ImportConfiguration(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Configuration file not found: {filePath}");
                return;
            }

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (config is not null)
            {
                SaveConfiguration(config);
                Console.WriteLine($"Configuration imported from {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing configuration: {ex.Message}");
        }
    }
}
