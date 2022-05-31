// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

/// <summary>
/// Interface for application logging. Supports structured logging with different levels.
/// </summary>
public interface ILogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Error(Exception exception, string message = "");
    void Fatal(string message);
}

/// <summary>
/// Console-based logger implementation for CLI output.
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly bool _verbose;

    public ConsoleLogger(bool verbose = false)
    {
        _verbose = verbose;
    }

    public void Debug(string message)
    {
        if (_verbose)
            Console.WriteLine($"[DEBUG] {message}");
    }

    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public void Error(Exception exception, string message = "")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        var msg = string.IsNullOrEmpty(message) ? exception.Message : $"{message}: {exception.Message}";
        Console.WriteLine($"[ERROR] {msg}");
        if (_verbose && exception.StackTrace != null)
            Console.WriteLine(exception.StackTrace);
        Console.ResetColor();
    }

    public void Fatal(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FATAL] {message}");
        Console.ResetColor();
    }
}
