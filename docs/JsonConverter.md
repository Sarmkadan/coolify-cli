# JsonConverter

A static utility class providing common JSON serialization, deserialization, transformation, and inspection operations. It is designed for scenarios where lightweight, ad-hoc JSON manipulation is required without heavy dependencies, such as configuration updates, dynamic data processing, or schema-agnostic transformations.

## API

### `public static string ToJson<T>(T? value)`

Serializes a given object of type `T` into a JSON string. Uses the default JSON serializer settings.

- **Parameters**:
  - `value` – The object to serialize. Can be `null`.
- **Return value**: A JSON string representation of `value`. Returns `null` if `value` is `null`.
- **Throws**: `System.Text.Json.JsonException` if serialization fails.

---

### `public static T? FromJson<T>(string? json)`

Deserializes a JSON string into an object of type `T`.

- **Parameters**:
  - `json` – The JSON string to deserialize. Can be `null`.
- **Return value**: An instance of type `T`, or `null` if `json` is `null` or deserialization fails.
- **Throws**: `System.Text.Json.JsonException` if deserialization fails.

---

### `public static dynamic? ParseDynamic(string? json)`

Parses a JSON string into a dynamic object using `System.Text.Json`.

- **Parameters**:
  - `json` – The JSON string to parse. Can be `null`.
- **Return value**: A dynamic object representing the parsed JSON, or `null` if `json` is `null` or parsing fails.
- **Throws**: `System.Text.Json.JsonException` if parsing fails.

---

### `public static T? Merge<T>(string? baseJson, string? patchJson)`

Merges two JSON strings by overlaying `patchJson` onto `baseJson`. Uses shallow merge semantics: properties in `patchJson` override those in `baseJson`.

- **Parameters**:
  - `baseJson` – The base JSON string. Can be `null`.
  - `patchJson` – The JSON string containing overrides. Can be `null`.
- **Return value**: A new JSON string representing the merged result, or `null` if either input is `null` or merging fails.
- **Throws**: `System.Text.Json.JsonException` if either input is invalid JSON.

---

### `public static T? ExtractValue<T>(string? json, string propertyPath)`

Extracts a value from a JSON string by navigating a dot-separated property path (e.g., `"user.address.city"`).

- **Parameters**:
  - `json` – The JSON string to query. Can be `null`.
  - `propertyPath` – The dot-separated path to the desired property.
- **Return value**: The extracted value of type `T`, or `null` if `json` is `null`, `propertyPath` is invalid, or the property does not exist.
- **Throws**: `System.Text.Json.JsonException` if `json` is invalid.

---

### `public static string SetValue(string json, string propertyPath, object? newValue)`

Updates a JSON string by setting the value at the specified dot-separated property path. Creates intermediate objects if necessary.

- **Parameters**:
  - `json` – The JSON string to modify. Cannot be `null`.
  - `propertyPath` – The dot-separated path to the property to set.
  - `newValue` – The new value to assign.
- **Return value**: A new JSON string with the updated value.
- **Throws**:
  - `System.ArgumentNullException` if `json` is `null`.
  - `System.Text.Json.JsonException` if `json` is invalid or modification fails.

---

### `public static string RemoveProperty(string json, string propertyPath)`

Removes a property from a JSON string at the specified dot-separated path. Silently ignores missing paths.

- **Parameters**:
  - `json` – The JSON string to modify. Cannot be `null`.
  - `propertyPath` – The dot-separated path to the property to remove.
- **Return value**: A new JSON string with the property removed.
- **Throws**:
  - `System.ArgumentNullException` if `json` is `null`.
  - `System.Text.Json.JsonException` if `json` is invalid.

---
### `public static string Reformat(string? json)`

Reformats a JSON string with consistent indentation and whitespace for readability.

- **Parameters**:
  - `json` – The JSON string to reformat. Can be `null`.
- **Return value**: A reformatted JSON string, or `null` if `json` is `null` or reformatting fails.
- **Throws**: `System.Text.Json.JsonException` if `json` is invalid.

---
### `public static bool IsValidJson(string? json)`

Determines whether a string is valid JSON.

- **Parameters**:
  - `json` – The string to validate. Can be `null`.
- **Return value**: `true` if `json` is valid JSON; otherwise, `false`.

---
### `public static int GetJsonSize(string? json)`

Returns the approximate byte size of a JSON string when encoded in UTF-8.

- **Parameters**:
  - `json` – The JSON string. Can be `null`.
- **Return value**: The size in bytes, or `0` if `json` is `null`.

---
### `public static string DictionaryToJson(Dictionary<string, object?>? dictionary)`

Serializes a dictionary of string-to-object values into a JSON string.

- **Parameters**:
  - `dictionary` – The dictionary to serialize. Can be `null`.
- **Return value**: A JSON string representing the dictionary, or `null` if `dictionary` is `null`.
- **Throws**: `System.Text.Json.JsonException` if serialization fails.

---
### `public static Dictionary<string, object?>? JsonToDictionary(string? json)`

Deserializes a JSON string into a dictionary of string-to-object values. Assumes the JSON object has string keys.

- **Parameters**:
  - `json` – The JSON string to deserialize. Can be `null`.
- **Return value**: A dictionary representing the JSON object, or `null` if `json` is `null` or deserialization fails.
- **Throws**: `System.Text.Json.JsonException` if `json` is invalid or not an object.

---
### `public static string SanitizeJson(string? json)`

Removes control characters and ensures the JSON string is safe for logging or display. Does not alter structure.

- **Parameters**:
  - `json` – The JSON string to sanitize. Can be `null`.
- **Return value**: A sanitized JSON string, or `null` if `json` is `null`.

---
### `public static bool JsonEquals(string? a, string? b)`

Compares two JSON strings for structural and value equality after normalization.

- **Parameters**:
  - `a` – The first JSON string. Can be `null`.
  - `b` – The second JSON string. Can be `null`.
- **Return value**: `true` if both strings represent the same JSON structure and values; otherwise, `false`.

---
### `public static Dictionary<string, (object? OldValue, object? NewValue)> GetJsonDifferences(string? oldJson, string? newJson)`

Computes a shallow diff between two JSON objects. Returns a dictionary mapping property paths to tuples of old and new values. Only includes paths where values differ.

- **Parameters**:
  - `oldJson` – The original JSON string. Can be `null`.
  - `newJson` – The updated JSON string. Can be `null`.
- **Return value**: A dictionary of differences, or `null` if either input is `null` or comparison fails.
- **Throws**: `System.Text.Json.JsonException` if either input is invalid JSON.

## Usage

### Example 1: Configuration Update
