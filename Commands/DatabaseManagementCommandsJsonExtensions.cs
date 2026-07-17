#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolifyCli.Commands;

/// <summary>
/// Provides System.Text.Json serialization and deserialization helpers for <see cref="DatabaseManagementCommands"/>.
/// </summary>
public static class DatabaseManagementCommandsJsonExtensions
{
    // Cached options: camelCase naming policy, ignore null values.
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="DatabaseManagementCommands"/> to a JSON string.
    /// </summary>
    /// <param name="value">The database management commands to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented for readability.</param>
    /// <returns>A JSON representation of the database management commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DatabaseManagementCommands value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DatabaseManagementCommands"/>.
    /// </summary>
    /// <param name="json">The JSON string representing database management commands.</param>
    /// <returns>The deserialized <see cref="DatabaseManagementCommands"/> instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DatabaseManagementCommands? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<DatabaseManagementCommands>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="DatabaseManagementCommands"/>.
    /// </summary>
    /// <param name="json">The JSON string representing database management commands.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="DatabaseManagementCommands"/> if the operation succeeded; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static bool TryFromJson(string json, out DatabaseManagementCommands? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}