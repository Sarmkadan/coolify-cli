// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace CoolifiCli.Caching;

/// <summary>
/// In-memory cache provider implementation using ConcurrentDictionary.
/// Supports TTL-based expiration and automatic cleanup of expired entries.
/// Thread-safe for concurrent access.
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer? _cleanupTimer;
    private readonly object _lockObject = new();

    public MemoryCacheProvider(TimeSpan? cleanupInterval = null)
    {
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
        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache[key] = entry;
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
        if (!_cache.ContainsKey(key))
            return false;

        var entry = _cache[key];
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
    /// Estimates total size in bytes of cached objects.
    /// </summary>
    public long SizeBytes
    {
        get
        {
            long total = 0;

            foreach (var entry in _cache.Values)
            {
                if (entry.Value != null)
                {
                    total += System.Runtime.InteropServices.Marshal.SizeOf(entry.Value);
                }
            }

            return total;
        }
    }

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

        /// <summary>
        /// Checks if this entry has expired.
        /// </summary>
        public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}
