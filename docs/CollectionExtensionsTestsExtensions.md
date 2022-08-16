# CollectionExtensionsTestsExtensions

Utility extension methods designed to simplify common patterns when working with collections in test scenarios. These helpers provide materialized snapshots of enumerable sequences, safe enumeration counting, and deterministic fallback retrieval—reducing boilerplate in assertions and setup code.

## API

### ToMaterializedList\<T\>

```csharp
public static List<T> ToMaterializedList<T>(this IEnumerable<T> source)
```

Creates a new `List<T>` containing all elements from the source sequence. Forces immediate enumeration, guaranteeing a stable snapshot that can be inspected repeatedly without re-executing the underlying query.

**Parameters:**
- `source` (`IEnumerable<T>`) — The sequence to materialize.

**Returns:** A `List<T>` populated with every element yielded by `source`, in iteration order.

**Throws:** `ArgumentNullException` if `source` is null.

---

### ToMaterializedArray\<T\>

```csharp
public static T[] ToMaterializedArray<T>(this IEnumerable<T> source)
```

Creates a new array containing all elements from the source sequence. Like `ToMaterializedList`, this forces immediate evaluation and provides a fixed-size, indexable snapshot.

**Parameters:**
- `source` (`IEnumerable<T>`) — The sequence to materialize.

**Returns:** A `T[]` containing every element yielded by `source`, in iteration order.

**Throws:** `ArgumentNullException` if `source` is null.

---

### SafeCount\<T\>

```csharp
public static int SafeCount<T>(this IEnumerable<T> source)
```

Returns the number of elements in the sequence without risking multiple enumeration or null-reference exceptions. If the source is null, the count is treated as zero.

**Parameters:**
- `source` (`IEnumerable<T>`) — The sequence to count, or null.

**Returns:** The number of elements in `source`, or `0` if `source` is null.

**Throws:** Does not throw for null input. May throw if the underlying enumerator throws during enumeration.

---

### FirstOrDefaultWithDefault\<T\>

```csharp
public static T FirstOrDefaultWithDefault<T>(this IEnumerable<T> source, T defaultValue)
```

Returns the first element of the sequence, or an explicitly supplied default value when the sequence is empty. Unlike `FirstOrDefault`, which returns the language-defined default for the type, this allows the caller to control the fallback value.

**Parameters:**
- `source` (`IEnumerable<T>`) — The sequence to inspect.
- `defaultValue` (`T`) — The value to return when `source` contains no elements.

**Returns:** The first element of `source`, or `defaultValue` if the sequence is empty.

**Throws:** `ArgumentNullException` if `source` is null.

## Usage

### Example 1: Snapshotting a filtered query for multiple assertions

```csharp
IEnumerable<Order> pendingOrders = orderService.GetPendingOrders();
List<Order> snapshot = pendingOrders.ToMaterializedList();

Assert.Equal(3, snapshot.Count);
Assert.Contains(snapshot, o => o.Status == OrderStatus.Pending);
Assert.All(snapshot, o => Assert.NotNull(o.CustomerId));
```

### Example 2: Safe count with null collections and custom fallback

```csharp
IEnumerable<string> tags = article.Tags; // may be null
int tagCount = tags.SafeCount();
Console.WriteLine($"Article has {tagCount} tags.");

string firstTag = tags.FirstOrDefaultWithDefault("untagged");
Console.WriteLine($"Primary tag: {firstTag}");
```

## Notes

- **Materialization semantics:** `ToMaterializedList` and `ToMaterializedArray` always allocate new collections. They are intended for test code where predictable, repeatable enumeration matters more than memory efficiency. Do not use them in hot paths where deferred execution is desirable.
- **Null handling asymmetry:** `SafeCount` accepts null and returns 0; `FirstOrDefaultWithDefault` throws on null. Callers should guard accordingly or combine `SafeCount` checks before retrieving elements.
- **Thread safety:** None of these methods introduce synchronization. If the underlying `IEnumerable<T>` is modified concurrently during materialization or enumeration, the behavior is undefined and may result in exceptions or inconsistent snapshots.
- **Empty sequences:** `FirstOrDefaultWithDefault` returns the caller-supplied `defaultValue` for empty sequences, which may be `null` for reference types. This is intentional and differs from `FirstOrDefault`’s behavior of returning `default(T)`.
