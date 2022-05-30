#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolifiCli.Integration;

/// <summary>
/// Handles webhook processing for deployment events, status updates, and notifications.
/// Validates webhook signatures and routes events to appropriate handlers.
/// Provides a flexible event-driven architecture for external integrations.
/// </summary>
public class WebhookHandler
{
    private readonly Dictionary<string, List<Func<JsonObject, Task>>> _handlers = new();
    private readonly string? _webhookSecret;

    public WebhookHandler(string? webhookSecret = null)
    {
        _webhookSecret = webhookSecret;
    }

    /// <summary>
    /// Registers a handler function for a specific event type.
    /// Multiple handlers can be registered for the same event.
    /// </summary>
    public void On<T>(string eventType, Func<T, Task> handler) where T : WebhookEvent
    {
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Func<JsonObject, Task>>();

        _handlers[eventType].Add(async (payload) =>
        {
            var evt = payload.Deserialize<T>();
            if (evt is not null)
                await handler(evt);
        });
    }

    /// <summary>
    /// Registers a generic event handler that receives the raw event data.
    /// </summary>
    public void OnRaw(string eventType, Func<JsonObject, Task> handler)
    {
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Func<JsonObject, Task>>();

        _handlers[eventType].Add(handler);
    }

    /// <summary>
    /// Validates webhook signature using HMAC-SHA256.
    /// </summary>
    public bool ValidateSignature(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(_webhookSecret))
            return true; // No validation if secret not set

        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        var computed = System.Convert.ToBase64String(hash);

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(signature),
            System.Text.Encoding.UTF8.GetBytes(computed));
    }

    /// <summary>
    /// Processes an incoming webhook payload.
    /// Validates signature, parses JSON, and routes to appropriate handlers.
    /// </summary>
    public async Task HandleWebhookAsync(string payload, string? signature = null)
    {
        if (!string.IsNullOrWhiteSpace(signature) && !ValidateSignature(payload, signature))
            throw new InvalidOperationException("Webhook signature validation failed");

        try
        {
            var jObject = JsonNode.Parse(payload)?.AsObject()
                ?? throw new InvalidOperationException("Payload is not a JSON object");

            var eventType = jObject["type"]?.GetValue<string>() ?? "unknown";

            if (_handlers.ContainsKey(eventType))
            {
                var tasks = _handlers[eventType].Select(handler => handler(jObject));
                await Task.WhenAll(tasks);
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to process webhook: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Base class for webhook events.
/// </summary>
public abstract class WebhookEvent
{
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string EventId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Webhook event for application deployments.
/// </summary>
public class DeploymentWebhookEvent : WebhookEvent
{
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string DeploymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CommitHash { get; set; }
    public string? CommitMessage { get; set; }
    public long? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Webhook event for database operations.
/// </summary>
public class DatabaseWebhookEvent : WebhookEvent
{
    public int DatabaseId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty; // backup, restore, optimize
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Webhook event for system health checks.
/// </summary>
public class HealthCheckWebhookEvent : WebhookEvent
{
    public string ComponentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // healthy, warning, critical
    public Dictionary<string, object> Metrics { get; set; } = new();
}
