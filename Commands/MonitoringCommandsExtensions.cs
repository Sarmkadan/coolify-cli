#nullable enable

using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace CoolifyCli.Commands;

/// <summary>
/// Custom validation exception for monitoring command validation errors.
/// </summary>
public class MonitoringValidationException : Exception
{
    public MonitoringValidationException(string message) : base(message) { }
}

/// <summary>
/// Extension methods for monitoring commands providing additional functionality for command configuration,
/// argument validation, and enhanced monitoring operations.
/// </summary>
public static class MonitoringCommandsExtensions
{
    /// <summary>
    /// Adds common options to a monitoring command for resource specification and output formatting.
    /// </summary>
    /// <param name="command">The command to enhance with standard options.</param>
    /// <returns>The enhanced command with added options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
    public static Command WithStandardOptions(this Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var formatOption = new Option<string>("--format", ["-f"])
        {
            Description = "Output format: text, json, csv",
            DefaultValueFactory = _ => "text"
        };

        var resourceOption = new Option<string>("--resource", ["-r"])
        {
            Description = "Resource identifier or name"
        };

        command.Add(formatOption);
        command.Add(resourceOption);

        return command;
    }

    /// <summary>
    /// Adds timeout option to a monitoring command for controlling operation duration.
    /// </summary>
    /// <param name="command">The command to enhance with timeout option.</param>
    /// <param name="defaultSeconds">Default timeout value in seconds.</param>
    /// <returns>The enhanced command with timeout option.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
    public static Command WithTimeoutOption(this Command command, int defaultSeconds = 30)
    {
        ArgumentNullException.ThrowIfNull(command);

        var timeoutOption = new Option<int>("--timeout", ["-t"])
        {
            Description = "Operation timeout in seconds",
            DefaultValueFactory = _ => defaultSeconds
        };

        command.Add(timeoutOption);
        return command;
    }

    /// <summary>
    /// Validates that a resource identifier is positive and within acceptable range.
    /// </summary>
    /// <param name="parseResult">Command parse result containing the resource identifier.</param>
    /// <param name="argumentName">Name of the argument to validate.</param>
    /// <param name="maxValue">Maximum allowed value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parseResult"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argumentName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="MonitoringValidationException">Thrown when validation fails.</exception>
    public static void ValidateResourceId(this ParseResult parseResult, string argumentName, int maxValue = 10000)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentException.ThrowIfNullOrEmpty(argumentName);

        var value = parseResult.GetValue<int>(argumentName);
        if (value <= 0)
        {
            throw new MonitoringValidationException($"{argumentName} must be a positive integer");
        }

        if (value > maxValue)
        {
            throw new MonitoringValidationException($"{argumentName} cannot exceed {maxValue}");
        }
    }

    /// <summary>
    /// Adds output formatting helpers to a monitoring command for consistent data presentation.
    /// </summary>
    /// <param name="command">The command to enhance with formatting helpers.</param>
    /// <returns>The enhanced command with formatting capabilities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
    public static Command WithOutputFormatting(this Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tableOption = new Option<bool>("--table", ["-t"])
        {
            Description = "Display results in table format",
            DefaultValueFactory = _ => false
        };

        var sortOption = new Option<string>("--sort", ["-s"])
        {
            Description = "Sort field: timestamp, level, name",
            DefaultValueFactory = _ => "timestamp"
        };

        command.Add(tableOption);
        command.Add(sortOption);

        return command;
    }
}