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
/// </summary>
public class WebhookNotificationChannel : INotificationChannel
{
    public string ChannelName => "webhook";
    private readonly string _webhookUrl;
    private readonly HttpClient _httpClient;

    public WebhookNotificationChannel(string webhookUrl)
    {
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _httpClient = new HttpClient();
    }

    public async Task SendAsync(Notification notification)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(notification);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(_webhookUrl, content);
        }
        catch (Exception ex)
        {
            // Log but don't throw - notification failures shouldn't break the app
            Console.WriteLine($"Failed to send webhook notification: {ex.Message}");
        }
    }
}

/// <summary>
/// Notification service managing multiple notification channels.
/// </summary>
public class NotificationService
{
    private readonly List<INotificationChannel> _channels = new();

    public NotificationService()
    {
    }

    /// <summary>
    /// Adds a notification channel.
    /// </summary>
    public void AddChannel(INotificationChannel channel)
    {
        _channels.Add(channel);
    }

    /// <summary>
    /// Sends a notification through all registered channels.
    /// </summary>
    public async Task SendAsync(Notification notification)
    {
        var tasks = _channels.Select(c => c.SendAsync(notification));
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
