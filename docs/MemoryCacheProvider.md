# MemoryCacheProvider
The `MemoryCacheProvider` class is designed to provide a simple, in-memory caching mechanism for storing and retrieving data. It allows for the storage of objects of any type, with optional expiration times, and provides methods for adding, retrieving, and removing cached items.

## API
### Constructors
* `public MemoryCacheProvider`: Initializes a new instance of the `MemoryCacheProvider` class.

### Methods
* `public T? Get<T>(string key)`: Retrieves the cached value associated with the specified key. Returns the cached value if it exists, or `null` if it does not.
* `public bool TryGet<T>(string key, out T value)`: Attempts to retrieve the cached value associated with the specified key. Returns `true` if the value is successfully retrieved, or `false` if it does not exist.
* `public void Set<T>(string key, T value)`: Adds or updates the cached value associated with the specified key.
* `public void Remove(string key)`: Removes the cached value associated with the specified key.
* `public void Clear()`: Removes all cached values.
* `public bool Exists(string key)`: Checks if a cached value exists for the specified key. Returns `true` if the value exists, or `false` if it does not.
* `public T GetOrAdd<T>(string key, T value)`: Retrieves the cached value associated with the specified key, or adds the specified value if it does not exist.
* `public async Task<T> GetOrAddAsync<T>(string key, T value)`: Asynchronously retrieves the cached value associated with the specified key, or adds the specified value if it does not exist.
* `public void SetExpiration(string key, DateTime expiresAt)`: Sets the expiration time for the cached value associated with the specified key.
* `public IEnumerable<string> GetAllKeys()`: Retrieves all keys currently in the cache.

### Properties
* `public object? Value { get; }`: Gets the cached value.
* `public DateTime CreatedAt { get; }`: Gets the time at which the cache was created.
* `public DateTime LastAccessedAt { get; }`: Gets the time at which the cache was last accessed.
* `public DateTime? ExpiresAt { get; }`: Gets the expiration time for the cache.
* `public bool IsExpired { get; }`: Checks if the cache has expired.

### Disposal
* `public void Dispose()`: Releases all resources held by the cache.

## Usage
The following examples demonstrate how to use the `MemoryCacheProvider` class:
```csharp
// Example 1: Basic caching
var cache = new MemoryCacheProvider();
cache.Set("username", "JohnDoe");
var cachedUsername = cache.Get<string>("username");
Console.WriteLine(cachedUsername); // Output: JohnDoe

// Example 2: Expiring cache
var expiringCache = new MemoryCacheProvider();
expiringCache.Set("token", "abc123");
expiringCache.SetExpiration("token", DateTime.Now.AddMinutes(30));
var cachedToken = expiringCache.Get<string>("token");
Console.WriteLine(cachedToken); // Output: abc123
```

## Notes
* The `MemoryCacheProvider` class is not thread-safe by default. If you plan to use it in a multi-threaded environment, you should implement synchronization mechanisms to prevent concurrent access issues.
* Cached values are stored in memory and will be lost when the application restarts. If persistence is required, consider using a different caching mechanism, such as a disk-based cache.
* Expiration times are based on the system clock and may not be perfectly accurate. If high precision is required, consider using a more advanced caching solution.
