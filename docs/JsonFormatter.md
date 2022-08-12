# JsonFormatter

`JsonFormatter` provides a set of utilities for formatting, transforming, and inspecting JSON strings. It is designed for command-line tooling scenarios where JSON payloads need to be minified, prettified, restructured, or have specific fields extracted without external dependencies beyond the standard library.

## API

### `public JsonFormatter`

Default parameterless constructor. Creates a new instance of the formatter with no initial state. No configuration is required before calling any instance method.

### `public string Format(string json)`

Applies a default, opinionated formatting to the provided JSON string. The exact style (indentation, line breaks, key ordering) is implementation-defined but intended to produce human-readable output.

- **Parameters**: `json` — a valid JSON string.
- **Returns**: the formatted JSON string.
- **Throws**: `JsonReaderException` or equivalent if `json` is not syntactically valid JSON.

### `public string FormatCollection<T>(IEnumerable<T> collection)`

Serializes a generic collection of objects into a formatted JSON array. Each element is serialized using the default JSON serialization rules for its type.

- **Parameters**: `collection` — an enumerable sequence of objects of type `T`.
- **Returns**: a JSON array string containing the serialized elements.
- **Throws**: `JsonSerializationException` or equivalent if any element cannot be serialized.

### `public string FormatDictionary(IDictionary<string, object> dictionary)`

Serializes a dictionary with string keys and object values into a formatted JSON object.

- **Parameters**: `dictionary` — a dictionary where each key is a string and each value is an object.
- **Returns**: a JSON object string representing the dictionary.
- **Throws**: `JsonSerializationException` or equivalent if any value cannot be serialized.

### `public string ReformatJson(string json, string sourceFormat, string targetFormat)`

Converts a JSON string from one structural convention to another. The `sourceFormat` and `targetFormat` parameters accept format identifiers (e.g., `"camelCase"`, `"PascalCase"`, `"snake_case"`) to control key casing and structural conventions.

- **Parameters**:
  - `json` — a valid JSON string.
  - `sourceFormat` — identifier for the current format convention.
  - `targetFormat` — identifier for the desired output convention.
- **Returns**: the JSON string transformed to the target convention.
- **Throws**: `ArgumentException` if either format identifier is unrecognized; `JsonReaderException` if `json` is invalid.

### `public string Minify(string json)`

Removes all insignificant whitespace from the JSON string, producing the most compact representation possible.

- **Parameters**: `json` — a valid JSON string.
- **Returns**: a single-line JSON string with no unnecessary whitespace.
- **Throws**: `JsonReaderException` if `json` is not syntactically valid JSON.

### `public string Prettify(string json)`

Adds indentation and line breaks to a JSON string to maximize readability. Typically uses a standard indentation width (e.g., two or four spaces) and places each object property and array element on its own line.

- **Parameters**: `json` — a valid JSON string.
- **Returns**: an indented, multi-line JSON string.
- **Throws**: `JsonReaderException` if `json` is not syntactically valid JSON.

### `public string? ExtractField(string json, string fieldPath)`

Navigates into a JSON structure using a path expression and returns the value at that location as a raw JSON string. Returns `null` if the field does not exist.

- **Parameters**:
  - `json` — a valid JSON string.
  - `fieldPath` — a dot-separated or bracket-notation path (e.g., `"address.city"` or `"items[0].name"`).
- **Returns**: the JSON value at the specified path as a string, or `null` if the path cannot be resolved.
- **Throws**: `JsonReaderException` if `json` is invalid; `ArgumentException` if `fieldPath` is malformed.

## Usage

### Example 1: Minifying and Prettifying

```csharp
var formatter = new JsonFormatter();

string original = @"
{
    ""name"": ""coolify-cli"",
    ""version"": ""1.0.0""
}";

string minified = formatter.Minify(original);
Console.WriteLine(minified);
// Output: {"name":"coolify-cli","version":"1.0.0"}

string pretty = formatter.Prettify(minified);
Console.WriteLine(pretty);
// Output:
// {
//   "name": "coolify-cli",
//   "version": "1.0.0"
// }
```

### Example 2: Extracting a Field and Reformatting

```csharp
var formatter = new JsonFormatter();

string configJson = @"
{
    ""ServerSettings"": {
        ""HostName"": ""localhost"",
        ""PortNumber"": 8080
    }
}";

string? host = formatter.ExtractField(configJson, "ServerSettings.HostName");
Console.WriteLine(host);
// Output: "localhost"

string reformatted = formatter.ReformatJson(configJson, "PascalCase", "camelCase");
Console.WriteLine(reformatted);
// Output:
// {
//   "serverSettings": {
//     "hostName": "localhost",
//     "portNumber": 8080
//   }
// }
```

## Notes

- All methods that accept a `json` parameter require the input to be well-formed JSON. Malformed input results in a `JsonReaderException` (or an equivalent parsing exception) being thrown. Callers should validate or sanitize input when the source is untrusted.
- `ExtractField` returns `null` for genuinely missing fields, not for empty string values or `null` JSON tokens. A field whose value is the JSON literal `null` will return the string `"null"`, not a C# `null` reference.
- `ReformatJson` performs structural key transformation only; it does not alter value types or reorder properties beyond what the target convention implies.
- `FormatCollection<T>` and `FormatDictionary` rely on the default serialization behavior of the underlying runtime. Complex object graphs, circular references, or non-serializable types will cause exceptions at serialization time.
- The class is stateless. All methods are safe to call concurrently from multiple threads without external synchronization.
