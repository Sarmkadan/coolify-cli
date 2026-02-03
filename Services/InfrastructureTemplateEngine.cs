#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

using CoolifiCli.Infrastructure;
using CoolifiCli.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Production implementation of <see cref="IInfrastructureTemplateEngine"/> that reads YAML
/// templates from disk, validates their structure, computes a live-state diff via the Coolify
/// API, and reconciles resources through <see cref="ApplicationService"/> and
/// <see cref="DatabaseService"/>.
/// </summary>
public sealed class InfrastructureTemplateEngine : IInfrastructureTemplateEngine
{
    private readonly ApplicationService _appService;
    private readonly DatabaseService _dbService;
    private readonly ILogger _logger;

    // Shared, thread-safe YAML deserialiser configured for camelCase template keys.
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    // Shared, thread-safe YAML serialiser that omits null properties to keep output concise.
    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <summary>
    /// Initialises a new <see cref="InfrastructureTemplateEngine"/> with its required dependencies.
    /// </summary>
    /// <param name="appService">Service for application lifecycle operations.</param>
    /// <param name="dbService">Service for database management operations.</param>
    /// <param name="logger">Logger for structured diagnostic output.</param>
    public InfrastructureTemplateEngine(
        ApplicationService appService,
        DatabaseService dbService,
        ILogger logger)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _dbService  = dbService  ?? throw new ArgumentNullException(nameof(dbService));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<InfrastructureTemplate>> LoadTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                "Template file path cannot be empty.", 400);

        var resolvedPath = Path.GetFullPath(filePath);

        if (!File.Exists(resolvedPath))
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Template file not found: {resolvedPath}", 404);

        try
        {
            _logger.Info($"Loading infrastructure template from: {resolvedPath}");
            var yaml = await File.ReadAllTextAsync(resolvedPath, cancellationToken);

            if (string.IsNullOrWhiteSpace(yaml))
                return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                    "Template file is empty.", 400);

            var template = YamlDeserializer.Deserialize<InfrastructureTemplate>(yaml);

            if (template is null)
                return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                    "The YAML document is empty or could not be parsed into a template.", 400);

            _logger.Info($"Template '{template.Metadata?.Name ?? "(unnamed)"}' loaded — " +
                         $"{template.Applications.Count} application(s), " +
                         $"{template.Databases.Count} database(s)");

            return ApiResponse<InfrastructureTemplate>.SuccessResponse(template);
        }
        catch (YamlDotNet.Core.YamlException yamlEx)
        {
            _logger.Error($"YAML parse error in '{resolvedPath}': {yamlEx.Message}");
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"YAML parse error at {yamlEx.Start}: {yamlEx.Message}", 400);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error loading template from '{resolvedPath}'");
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Failed to load template: {ex.Message}", 500);
        }
    }

    /// <inheritdoc/>
    public Task<ApiResponse<TemplateValidationResult>> ValidateTemplateAsync(
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        _logger.Info($"Validating template '{template.Metadata?.Name}'");

        var errors   = template.Validate().ToList();
        var warnings = CollectWarnings(template);

        var result = new TemplateValidationResult
        {
            Errors       = errors,
            Warnings     = warnings,
            TemplateName = template.Metadata?.Name
        };

        if (result.IsValid)
            _logger.Info($"Template '{result.TemplateName}' passed validation " +
                         $"({warnings.Count} warning(s))");
        else
            _logger.Error($"Template '{result.TemplateName}' failed validation — " +
                          $"{errors.Count} error(s), {warnings.Count} warning(s)");

        return Task.FromResult(ApiResponse<TemplateValidationResult>.SuccessResponse(result));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<TemplateDiffResult>> ComputeDiffAsync(
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        _logger.Info($"Computing diff for template '{template.Metadata?.Name}'");

        try
        {
            var (liveApps, liveDbs) = await FetchLiveStateAsync(cancellationToken);

            var diff = new TemplateDiffResult
            {
                Added     = BuildAddedEntries(template, liveApps, liveDbs),
                Modified  = BuildModifiedEntries(template, liveApps, liveDbs),
                Removed   = BuildRemovedEntries(template, liveApps, liveDbs),
                Unchanged = BuildUnchangedEntries(template, liveApps, liveDbs)
            };

            _logger.Info($"Diff — add: {diff.Added.Count}, " +
                         $"modify: {diff.Modified.Count}, " +
                         $"orphan: {diff.Removed.Count}, " +
                         $"unchanged: {diff.Unchanged.Count}");

            return ApiResponse<TemplateDiffResult>.SuccessResponse(diff);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to compute template diff");
            return ApiResponse<TemplateDiffResult>.ErrorResponse(
                $"Diff computation failed: {ex.Message}", 500);
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<TemplateApplyResult>> ApplyTemplateAsync(
        InfrastructureTemplate template,
        IacTemplateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        options ??= IacTemplateOptions.Default;

        var dryRunLabel = options.DryRun ? " [DRY RUN]" : string.Empty;
        _logger.Info($"Applying template '{template.Metadata?.Name}'{dryRunLabel}");

        var started    = DateTime.UtcNow;
        var operations = new List<TemplateApplyOperation>();

        try
        {
            if (!options.SkipValidation)
            {
                var validation = await ValidateTemplateAsync(template, cancellationToken);
                if (validation.Data is { IsValid: false } v)
                {
                    var summary = string.Join("; ", v.Errors);
                    return ApiResponse<TemplateApplyResult>.ErrorResponse(
                        $"Template validation failed — {summary}", 422);
                }
            }

            var diffResult = await ComputeDiffAsync(template, cancellationToken);
            if (!diffResult.Success)
                return ApiResponse<TemplateApplyResult>.ErrorResponse(diffResult.Message!, 500);

            var diff = diffResult.Data!;

            if (!diff.HasChanges)
            {
                _logger.Info("Live environment already matches the template — nothing to apply");
                return ApiResponse<TemplateApplyResult>.SuccessResponse(new TemplateApplyResult
                {
                    Operations = [],
                    Duration   = DateTime.UtcNow - started
                }, "No changes required; environment is already in sync.");
            }

            await ReconcileAdditionsAsync(template, diff.Added, operations, options, cancellationToken);

            if (!ShouldAbort(operations, options))
                await ReconcileModificationsAsync(template, diff.Modified, operations, options, cancellationToken);

            var applyResult = new TemplateApplyResult
            {
                Operations = operations,
                Duration   = DateTime.UtcNow - started
            };

            _logger.Info($"Apply complete in {applyResult.Duration.TotalSeconds:F1}s — " +
                         $"{applyResult.SucceededCount} succeeded, {applyResult.FailedCount} failed");

            return ApiResponse<TemplateApplyResult>.SuccessResponse(applyResult);
        }
        catch (OperationCanceledException)
        {
            _logger.Warn("Template apply was cancelled");
            return ApiResponse<TemplateApplyResult>.ErrorResponse("Apply cancelled by request.", 499);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during template apply");
            return ApiResponse<TemplateApplyResult>.ErrorResponse(
                $"Apply failed unexpectedly: {ex.Message}", 500);
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<InfrastructureTemplate>> ExportCurrentStateAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.Info("Exporting current Coolify state as an infrastructure template");

        try
        {
            var (liveApps, liveDbs) = await FetchLiveStateAsync(cancellationToken);

            var templateApps = liveApps.Values
                .Select(a => new IacTemplateApplication
                {
                    Name       = a.Name,
                    Repository = a.Repository,
                    Branch     = string.IsNullOrWhiteSpace(a.Branch) ? "main" : a.Branch,
                    Ports      = a.Ports.Select(p => int.TryParse(p, out var n) ? n : 0)
                                         .Where(p => p > 0)
                                         .ToList(),
                    HealthCheck = string.IsNullOrWhiteSpace(a.HealthCheckUrl) ? null
                        : new IacHealthCheckSpec
                        {
                            Url             = a.HealthCheckUrl,
                            IntervalSeconds = a.HealthCheckIntervalSeconds
                        }
                })
                .ToList();

            var templateDbs = liveDbs.Values
                .Select(d => new IacTemplateDatabase
                {
                    Name           = d.Name,
                    Type           = d.Type,
                    Version        = string.IsNullOrWhiteSpace(d.Version) ? null : d.Version,
                    MaxConnections = d.MaxConnections,
                    Backup = d.EnableBackups
                        ? new IacBackupSpec
                        {
                            Enabled        = true,
                            RetentionDays  = d.BackupRetentionDays,
                            Schedule       = d.BackupSchedule
                        }
                        : new IacBackupSpec { Enabled = false }
                })
                .ToList();

            var exported = new InfrastructureTemplate
            {
                ApiVersion = "v2",
                Kind       = "CoolifyInfrastructure",
                Metadata   = new IacTemplateMetadata
                {
                    Name        = "exported-stack",
                    Description = $"Exported from live Coolify environment on " +
                                  $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                    Environment = "production",
                    Version     = "1.0.0"
                },
                Applications = templateApps,
                Databases    = templateDbs
            };

            _logger.Info($"Export complete — {templateApps.Count} application(s), " +
                         $"{templateDbs.Count} database(s)");

            return ApiResponse<InfrastructureTemplate>.SuccessResponse(exported);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to export current state");
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Export failed: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Serialises <paramref name="template"/> to a YAML string using the shared serialiser.
    /// Null properties are omitted to keep the output concise.
    /// </summary>
    /// <param name="template">The template to serialise.</param>
    /// <returns>A YAML string representation of <paramref name="template"/>.</returns>
    public static string SerializeToYaml(InfrastructureTemplate template) =>
        YamlSerializer.Serialize(template);

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task<(Dictionary<string, ApplicationDeployment> apps,
                         Dictionary<string, DatabaseConfiguration> dbs)>
        FetchLiveStateAsync(CancellationToken ct)
    {
        var appsTask = _appService.GetAllApplicationsAsync();
        var dbsTask  = _dbService.GetAllDatabasesAsync();
        await Task.WhenAll(appsTask, dbsTask);

        var apps = (appsTask.Result.Data ?? [])
            .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

        var dbs = (dbsTask.Result.Data ?? [])
            .ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

        return (apps, dbs);
    }

    private static bool ShouldAbort(List<TemplateApplyOperation> ops, IacTemplateOptions opts) =>
        opts.FailFast && ops.Any(o => !o.Succeeded);

    // ─── Diff builders ────────────────────────────────────────────────────────

    private static List<TemplateDiffEntry> BuildAddedEntries(
        InfrastructureTemplate template,
        Dictionary<string, ApplicationDeployment> liveApps,
        Dictionary<string, DatabaseConfiguration> liveDbs)
    {
        var entries = new List<TemplateDiffEntry>();

        entries.AddRange(template.Applications
            .Where(a => !liveApps.ContainsKey(a.Name))
            .Select(a => new TemplateDiffEntry
            {
                ResourceType      = "Application",
                Name              = a.Name,
                ChangeDescription = $"Will be created from {a.Repository}@{a.Branch}"
            }));

        entries.AddRange(template.Databases
            .Where(d => !liveDbs.ContainsKey(d.Name))
            .Select(d => new TemplateDiffEntry
            {
                ResourceType      = "Database",
                Name              = d.Name,
                ChangeDescription = $"Will be created as {d.Type}{(d.Version is null ? string.Empty : $" {d.Version}")}"
            }));

        return entries;
    }

    private static List<TemplateDiffEntry> BuildModifiedEntries(
        InfrastructureTemplate template,
        Dictionary<string, ApplicationDeployment> liveApps,
        Dictionary<string, DatabaseConfiguration> liveDbs)
    {
        var entries = new List<TemplateDiffEntry>();

        foreach (var app in template.Applications.Where(a => liveApps.ContainsKey(a.Name)))
        {
            var changes = DetectApplicationChanges(app, liveApps[app.Name]);
            if (changes.Count > 0)
                entries.Add(new TemplateDiffEntry
                {
                    ResourceType      = "Application",
                    Name              = app.Name,
                    ChangeDescription = string.Join(", ", changes)
                });
        }

        foreach (var db in template.Databases.Where(d => liveDbs.ContainsKey(d.Name)))
        {
            var changes = DetectDatabaseChanges(db, liveDbs[db.Name]);
            if (changes.Count > 0)
                entries.Add(new TemplateDiffEntry
                {
                    ResourceType      = "Database",
                    Name              = db.Name,
                    ChangeDescription = string.Join(", ", changes)
                });
        }

        return entries;
    }

    private static List<TemplateDiffEntry> BuildRemovedEntries(
        InfrastructureTemplate template,
        Dictionary<string, ApplicationDeployment> liveApps,
        Dictionary<string, DatabaseConfiguration> liveDbs)
    {
        var templateAppNames = template.Applications
            .Select(a => a.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var templateDbNames = template.Databases
            .Select(d => d.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entries = new List<TemplateDiffEntry>();

        entries.AddRange(liveApps.Keys
            .Where(n => !templateAppNames.Contains(n))
            .Select(n => new TemplateDiffEntry
            {
                ResourceType      = "Application",
                Name              = n,
                ChangeDescription = "Exists in live environment but not in template (not removed automatically)"
            }));

        entries.AddRange(liveDbs.Keys
            .Where(n => !templateDbNames.Contains(n))
            .Select(n => new TemplateDiffEntry
            {
                ResourceType      = "Database",
                Name              = n,
                ChangeDescription = "Exists in live environment but not in template (not removed automatically)"
            }));

        return entries;
    }

    private static List<TemplateDiffEntry> BuildUnchangedEntries(
        InfrastructureTemplate template,
        Dictionary<string, ApplicationDeployment> liveApps,
        Dictionary<string, DatabaseConfiguration> liveDbs)
    {
        var entries = new List<TemplateDiffEntry>();

        entries.AddRange(template.Applications
            .Where(a => liveApps.ContainsKey(a.Name) &&
                        DetectApplicationChanges(a, liveApps[a.Name]).Count == 0)
            .Select(a => new TemplateDiffEntry { ResourceType = "Application", Name = a.Name }));

        entries.AddRange(template.Databases
            .Where(d => liveDbs.ContainsKey(d.Name) &&
                        DetectDatabaseChanges(d, liveDbs[d.Name]).Count == 0)
            .Select(d => new TemplateDiffEntry { ResourceType = "Database", Name = d.Name }));

        return entries;
    }

    private static List<string> DetectApplicationChanges(
        IacTemplateApplication spec,
        ApplicationDeployment live)
    {
        var changes = new List<string>();

        if (!string.Equals(spec.Repository, live.Repository, StringComparison.OrdinalIgnoreCase))
            changes.Add($"repository: '{live.Repository}' → '{spec.Repository}'");

        if (!string.Equals(spec.Branch, live.Branch, StringComparison.OrdinalIgnoreCase))
            changes.Add($"branch: '{live.Branch}' → '{spec.Branch}'");

        if (spec.HealthCheck?.Url is { } hcUrl &&
            !string.Equals(hcUrl, live.HealthCheckUrl, StringComparison.OrdinalIgnoreCase))
            changes.Add($"healthCheck.url: '{live.HealthCheckUrl}' → '{hcUrl}'");

        return changes;
    }

    private static List<string> DetectDatabaseChanges(
        IacTemplateDatabase spec,
        DatabaseConfiguration live)
    {
        var changes = new List<string>();

        if (spec.MaxConnections.HasValue && spec.MaxConnections.Value != live.MaxConnections)
            changes.Add($"maxConnections: {live.MaxConnections} → {spec.MaxConnections}");

        if (spec.ConnectionTimeoutSeconds.HasValue &&
            spec.ConnectionTimeoutSeconds.Value != live.ConnectionTimeoutSeconds)
            changes.Add($"connectionTimeoutSeconds: {live.ConnectionTimeoutSeconds} → {spec.ConnectionTimeoutSeconds}");

        if (spec.Backup is { Enabled: true } bk && bk.RetentionDays != live.BackupRetentionDays)
            changes.Add($"backup.retentionDays: {live.BackupRetentionDays} → {bk.RetentionDays}");

        return changes;
    }

    // ─── Reconciliation ───────────────────────────────────────────────────────

    private async Task ReconcileAdditionsAsync(
        InfrastructureTemplate template,
        List<TemplateDiffEntry> added,
        List<TemplateApplyOperation> operations,
        IacTemplateOptions options,
        CancellationToken ct)
    {
        foreach (var entry in added)
        {
            if (ct.IsCancellationRequested || ShouldAbort(operations, options)) break;

            var op = entry.ResourceType switch
            {
                "Application" => await CreateApplicationAsync(
                    template.Applications.First(a => a.Name == entry.Name), options, ct),
                "Database" => await CreateDatabaseAsync(
                    template.Databases.First(d => d.Name == entry.Name), options, ct),
                _ => SkippedOperation(entry.ResourceType, entry.Name, "Create")
            };

            operations.Add(op);
        }
    }

    private async Task ReconcileModificationsAsync(
        InfrastructureTemplate template,
        List<TemplateDiffEntry> modified,
        List<TemplateApplyOperation> operations,
        IacTemplateOptions options,
        CancellationToken ct)
    {
        foreach (var entry in modified)
        {
            if (ct.IsCancellationRequested || ShouldAbort(operations, options)) break;

            var op = entry.ResourceType switch
            {
                "Application" => await UpdateApplicationAsync(
                    template.Applications.First(a => a.Name == entry.Name), options, ct),
                "Database" => await UpdateDatabaseAsync(
                    template.Databases.First(d => d.Name == entry.Name), options, ct),
                _ => SkippedOperation(entry.ResourceType, entry.Name, "Update")
            };

            operations.Add(op);
        }
    }

    // ─── Per-resource operations ──────────────────────────────────────────────

    private async Task<TemplateApplyOperation> CreateApplicationAsync(
        IacTemplateApplication spec,
        IacTemplateOptions options,
        CancellationToken _)
    {
        const string action = "Create";

        if (options.DryRun)
        {
            _logger.Info($"[DRY RUN] Would create application '{spec.Name}'");
            return DryRunOperation("Application", spec.Name, action);
        }

        var envId = spec.EnvironmentId
            ?? System.Environment.GetEnvironmentVariable("COOLIFY_ENVIRONMENT_ID")
            ?? string.Empty;

        var build = spec.BuildCommand ?? DefaultBuildCommand(spec.Runtime);
        var start = spec.StartCommand ?? string.Empty;

        var deployment = new ApplicationDeployment
        {
            Name                     = spec.Name,
            Repository               = spec.Repository,
            Branch                   = spec.Branch,
            EnvironmentId            = envId,
            BuildCommand             = build,
            StartCommand             = start,
            Ports                    = spec.Ports.Select(p => p.ToString()).ToList(),
            HealthCheckUrl           = spec.HealthCheck?.Url,
            HealthCheckIntervalSeconds = spec.HealthCheck?.IntervalSeconds ?? 30
        };

        var result = await _appService.CreateApplicationAsync(deployment);

        return result.Success
            ? SucceededOperation("Application", spec.Name, action, $"Created with ID {result.Data?.Id}")
            : FailedOperation("Application", spec.Name, action, result.Message);
    }

    private async Task<TemplateApplyOperation> UpdateApplicationAsync(
        IacTemplateApplication spec,
        IacTemplateOptions options,
        CancellationToken _)
    {
        const string action = "Update";

        if (options.DryRun)
        {
            _logger.Info($"[DRY RUN] Would update application '{spec.Name}'");
            return DryRunOperation("Application", spec.Name, action);
        }

        var listResult = await _appService.GetAllApplicationsAsync();
        var live = listResult.Data?.FirstOrDefault(
            a => string.Equals(a.Name, spec.Name, StringComparison.OrdinalIgnoreCase));

        if (live is null)
            return FailedOperation("Application", spec.Name, action, "Not found in live environment");

        live.Repository              = spec.Repository;
        live.Branch                  = spec.Branch;
        live.Ports                   = spec.Ports.Select(p => p.ToString()).ToList();
        live.HealthCheckUrl          = spec.HealthCheck?.Url ?? live.HealthCheckUrl;
        live.HealthCheckIntervalSeconds =
            spec.HealthCheck?.IntervalSeconds ?? live.HealthCheckIntervalSeconds;

        if (!string.IsNullOrWhiteSpace(spec.BuildCommand))
            live.BuildCommand = spec.BuildCommand;

        if (!string.IsNullOrWhiteSpace(spec.StartCommand))
            live.StartCommand = spec.StartCommand;

        var result = await _appService.UpdateApplicationAsync(live.Id, live);

        return result.Success
            ? SucceededOperation("Application", spec.Name, action, "Updated successfully")
            : FailedOperation("Application", spec.Name, action, result.Message);
    }

    private async Task<TemplateApplyOperation> CreateDatabaseAsync(
        IacTemplateDatabase spec,
        IacTemplateOptions options,
        CancellationToken _)
    {
        const string action = "Create";

        if (options.DryRun)
        {
            _logger.Info($"[DRY RUN] Would create {spec.Type} database '{spec.Name}'");
            return DryRunOperation("Database", spec.Name, action);
        }

        var rootPassword = ResolveDbSecret(spec.Name, "ROOT_PASSWORD");
        if (string.IsNullOrWhiteSpace(rootPassword))
            return FailedOperation("Database", spec.Name, action,
                $"Root password not found. Set environment variable " +
                $"COOLIFY_DB_{SanitiseName(spec.Name)}_ROOT_PASSWORD before applying.");

        var config = new DatabaseConfiguration
        {
            Name                     = spec.Name,
            Type                     = spec.Type,
            Version                  = spec.Version ?? string.Empty,
            Host                     = "localhost",
            Port                     = DatabaseConfiguration.GetDefaultPort(spec.Type),
            RootUsername             = "root",
            RootPassword             = rootPassword,
            DefaultDatabase          = spec.Name.ToLowerInvariant(),
            MaxConnections           = spec.MaxConnections ?? Constants.Database.DefaultMaxConnections,
            ConnectionTimeoutSeconds = spec.ConnectionTimeoutSeconds ?? Constants.Database.DefaultConnectionTimeoutSeconds,
            EnableBackups            = spec.Backup?.Enabled ?? false,
            BackupRetentionDays      = spec.Backup?.RetentionDays ?? Constants.Database.DefaultBackupRetentionDays,
            BackupSchedule           = spec.Backup?.Schedule ?? "0 2 * * *"
        };

        var result = await _dbService.CreateDatabaseAsync(config);

        return result.Success
            ? SucceededOperation("Database", spec.Name, action, $"Created with ID {result.Data?.Id}")
            : FailedOperation("Database", spec.Name, action, result.Message);
    }

    private async Task<TemplateApplyOperation> UpdateDatabaseAsync(
        IacTemplateDatabase spec,
        IacTemplateOptions options,
        CancellationToken _)
    {
        const string action = "Update";

        if (options.DryRun)
        {
            _logger.Info($"[DRY RUN] Would update database '{spec.Name}'");
            return DryRunOperation("Database", spec.Name, action);
        }

        var listResult = await _dbService.GetAllDatabasesAsync();
        var live = listResult.Data?.FirstOrDefault(
            d => string.Equals(d.Name, spec.Name, StringComparison.OrdinalIgnoreCase));

        if (live is null)
            return FailedOperation("Database", spec.Name, action, "Not found in live environment");

        if (spec.MaxConnections.HasValue)
            live.MaxConnections = spec.MaxConnections.Value;

        if (spec.ConnectionTimeoutSeconds.HasValue)
            live.ConnectionTimeoutSeconds = spec.ConnectionTimeoutSeconds.Value;

        if (spec.Backup is { } bk)
        {
            live.EnableBackups       = bk.Enabled;
            live.BackupRetentionDays = bk.RetentionDays;
            if (!string.IsNullOrWhiteSpace(bk.Schedule))
                live.BackupSchedule = bk.Schedule;
        }

        var result = await _dbService.UpdateDatabaseAsync(live.Id, live);

        return result.Success
            ? SucceededOperation("Database", spec.Name, action, "Updated successfully")
            : FailedOperation("Database", spec.Name, action, result.Message);
    }

    // ─── Warnings and advisory checks ────────────────────────────────────────

    private static List<string> CollectWarnings(InfrastructureTemplate template)
    {
        var warnings = new List<string>();

        foreach (var app in template.Applications)
        {
            if (app.HealthCheck is null)
                warnings.Add($"Application '{app.Name}' has no healthCheck defined — " +
                             "platform-level liveness probing will be unavailable");

            if (app.Scaling is { Instances: > 5 })
                warnings.Add($"Application '{app.Name}' requests {app.Scaling.Instances} instances — " +
                             "verify cluster capacity before applying");

            var sensitiveKeys = app.Environment.Keys
                .Where(k => k.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) ||
                            k.Contains("SECRET",   StringComparison.OrdinalIgnoreCase) ||
                            k.Contains("TOKEN",    StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (sensitiveKeys.Count > 0)
                warnings.Add($"Application '{app.Name}' has sensitive keys inline " +
                             $"({string.Join(", ", sensitiveKeys)}) — use a secrets manager instead");

            if (string.IsNullOrWhiteSpace(app.EnvironmentId) &&
                string.IsNullOrWhiteSpace(
                    System.Environment.GetEnvironmentVariable("COOLIFY_ENVIRONMENT_ID")))
                warnings.Add($"Application '{app.Name}' has no environmentId set and " +
                             "COOLIFY_ENVIRONMENT_ID is not in the environment");
        }

        foreach (var db in template.Databases)
        {
            if (db.Backup is null || !db.Backup.Enabled)
                warnings.Add($"Database '{db.Name}' has backups disabled — " +
                             "data loss risk in the event of a failure");
        }

        return warnings;
    }

    // ─── Utility helpers ──────────────────────────────────────────────────────

    private static string DefaultBuildCommand(RuntimeEnvironment runtime) => runtime switch
    {
        RuntimeEnvironment.Docker  => "docker build .",
        RuntimeEnvironment.NodeJs  => "npm run build",
        RuntimeEnvironment.Python  => "pip install -r requirements.txt",
        RuntimeEnvironment.DotNet  => "dotnet publish -c Release",
        RuntimeEnvironment.Go      => "go build ./...",
        RuntimeEnvironment.Java    => "mvn package -DskipTests",
        RuntimeEnvironment.Ruby    => "bundle install",
        RuntimeEnvironment.PHP     => "composer install",
        _                          => "docker build ."
    };

    private static string ResolveDbSecret(string dbName, string secretSuffix)
    {
        var envKey = $"COOLIFY_DB_{SanitiseName(dbName)}_{secretSuffix}";
        return System.Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
    }

    private static string SanitiseName(string name) =>
        name.ToUpperInvariant().Replace("-", "_").Replace(".", "_").Replace(" ", "_");

    private static TemplateApplyOperation DryRunOperation(
        string resourceType, string name, string action) =>
        new() { ResourceType = resourceType, ResourceName = name, Action = action,
                Succeeded = true, Message = "Dry run — no changes applied" };

    private static TemplateApplyOperation SucceededOperation(
        string resourceType, string name, string action, string? message = null) =>
        new() { ResourceType = resourceType, ResourceName = name, Action = action,
                Succeeded = true, Message = message };

    private static TemplateApplyOperation FailedOperation(
        string resourceType, string name, string action, string? message) =>
        new() { ResourceType = resourceType, ResourceName = name, Action = action,
                Succeeded = false, Message = message ?? "Unknown error" };

    private static TemplateApplyOperation SkippedOperation(
        string resourceType, string name, string action) =>
        new() { ResourceType = resourceType, ResourceName = name, Action = action,
                Succeeded = true, Message = "Resource type not supported by this engine version" };
}
