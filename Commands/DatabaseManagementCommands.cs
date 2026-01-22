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
/// Database management commands for backup, restore, and maintenance operations.
/// Handles critical database lifecycle management with safety validations.
/// </summary>
public class DatabaseManagementCommands : CommandBase
{
    private readonly DatabaseService _dbService;

    public DatabaseManagementCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _dbService = new DatabaseService(apiClient, logger);
    }

    /// <summary>
    /// Creates command to initiate database backup. Includes options for backup type
    /// (full/incremental) and storage destination.
    /// </summary>
    public Command CreateBackupCommand()
    {
        var backupCmd = new Command("backup", "Backup a database");
        var dbIdArg = new Argument<int>("id", "Database ID");
        var typeOption = new Option<string>(["--type", "-t"], getDefaultValue: () => "full", "Backup type: full or incremental");
        var destOption = new Option<string>(["--destination", "-d"], "Backup storage destination (S3, local, etc.)");
        var retentionOption = new Option<int>(["--retention", "-r"], getDefaultValue: () => 30, "Retention days for backup");

        backupCmd.AddArgument(dbIdArg);
        backupCmd.AddOption(typeOption);
        backupCmd.AddOption(destOption);
        backupCmd.AddOption(retentionOption);

        backupCmd.SetHandler(async (dbId, type, dest, retention) =>
        {
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (type != "full" && type != "incremental")
                {
                    throw new ValidationException("Backup type must be 'full' or 'incremental'");
                }

                if (retention < 1)
                {
                    throw new ValidationException("Retention must be at least 1 day");
                }

                var config = new DatabaseConfiguration
                {
                    Id = dbId,
                    BackupType = type,
                    RetentionDays = retention
                };

                Logger.Info($"Starting {type} backup for database {dbId} with {retention} day retention");
                var result = await _dbService.CreateBackupAsync(dbId, config);

                if (result.Success)
                {
                    WriteSuccess($"Backup initiated for database {dbId}");
                    Console.WriteLine($"Backup ID: {result.Data?.BackupId}");
                    Console.WriteLine($"Type: {type}");
                    Console.WriteLine($"Retention: {retention} days");
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
        }, dbIdArg, typeOption, destOption, retentionOption);

        return backupCmd;
    }

    /// <summary>
    /// Creates command to restore database from backup. Includes validation for
    /// backup existence and point-in-time recovery options.
    /// </summary>
    public Command CreateRestoreCommand()
    {
        var restoreCmd = new Command("restore", "Restore a database from backup");
        var dbIdArg = new Argument<int>("id", "Database ID");
        var backupIdOption = new Option<string>(["--backup", "-b"], "Backup ID to restore from");
        var timeOption = new Option<DateTime>(["--time", "-t"], "Point-in-time recovery timestamp (ISO 8601)");
        var forceOption = new Option<bool>(["--force", "-f"], "Skip confirmation prompt");

        restoreCmd.AddArgument(dbIdArg);
        restoreCmd.AddOption(backupIdOption);
        restoreCmd.AddOption(timeOption);
        restoreCmd.AddOption(forceOption);

        restoreCmd.SetHandler(async (dbId, backupId, time, force) =>
        {
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (string.IsNullOrWhiteSpace(backupId) && time == default)
                {
                    throw new ValidationException("Either --backup or --time must be specified");
                }

                // Confirmation prompt unless forced
                if (!force)
                {
                    WriteWarning("This will restore the database from backup. All current data may be lost.");
                    Console.Write("Continue? (yes/no): ");
                    var response = Console.ReadLine();
                    if (response?.ToLower() != "yes")
                    {
                        Console.WriteLine("Restore cancelled");
                        return;
                    }
                }

                Logger.Info($"Restoring database {dbId} from backup {backupId ?? "point-in-time"}");

                var config = new DatabaseConfiguration
                {
                    Id = dbId,
                    BackupId = backupId,
                    PointInTimeRecovery = time != default ? time : null
                };

                var result = await _dbService.RestoreFromBackupAsync(dbId, config);

                if (result.Success)
                {
                    WriteSuccess($"Database {dbId} restore initiated");
                    Console.WriteLine($"Restore ID: {result.Data?.RestoreId}");
                    Console.WriteLine($"Estimated completion: {result.Data?.EstimatedCompletionTime}");
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
        }, dbIdArg, backupIdOption, timeOption, forceOption);

        return restoreCmd;
    }

    /// <summary>
    /// Creates command to optimize database indexes and tables. Triggers maintenance
    /// operations like VACUUM, REINDEX, and statistics update.
    /// </summary>
    public Command CreateOptimizeCommand()
    {
        var optimizeCmd = new Command("optimize", "Optimize database performance");
        var dbIdArg = new Argument<int>("id", "Database ID");
        var modeOption = new Option<string>(
            ["--mode", "-m"],
            getDefaultValue: () => "standard",
            "Optimization mode: quick, standard, or full");

        optimizeCmd.AddArgument(dbIdArg);
        optimizeCmd.AddOption(modeOption);

        optimizeCmd.SetHandler(async (dbId, mode) =>
        {
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (!new[] { "quick", "standard", "full" }.Contains(mode))
                {
                    throw new ValidationException("Mode must be 'quick', 'standard', or 'full'");
                }

                Logger.Info($"Starting {mode} optimization for database {dbId}");
                var config = new DatabaseConfiguration { Id = dbId, OptimizationMode = mode };

                var result = await _dbService.OptimizeDatabaseAsync(dbId, config);

                if (result.Success)
                {
                    WriteSuccess($"Database {dbId} optimization started ({mode} mode)");
                    Console.WriteLine($"Duration estimate: {result.Data?.EstimatedDuration} seconds");
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
        }, dbIdArg, modeOption);

        return optimizeCmd;
    }

    /// <summary>
    /// Creates command to manage database user credentials. Allows resetting passwords
    /// and rotating service account credentials.
    /// </summary>
    public Command CreateCredentialsCommand()
    {
        var credsCmd = new Command("credentials", "Manage database credentials");
        var dbIdArg = new Argument<int>("id", "Database ID");
        var resetOption = new Option<bool>(["--reset"], "Reset all credentials");
        var userOption = new Option<string>(["--user", "-u"], "Specific user account to reset");

        credsCmd.AddArgument(dbIdArg);
        credsCmd.AddOption(resetOption);
        credsCmd.AddOption(userOption);

        credsCmd.SetHandler(async (dbId, reset, user) =>
        {
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (!reset)
                {
                    WriteWarning("Use --reset flag to reset credentials");
                    return;
                }

                var targetUser = string.IsNullOrWhiteSpace(user) ? "all" : user;
                Logger.Info($"Resetting credentials for database {dbId} (user={targetUser})");

                var result = await _dbService.ResetCredentialsAsync(dbId, user);

                if (result.Success)
                {
                    WriteSuccess($"Credentials reset for database {dbId}");
                    if (!string.IsNullOrEmpty(user))
                    {
                        Console.WriteLine($"New password: {result.Data?.NewPassword}");
                    }
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
        }, dbIdArg, resetOption, userOption);

        return credsCmd;
    }
}
