// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CoolifiCli.Utilities;

/// <summary>
/// JSON conversion utilities for serialization, deserialization, and transformation.
/// Handles type-safe conversions with error handling and formatting options.
/// </summary>
public static class JsonConverter
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    public static string ToJson<T>(T? obj, bool prettyPrint = false)
    {
        if (obj == null)
            return "null";

        return JsonSerializer.Serialize(obj, prettyPrint ? PrettyOptions : DefaultOptions);
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
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
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
            return JsonNode.Parse(json);
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

        var node1 = JsonNode.Parse(ToJson(obj1))?.AsObject();
        var node2 = JsonNode.Parse(ToJson(obj2))?.AsObject();

        if (node1 == null) return obj2;
        if (node2 == null) return obj1;

        foreach (var kvp in node2)
            node1[kvp.Key] = kvp.Value?.DeepClone();

        return JsonSerializer.Deserialize<T>(node1.ToJsonString(), DefaultOptions);
    }

    /// <summary>
    /// Extracts a value from JSON using a dot-notation path expression.
    /// </summary>
    public static T? ExtractValue<T>(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            JsonNode? node = JsonNode.Parse(json);
            foreach (var part in path.Split('.'))
            {
                if (node is JsonObject obj)
                    node = obj[part];
                else
                    return default;
            }

            return node.Deserialize<T>(DefaultOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a value in JSON by key.
    /// </summary>
    public static string SetValue(string json, string path, object? value)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var jObject = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
            jObject[path] = value != null ? JsonSerializer.SerializeToNode(value, DefaultOptions) : null;
            return jObject.ToJsonString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set JSON value: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Removes a property from JSON by key.
    /// </summary>
    public static string RemoveProperty(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var jObject = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
            jObject.Remove(path);
            return jObject.ToJsonString();
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
            var node = JsonNode.Parse(json);
            return node?.ToJsonString(prettyPrint ? PrettyOptions : DefaultOptions) ?? "{}";
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
            JsonDocument.Parse(json).Dispose();
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
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, DefaultOptions);
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
            var jObject = JsonNode.Parse(json)?.AsObject();
            if (jObject == null) return json;

            if (pathsToRemove != null)
            {
                foreach (var path in pathsToRemove)
                    jObject.Remove(path);
            }

            return jObject.ToJsonString();
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
            using var doc1 = JsonDocument.Parse(json1);
            using var doc2 = JsonDocument.Parse(json2);
            return JsonSerializer.Serialize(doc1.RootElement) == JsonSerializer.Serialize(doc2.RootElement);
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
            var obj1 = JsonNode.Parse(json1)?.AsObject();
            var obj2 = JsonNode.Parse(json2)?.AsObject();

            if (obj1 == null || obj2 == null) return differences;

            var allKeys = new HashSet<string>(
                obj1.Select(p => p.Key).Concat(obj2.Select(p => p.Key)));

            foreach (var key in allKeys)
            {
                var val1 = obj1[key]?.ToJsonString();
                var val2 = obj2[key]?.ToJsonString();

                if (val1 != val2)
                    differences[key] = (val1, val2);
            }
        }
        catch
        {
            // Return empty if comparison fails
        }

        return differences;
    }
}
