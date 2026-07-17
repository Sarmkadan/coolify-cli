# TemplateDiffFormatterJsonExtensions

Provides JSON serialization and deserialization extensions for template-related result types, enabling conversion of `TemplateDiffResult`, `TemplateApplyResult`, and `TemplateValidationResult` objects to and from their JSON string representations. This class is part of the `coolify-cli` project and serves as a bridge between the domain model and JSON-based persistence or transport layers.

## API

### ToJson (TemplateDiffResult)

```csharp
public static string ToJson(this TemplateDiffResult result)
```

Serializes a `TemplateDiffResult` instance to its JSON string representation.

**Parameters:**
- `result` — The `TemplateDiffResult` to serialize. Must not be `null`.

**Returns:** A JSON string representing the diff result.

**Throws:** `ArgumentNullException` when `result` is `null`. May throw `JsonException` if serialization fails due to an object graph that cannot be mapped to JSON.

---

### ToJson (TemplateApplyResult)

```csharp
public static string ToJson(this TemplateApplyResult result)
```

Serializes a `TemplateApplyResult` instance to its JSON string representation.

**Parameters:**
- `result` — The `TemplateApplyResult` to serialize. Must not be `null`.

**Returns:** A JSON string representing the apply result.

**Throws:** `ArgumentNullException` when `result` is `null`. May throw `JsonException` if serialization encounters an unsupported type or structure.

---

### ToJson (TemplateValidationResult)

```csharp
public static string ToJson(this TemplateValidationResult result)
```

Serializes a `TemplateValidationResult` instance to its JSON string representation.

**Parameters:**
- `result` — The `TemplateValidationResult` to serialize. Must not be `null`.

**Returns:** A JSON string representing the validation result.

**Throws:** `ArgumentNullException` when `result` is `null`. May throw `JsonException` if serialization fails.

---

### FromJsonToDiffResult

```csharp
public static TemplateDiffResult? FromJsonToDiffResult(this string json)
```

Deserializes a JSON string into a `TemplateDiffResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Can be `null` or empty.

**Returns:** A `TemplateDiffResult` instance if deserialization succeeds; `null` if the input is `null`, empty, or whitespace, or if the JSON is invalid.

**Throws:** Does not throw exceptions for invalid JSON; returns `null` instead.

---

### FromJsonToApplyResult

```csharp
public static TemplateApplyResult? FromJsonToApplyResult(this string json)
```

Parses a JSON string into a `TemplateApplyResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Can be `null` or empty.

**Returns:** A `TemplateApplyResult` instance if deserialization succeeds; `null` if the input is `null`, empty, or whitespace, or when the JSON is malformed.

**Throws:** Does not throw exceptions for invalid JSON; returns `null` instead.

---

### FromJsonToValidationResult

```csharp
public static TemplateValidationResult? FromJsonToValidationResult(this string json)
```

Parses a JSON string into a `TemplateValidationResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Can be `null` or empty.

**Returns:** A `TemplateValidationResult` instance if deserialization succeeds; `null` if the input is `null`, empty, or whitespace, or when the JSON cannot be parsed.

**Throws:** Does not throw exceptions for invalid JSON; returns `null` instead.

---

### TryFromJson (TemplateDiffResult)

```csharp
public static bool TryFromJson(this string json, out TemplateDiffResult? result)
```

Attempts to parse a JSON string into a `TemplateDiffResult`, returning a success indicator rather than throwing.

**Parameters:**
- `json` — The JSON string to parse. Can be `null` or empty.
- `result` — When this method returns `true`, contains the deserialized `TemplateDiffResult`; when `false`, contains `null`.

**Returns:** `true` if the JSON was successfully parsed into a `TemplateDiffResult`; `false` if the input is `null`, empty, whitespace, or invalid JSON.

**Throws:** Does not throw exceptions.

---

### TryFromJson (TemplateApplyResult)

```csharp
public static bool TryFromJson(this string json, out TemplateApplyResult? result)
```

Attempts to parse a JSON string into a `TemplateApplyResult`, returning a boolean indicating success.

**Parameters:**
- `json` — The JSON string to parse. Can be `null` or empty.
- `result` — When this method returns `true`, contains the deserialized `TemplateApplyResult`; when `false`, contains `null`.

**Returns:** `true` if parsing succeeded; `false` otherwise.

**Throws:** Does not throw exceptions.

---

### TryFromJson (TemplateValidationResult)

```csharp
public static bool TryFromJson(this string json, out TemplateValidationResult? result)
```

Attempts to parse a JSON string into a `TemplateValidationResult`, returning a boolean indicating success.

**Parameters:**
- `json` — The JSON string to parse. Can be `null` or empty.
- `result` — When this method returns `true`, contains the deserialized `TemplateValidationResult`; when `false`, contains `null`.

**Returns:** `true` if parsing succeeded; `false` otherwise.

**Throws:** Does not throw exceptions.

## Usage

### Example 1: Round-Trip Serialization and Deserialization

```csharp
using coolify_cli.Extensions;

// Create a diff result from some template comparison
TemplateDiffResult originalDiff = templateService.Compare(source, target);

// Serialize to JSON for storage or transmission
string json = originalDiff.ToJson();

// Later, deserialize back to an object
TemplateDiffResult? restoredDiff = json.FromJsonToDiffResult();

if (restoredDiff is not null)
{
    Console.WriteLine($"Restored diff has {restoredDiff.Entries.Count} entries.");
}
```

### Example 2: Safe Parsing with TryFromJson

```csharp
using coolify_cli.Extensions;

string incomingJson = GetJsonFromApiResponse();

// Attempt to parse without risking an exception
if (incomingJson.TryFromJson(out TemplateApplyResult? applyResult))
{
    // applyResult is guaranteed non-null here
    Console.WriteLine($"Apply result status: {applyResult.Status}");
}
else
{
    Console.WriteLine("Invalid or empty JSON received; skipping processing.");
}
```

## Notes

- All `FromJson*` methods return `null` for `null`, empty, or whitespace input, making them safe to call without prior validation. They do not throw exceptions for malformed JSON.
- The `TryFromJson` overloads provide a non-exception-based parsing path, suitable for high-throughput scenarios where invalid JSON is expected and performance is a concern.
- The `ToJson` methods throw `ArgumentNullException` when passed `null`, so callers must guard against null references before serialization.
- These methods are static extension methods and do not maintain any internal state. They are inherently thread-safe and can be called concurrently from multiple threads without synchronization.
- The underlying JSON serializer configuration (casing, naming policy, indentation) is determined by the implementation within the extension class and is not configurable through these method signatures. Callers should not rely on specific formatting details unless documented elsewhere.
- Deserialized objects are independent instances; modifying a deserialized result does not affect the original object or any cached state.
