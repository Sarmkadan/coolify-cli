#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Infrastructure;
using CoolifiCli.Services;
using System.CommandLine;

namespace CoolifiCli.Commands;

/// <summary>
/// Base class for all CLI commands providing common functionality like logging,
/// error handling, and service access. Reduces code duplication across command implementations.
/// </summary>
public abstract class CommandBase
{
    protected readonly CoolifyApiClient ApiClient;
    protected readonly ILogger Logger;
    protected readonly CoolifyConfiguration Configuration;

    public CommandBase(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
    {
        ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Configuration = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Creates a Command with automatic error handling and logging integration.
    /// Wraps handler execution to catch and log exceptions consistently.
    /// </summary>
    protected Command CreateCommand(string name, string description, Func<Task> handler)
    {
        var command = new Command(name, description);
        command.SetAction(async (parseResult, ct) =>
        {
            try
            {
                Logger.Debug($"Executing command: {name}");
                await handler();
            }
            catch (ApiException apiEx)
            {
                Logger.Error($"API Error ({apiEx.StatusCode}): {apiEx.Message}");
                Environment.ExitCode = Constants.ExitCodes.ApiError;
            }
            catch (ValidationException valEx)
            {
                Logger.Error($"Validation Error: {valEx.Message}");
                Environment.ExitCode = Constants.ExitCodes.ValidationError;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unexpected error in command '{name}'");
                Environment.ExitCode = Constants.ExitCodes.UnhandledError;
            }
        });

        return command;
    }

    /// <summary>
    /// Validates required arguments and options, throwing ValidationException if invalid.
    /// Centralizes validation logic to prevent duplication across command handlers.
    /// </summary>
    protected void ValidateRequired(object? value, string fieldName)
    {
        if (value is null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            throw new ValidationException($"{fieldName} is required");
        }
    }

    /// <summary>
    /// Validates an integer ID is positive, throwing ValidationException if not.
    /// </summary>
    protected void ValidatePositiveId(int id, string fieldName = "ID")
    {
        if (id <= 0)
        {
            throw new ValidationException($"{fieldName} must be a positive integer");
        }
    }

    /// <summary>
    /// Writes a success message to console with green color, then resets color.
    /// </summary>
    protected void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Writes an error message to console with red color, then resets color.
    /// </summary>
    protected void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Writes a warning message to console with yellow color, then resets color.
    /// </summary>
    protected void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Writes an info message to console with cyan color, then resets color.
    /// </summary>
    protected void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }
}
