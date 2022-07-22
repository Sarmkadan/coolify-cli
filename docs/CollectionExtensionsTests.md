# CollectionExtensionsTests

`CollectionExtensionsTests` is a unit test class that validates the behavior of extension methods provided by the `CollectionExtensions` utility. Each test method targets a specific scenario—such as null handling, empty collections, batching, deduplication, partitioning, flattening, aggregation, filtering, merging, serialization, and grouping—ensuring the extensions behave correctly under both normal and edge-case conditions.

## API

### `IsNullOrEmpty_WithNullCollection_ReturnsTrue`
Verifies that `IsNullOrEmpty` returns `true` when invoked on a `null` collection reference.

### `IsNullOrEmpty_WithEmptyCollection_ReturnsTrue`
Verifies that `IsNullOrEmpty` returns `true` when invoked on an empty collection (zero elements).

### `IsNullOrEmpty_WithNonEmptyCollection_ReturnsFalse`
Verifies that `IsNullOrEmpty` returns `false` when invoked on a collection containing at least one element.

### `Batch_WithExactMultiple_ProducesEqualSizedBatches`
Confirms that when the source collection size is an exact multiple of the batch size, every produced batch has exactly the requested number of items.

### `Batch_WithRemainder_LastBatchContainsLeftoverItems`
Confirms that when the source collection size is not an exact multiple of the batch size, the final batch contains the remaining items and is smaller than the batch size.

### `Batch_WithZeroSize_ThrowsArgumentException`
Ensures that requesting a batch size of zero throws an `ArgumentException` (or an appropriate derived exception), preventing invalid partitioning.

### `Batch_WithEmptySource_ProducesNoBatches`
Ensures that batching an empty source collection yields an empty sequence (no batches emitted).

### `DistinctBy_RemovesDuplicatesBasedOnKeySelector`
Validates that `DistinctBy` removes elements that produce duplicate keys according to the specified key selector, preserving the first occurrence.

### `Split_PartitionsItemsByPredicate`
Validates that `Split` divides the source collection into two groups: items satisfying the predicate and items not satisfying it, returning both partitions.

### `Flatten_CombinesNestedCollectionsIntoSingleSequence`
Verifies that `Flatten` recursively unwinds nested collections (e.g., `IEnumerable<IEnumerable<T>>`) into a single flat sequence of elements.

### `MaxBy_ReturnsItemWithLargestKeyValue`
Confirms that `MaxBy` returns the element whose key, obtained via the key selector, is the maximum among all elements.

### `MinBy_ReturnsItemWithSmallestKeyValue`
Confirms that `MinBy` returns the element whose key, obtained via the key selector, is the minimum among all elements.

### `MaxBy_WithEmptyCollection_ReturnsDefault`
Ensures that calling `MaxBy` on an empty collection returns the default value for the element type (e.g., `null` for reference types, zero for value types) rather than throwing.

### `WhereNotNull_FiltersOutNullReferences`
Verifies that `WhereNotNull` removes all `null` elements from a sequence, returning only non-null references.

### `Merge_SecondDictionaryValuesOverwriteFirst`
Confirms that when merging two dictionaries, keys present in the second dictionary overwrite the values of matching keys from the first dictionary.

### `Merge_DoesNotModifyOriginalDictionaries`
Ensures that the merge operation produces a new dictionary and leaves both source dictionaries unmodified.

### `ToQueryString_WithMultipleEntries_ProducesAmpersandSeparatedPairs`
Validates that converting a dictionary with multiple key-value pairs to a query string produces an ampersand-separated (`&`) sequence of `key=value` pairs.

### `ToQueryString_WithEmptyDictionary_ReturnsEmptyString`
Ensures that converting an empty dictionary to a query string returns an empty string rather than `null` or a string containing only separators.

### `GroupConsecutive_GroupsAdjacentItemsMeetingCondition`
Validates that `GroupConsecutive` groups only adjacent elements that satisfy a specified condition, creating separate groups when the condition is broken.

## Usage

```csharp
// Example 1: Batching items for bulk processing and merging configuration dictionaries
var items = Enumerable.Range(1, 100);
foreach (var batch in items.Batch(20))
{
    // Process 20 items at a time; last batch handles remainder
    ProcessBatch(batch);
}

var defaultConfig = new Dictionary<string, string> { ["host"] = "localhost", ["port"] = "8080" };
var overrideConfig = new Dictionary<string, string> { ["port"] = "9090" };
var finalConfig = defaultConfig.Merge(overrideConfig);
// finalConfig["port"] == "9090", defaultConfig remains unchanged
```

```csharp
// Example 2: Deduplication by key, splitting, and consecutive grouping
var records = new[] { "apple", "apricot", "banana", "blueberry", "cherry" };
var distinctByFirstLetter = records.DistinctBy(r => r[0]); // "apple", "banana", "cherry"

var (longWords, shortWords) = records.Split(r => r.Length > 5);
// longWords: "apricot", "banana", "blueberry", "cherry"
// shortWords: "apple"

var numbers = new[] { 2, 4, 6, 1, 3, 5, 8, 10 };
var evenGroups = numbers.GroupConsecutive(n => n % 2 == 0);
// Groups: [2,4,6], [1], [3], [5], [8,10]
```

## Notes

- **Null handling**: Methods like `IsNullOrEmpty` and `WhereNotNull` explicitly guard against `null` references. Tests confirm behavior for `null` sources; consumers should still validate inputs where the extension does not tolerate `null`.
- **Empty collections**: Several tests (`Batch_WithEmptySource`, `MaxBy_WithEmptyCollection`, `ToQueryString_WithEmptyDictionary`) verify that empty inputs produce sensible outputs (empty sequences, default values, empty strings) rather than throwing exceptions.
- **Immutability**: The `Merge` test explicitly verifies that original dictionaries are not mutated. This implies the merge produces a new dictionary instance; callers can safely reuse source dictionaries.
- **Batch size validation**: `Batch_WithZeroSize_ThrowsArgumentException` documents that a batch size of zero is invalid. Negative sizes are likely treated similarly, though not explicitly tested here.
- **Thread safety**: These are unit tests for pure extension methods operating on their own input sequences. The underlying extensions are presumed to be stateless and thread-safe when provided with collections that are not concurrently modified during enumeration. No synchronization guarantees are implied for shared mutable sources.
- **Key comparers**: `DistinctBy`, `MaxBy`, and `MinBy` rely on default key comparers unless overloads exist. The tests do not cover custom comparers; behavior with custom equality or ordering is assumed consistent with standard LINQ semantics.
