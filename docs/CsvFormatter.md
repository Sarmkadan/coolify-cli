# CsvFormatter

A utility class for converting between C# objects and CSV-formatted strings, supporting both single-object formatting and collection serialization, as well as parsing CSV strings back into structured dictionaries.

## API

### `public CsvFormatter`

Default constructor. Initializes a new instance of the `CsvFormatter` class with default settings.

### `public string Format(object obj)`

Serializes a single object into a CSV-formatted string.

- **Parameters**:
  - `obj`: The object to serialize. Must be non-null.
- **Return value**: A string containing the CSV representation of the object.
- **Exceptions**:
  - Throws `ArgumentNullException` if `obj` is `null`.
  - Throws `InvalidOperationException` if the object cannot be converted to a dictionary of key-value pairs.

### `public string FormatCollection<T>(IEnumerable<T> collection)`

Serializes a collection of objects into a CSV-formatted string, including a header row.

- **Parameters**:
  - `collection`: The collection of objects to serialize. Must be non-null and contain only non-null elements.
- **Return value**: A string containing the CSV representation of the collection, with headers derived from the first object's properties.
- **Exceptions**:
  - Throws `ArgumentNullException` if `collection` is `null`.
  - Throws `InvalidOperationException` if the collection is empty or if any element cannot be converted to a dictionary of key-value pairs.

### `public string FormatDictionary(Dictionary<string, string> dictionary)`

Serializes a dictionary of string key-value pairs into a single CSV-formatted line.

- **Parameters**:
  - `dictionary`: The dictionary to serialize. Must be non-null.
- **Return value**: A string containing the CSV representation of the dictionary.
- **Exceptions**:
  - Throws `ArgumentNullException` if `dictionary` is `null`.

### `public List<Dictionary<string, string>> ParseCsv(string csv)`

Parses a CSV-formatted string into a list of dictionaries, where each dictionary represents a row with column names as keys.

- **Parameters**:
  - `csv`: The CSV string to parse. Must be non-null.
- **Return value**: A list of dictionaries, each mapping column names to cell values. Empty list if the input is empty.
- **Exceptions**:
  - Throws `ArgumentNullException` if `csv` is `null`.

## Usage

### Example 1: Formatting a single object
