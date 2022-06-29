#nullable enable
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

/// <summary>
/// Provides the <c>diff</c> sub-command under <c>app</c>, showing what configuration
/// changes will be applied before the operator triggers a deployment.
/// </summary>
public class DeploymentDiffCommands : CommandBase
{
    private readonly ApplicationService _appService;

    public DeploymentDiffCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _appService = new ApplicationService(apiClient, logger);
    }

    /// <summary>
    /// Creates the <c>diff</c> command that previews pending deployment changes.
    /// </summary>
    public Command CreateDiffCommand()
    {
        var diffCmd = new Command("diff", "Preview configuration changes before deploying");

        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var branchOption = new Option<string?>("--branch", ["-b"])
            { Description = "Target branch (overrides current)" };
        var buildCmdOption = new Option<string?>("--build-command")
            { Description = "Proposed build command" };
        var startCmdOption = new Option<string?>("--start-command")
            { Description = "Proposed start command" };
        var showAllOption = new Option<bool>("--show-all", ["-a"])
            { Description = "Include unchanged properties in the output" };

        diffCmd.Add(appIdArg);
        diffCmd.Add(branchOption);
        diffCmd.Add(buildCmdOption);
        diffCmd.Add(startCmdOption);
        diffCmd.Add(showAllOption);

        diffCmd.SetAction(async (parseResult, ct) =>
        {
            var appId     = parseResult.GetValue(appIdArg);
            var branch    = parseResult.GetValue(branchOption);
            var buildCmd  = parseResult.GetValue(buildCmdOption);
            var startCmd  = parseResult.GetValue(startCmdOption);
            var showAll   = parseResult.GetValue(showAllOption);

            try
            {
                ValidatePositiveId(appId);

                // Fetch the current live configuration to use as the base for the proposed state.
                var currentResult = await _appService.GetApplicationAsync(appId);
                if (!currentResult.Success || currentResult.Data is null)
                {
                    WriteError($"Failed to fetch application: {currentResult.Message}");
                    Environment.ExitCode = 1;
                    return;
                }

                // Build the proposed config by cloning current and applying overrides.
                var proposed = new ApplicationDeployment
                {
                    Id             = currentResult.Data.Id,
                    Name           = currentResult.Data.Name,
                    Description    = currentResult.Data.Description,
                    Repository     = currentResult.Data.Repository,
                    Branch         = branch ?? currentResult.Data.Branch,
                    EnvironmentId  = currentResult.Data.EnvironmentId,
                    Status         = currentResult.Data.Status,
                    BuildCommand   = buildCmd ?? currentResult.Data.BuildCommand,
                    StartCommand   = startCmd ?? currentResult.Data.StartCommand,
                    Ports          = new List<string>(currentResult.Data.Ports),
                    HealthCheckUrl = currentResult.Data.HealthCheckUrl,
                    HealthCheckIntervalSeconds = currentResult.Data.HealthCheckIntervalSeconds,
                    EnvironmentVariables = new Dictionary<string, string>(currentResult.Data.EnvironmentVariables),
                };

                var diffService = new DeploymentDiffService(_appService, Logger);
                var diffResult  = await diffService.ComputeDiffAsync(appId, proposed);

                if (!diffResult.Success || diffResult.Data is null)
                {
                    WriteError($"Diff computation failed: {diffResult.Message}");
                    Environment.ExitCode = 1;
                    return;
                }

                diffService.RenderDiff(diffResult.Data, showAll);
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
                Environment.ExitCode = Constants.ExitCodes.ValidationError;
            }
            catch (Exception ex)
            {
                WriteError($"Unexpected error: {ex.Message}");
                Logger.Error(ex, "Deployment diff failed");
                Environment.ExitCode = Constants.ExitCodes.UnhandledError;
            }
        });

        return diffCmd;
    }
}
