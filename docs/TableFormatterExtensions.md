# TableFormatterExtensions

Static helper class that provides extension‑style methods for turning collections, dictionaries, and single‑column data into plain‑text tables suitable for console output or logging.

## API

### FormatCollection<T>(IEnumerable<T> items)
- **Purpose:** Produces a multi‑column table where each public property or field of `T` becomes a column, using the default `ToString()` representation for values.
- **Parameters:** `items` – The collection to format.  
- **Return value:** A string containing the formatted table, with rows separated by newline characters.  
- **Throws:** `ArgumentNullException` if `items` is `null`.

### FormatCollection<T>(IEnumerable<T> items, Func<T, string> formatter)
- **Purpose:** Produces a single‑column table where each row is the result of applying `formatter` to an element of the collection.
- **Parameters:**  
  - `items` – The collection to format.  
  - `formatter` – A function that converts each item to its display string.  
- **Return value:** A string containing the formatted table.  
- **Throws:** `ArgumentNullException` if `items` or `formatter` is `null`.

### FormatDictionary(IDictionary dictionary)
- **Purpose:** Produces a two‑column table showing each dictionary entry as a key/value pair.
- **Parameters:** `dictionary` – The dictionary to format.  
- **Return value:** A string containing the formatted table.  
- **Throws:** `ArgumentNullException` if `dictionary` is `null`.

### FormatSingleColumn<T>(IEnumerable<T> items)
- **Purpose:** Produces a single‑column table where each row is the `ToString()` result of an element.
- **Parameters:** `items` – The collection to format.  
- **Return value:** A string containing the formatted table.  
- **Throws:** `ArgumentNullException` if `items` is `null`.

## Usage

```csharp
// Example 1: Format a list of objects with default property columns.
var products = new List<Product>
{
    new Product { Id = 1, Name = "Widget", Price = 9.99m },
    new Product { Id = 2, Name = "Gadget", Price = 14.95m }
};
string table = TableFormatterExtensions.FormatCollection(products);
Console.WriteLine(table);
```

```csharp
// Example 2: Format a dictionary as a key/value table.
var config = new Dictionary<string, object>
{
    ["Timeout"] = 30,
    ["Retries"] = 3,
    ["Enabled"] = true
};
string table = TableFormatterExtensions.FormatDictionary(config);
Console.WriteLine(table);
```

## Notes

- All methods are stateless and do not modify the supplied collections.
- The methods are thread‑safe provided the input collections are not altered while being enumerated.
- If the collection contains `null` elements, the default `ToString()` or the supplied `formatter` will be invoked on `null`, which may raise a `NullReferenceException`. Callers should either ensure elements are non‑null or supply a `formatter` that handles `null` gracefully.
- `FormatCollection<T>` without a formatter uses reflection to discover public properties/fields; performance may degrade with very large collections or types with many members.
- `FormatDictionary` expects non‑null keys and values; `null` keys or values appear as empty cells in the output.
- The returned string uses `\n` for line breaks; adjust to environment‑specific line endings if necessary.
