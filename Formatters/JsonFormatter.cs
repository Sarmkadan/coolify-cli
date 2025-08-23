// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoolifiCli.Formatters;

/// <summary>
/// Formats data as JSON output. Supports pretty-printing, filtering, and selective serialization.
/// Provides both object-to-JSON and JSON-to-JSON transformations with formatting options.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    private readonly JsonSerializerSettings _settings;
    private readonly bool _prettyPrint;
    private readonly List<string>? _includeFields;
    private readonly List<string>? _excludeFields;

    public JsonFormatter(bool prettyPrint = false, List<string>? includeFields = null, List<string>? excludeFields = null)
    {
        _prettyPrint = prettyPrint;
        _includeFields = includeFields;
        _excludeFields = excludeFields;

        _settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    /// <summary>
    /// Formats an object as JSON string.
    /// </summary>
    public string Format(object? data)
    {
        if (data == null)
            return "null";

        var json = JsonConvert.SerializeObject(data, _settings);
        return FilterJson(json);
    }

    /// <summary>
    /// Formats a collection of objects as JSON array.
    /// </summary>
    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items == null)
            return "[]";

        var json = JsonConvert.SerializeObject(items.ToList(), _settings);
        return FilterJson(json);
    }

    /// <summary>
    /// Formats a key-value dictionary as JSON object.
    /// </summary>
    public string FormatDictionary(Dictionary<string, object?> data)
    {
        if (data == null || data.Count == 0)
            return "{}";

        var json = JsonConvert.SerializeObject(data, _settings);
        return FilterJson(json);
    }

    /// <summary>
    /// Parses JSON string and reformats according to formatter settings.
    /// </summary>
    public string ReformatJson(string jsonString)
    {
        try
        {
            var jObject = JObject.Parse(jsonString);
            var reformatted = jObject.ToString(_settings.Formatting);
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
            var jObject = JObject.Parse(jsonString);
            return jObject.ToString(Formatting.None);
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
            var jObject = JObject.Parse(jsonString);
            return jObject.ToString(Formatting.Indented);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Extracts a specific field from JSON using dot notation (e.g., "data.items[0].name").
    /// </summary>
    public string? ExtractField(string jsonString, string fieldPath)
    {
        try
        {
            var jObject = JObject.Parse(jsonString);
            var token = jObject.SelectToken(fieldPath);
            return token?.ToString();
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
        if (_includeFields == null && _excludeFields == null)
            return json;

        try
        {
            var jObject = JObject.Parse(json);

            if (_includeFields != null && _includeFields.Count > 0)
            {
                jObject = FilterByInclude(jObject);
            }

            if (_excludeFields != null && _excludeFields.Count > 0)
            {
                jObject = FilterByExclude(jObject);
            }

            return jObject.ToString(_settings.Formatting);
        }
        catch
        {
            return json; // Return original if filtering fails
        }
    }

    /// <summary>
    /// Filters JSON to include only specified fields.
    /// </summary>
    private JObject FilterByInclude(JObject jObject)
    {
        var filtered = new JObject();

        foreach (var field in _includeFields!)
        {
            if (jObject.ContainsKey(field))
            {
                filtered[field] = jObject[field];
            }
        }

        return filtered;
    }

    /// <summary>
    /// Filters JSON to exclude specified fields.
    /// </summary>
    private JObject FilterByExclude(JObject jObject)
    {
        var filtered = new JObject(jObject);

        foreach (var field in _excludeFields!)
        {
            filtered.Remove(field);
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
