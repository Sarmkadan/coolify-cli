// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Services;

namespace CoolifiCli.Middleware;

/// <summary>
/// Middleware for request/response logging. Captures command name, arguments, duration,
/// and exit code. Provides visibility into command execution for debugging and auditing.
/// </summary>
public class LoggingMiddleware : ICommandMiddleware
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs command execution details including start, duration, and exit code.
    /// Preserves exceptions for downstream middleware and logging.
    /// </summary>
    public async Task ProcessAsync(CommandContext context, CommandMiddlewareDelegate next)
    {
        context.StartTime = DateTime.UtcNow;

        try
        {
            _logger.Debug($"Command started: {context.CommandName} {string.Join(" ", context.Arguments)}");

            // Execute next middleware in pipeline
            await next(context);

            // Log successful completion
            context.EndTime = DateTime.UtcNow;
            context.DurationMs = (long)(context.EndTime.Value - context.StartTime).TotalMilliseconds;

            _logger.Info($"Command completed: {context.CommandName} [{context.DurationMs}ms] (exit code: {context.ExitCode})");
        }
        catch (Exception ex)
        {
            context.EndTime = DateTime.UtcNow;
            context.DurationMs = (long)(context.EndTime.Value - context.StartTime).TotalMilliseconds;
            context.Exception = ex;

            _logger.Error(ex, $"Command failed: {context.CommandName} [{context.DurationMs}ms]");

            // Re-throw to allow other middleware to handle
            throw;
        }
    }
}
