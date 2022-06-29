#nullable enable
namespace CoolifyCli.Caching;

/// <summary>
/// Interface for cache provider implementations.
/// Abstracts cache storage to allow different implementations (memory, Redis, etc.).
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <returns>The cached value or null if not found or expired</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Attempts to get a cached value, returning success status.
    /// </summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Clears all values from the cache.
    /// </summary>
    void Clear();

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// Gets the number of items in the cache.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the total size of the cache in bytes.
    /// </summary>
    long SizeBytes { get; }

    /// <summary>
    /// Gets or creates a value in the cache, executing the factory function if needed.
    /// </summary>
    T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Asynchronous version of GetOrAdd.
    /// </summary>
    Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Sets the cache to invalidate after a given time.
    /// </summary>
    void SetExpiration(string key, TimeSpan expiration);

    /// <summary>
    /// Gets all keys currently in the cache.
    /// </summary>
    IEnumerable<string> GetAllKeys();
}
