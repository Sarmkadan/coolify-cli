#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Caching;
using CoolifyCli.Events;
using CoolifyCli.Infrastructure;
using CoolifyCli.Services;

namespace CoolifyCli.Models;

/// <summary>
/// Global context for CLI execution. Acts as a service container providing access to
/// core services, cache, event publisher, and configuration. Enables dependency injection
/// for commands and middleware.
/// </summary>
public class CliContext
{
    public required CoolifyApiClient ApiClient { get; init; }
    public required ILogger Logger { get; init; }
    public required CoolifyConfiguration Configuration { get; init; }
    public required ICacheProvider CacheProvider { get; init; }
    public required IEventPublisher EventPublisher { get; init; }

    /// <summary>
    /// Gets or creates a context for the current CLI invocation.
    /// </summary>
    public static CliContext Create(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
    {
        return new CliContext
        {
            ApiClient = apiClient,
            Logger = logger,
            Configuration = config,
            CacheProvider = new MemoryCacheProvider(),
            EventPublisher = new EventPublisher(logger)
        };
    }

    /// <summary>
    /// Gets a service from the context by type.
    /// Useful for generic service resolution.
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(CoolifyApiClient) => ApiClient as T,
            nameof(ILogger) => Logger as T,
            "CoolifyConfiguration" => Configuration as T,
            nameof(ICacheProvider) => CacheProvider as T,
            nameof(IEventPublisher) => EventPublisher as T,
            _ => null
        };
    }

    /// <summary>
    /// Gets a cached value or computes it using a factory function.
    /// </summary>
    public T GetOrCompute<T>(string cacheKey, Func<T> factory, TimeSpan? cacheDuration = null)
    {
        var duration = cacheDuration ?? TimeSpan.FromMinutes(5);
        return CacheProvider.GetOrAdd(cacheKey, factory, duration);
    }

    /// <summary>
    /// Asynchronously gets a cached value or computes it using an async factory.
    /// </summary>
    public async Task<T> GetOrComputeAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? cacheDuration = null)
    {
        var duration = cacheDuration ?? TimeSpan.FromMinutes(5);
        return await CacheProvider.GetOrAddAsync(cacheKey, factory, duration);
    }

    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    public void PublishEvent<T>(T @event) where T : DomainEvent
    {
        EventPublisher.Publish(@event);
    }

    /// <summary>
    /// Asynchronously publishes an event to all subscribers.
    /// </summary>
    public async Task PublishEventAsync<T>(T @event) where T : DomainEvent
    {
        await EventPublisher.PublishAsync(@event);
    }

    /// <summary>
    /// Clears all caches.
    /// </summary>
    public void ClearCache()
    {
        CacheProvider.Clear();
        Logger.Info("Cache cleared");
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public (int Items, long SizeBytes) GetCacheStats()
    {
        return (CacheProvider.Count, CacheProvider.SizeBytes);
    }
}
