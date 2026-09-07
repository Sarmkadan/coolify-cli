#nullable enable
using System.Collections.Concurrent;
using System.Text.Json;

namespace CoolifyCli.Caching;

/// <summary>
/// In-memory cache provider implementation using ConcurrentDictionary.
/// Supports TTL-based expiration, automatic cleanup of expired entries, and
/// least-recently-used eviction when the configured entry limit is exceeded.
/// Thread-safe for concurrent access.
/// </summary>
public class MemoryCacheProvider : ICacheProvider, IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer? _cleanupTimer;
    private readonly object _evictionLock = new();
    private readonly int _maxEntries;
    private int _evictions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
    /// </summary>
    /// <param name="cleanupInterval">The interval at which expired entries are removed.</param>
    /// <param name="maxEntries">The maximum number of entries retained in the cache.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxEntries"/> is less than one.
    /// </exception>
    public MemoryCacheProvider(TimeSpan? cleanupInterval = null, int maxEntries = 1000)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxEntries, 1);
        _maxEntries = maxEntries;

        // Start cleanup timer to remove expired entries periodically
        var interval = cleanupInterval ?? TimeSpan.FromMinutes(5);
        _cleanupTimer = new Timer(_ => CleanupExpiredEntries(), null, interval, interval);
    }

    /// <summary>
    /// Gets a cached value, removing it if expired.
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired())
            {
                Remove(key);
                return default;
            }

            entry.LastAccessedAt = DateTime.UtcNow;
            return (T?)entry.Value;
        }

        return default;
    }

    /// <summary>
    /// Attempts to get a value from cache.
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired())
            {
                Remove(key);
                return false;
            }

            entry.LastAccessedAt = DateTime.UtcNow;
            value = (T?)entry.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var now = DateTime.UtcNow;
        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = now,
            LastAccessedAt = now,
            ExpiresAt = expiration.HasValue ? now.Add(expiration.Value) : null,
            SizeBytes = EstimateSizeBytes(value)
        };

        lock (_evictionLock)
        {
            _cache[key] = entry;
            EvictLeastRecentlyUsedEntries();
        }
    }

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all values from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Checks if a key exists and is not expired.
    /// </summary>
    public bool Exists(string key)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return false;

        if (entry.IsExpired())
        {
            Remove(key);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the current number of items in the cache.
    /// Note: This includes expired entries that haven't been cleaned up yet.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Gets a snapshot of the current entry count and total number of LRU evictions.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics(_cache.Count, Volatile.Read(ref _evictions));
    }

    /// <summary>
    /// Estimates total size in bytes of cached objects.
    /// Sizes are captured when entries are stored: strings are measured by their UTF-16
    /// payload, byte arrays by their length, and other values by their JSON-serialized
    /// length. Values that cannot be serialized contribute zero, so the result is a
    /// lower-bound approximation rather than an exact managed-heap measurement.
    /// </summary>
    public long SizeBytes => _cache.Values.Sum(e => e.SizeBytes);

    /// <summary>
    /// Gets or adds a value to the cache, using factory if not found.
    /// </summary>
    public T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (TryGet<T>(key, out var cached))
        {
            return cached!;
        }

        var value = factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Asynchronous version of GetOrAdd.
    /// </summary>
    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (TryGet<T>(key, out var cached))
        {
            return cached!;
        }

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Updates the expiration time for an existing cache entry.
    /// </summary>
    public void SetExpiration(string key, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            entry.ExpiresAt = DateTime.UtcNow.Add(expiration);
        }
    }

    /// <summary>
    /// Gets all keys currently in the cache (including expired).
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        return _cache.Keys;
    }

    /// <summary>
    /// Estimates the in-memory footprint of a cached value in bytes.
    /// </summary>
    private static long EstimateSizeBytes(object? value)
    {
        if (value is null)
            return 0;

        try
        {
            return value switch
            {
                string s => sizeof(char) * (long)s.Length,
                byte[] b => b.LongLength,
                _ => JsonSerializer.SerializeToUtf8Bytes(value, value.GetType()).LongLength
            };
        }
        catch (NotSupportedException)
        {
            return 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Removes all expired entries from the cache.
    /// Called periodically by cleanup timer.
    /// </summary>
    private void CleanupExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            Remove(key);
        }
    }

    /// <summary>
    /// Removes the least recently accessed entries until the cache is within its limit.
    /// The caller must hold <see cref="_evictionLock"/>.
    /// </summary>
    private void EvictLeastRecentlyUsedEntries()
    {
        while (_cache.Count > _maxEntries)
        {
            var oldestEntry = _cache
                .OrderBy(kvp => kvp.Value.LastAccessedAt)
                .FirstOrDefault();

            if (oldestEntry.Key is null)
            {
                break;
            }

            if (!_cache.TryRemove(oldestEntry.Key, out _))
            {
                continue;
            }

            Interlocked.Increment(ref _evictions);
        }
    }

    /// <summary>
    /// Disposes the cleanup timer.
    /// </summary>
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    /// <summary>
    /// Internal cache entry wrapper.
    /// </summary>
    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long SizeBytes { get; init; }

        /// <summary>
        /// Checks if this entry has expired.
        /// </summary>
        public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}

/// <summary>
/// Represents a snapshot of memory cache usage and eviction statistics.
/// </summary>
/// <param name="Count">The number of entries currently in the cache.</param>
/// <param name="Evictions">The total number of entries removed by LRU eviction.</param>
public readonly record struct CacheStatistics(int Count, int Evictions);
