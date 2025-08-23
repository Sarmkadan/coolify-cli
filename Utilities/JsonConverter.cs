// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoolifiCli.Utilities;

/// <summary>
/// JSON conversion utilities for serialization, deserialization, and transformation.
/// Handles type-safe conversions with error handling and formatting options.
/// </summary>
public static class JsonConverter
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    public static string ToJson<T>(T? obj, bool prettyPrint = false)
    {
        if (obj == null)
            return "null";

        var settings = new JsonSerializerSettings(DefaultSettings)
        {
            Formatting = prettyPrint ? Formatting.Indented : Formatting.None
        };

        return JsonConvert.SerializeObject(obj, settings);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    public static T? FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses JSON string and returns as dynamic object.
    /// </summary>
    public static dynamic? ParseDynamic(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<dynamic>(json, DefaultSettings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Merges two objects into a single JSON object.
    /// Later object properties override earlier ones.
    /// </summary>
    public static T? Merge<T>(T? obj1, T? obj2) where T : class
    {
        if (obj1 == null)
            return obj2;

        if (obj2 == null)
            return obj1;

        var json1 = JObject.Parse(ToJson(obj1));
        var json2 = JObject.Parse(ToJson(obj2));

        json1.Merge(json2);

        return json1.ToObject<T>(JsonSerializer.Create(DefaultSettings));
    }

    /// <summary>
    /// Extracts a value from JSON using a JSON path expression.
    /// </summary>
    public static T? ExtractValue<T>(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            var jObject = JObject.Parse(json);
            var token = jObject.SelectToken(path);
            return token?.ToObject<T>();
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a value in JSON using a JSON path expression.
    /// </summary>
    public static string SetValue(string json, string path, object? value)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var jObject = JObject.Parse(json);
            jObject[path] = JToken.FromObject(value);
            return jObject.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set JSON value: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Removes a property from JSON using a JSON path expression.
    /// </summary>
    public static string RemoveProperty(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var jObject = JObject.Parse(json);
            var token = jObject.SelectToken(path);
            token?.Parent?.Remove();
            return jObject.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to remove property: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts JSON from one format to another (e.g., compact to pretty-printed).
    /// </summary>
    public static string Reformat(string json, bool prettyPrint = false)
    {
        try
        {
            var jObject = JObject.Parse(json);
            return jObject.ToString(prettyPrint ? Formatting.Indented : Formatting.None);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that a string is valid JSON.
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JToken.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the size of JSON in bytes when serialized.
    /// </summary>
    public static int GetJsonSize(object? obj)
    {
        if (obj == null)
            return 4; // "null"

        var json = ToJson(obj);
        return System.Text.Encoding.UTF8.GetByteCount(json);
    }

    /// <summary>
    /// Converts a dictionary to JSON.
    /// </summary>
    public static string DictionaryToJson(Dictionary<string, object?> dict, bool prettyPrint = false)
    {
        if (dict == null || dict.Count == 0)
            return "{}";

        return ToJson(dict, prettyPrint);
    }

    /// <summary>
    /// Converts JSON to a dictionary.
    /// </summary>
    public static Dictionary<string, object?>? JsonToDictionary(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object?>>(json, DefaultSettings);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates and sanitizes JSON by removing potentially harmful content.
    /// </summary>
    public static string SanitizeJson(string json, List<string>? pathsToRemove = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var jObject = JObject.Parse(json);

            // Remove sensitive paths if specified
            if (pathsToRemove != null)
            {
                foreach (var path in pathsToRemove)
                {
                    var token = jObject.SelectToken(path);
                    token?.Parent?.Remove();
                }
            }

            return jObject.ToString();
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Compares two JSON objects for equality (ignoring formatting).
    /// </summary>
    public static bool JsonEquals(string json1, string json2)
    {
        try
        {
            var obj1 = JToken.Parse(json1);
            var obj2 = JToken.Parse(json2);
            return JToken.DeepEquals(obj1, obj2);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets differences between two JSON objects.
    /// </summary>
    public static Dictionary<string, (object? OldValue, object? NewValue)> GetJsonDifferences(string json1, string json2)
    {
        var differences = new Dictionary<string, (object?, object?)>();

        try
        {
            var obj1 = JObject.Parse(json1);
            var obj2 = JObject.Parse(json2);

            var allKeys = new HashSet<string>(obj1.Properties().Select(p => p.Name)
                .Concat(obj2.Properties().Select(p => p.Name)));

            foreach (var key in allKeys)
            {
                var val1 = obj1[key]?.ToString();
                var val2 = obj2[key]?.ToString();

                if (val1 != val2)
                {
                    differences[key] = (val1, val2);
                }
            }
        }
        catch
        {
            // Return empty if comparison fails
        }

        return differences;
    }
}
