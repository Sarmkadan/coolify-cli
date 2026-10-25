#nullable enable
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

/// <summary>
/// Environment import commands. Reads a <c>.env</c> file and stores its variables on an application
/// through <see cref="EnvironmentVariableService"/>.
/// </summary>
public sealed class EnvironmentImportCommands : CommandBase
{
    private readonly EnvironmentVariableService _envVarService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentImportCommands"/> class.
    /// </summary>
    /// <param name="apiClient">The Coolify API client used to communicate with the Coolify server.</param>
    /// <param name="logger">The logger used for logging messages.</param>
    /// <param name="config">The configuration settings for the Coolify CLI.</param>
    public EnvironmentImportCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _envVarService = new EnvironmentVariableService(apiClient, logger);
    }

    /// <summary>
    /// Creates the <c>import</c> command: <c>import &lt;file&gt; --app &lt;id&gt; [--dry-run]</c>.
    /// </summary>
    /// <returns>The configured "import" command.</returns>
    public Command CreateImportCommand()
    {
        var importCmd = new Command("import", "Import environment variables from a .env file into an application");
        var fileArg = new Argument<string>("file") { Description = "Path to the .env file to import" };
        var appOption = new Option<int>("--app", ["-a"])
        {
            Description = "Application ID to import the variables into",
            DefaultValueFactory = _ => 0
        };
        var dryRunOption = new Option<bool>("--dry-run", ["-d"])
        {
            Description = "Parse and validate the file, then print what would be set without calling Coolify"
        };
        var allowManagedOption = new Option<bool>("--allow-managed")
        {
            Description = "Allow keys reserved for Coolify-managed SERVICE_* variables"
        };

        importCmd.Add(fileArg);
        importCmd.Add(appOption);
        importCmd.Add(dryRunOption);
        importCmd.Add(allowManagedOption);

        // Return the exit code from the action so it reaches Program's InvokeAsync result
        importCmd.SetAction((parseResult, ct) => HandleImportAsync(
            parseResult.GetValue(fileArg) ?? string.Empty,
            parseResult.GetValue(appOption),
            parseResult.GetValue(dryRunOption),
            parseResult.GetValue(allowManagedOption)));

        return importCmd;
    }

    private async Task<int> HandleImportAsync(string filePath, int appId, bool dryRun, bool allowManaged)
    {
        try
        {
            ValidateRequired(filePath, "File path");
            ValidatePositiveId(appId, "--app");

            if (!File.Exists(filePath))
            {
                throw new ValidationException($"Environment file not found: {filePath}");
            }

            var entries = EnvFileParser.ParseFile(filePath);
            if (entries.Count == 0)
            {
                WriteWarning($"No variables found in {filePath}");
                return Constants.ExitCodes.Success;
            }

            // Fail fast on batch rules (key names, managed prefixes, duplicates, newlines) before building anything
            EnvironmentVariableBatchValidator.ValidateOrThrow(entries, allowManaged);

            var variables = entries
                .Select(e => new EnvironmentVariable
                {
                    ApplicationId = appId.ToString(),
                    Key = e.Key,
                    Value = e.Value
                })
                .ToList();

            var fieldErrors = variables
                .SelectMany(v => v.Validate().Select(error => $"{v.Key}: {error}"))
                .ToList();
            if (fieldErrors.Count > 0)
            {
                foreach (var error in fieldErrors)
                {
                    WriteError(error);
                }
                return Constants.ExitCodes.ValidationError;
            }

            if (dryRun)
            {
                WriteInfo($"Dry run: {variables.Count} variable(s) from {filePath} would be set on application {appId}. Nothing was sent to Coolify:");
                foreach (var variable in variables)
                {
                    // ToString masks secret-looking values
                    Console.WriteLine($"  {variable}");
                }
                return Constants.ExitCodes.Success;
            }

            Logger.Info($"Importing {variables.Count} environment variables from {filePath} into application {appId}");
            var result = await _envVarService.BulkUpdateVariablesAsync(appId.ToString(), variables, allowManaged);

            if (!result.Success)
            {
                WriteError(result.Message ?? "Environment import failed");
                return Constants.ExitCodes.ApiError;
            }

            WriteSuccess($"Imported {variables.Count} environment variables into application {appId}");
            return Constants.ExitCodes.Success;
        }
        catch (EnvVarValidationException ex)
        {
            WriteEnvVarViolations(ex);
            return Constants.ExitCodes.EnvVarValidationError;
        }
        catch (ValidationException ex)
        {
            WriteError(ex.Message);
            return Constants.ExitCodes.ValidationError;
        }
        catch (ApiException ex)
        {
            WriteError($"API Error ({ex.StatusCode}): {ex.Message}");
            return Constants.ExitCodes.ApiError;
        }
    }
}
