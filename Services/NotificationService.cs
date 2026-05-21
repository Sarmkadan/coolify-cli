#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

/// <summary>
/// Notification service for sending notifications about deployment, health, and system events.
/// Supports multiple notification channels (console, email, webhooks, Slack).
/// </summary>
public interface INotificationChannel
{
    Task SendAsync(Notification notification);
    string ChannelName { get; }
}

/// <summary>
/// Console notification channel for displaying notifications in the terminal.
/// </summary>
public class ConsoleNotificationChannel : INotificationChannel
{
    public string ChannelName => "console";
    private readonly ILogger _logger;

    public ConsoleNotificationChannel(ILogger logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Notification notification)
    {
        var color = notification.Severity switch
        {
            NotificationSeverity.Info => ConsoleColor.Cyan,
            NotificationSeverity.Warning => ConsoleColor.Yellow,
            NotificationSeverity.Critical => ConsoleColor.Red,
            NotificationSeverity.Success => ConsoleColor.Green,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"[{notification.Severity}] {notification.Title}: {notification.Message}");
        Console.ResetColor();

        return Task.CompletedTask;
    }
}

/// <summary>
/// Webhook notification channel for sending notifications to external systems.
/// Accepts an HttpClient via constructor to support proper lifecycle management
/// through DI containers or IHttpClientFactory.
/// </summary>
public class WebhookNotificationChannel : INotificationChannel, IDisposable
{
    public string ChannelName => "webhook";
    private readonly string _webhookUrl;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a webhook channel with an externally managed HttpClient.
    /// The caller is responsible for disposing the HttpClient.
    /// </summary>
    /// <param name="webhookUrl">Target webhook URL.</param>
    /// <param name="httpClient">Shared HttpClient instance.</param>
    public WebhookNotificationChannel(string webhookUrl, HttpClient httpClient)
    {
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = false;

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid webhook URL format.", nameof(webhookUrl));
    }

    /// <summary>
    /// Creates a webhook channel with an internally managed HttpClient.
    /// The channel will dispose the HttpClient when it is disposed.
    /// </summary>
    /// <param name="webhookUrl">Target webhook URL.</param>
    public WebhookNotificationChannel(string webhookUrl)
    {
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _ownsHttpClient = true;

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid webhook URL format.", nameof(webhookUrl));
    }

    public async Task SendAsync(Notification notification)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WebhookNotificationChannel));

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(notification);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Webhook notification returned {(int)response.StatusCode} for URL: {_webhookUrl}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Webhook notification timed out for URL: {_webhookUrl}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to send webhook notification to {_webhookUrl}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsHttpClient)
                _httpClient.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Notification service managing multiple notification channels.
/// Supports channel registration, removal, and filtered dispatch.
/// </summary>
public class NotificationService : IDisposable
{
    private readonly List<INotificationChannel> _channels = new();
    private bool _disposed;

    public NotificationService()
    {
    }

    /// <summary>
    /// Adds a notification channel.
    /// </summary>
    /// <param name="channel">The channel to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when channel is null.</exception>
    public void AddChannel(INotificationChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        _channels.Add(channel);
    }

    /// <summary>
    /// Removes a notification channel by name.
    /// </summary>
    /// <param name="channelName">Name of the channel to remove.</param>
    /// <returns>True if the channel was found and removed.</returns>
    public bool RemoveChannel(string channelName)
    {
        var channel = _channels.FirstOrDefault(c => c.ChannelName == channelName);
        if (channel is null)
            return false;

        _channels.Remove(channel);
        if (channel is IDisposable disposable)
            disposable.Dispose();

        return true;
    }

    /// <summary>
    /// Sends a notification through all registered channels.
    /// Individual channel failures are caught and logged to prevent
    /// one broken channel from blocking delivery to others.
    /// </summary>
    public async Task SendAsync(Notification notification)
    {
        var tasks = _channels.Select(async c =>
        {
            try
            {
                await c.SendAsync(notification);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Notification channel '{c.ChannelName}' failed: {ex.Message}");
            }
        });
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends an info notification.
    /// </summary>
    public async Task SendInfoAsync(string title, string message)
    {
        await SendAsync(new Notification
        {
            Title = title,
            Message = message,
            Severity = NotificationSeverity.Info,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a success notification.
    /// </summary>
    public async Task SendSuccessAsync(string title, string message)
    {
        await SendAsync(new Notification
        {
            Title = title,
            Message = message,
            Severity = NotificationSeverity.Success,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a warning notification.
    /// </summary>
    public async Task SendWarningAsync(string title, string message)
    {
        await SendAsync(new Notification
        {
            Title = title,
            Message = message,
            Severity = NotificationSeverity.Warning,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a critical notification.
    /// </summary>
    public async Task SendCriticalAsync(string title, string message)
    {
        await SendAsync(new Notification
        {
            Title = title,
            Message = message,
            Severity = NotificationSeverity.Critical,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets the count of registered channels.
    /// </summary>
    public int ChannelCount => _channels.Count;

    /// <summary>
    /// Sends a notification only to channels matching the specified name.
    /// </summary>
    /// <param name="channelName">Target channel name.</param>
    /// <param name="notification">Notification to send.</param>
    public async Task SendToChannelAsync(string channelName, Notification notification)
    {
        var matchingChannels = _channels.Where(c => c.ChannelName == channelName);
        var tasks = matchingChannels.Select(c => c.SendAsync(notification));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Gets the names of all registered channels.
    /// </summary>
    /// <returns>List of channel names.</returns>
    public IReadOnlyList<string> GetRegisteredChannels()
    {
        return _channels.Select(c => c.ChannelName).ToList().AsReadOnly();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var channel in _channels)
            {
                if (channel is IDisposable disposable)
                    disposable.Dispose();
            }
            _channels.Clear();
            _disposed = true;
        }
    }
}

/// <summary>
/// Notification data model.
/// </summary>
public class Notification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SourceApplication { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Notification severity levels.
/// </summary>
public enum NotificationSeverity
{
    Info,
    Success,
    Warning,
    Critical
}
