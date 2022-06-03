// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

namespace CoolifiCli.Services;

/// <summary>
/// Console-based logger implementation. Outputs log messages to console with
/// color coding based on log level. Supports verbose and quiet modes.
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly bool _verboseLogging;
    private readonly bool _colorOutput;
    private readonly object _lockObject = new();

    public ConsoleLogger(bool verbose = false, bool colorOutput = true)
    {
        _verboseLogging = verbose;
        _colorOutput = colorOutput;
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    public void Info(string message)
    {
        Log("INFO", message, ConsoleColor.White);
    }

    /// <summary>
    /// Logs a debug message (only shown in verbose mode).
    /// </summary>
    public void Debug(string message)
    {
        if (_verboseLogging)
        {
            Log("DEBUG", message, ConsoleColor.Gray);
        }
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void Warn(string message)
    {
        Log("WARN", message, ConsoleColor.Yellow);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void Error(string message)
    {
        Log("ERROR", message, ConsoleColor.Red);
    }

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    public void Error(Exception exception, string message)
    {
        Error($"{message}: {exception.Message}");

        if (_verboseLogging)
        {
            var lines = exception.StackTrace?.Split('\n') ?? Array.Empty<string>();
            foreach (var line in lines.Take(5))
            {
                Debug($"  {line.Trim()}");
            }
        }
    }

    /// <summary>
    /// Logs a fatal error message.
    /// </summary>
    public void Fatal(string message)
    {
        Log("FATAL", message, ConsoleColor.DarkRed);
    }

    /// <summary>
    /// Logs a fatal error with exception details.
    /// </summary>
    public void Fatal(Exception exception, string message)
    {
        Fatal($"{message}: {exception.Message}");

        if (_verboseLogging && exception.StackTrace != null)
        {
            var lines = exception.StackTrace.Split('\n');
            foreach (var line in lines)
            {
                Debug($"  {line.Trim()}");
            }
        }
    }

    /// <summary>
    /// Internal method to log with formatting.
    /// </summary>
    private void Log(string level, string message, ConsoleColor color)
    {
        lock (_lockObject)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var prefix = $"[{timestamp}] [{level}]";

            if (_colorOutput)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{prefix} {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{prefix} {message}");
            }
        }
    }
}
