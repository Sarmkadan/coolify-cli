#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolifyCli.Formatters;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="TableFormatter"/>.
/// </summary>
public static class TableFormatterJsonExtensions
{
    /// <summary>
    /// Gets the default JSON serialization options used by the extension methods.
    /// Uses camelCase property naming, ignores null values, and includes enum string conversion.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="TableFormatter"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The table formatter to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="TableFormatter"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this TableFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="TableFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>A <see cref="TableFormatter"/> instance if deserialization succeeds; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static TableFormatter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<TableFormatter>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="TableFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <param name="value">Receives the deserialized <see cref="TableFormatter"/> instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out TableFormatter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<TableFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}