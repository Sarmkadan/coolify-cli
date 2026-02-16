#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Middleware;

/// <summary>
/// Interface for command execution middleware. Enables pipeline processing of commands
/// with cross-cutting concerns like logging, authentication, rate limiting, and error handling.
/// Implements chain-of-responsibility pattern for flexible middleware stacking.
/// </summary>
public interface ICommandMiddleware
{
    /// <summary>
    /// Processes a command with the option to pass to the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The command execution context</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <returns>Task that completes when middleware processing is done</returns>
    Task ProcessAsync(CommandContext context, CommandMiddlewareDelegate next);
}

/// <summary>
/// Delegate type for the next middleware in the pipeline.
/// </summary>
public delegate Task CommandMiddlewareDelegate(CommandContext context);

/// <summary>
/// Context containing information about command execution.
/// Allows middleware to inspect and modify command state.
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Gets or sets the command name being executed.
    /// </summary>
    public string CommandName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command arguments.
    /// </summary>
    public string[] Arguments { get; set; } = new string[0];

    /// <summary>
    /// Gets or sets the timestamp when command execution started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when command execution completed.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the execution duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the exit code of the command.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets or sets any error or exception that occurred during execution.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets metadata dictionary for passing data between middleware.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the authenticated user/API key identifier.
    /// </summary>
    public string? AuthenticatedUser { get; set; }

    /// <summary>
    /// Gets or sets whether the command requires authentication.
    /// </summary>
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// Gets or sets whether the command has been authenticated successfully.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the request count for rate limiting purposes.
    /// </summary>
    public int RequestCount { get; set; }
}
