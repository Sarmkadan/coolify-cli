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
    void Warn(string message);
    void Error(string message);
    void Error(Exception exception, string message = "");
    void Fatal(string message);
}
