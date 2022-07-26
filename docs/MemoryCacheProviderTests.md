# MemoryCacheProviderTests

The `MemoryCacheProviderTests` class serves as the comprehensive test suite for validating the functionality, reliability, and edge-case handling of the `MemoryCacheProvider` implementation within the `coolify-cli` project. It encapsulates a series of unit tests designed to verify core caching operations such as storage, retrieval, expiration management, and collection manipulation, ensuring that the underlying cache provider adheres to expected behavioral contracts under various conditions.

## API

### `public MemoryCacheProviderTests()`
Initializes a new instance of the `MemoryCacheProviderTests` class. This constructor typically sets up the necessary test fixtures, including instantiating the `MemoryCacheProvider` instance to be tested and preparing any required state before individual test methods execute.

### `public void Dispose()`
Releases unmanaged resources and performs cleanup operations associated with the test class. This method is invoked to ensure that the cache provider and any related resources are properly disposed of after the test suite execution completes, preventing resource leaks during test runs.

### `public void Set_AndGet_ReturnsStoredValue()`
Verifies that a value stored in the cache using a specific key can be successfully retrieved with the same key. It asserts that the retrieved object matches the originally stored instance. No parameters are passed directly; the test utilizes internal fixture data. It throws an assertion exception if the retrieved value differs from the stored value or is null.

### `public void Get_WhenKeyAbsent_ReturnsDefault()`
Validates the behavior of the `Get` method when requesting a key that does not exist in the cache. It asserts that the method returns the default value for the generic type (typically `null` for reference types) rather than throwing an exception. Throws an assertion exception if a non-default value is returned.

### `public void TryGet_WhenKeyPresent_ReturnsTrueAndValue()`
Tests the `TryGet` method scenario where the requested key exists. It confirms that the method returns `true` and outputs the correct stored value via the `out` parameter. Throws an assertion exception if the return value is `false` or the output value is incorrect.

### `public void TryGet_WhenKeyAbsent_ReturnsFalse()`
Tests the `TryGet` method scenario where the requested key is missing. It confirms that the method returns `false` and sets the output parameter to the default value. Throws an assertion exception if the return value is `true`.

### `public void Exists_WhenKeyPresent_ReturnsTrue()`
Asserts that the `Exists` method returns `true` when queried with a key currently held in the cache. Throws an assertion exception if the method returns `false`.

### `public void Exists_WhenKeyAbsent_ReturnsFalse()`
Asserts that the `Exists` method returns `false` when queried with a key that is not present in the cache. Throws an assertion exception if the method returns `true`.

### `public void Remove_DeletesEntryFromCache()`
Verifies that calling `Remove` with a valid key deletes the associated entry. The test subsequently checks that `Exists` returns `false` and `Get` returns the default value for that key. Throws an assertion exception if the entry persists after removal.

### `public void Clear_RemovesAllEntries()`
Validates that the `Clear` method empties the cache completely. It typically populates the cache with multiple entries, invokes `Clear`, and asserts that the count is zero and no keys exist. Throws an assertion exception if any entries remain.

### `public void Count_ReflectsNumberOfStoredEntries()`
Ensures that the `Count` property accurately reports the number of items currently stored in the cache. It tests increments upon adding items and decrements upon removal. Throws an assertion exception if the reported count does not match the actual number of stored items.

### `public void Set_WithExpiredTtl_ReturnsNullOnGet()`
Tests time-to-live (TTL) enforcement by setting an entry with an immediate or past expiration time. It asserts that a subsequent `Get` operation returns the default value, confirming the entry is treated as expired. Throws an assertion exception if the expired value is still retrievable.

### `public void Exists_AfterEntryExpires_ReturnsFalse()`
Validates that the `Exists` method correctly returns `false` for an entry that has passed its expiration threshold. Throws an assertion exception if `Exists` returns `true` for an expired key.

### `public void GetOrAdd_WhenKeyAbsent_InvokesFactoryAndCachesResult()`
Tests the `GetOrAdd` method when the key is missing. It verifies that the provided factory function is invoked, the resulting value is returned, and that the value is subsequently stored in the cache. Throws an assertion exception if the factory is not called or the value is not cached.

### `public async Task GetOrAddAsync_WhenKeyAbsent_InvokesAsyncFactoryAndCachesResult()`
Asynchronously tests the `GetOrAddAsync` method behavior for missing keys. It ensures the asynchronous factory is awaited, the result is returned, and the value is cached for future synchronous or asynchronous retrieval. Throws an assertion exception if the async flow fails to cache the result or invoke the factory.

### `public void GetAllKeys_ReturnsAllStoredKeys()`
Verifies that the `GetAllKeys` method returns a collection containing every key currently stored in the cache. It asserts that the returned collection size matches the cache count and contains all expected keys. Throws an assertion exception if keys are missing or extra keys are present.

### `public void SetExpiration_OnExistingEntry_RespectsNewExpiration()`
Tests the ability to update the expiration time of an existing entry. It sets a new TTL on a stored item and verifies that the item expires according to the new timeline rather than the original one. Throws an assertion exception if the original expiration time persists.

### `public void Set_OverwritesExistingEntry()`
Validates that calling `Set` with a key that already exists updates the stored value and resets any associated expiration metadata. It asserts that retrieving the key yields the new value. Throws an assertion exception if the old value persists.

## Usage

The following examples demonstrate how the test cases within `MemoryCacheProviderTests` conceptually map to usage patterns for the `MemoryCacheProvider` being tested.

### Example 1: Basic Storage and Retrieval with Expiration
This pattern illustrates the core workflow verified by `Set_AndGet_ReturnsStoredValue` and `Set_WithExpiredTtl_ReturnsNullOnGet`.

```csharp
using System;
using coolify_cli.Caching;

public class CacheUsageExample
{
    public void DemonstrateBasicFlow()
    {
        var cache = new MemoryCacheProvider();
        string key = "user_profile_123";
        var data = new { Id = 123, Name = "Alice" };

        // Set value with a 5-minute expiration
        cache.Set(key, data, TimeSpan.FromMinutes(5));

        // Retrieve value
        var retrieved = cache.Get<dynamic>(key);
        if (retrieved != null)
        {
            Console.WriteLine($"Retrieved: {retrieved.Name}");
        }

        // Simulate expiration logic check (verified in tests)
        // In a real scenario, time passes, and the next get returns null
        // cache.Set(key, data, TimeSpan.FromMilliseconds(0)); 
        // var expired = cache.Get<dynamic>(key); // Returns null
    }
}
```

### Example 2: Atomic Get-Or-Add Pattern
This pattern reflects the behavior validated by `GetOrAdd_WhenKeyAbsent_InvokesFactoryAndCachesResult` and `GetOrAddAsync_WhenKeyAbsent_InvokesAsyncFactoryAndCachesResult`.

```csharp
using System;
using System.Threading.Tasks;
using coolify_cli.Caching;

public class AtomicCacheExample
{
    private readonly MemoryCacheProvider _cache;

    public AtomicCacheExample()
    {
        _cache = new MemoryCacheProvider();
    }

    public async Task<string> GetDataAsync(string key)
    {
        // Ensures the factory is only invoked if the key is missing
        return await _cache.GetOrAddAsync(key, async () =>
        {
            // Simulate expensive I/O operation
            await Task.Delay(100);
            return $"GeneratedDataFor_{key}";
        }, TimeSpan.FromMinutes(10));
    }

    public void GetDataSync(string key)
    {
        var result = _cache.GetOrAdd(key, () =>
        {
            // Expensive synchronous computation
            return $"SyncData_{key}";
        }, TimeSpan.FromMinutes(10));
        
        Console.WriteLine(result);
    }
}
```

## Notes

*   **Thread Safety**: While the test suite validates logical correctness, the signatures involving `Set`, `Get`, and `Remove` imply that concurrent access scenarios should be considered. Implementations backing these tests should ensure that `Count`, `Clear`, and enumeration (`GetAllKeys`) are safe when called concurrently with modification methods, though specific locking strategies are implementation details not exposed by the test class itself.
*   **Expiration Granularity**: Tests such as `Set_WithExpiredTtl_ReturnsNullOnGet` and `SetExpiration_OnExistingEntry_RespectsNewExpiration` indicate that expiration is checked at read-time (`Get`/`Exists`) rather than via a background scavenger thread. An entry may physically remain in the collection until accessed or until a cleanup operation occurs, but it will logically appear absent once the TTL elapses.
*   **Factory Invocation**: The `GetOrAdd` tests strictly verify that the factory function is invoked *only* when the key is absent. Implementations must ensure atomicity here to prevent the factory from running multiple times for the same missing key under high concurrency, although the test class primarily validates the single-threaded logical outcome.
*   **Default Values**: The distinction between `Get` returning `default(T)` and `TryGet` returning `false` is critical. Consumers should prefer `TryGet` when `null` (or default) is a valid stored value to distinguish between "key missing" and "value is null".
