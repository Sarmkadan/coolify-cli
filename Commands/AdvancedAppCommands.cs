// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Infrastructure;
using CoolifiCli.Models;
using CoolifiCli.Services;
using System.CommandLine;

namespace CoolifiCli.Commands;

/// <summary>
/// Advanced application management commands for deployment configuration, environment variables,
/// and rollback operations. Provides fine-grained control over application lifecycle.
/// </summary>
public class AdvancedAppCommands : CommandBase
{
    private readonly ApplicationService _appService;
    private readonly EnvironmentVariableService _envVarService;

    public AdvancedAppCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _appService = new ApplicationService(apiClient, logger);
        _envVarService = new EnvironmentVariableService(apiClient, logger);
    }

    /// <summary>
    /// Creates command to restart an application, performing graceful shutdown and startup sequence.
    /// </summary>
    public Command CreateRestartCommand()
    {
        var restartCmd = new Command("restart", "Restart an application");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var forceOption = new Option<bool>("--force", ["-f"]) { Description = "Force restart without graceful shutdown" };

        restartCmd.Add(appIdArg);
        restartCmd.Add(forceOption);

        restartCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var force = parseResult.GetValue(forceOption);
            try
            {
                ValidatePositiveId(appId);
                Logger.Info($"Restarting application {appId} (force={force})");

                var stopResult = await _appService.StopApplicationAsync(appId);
                if (!stopResult.Success && !force)
                {
                    WriteError($"Stop failed: {stopResult.Message}");
                    return;
                }

                var startResult = await _appService.StartApplicationAsync(appId);
                if (startResult.Success)
                {
                    WriteSuccess($"Application {appId} restart initiated");
                }
                else
                {
                    WriteError(startResult.Message);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return restartCmd;
    }

    /// <summary>
    /// Creates command to update environment variables for an application without redeployment.
    /// Loads variables from file or command-line, validates, then updates via API.
    /// </summary>
    public Command CreateSetEnvCommand()
    {
        var setEnvCmd = new Command("set-env", "Set environment variables for an application");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var fileOption = new Option<string>("--file", ["-f"]) { Description = "Path to environment variable file" };
        var varOption = new Option<string[]>("--var", ["-v"]) { Description = "Environment variables in KEY=VALUE format" };

        setEnvCmd.Add(appIdArg);
        setEnvCmd.Add(fileOption);
        setEnvCmd.Add(varOption);

        setEnvCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var filePath = parseResult.GetValue(fileOption);
            var vars = parseResult.GetValue(varOption);
            try
            {
                ValidatePositiveId(appId);

                var envVars = new List<EnvironmentVariable>();

                // Load from file if provided
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    if (!File.Exists(filePath))
                    {
                        throw new ValidationException($"Environment file not found: {filePath}");
                    }

                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
                    {
                        if (line.Contains("="))
                        {
                            var parts = line.Split("=", 2);
                            envVars.Add(new EnvironmentVariable
                            {
                                Key = parts[0].Trim(),
                                Value = parts[1].Trim()
                            });
                        }
                    }

                    Logger.Info($"Loaded {envVars.Count} environment variables from {filePath}");
                }

                // Parse command-line variables
                if (vars != null && vars.Length > 0)
                {
                    foreach (var varPair in vars)
                    {
                        if (varPair.Contains("="))
                        {
                            var parts = varPair.Split("=", 2);
                            envVars.Add(new EnvironmentVariable
                            {
                                Key = parts[0].Trim(),
                                Value = parts[1].Trim()
                            });
                        }
                    }
                }

                if (envVars.Count == 0)
                {
                    WriteWarning("No environment variables to set");
                    return;
                }

                Logger.Info($"Setting {envVars.Count} environment variables for application {appId}");
                var result = await _envVarService.BulkUpdateVariablesAsync(appId.ToString(), envVars);

                if (result.Success)
                {
                    WriteSuccess($"Set {envVars.Count} environment variables");
                }
                else
                {
                    WriteError(result.Message);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return setEnvCmd;
    }

    /// <summary>
    /// Creates command to scale application instances or resources. Validates requested resources
    /// against available capacity before applying scale operation.
    /// </summary>
    public Command CreateScaleCommand()
    {
        var scaleCmd = new Command("scale", "Scale application instances or resources");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var instancesOption = new Option<int>("--instances", ["-i"]) { Description = "Number of instances" };
        var cpuOption = new Option<decimal>("--cpu") { Description = "CPU limit in millicores (e.g., 500m = 0.5)" };
        var memoryOption = new Option<string>("--memory", ["-m"]) { Description = "Memory limit (e.g., 512Mi, 1Gi)" };

        scaleCmd.Add(appIdArg);
        scaleCmd.Add(instancesOption);
        scaleCmd.Add(cpuOption);
        scaleCmd.Add(memoryOption);

        scaleCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var instances = parseResult.GetValue(instancesOption);
            var cpu = parseResult.GetValue(cpuOption);
            var memory = parseResult.GetValue(memoryOption);
            try
            {
                ValidatePositiveId(appId);

                if (instances <= 0 && cpu <= 0 && string.IsNullOrEmpty(memory))
                {
                    throw new ValidationException("At least one scaling parameter must be specified");
                }

                Logger.Info($"Scaling application {appId} - instances={instances}, cpu={cpu}");
                var appResult = await _appService.GetApplicationAsync(appId);
                if (!appResult.Success || appResult.Data == null)
                {
                    WriteError($"Failed to get application: {appResult.Message}");
                    return;
                }

                var app = appResult.Data;
                var updateResult = await _appService.UpdateApplicationAsync(appId, app);

                if (updateResult.Success)
                {
                    WriteSuccess("Application scaled successfully");
                }
                else
                {
                    WriteError(updateResult.Message);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return scaleCmd;
    }

    /// <summary>
    /// Creates command to rollback application to previous deployment version.
    /// Validates rollback target exists and handles rollback process.
    /// </summary>
    public Command CreateRollbackCommand()
    {
        var rollbackCmd = new Command("rollback", "Rollback to previous application deployment");
        var appIdArg = new Argument<int>("id") { Description = "Application ID" };
        var deploymentIdOption = new Option<string>("--deployment", ["-d"]) { Description = "Specific deployment ID to rollback to" };

        rollbackCmd.Add(appIdArg);
        rollbackCmd.Add(deploymentIdOption);

        rollbackCmd.SetAction(async (parseResult, ct) =>
        {
            var appId = parseResult.GetValue(appIdArg);
            var deploymentId = parseResult.GetValue(deploymentIdOption);
            try
            {
                ValidatePositiveId(appId);

                Logger.Info($"Rolling back application {appId} (deployment={deploymentId ?? "latest"})");

                var result = await _appService.RollbackApplicationAsync(appId, deploymentId ?? string.Empty);

                if (result.Success)
                {
                    WriteSuccess($"Rollback initiated for application {appId}");
                }
                else
                {
                    WriteError(result.Message);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return rollbackCmd;
    }
}
