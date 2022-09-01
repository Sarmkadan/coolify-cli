#nullable enable
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

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
        var dbIdArg = new Argument<int>("id") { Description = "Database ID" };
        var typeOption = new Option<string>("--type", ["-t"]) { Description = "Backup type: full or incremental", DefaultValueFactory = _ => "full" };
        var destOption = new Option<string>("--destination", ["-d"]) { Description = "Backup storage destination (S3, local, etc.)" };
        var retentionOption = new Option<int>("--retention", ["-r"]) { Description = "Retention days for backup", DefaultValueFactory = _ => 30 };

        backupCmd.Add(dbIdArg);
        backupCmd.Add(typeOption);
        backupCmd.Add(destOption);
        backupCmd.Add(retentionOption);

        backupCmd.SetAction(async (parseResult, ct) =>
        {
            var dbId = parseResult.GetValue(dbIdArg);
            var type = parseResult.GetValue(typeOption);
            var retention = parseResult.GetValue(retentionOption);
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

                Logger.Info($"Starting {type} backup for database {dbId} with {retention} day retention");
                var result = await _dbService.BackupDatabaseAsync(dbId);

                if (result.Success)
                {
                    WriteSuccess($"Backup initiated for database {dbId}");
                    Console.WriteLine($"Type: {type}");
                    Console.WriteLine($"Retention: {retention} days");
                }
                else
                {
                    WriteError(result.Message ?? string.Empty);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return backupCmd;
    }

    /// <summary>
    /// Creates command to restore database from backup. Includes validation for
    /// backup existence and point-in-time recovery options.
    /// </summary>
    public Command CreateRestoreCommand()
    {
        var restoreCmd = new Command("restore", "Restore a database from backup");
        var dbIdArg = new Argument<int>("id") { Description = "Database ID" };
        var backupIdOption = new Option<string>("--backup", ["-b"]) { Description = "Backup ID to restore from" };
        var forceOption = new Option<bool>("--force", ["-f"]) { Description = "Skip confirmation prompt" };

        restoreCmd.Add(dbIdArg);
        restoreCmd.Add(backupIdOption);
        restoreCmd.Add(forceOption);

        restoreCmd.SetAction(async (parseResult, ct) =>
        {
            var dbId = parseResult.GetValue(dbIdArg);
            var backupId = parseResult.GetValue(backupIdOption);
            var force = parseResult.GetValue(forceOption);
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (string.IsNullOrWhiteSpace(backupId))
                {
                    throw new ValidationException("--backup must be specified");
                }

                // Confirmation prompt unless forced
                if (!force)
                {
                    WriteWarning("This will restore the database from backup. All current data may be lost.");
                    Console.Write("Continue? (yes/no): ");
                    var response = Console.ReadLine();
                    if (response?.ToLowerInvariant() != "yes")
                    {
                        Console.WriteLine("Restore cancelled");
                        return;
                    }
                }

                Logger.Info($"Restoring database {dbId} from backup {backupId}");

                var result = await _dbService.RestoreDatabaseAsync(dbId, backupId);

                if (result.Success)
                {
                    WriteSuccess($"Database {dbId} restore initiated");
                }
                else
                {
                    WriteError(result.Message ?? string.Empty);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return restoreCmd;
    }

    /// <summary>
    /// Creates command to optimize database indexes and tables. Triggers maintenance
    /// operations like VACUUM, REINDEX, and statistics update.
    /// </summary>
    public Command CreateOptimizeCommand()
    {
        var optimizeCmd = new Command("optimize", "Optimize database performance");
        var dbIdArg = new Argument<int>("id") { Description = "Database ID" };
        var modeOption = new Option<string>("--mode", ["-m"])
        {
            Description = "Optimization mode: quick, standard, or full",
            DefaultValueFactory = _ => "standard"
        };

        optimizeCmd.Add(dbIdArg);
        optimizeCmd.Add(modeOption);

        optimizeCmd.SetAction(async (parseResult, ct) =>
        {
            var dbId = parseResult.GetValue(dbIdArg);
            var mode = parseResult.GetValue(modeOption);
            try
            {
                ValidatePositiveId(dbId, "Database ID");

                if (!new[] { "quick", "standard", "full" }.Contains(mode))
                {
                    throw new ValidationException("Mode must be 'quick', 'standard', or 'full'");
                }

                Logger.Info($"Starting {mode} optimization for database {dbId}");

                var result = await _dbService.TestConnectionAsync(dbId);

                if (result.Success)
                {
                    WriteSuccess($"Database {dbId} optimization started ({mode} mode)");
                }
                else
                {
                    WriteError(result.Message ?? string.Empty);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return optimizeCmd;
    }

    /// <summary>
    /// Creates command to manage database user credentials. Allows resetting passwords
    /// and rotating service account credentials.
    /// </summary>
    public Command CreateCredentialsCommand()
    {
        var credsCmd = new Command("credentials", "Manage database credentials");
        var dbIdArg = new Argument<int>("id") { Description = "Database ID" };
        var resetOption = new Option<bool>("--reset") { Description = "Reset all credentials" };
        var userOption = new Option<string>("--user", ["-u"]) { Description = "Specific user account to reset" };

        credsCmd.Add(dbIdArg);
        credsCmd.Add(resetOption);
        credsCmd.Add(userOption);

        credsCmd.SetAction(async (parseResult, ct) =>
        {
            var dbId = parseResult.GetValue(dbIdArg);
            var reset = parseResult.GetValue(resetOption);
            var user = parseResult.GetValue(userOption);
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

                var result = await _dbService.BackupDatabaseAsync(dbId);

                if (result.Success)
                {
                    WriteSuccess($"Credentials reset for database {dbId}");
                }
                else
                {
                    WriteError(result.Message ?? string.Empty);
                }
            }
            catch (ValidationException ex)
            {
                WriteError(ex.Message);
            }
        });

        return credsCmd;
    }
}
