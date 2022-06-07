#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CoolifyCli.Formatters;

/// <summary>
/// Formats data as JSON output. Supports pretty-printing, filtering, and selective serialization.
/// Provides both object-to-JSON and JSON-to-JSON transformations with formatting options.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _options;
    private readonly bool _prettyPrint;
    private readonly List<string>? _includeFields;
    private readonly List<string>? _excludeFields;

    public JsonFormatter(bool prettyPrint = false, List<string>? includeFields = null, List<string>? excludeFields = null)
    {
        _prettyPrint = prettyPrint;
        _includeFields = includeFields;
        _excludeFields = excludeFields;

        _options = new JsonSerializerOptions
        {
            WriteIndented = prettyPrint,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    /// <summary>
    /// Formats an object as JSON string.
    /// </summary>
    public string Format(object? data)
    {
        if (data is null)
            return "null";

        var json = JsonSerializer.Serialize(data, _options);
        return FilterJson(json);
    }

    /// <summary>
    /// Formats a collection of objects as JSON array.
    /// </summary>
    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null)
            return "[]";

        var json = JsonSerializer.Serialize(items.ToList(), _options);
        return FilterJson(json);
    }

    /// <summary>
    /// Formats a key-value dictionary as JSON object.
    /// </summary>
    public string FormatDictionary(Dictionary<string, object?> data)
    {
        if (data is null || data.Count == 0)
            return "{}";

        var json = JsonSerializer.Serialize(data, _options);
        return FilterJson(json);
    }

    /// <summary>
    /// Parses JSON string and reformats according to formatter settings.
    /// </summary>
    public string ReformatJson(string jsonString)
    {
        try
        {
            var node = JsonNode.Parse(jsonString);
            var reformatted = node?.ToJsonString(_options) ?? "{}";
            return FilterJson(reformatted);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Minifies JSON by removing whitespace.
    /// </summary>
    public string Minify(string jsonString)
    {
        try
        {
            var node = JsonNode.Parse(jsonString);
            return node?.ToJsonString() ?? "{}";
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Prettifies JSON with indentation.
    /// </summary>
    public string Prettify(string jsonString)
    {
        try
        {
            var node = JsonNode.Parse(jsonString);
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "{}";
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Extracts a specific field from JSON using dot notation (e.g., "data.items").
    /// </summary>
    public string? ExtractField(string jsonString, string fieldPath)
    {
        try
        {
            JsonNode? node = JsonNode.Parse(jsonString);
            foreach (var part in fieldPath.Split('.'))
            {
                if (node is JsonObject obj)
                    node = obj[part];
                else
                    return null;
            }

            return node?.ToJsonString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Filters JSON based on include/exclude field lists.
    /// </summary>
    private string FilterJson(string json)
    {
        if (_includeFields is null && _excludeFields is null)
            return json;

        try
        {
            var jObject = JsonNode.Parse(json)?.AsObject();
            if (jObject is null) return json;

            if (_includeFields is not null && _includeFields.Count > 0)
                jObject = FilterByInclude(jObject);

            if (_excludeFields is not null && _excludeFields.Count > 0)
                jObject = FilterByExclude(jObject);

            return jObject.ToJsonString(_options);
        }
        catch
        {
            return json; // Return original if filtering fails
        }
    }

    /// <summary>
    /// Filters JSON to include only specified fields.
    /// </summary>
    private JsonObject FilterByInclude(JsonObject jObject)
    {
        var filtered = new JsonObject();

        foreach (var field in _includeFields!)
        {
            if (jObject.ContainsKey(field))
                filtered[field] = jObject[field]?.DeepClone();
        }

        return filtered;
    }

    /// <summary>
    /// Filters JSON to exclude specified fields.
    /// </summary>
    private JsonObject FilterByExclude(JsonObject jObject)
    {
        var filtered = new JsonObject();

        foreach (var kvp in jObject)
        {
            if (!_excludeFields!.Contains(kvp.Key))
                filtered[kvp.Key] = kvp.Value?.DeepClone();
        }

        return filtered;
    }
}

/// <summary>
/// Interface for output formatters (JSON, CSV, Table, etc.).
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Formats a single object.
    /// </summary>
    string Format(object? data);

    /// <summary>
    /// Formats a collection of items.
    /// </summary>
    string FormatCollection<T>(IEnumerable<T> items);

    /// <summary>
    /// Formats a dictionary of key-value pairs.
    /// </summary>
    string FormatDictionary(Dictionary<string, object?> data);
}
