#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Services;

namespace CoolifiCli.Events;

/// <summary>
/// Event publisher implementing pub-sub pattern. Allows decoupled communication between
/// components through a central event bus. Supports synchronous and asynchronous subscriptions.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger _logger;

    public EventPublisher(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribes to events of type T with a synchronous handler.
    /// </summary>
    public IDisposable Subscribe<T>(Action<T> handler) where T : DomainEvent
    {
        var eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        _subscribers[eventType].Add(handler);
        _logger.Debug($"Subscriber registered for event type: {eventType.Name}");

        // Return disposable to allow unsubscribing
        return new Unsubscriber(_subscribers, eventType, handler);
    }

    /// <summary>
    /// Subscribes to events of type T with an asynchronous handler.
    /// </summary>
    public IDisposable SubscribeAsync<T>(Func<T, Task> handler) where T : DomainEvent
    {
        var eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        _subscribers[eventType].Add(handler);
        _logger.Debug($"Async subscriber registered for event type: {eventType.Name}");

        return new Unsubscriber(_subscribers, eventType, handler);
    }

    /// <summary>
    /// Publishes an event synchronously to all registered subscribers.
    /// Exceptions from handlers are logged but don't prevent other handlers from executing.
    /// </summary>
    public void Publish<T>(T @event) where T : DomainEvent
    {
        var eventType = typeof(T);
        _logger.Info($"Publishing event: {eventType.Name}");

        if (!_subscribers.ContainsKey(eventType))
        {
            _logger.Debug($"No subscribers for event type: {eventType.Name}");
            return;
        }

        var handlers = _subscribers[eventType];

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Action<T> action)
                {
                    action(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in event handler for {eventType.Name}");
            }
        }
    }

    /// <summary>
    /// Publishes an event asynchronously to all registered subscribers.
    /// </summary>
    public async Task PublishAsync<T>(T @event) where T : DomainEvent
    {
        var eventType = typeof(T);
        _logger.Info($"Publishing event (async): {eventType.Name}");

        if (!_subscribers.ContainsKey(eventType))
        {
            _logger.Debug($"No subscribers for event type: {eventType.Name}");
            return;
        }

        var handlers = _subscribers[eventType];
        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Func<T, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(@event));
                }
                else if (handler is Action<T> action)
                {
                    action(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in event handler for {eventType.Name}");
            }
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in async event handlers");
            }
        }
    }

    /// <summary>
    /// Gets the count of subscribers for a specific event type.
    /// </summary>
    public int GetSubscriberCount<T>() where T : DomainEvent
    {
        var eventType = typeof(T);
        return _subscribers.ContainsKey(eventType) ? _subscribers[eventType].Count : 0;
    }

    /// <summary>
    /// Clears all subscribers for a specific event type.
    /// </summary>
    public void ClearSubscribers<T>() where T : DomainEvent
    {
        var eventType = typeof(T);
        if (_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType].Clear();
            _logger.Debug($"Cleared all subscribers for event type: {eventType.Name}");
        }
    }

    /// <summary>
    /// Unsubscriber implementation for managing subscriptions.
    /// </summary>
    private class Unsubscriber : IDisposable
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers;
        private readonly Type _eventType;
        private readonly Delegate _handler;

        public Unsubscriber(Dictionary<Type, List<Delegate>> subscribers, Type eventType, Delegate handler)
        {
            _subscribers = subscribers;
            _eventType = eventType;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_subscribers.ContainsKey(_eventType))
            {
                _subscribers[_eventType].Remove(_handler);
            }
        }
    }
}

/// <summary>
/// Interface for the event publisher.
/// </summary>
public interface IEventPublisher
{
    IDisposable Subscribe<T>(Action<T> handler) where T : DomainEvent;
    IDisposable SubscribeAsync<T>(Func<T, Task> handler) where T : DomainEvent;
    void Publish<T>(T @event) where T : DomainEvent;
    Task PublishAsync<T>(T @event) where T : DomainEvent;
    int GetSubscriberCount<T>() where T : DomainEvent;
    void ClearSubscribers<T>() where T : DomainEvent;
}

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract class DomainEvent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Event for application deployment started.
/// </summary>
public class ApplicationDeploymentStartedEvent : DomainEvent
{
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string DeploymentId { get; set; } = string.Empty;
}

/// <summary>
/// Event for application deployment completed.
/// </summary>
public class ApplicationDeploymentCompletedEvent : DomainEvent
{
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string DeploymentId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}

/// <summary>
/// Event for database operation completed.
/// </summary>
public class DatabaseOperationCompletedEvent : DomainEvent
{
    public int DatabaseId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Result { get; set; }
}

/// <summary>
/// Event for system health status change.
/// </summary>
public class HealthStatusChangedEvent : DomainEvent
{
    public string ComponentName { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
}
