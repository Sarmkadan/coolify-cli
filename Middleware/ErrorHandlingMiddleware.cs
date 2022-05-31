// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
#nullable enable

using CoolifiCli.Infrastructure;
using CoolifiCli.Services;

namespace CoolifiCli.Middleware;

/// <summary>
/// Middleware for centralized error handling. Catches exceptions, categorizes them,
/// and provides consistent error response formatting. Prevents unhandled exceptions
/// from propagating and provides meaningful error messages to users.
/// </summary>
public class ErrorHandlingMiddleware : ICommandMiddleware
{
    private readonly ILogger _logger;

    public ErrorHandlingMiddleware(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Catches and handles exceptions from command execution.
    /// Maps exception types to appropriate exit codes and error messages.
    /// </summary>
    public async Task ProcessAsync(CommandContext context, CommandMiddlewareDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException vex)
        {
            HandleValidationError(context, vex);
        }
        catch (ApiException apiEx)
        {
            HandleApiError(context, apiEx);
        }
        catch (UnauthorizedAccessException uaEx)
        {
            HandleUnauthorizedError(context, uaEx);
        }
        catch (TimeoutException tEx)
        {
            HandleTimeoutError(context, tEx);
        }
        catch (Exception ex)
        {
            HandleGenericError(context, ex);
        }
    }

    /// <summary>
    /// Handles validation errors from input validation middleware.
    /// </summary>
    private void HandleValidationError(CommandContext context, ValidationException ex)
    {
        context.ExitCode = Constants.ExitCodes.ValidationError;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Validation Error: {ex.Message}");
        Console.ResetColor();

        _logger.Warn($"Validation error in command '{context.CommandName}': {ex.Message}");
    }

    /// <summary>
    /// Handles Coolify API errors with status code awareness.
    /// </summary>
    private void HandleApiError(CommandContext context, ApiException ex)
    {
        context.ExitCode = Constants.ExitCodes.ApiError;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ API Error ({ex.StatusCode}): {ex.Message}");
        Console.ResetColor();

        // Log additional details for server errors
        if (ex.StatusCode >= 500)
        {
            _logger.Error(ex, $"Server error in API call for command '{context.CommandName}'");
        }
        else
        {
            _logger.Warn($"API error ({ex.StatusCode}) in command '{context.CommandName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Handles unauthorized access errors.
    /// </summary>
    private void HandleUnauthorizedError(CommandContext context, UnauthorizedAccessException ex)
    {
        context.ExitCode = Constants.ExitCodes.UnauthorizedAccess;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("❌ Unauthorized: API key is missing or invalid");
        Console.ResetColor();

        _logger.Warn("Unauthorized access attempt - check API key configuration. If using a refresh token, ensure it's properly configured and not expired.");
    }

    /// <summary>
    /// Handles timeout errors from long-running operations.
    /// </summary>
    private void HandleTimeoutError(CommandContext context, TimeoutException ex)
    {
        context.ExitCode = Constants.ExitCodes.TimeoutError;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"❌ Timeout: The operation took too long. {ex.Message}");
        Console.ResetColor();

        _logger.Warn($"Command '{context.CommandName}' timed out after {context.DurationMs}ms");
    }

    /// <summary>
    /// Handles unexpected generic errors.
    /// </summary>
    private void HandleGenericError(CommandContext context, Exception ex)
    {
        context.ExitCode = Constants.ExitCodes.UnhandledError;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
        Console.ResetColor();

        _logger.Error(ex, $"Unhandled exception in command '{context.CommandName}'");
    }
}