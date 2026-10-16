#nullable enable
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Runs the <see cref="CoolifyOptions"/> startup check before any command executes.
/// The check starts a minimal host, which triggers <c>ValidateOnStart</c>, and makes no HTTP calls.
/// </summary>
public static class CoolifyStartupValidator
{
    /// <summary>The documentation file that describes every option name and the precedence rules.</summary>
    public const string DocumentationPath = "docs/CoolifyOptions.md";

    /// <summary>
    /// Validates the configuration built from <paramref name="args"/> and the environment.
    /// </summary>
    /// <param name="args">Process arguments; <c>--coolify-*</c> flags are read from them.</param>
    /// <param name="error">Where the single readable error message is written. Defaults to standard error.</param>
    /// <param name="getEnvironmentVariable">Environment lookup; defaults to the process environment.</param>
    /// <returns><see cref="Constants.ExitCodes.Success"/> when valid; <see cref="Constants.ExitCodes.ConfigurationError"/> otherwise.</returns>
    public static async Task<int> ValidateAsync(
        IEnumerable<string> args,
        TextWriter? error = null,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        IReadOnlyList<string> problems;

        try
        {
            var configuration = CoolifyOptionsConfiguration.Build(args, getEnvironmentVariable);

            var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
            builder.Services.AddCoolifyOptions(configuration);

            using var host = builder.Build();
            await host.StartAsync();
            await host.StopAsync();

            return Constants.ExitCodes.Success;
        }
        catch (OptionsValidationException ex)
        {
            problems = ex.Failures.ToList();
        }
        catch (Exception ex) when (ex is InvalidDataException or JsonException or FormatException or InvalidOperationException or IOException)
        {
            // Config file parse errors, malformed flags, and values the binder cannot convert,
            // such as a non-numeric COOLIFY_TIMEOUT. Reported as a single problem.
            problems = [ex.Message];
        }

        (error ?? Console.Error).WriteLine(Format(problems));
        return Constants.ExitCodes.ConfigurationError;
    }

    /// <summary>
    /// Formats the problems as one message that lists each of them.
    /// </summary>
    /// <param name="problems">The validation problems to list.</param>
    /// <returns>The complete, multi-line message.</returns>
    public static string Format(IReadOnlyList<string> problems)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Invalid Coolify configuration ({problems.Count} problem{(problems.Count == 1 ? string.Empty : "s")}):");
        foreach (var problem in problems)
        {
            builder.AppendLine($"  - {problem}");
        }

        builder.Append($"Set values with --coolify-* flags, COOLIFY_* environment variables, or the config file. See {DocumentationPath}.");
        return builder.ToString();
    }
}
