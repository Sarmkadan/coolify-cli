# CollectionExtensions

A utility class providing common and advanced operations for working with collections, lists, dictionaries, and enumerables in C#. It offers methods for filtering, grouping, transforming, and querying collections with concise syntax, reducing boilerplate code and improving readability.

## API

### `IsNullOrEmpty<T>(IEnumerable<T>? collection)`
Determines whether a collection is either `null` or empty.

- **Parameters**:
  - `collection` (`IEnumerable<T>?`): The collection to check.
- **Returns**: `true` if the collection is `null` or has no elements; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---

### `Batch<T>(IEnumerable<T> source, int size)`
Splits the source sequence into batches (subsequences) of the specified size.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence to batch.
  - `size` (`int`): The maximum number of elements per batch. Must be positive.
- **Returns**: An `IEnumerable<List<T>>` containing batches of elements.
- **Throws**: `ArgumentOutOfRangeException` if `size <= 0`.

---

### `DistinctBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`
Returns distinct elements from the source sequence based on a key selector function.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `keySelector` (`Func<T, TKey>`): A function to extract the key for each element.
- **Returns**: An `IEnumerable<T>` with duplicates removed based on the key.
- **Throws**: `ArgumentNullException` if `source` or `keySelector` is `null`.

---

### `Split<T>(IEnumerable<T> source, Func<T, bool> predicate)`
Splits the source sequence into two lists: elements that match the predicate and those that do not.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `predicate` (`Func<T, bool>`): A function to test each element.
- **Returns**: A tuple `(List<T> Matches, List<T> NonMatches)` containing two lists.
- **Throws**: `ArgumentNullException` if `source` or `predicate` is `null`.

---
### `Flatten<T>(IEnumerable<IEnumerable<T>> source)`
Flattens a sequence of sequences into a single sequence.

- **Parameters**:
  - `source` (`IEnumerable<IEnumerable<T>>`): The source sequence of sequences.
- **Returns**: An `IEnumerable<T>` containing all elements from all inner sequences.
- **Throws**: `ArgumentNullException` if `source` is `null`.

---
### `Partition<T>(IEnumerable<T> source, int size)`
Partitions the source sequence into lists of the specified size, with the last partition possibly smaller.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `size` (`int`): The maximum number of elements per partition. Must be positive.
- **Returns**: A `List<List<T>>` containing the partitioned lists.
- **Throws**: `ArgumentOutOfRangeException` if `size <= 0`.

---
### `SkipWhile<T>(IEnumerable<T> source, Func<T, bool> predicate)`
Bypasses elements in the source sequence while the specified predicate returns `true`, then returns the remaining elements.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `predicate` (`Func<T, bool>`): A function to test each element.
- **Returns**: An `IEnumerable<T>` containing the remaining elements after the predicate fails.
- **Throws**: `ArgumentNullException` if `source` or `predicate` is `null`.

---
### `OrderByDescending<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`
Sorts the elements of a sequence in descending order according to a key.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `keySelector` (`Func<T, TKey>`): A function to extract the key for sorting.
- **Returns**: An `IEnumerable<T>` sorted in descending order by the key.
- **Throws**: `ArgumentNullException` if `source` or `keySelector` is `null`.

---
### `MaxBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`
Returns the maximum element in the sequence based on the specified key selector.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `keySelector` (`Func<T, TKey>`): A function to extract the key for comparison.
- **Returns**: The element with the maximum key value, or `null` if the sequence is empty.
- **Throws**: `ArgumentNullException` if `source` or `keySelector` is `null`.

---
### `MinBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`
Returns the minimum element in the sequence based on the specified key selector.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
  - `keySelector` (`Func<T, TKey>`): A function to extract the key for comparison.
- **Returns**: The element with the minimum key value, or `null` if the sequence is empty.
- **Throws**: `ArgumentNullException` if `source` or `keySelector` is `null`.

---
### `GroupConsecutive<T>(IEnumerable<T> source)`
Groups consecutive identical elements in the source sequence.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence.
- **Returns**: An `IEnumerable<List<T>>` where each list contains consecutive identical elements.
- **Throws**: `ArgumentNullException` if `source` is `null`.

---
### `WhereNotNull<T>(IEnumerable<T?> source)`
Filters out `null` elements from the source sequence.

- **Parameters**:
  - `source` (`IEnumerable<T?>`): The source sequence, potentially containing `null` values.
- **Returns**: An `IEnumerable<T>` containing only non-null elements.
- **Throws**: `ArgumentNullException` if `source` is `null`.

---
### `GetAtIndexOrDefault<T>(IList<T> list, int index)`
Returns the element at the specified index or the default value if the index is out of range.

- **Parameters**:
  - `list` (`IList<T>`): The list to access.
  - `index` (`int`): The zero-based index of the element to retrieve.
- **Returns**: The element at the specified index, or `default(T)` if the index is invalid.
- **Throws**: Does not throw exceptions.

---
### `Shuffle<T>(IEnumerable<T> source)`
Returns a new sequence with the elements of the source sequence in random order.

- **Parameters**:
  - `source` (`IEnumerable<T>`): The source sequence to shuffle.
- **Returns**: An `IEnumerable<T>` with elements in random order.
- **Throws**: `ArgumentNullException` if `source` is `null`.

---
### `ToQueryString<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> pairs)`
Converts a sequence of key-value pairs into a URL query string format (e.g., `key1=value1&key2=value2`).

- **Parameters**:
  - `pairs` (`IEnumerable<KeyValuePair<TKey, TValue>>`): The sequence of key-value pairs.
- **Returns**: A `string` representing the query string.
- **Throws**: `ArgumentNullException` if `pairs` is `null`.

---
### `ToKeyValueString<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> pairs)`
Converts a sequence of key-value pairs into a string in the format `key=value` (e.g., `key1=value1,key2=value2`).

- **Parameters**:
  - `pairs` (`IEnumerable<KeyValuePair<TKey, TValue>>`): The sequence of key-value pairs.
- **Returns**: A `string` representing the key-value pairs.
- **Throws**: `ArgumentNullException` if `pairs` is `null`.

---
### `Merge<TKey, TValue>(params IDictionary<TKey, TValue>[] dictionaries)`
Merges multiple dictionaries into a single dictionary. Later dictionaries overwrite values from earlier ones for duplicate keys.

- **Parameters**:
  - `dictionaries` (`params IDictionary<TKey, TValue>[]`): The dictionaries to merge.
- **Returns**: A `Dictionary<TKey, TValue>` containing all key-value pairs from the input dictionaries.
- **Throws**: `ArgumentNullException` if `dictionaries` is `null` or any element is `null`.

## Usage
