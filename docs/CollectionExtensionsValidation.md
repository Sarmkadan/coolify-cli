# CollectionExtensionsValidation

Provides extension methods for validating collections and dictionaries. The methods return validation error messages, a boolean validity flag, or throw an exception when a collection fails to meet validation rules defined elsewhere in the codebase.

## API

### Validate(IEnumerable source)
- **Purpose:** Returns a read‑only list of validation error messages for the supplied enumerable collection.
- **Parameters:** `source` – The collection to validate.
- **Return value:** An `IReadOnlyList<string>` containing zero or more error messages; an empty list indicates the collection is valid.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### Validate(ICollection source)
- **Purpose:** Returns a read‑only list of validation error messages for the supplied collection.
- **Parameters:** `source` – The collection to validate.
- **Return value:** An `IReadOnlyList<string>` of validation errors; empty when the collection passes validation.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### Validate(IList source)
- **Purpose:** Returns a read‑only list of validation error messages for the supplied list.
- **Parameters:** `source` – The list to validate.
- **Return value:** An `IReadOnlyList<string>` of validation errors; empty when the list is valid.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### Validate(IDictionary source)
- **Purpose:** Returns a read‑only list of validation error messages for the supplied dictionary.
- **Parameters:** `source` – The dictionary to validate.
- **Return value:** An `IReadOnlyList<string>` of validation errors; empty when the dictionary is valid.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### Validate<TKey,TValue>(IDictionary<TKey,TValue> source)
- **Purpose:** Returns a read‑only list of validation error messages for the supplied generic dictionary.
- **Parameters:** `source` – The dictionary to validate.
- **Return value:** An `IReadOnlyList<string>` of validation errors; empty when the dictionary is valid.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### IsValid(IEnumerable source)
- **Purpose:** Indicates whether the supplied enumerable collection passes validation.
- **Parameters:** `source` – The collection to check.
- **Return value:** `true` if the collection has no validation errors; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### IsValid(ICollection source)
- **Purpose:** Indicates whether the supplied collection passes validation.
- **Parameters:** `source` – The collection to check.
- **Return value:** `true` if the collection has no validation errors; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### IsValid(IList source)
- **Purpose:** Indicates whether the supplied list passes validation.
- **Parameters:** `source` – The list to check.
- **Return value:** `true` if the list has no validation errors; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### IsValid(IDictionary source)
- **Purpose:** Indicates whether the supplied dictionary passes validation.
- **Parameters:** `source` – The dictionary to check.
- **Return value:** `true` if the dictionary has no validation errors; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### IsValid<TKey,TValue>(IDictionary<TKey,TValue> source)
- **Purpose:** Indicates whether the supplied generic dictionary passes validation.
- **Parameters:** `source` – The dictionary to check.
- **Return value:** `true` if the dictionary has no validation errors; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` if `source` is `null`.

### EnsureValid(IEnumerable source)
- **Purpose:** Validates the supplied enumerable collection and throws an exception if validation fails.
- **Parameters:** `source` – The collection to validate.
- **Return value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` containing the concatenated validation error messages if the collection is invalid.

### EnsureValid(ICollection source)
- **Purpose:** Validates the supplied collection and throws an exception if validation fails.
- **Parameters:** `source` – The collection to validate.
- **Return value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` containing the concatenated validation error messages if the collection is invalid.

### EnsureValid(IList source)
- **Purpose:** Validates the supplied list and throws an exception if validation fails.
- **Parameters:** `source` – The list to validate.
- **Return value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` containing the concatenated validation error messages if the list is invalid.

### EnsureValid(IDictionary source)
- **Purpose:** Validates the supplied dictionary and throws an exception if validation fails.
- **Parameters:** `source` – The dictionary to validate.
- **Return value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` containing the concatenated validation error messages if the dictionary is invalid.

### EnsureValid<TKey,TValue>(IDictionary<TKey,TValue> source)
- **Purpose:** Validates the supplied generic dictionary and throws an exception if validation fails.
- **Parameters:** `source` – The dictionary to validate.
- **Return value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` containing the concatenated validation error messages if the dictionary is invalid.

## Usage

```csharp
using CoolifyCli.Extensions; // assuming the extension methods are in this namespace

var numbers = new List<int> { 1, 2, 3 };
var errors = numbers.Validate(); // returns IReadOnlyList<string>
if (errors.Count > 0)
{
    foreach var e in errors
        Console.WriteLine(e);
}
```

```csharp
var settings = new Dictionary<string, string>
{
    ["host"] = "example.com",
    ["port"] = "8080"
};

settings.EnsureValid(); // throws InvalidOperationException if any validation rule fails
```

## Notes

- All methods treat a `null` input as an immediate error and throw `ArgumentNullException`; they do not consider `null` as a valid collection state.
- The validation logic itself is defined elsewhere; these extensions merely surface the results.
- The methods are safe to call concurrently from multiple threads as long as the source collection is not modified during the call; they do not retain any internal state.
- `EnsureValid` aggregates all validation messages into a single `InvalidOperationException` message, preserving the order in which the underlying validator returns them.
