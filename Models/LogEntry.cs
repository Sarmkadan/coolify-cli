#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Models;

/// <summary>
/// Represents a single log entry from an application or system component.
/// Supports structured logging with levels, timestamps, and source tracking.
/// </summary>
public class LogEntry
{
    public int Id { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public int? ExitCode { get; set; }
    public string? StackTrace { get; set; }

    /// <summary>
    /// Creates a log entry from an exception with all relevant details.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Source of the exception.</param>
    /// <returns>New LogEntry with exception details.</returns>
    public static LogEntry FromException(string applicationId, Exception exception, string? source = null)
    {
        return new LogEntry
        {
            ApplicationId = applicationId,
            Level = LogLevel.Error,
            Message = exception.Message,
            Source = source ?? exception.Source ?? "Unknown",
            StackTrace = exception.StackTrace,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                { "ExceptionType", exception.GetType().Name },
                { "InnerException", exception.InnerException?.Message ?? "None" }
            }
        };
    }

    /// <summary>
    /// Checks if this log entry indicates a critical issue.
    /// </summary>
    /// <returns>True if log level is Error or Fatal.</returns>
    public bool IsCritical() => Level is LogLevel.Error or LogLevel.Fatal;

    /// <summary>
    /// Returns a formatted string representation of the log entry.
    /// </summary>
    /// <returns>Formatted log line.</returns>
    public override string ToString()
    {
        var timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        var level = Level.ToString().ToUpper().PadRight(5);
        var prefix = string.IsNullOrEmpty(Source) ? string.Empty : $"[{Source}] ";
        return $"[{timestamp}] {level} {prefix}{Message}";
    }

    /// <summary>
    /// Adds metadata key-value pair to the log entry.
    /// </summary>
    /// <param name="key">Metadata key.</param>
    /// <param name="value">Metadata value.</param>
    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
    }

    /// <summary>
    /// Gets metadata value by key, returning default if not found.
    /// </summary>
    /// <param name="key">Metadata key.</param>
    /// <returns>Metadata value or empty string if not found.</returns>
    public string GetMetadata(string key) => Metadata.TryGetValue(key, out var value) ? value : string.Empty;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
