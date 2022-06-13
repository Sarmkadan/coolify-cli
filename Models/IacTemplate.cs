#nullable enable
namespace CoolifyCli.Models;

using YamlDotNet.Serialization;

/// <summary>
/// Root document model for a declarative infrastructure-as-code YAML template.
/// Describes the desired state of applications and databases to reconcile with Coolify.
/// </summary>
public record InfrastructureTemplate
{
    /// <summary>Gets the schema version of the template document.</summary>
    [YamlMember(Alias = "apiVersion")]
    public string ApiVersion { get; init; } = "v2";

    /// <summary>Gets the resource kind discriminator; must be <c>CoolifyInfrastructure</c>.</summary>
    [YamlMember(Alias = "kind")]
    public string Kind { get; init; } = "CoolifyInfrastructure";

    /// <summary>Gets the identification and labelling metadata for the template.</summary>
    [YamlMember(Alias = "metadata")]
    public required IacTemplateMetadata Metadata { get; init; }

    /// <summary>Gets the list of application resources to provision or reconcile.</summary>
    [YamlMember(Alias = "applications")]
    public List<IacTemplateApplication> Applications { get; init; } = [];

    /// <summary>Gets the list of managed database resources to provision or reconcile.</summary>
    [YamlMember(Alias = "databases")]
    public List<IacTemplateDatabase> Databases { get; init; } = [];

    /// <summary>
    /// Validates the template document structure, yielding an error message for every violation.
    /// Returns an empty sequence when the template is structurally valid.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Metadata?.Name))
            yield return "metadata.name is required";

        if (ApiVersion is not ("v1" or "v2"))
            yield return $"Unsupported apiVersion '{ApiVersion}'. Accepted values: v1, v2";

        if (Kind != "CoolifyInfrastructure")
            yield return $"Invalid kind '{Kind}'. Expected 'CoolifyInfrastructure'";

        if (Applications.Count == 0 && Databases.Count == 0)
            yield return "Template must define at least one application or database";

        var seenApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var app in Applications)
        {
            foreach (var error in app.Validate())
                yield return $"applications['{app.Name}']: {error}";

            if (!seenApps.Add(app.Name))
                yield return $"Duplicate application name: '{app.Name}'";
        }

        var seenDbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var db in Databases)
        {
            foreach (var error in db.Validate())
                yield return $"databases['{db.Name}']: {error}";

            if (!seenDbs.Add(db.Name))
                yield return $"Duplicate database name: '{db.Name}'";
        }
    }
}

/// <summary>
/// Metadata block providing identification, labelling, and environment targeting for a template.
/// </summary>
public record IacTemplateMetadata
{
    /// <summary>Gets the unique name for this infrastructure stack.</summary>
    [YamlMember(Alias = "name")]
    public required string Name { get; init; }

    /// <summary>Gets an optional human-readable description of the template's purpose.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; init; }

    /// <summary>Gets the target environment name (e.g., production, staging, development).</summary>
    [YamlMember(Alias = "environment")]
    public string? Environment { get; init; }

    /// <summary>Gets the semantic version of this template document for change tracking.</summary>
    [YamlMember(Alias = "version")]
    public string? Version { get; init; }

    /// <summary>Gets arbitrary key-value pairs for organisational tagging and filtering.</summary>
    [YamlMember(Alias = "labels")]
    public Dictionary<string, string> Labels { get; init; } = [];
}

/// <summary>
/// Declarative specification for a single application resource.
/// Maps to an <see cref="ApplicationDeployment"/> in the Coolify environment.
/// </summary>
public record IacTemplateApplication
{
    /// <summary>Gets the unique application identifier within this template.</summary>
    [YamlMember(Alias = "name")]
    public required string Name { get; init; }

    /// <summary>Gets the source repository URL (HTTPS or SSH) for the application.</summary>
    [YamlMember(Alias = "repository")]
    public required string Repository { get; init; }

    /// <summary>Gets the git branch to build and deploy from.</summary>
    [YamlMember(Alias = "branch")]
    public string Branch { get; init; } = "main";

    /// <summary>Gets the application runtime environment.</summary>
    [YamlMember(Alias = "runtime")]
    public RuntimeEnvironment Runtime { get; init; } = RuntimeEnvironment.Docker;

    /// <summary>
    /// Gets the Coolify environment identifier this application belongs to.
    /// Falls back to the <c>COOLIFY_ENVIRONMENT_ID</c> environment variable when omitted.
    /// </summary>
    [YamlMember(Alias = "environmentId")]
    public string? EnvironmentId { get; init; }

    /// <summary>Gets the build command used to compile or package the application.</summary>
    [YamlMember(Alias = "buildCommand")]
    public string? BuildCommand { get; init; }

    /// <summary>Gets the command used to start the application process.</summary>
    [YamlMember(Alias = "startCommand")]
    public string? StartCommand { get; init; }

    /// <summary>Gets the list of port numbers exposed by the application container.</summary>
    [YamlMember(Alias = "ports")]
    public List<int> Ports { get; init; } = [];

    /// <summary>Gets the HTTP health probe configuration for this application.</summary>
    [YamlMember(Alias = "healthCheck")]
    public IacHealthCheckSpec? HealthCheck { get; init; }

    /// <summary>Gets environment variable key-value pairs to inject at runtime.</summary>
    [YamlMember(Alias = "environment")]
    public Dictionary<string, string> Environment { get; init; } = [];

    /// <summary>Gets the instance count and auto-scaling policy configuration.</summary>
    [YamlMember(Alias = "scaling")]
    public IacScalingSpec? Scaling { get; init; }

    /// <summary>Gets CPU and memory resource limit constraints for the container.</summary>
    [YamlMember(Alias = "resources")]
    public IacResourceLimits? Resources { get; init; }

    /// <summary>
    /// Validates the application specification, yielding an error string for every violation.
    /// </summary>
    internal IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return "name is required";

        if (string.IsNullOrWhiteSpace(Repository))
            yield return "repository is required";
        else if (!Uri.TryCreate(Repository, UriKind.Absolute, out _))
            yield return $"repository '{Repository}' is not a valid URL";

        foreach (var port in Ports.Where(p => p is < 1 or > 65535))
            yield return $"port {port} is out of the valid range 1–65535";

        if (Scaling is { Instances: < 1 })
            yield return "scaling.instances must be at least 1";

        if (HealthCheck is { IntervalSeconds: < 5 or > 3600 })
            yield return "healthCheck.intervalSeconds must be between 5 and 3600";
    }
}

/// <summary>
/// HTTP probe configuration for application liveness and readiness checking.
/// </summary>
public record IacHealthCheckSpec
{
    /// <summary>Gets the HTTP path to probe for health (e.g., <c>/health</c>).</summary>
    [YamlMember(Alias = "url")]
    public required string Url { get; init; }

    /// <summary>Gets the number of seconds between consecutive health probe attempts.</summary>
    [YamlMember(Alias = "intervalSeconds")]
    public int IntervalSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the number of consecutive probe failures required to declare the application unhealthy.
    /// </summary>
    [YamlMember(Alias = "failureThreshold")]
    public int FailureThreshold { get; init; } = 3;
}

/// <summary>
/// Instance scaling specification controlling the desired replica count and auto-scaling policy.
/// </summary>
public record IacScalingSpec
{
    /// <summary>Gets the desired number of running application instances.</summary>
    [YamlMember(Alias = "instances")]
    public int Instances { get; init; } = 1;

    /// <summary>Gets the policy that governs automatic scale-out and scale-in behaviour.</summary>
    [YamlMember(Alias = "policy")]
    public ScalingPolicy Policy { get; init; } = ScalingPolicy.Manual;
}

/// <summary>
/// CPU and memory resource limits applied to an application container.
/// Values follow Kubernetes-style notation (e.g., <c>500m</c>, <c>512Mi</c>, <c>2Gi</c>).
/// </summary>
public record IacResourceLimits
{
    /// <summary>
    /// Gets the CPU limit in millicores notation (e.g., <c>250m</c> = 0.25 vCPU,
    /// <c>1000m</c> = 1 vCPU).
    /// </summary>
    [YamlMember(Alias = "cpuLimit")]
    public string? CpuLimit { get; init; }

    /// <summary>Gets the memory limit using SI binary suffixes (e.g., <c>512Mi</c>, <c>2Gi</c>).</summary>
    [YamlMember(Alias = "memoryLimit")]
    public string? MemoryLimit { get; init; }
}

/// <summary>
/// Declarative specification for a managed database resource.
/// Maps to a <see cref="DatabaseConfiguration"/> in the Coolify environment.
/// </summary>
public record IacTemplateDatabase
{
    /// <summary>Gets the unique database identifier within this template.</summary>
    [YamlMember(Alias = "name")]
    public required string Name { get; init; }

    /// <summary>Gets the database management system type to provision.</summary>
    [YamlMember(Alias = "type")]
    public DatabaseType Type { get; init; }

    /// <summary>Gets the engine version to use (e.g., <c>"15"</c> for PostgreSQL 15).</summary>
    [YamlMember(Alias = "version")]
    public string? Version { get; init; }

    /// <summary>Gets the maximum number of concurrent database connections to allow.</summary>
    [YamlMember(Alias = "maxConnections")]
    public int? MaxConnections { get; init; }

    /// <summary>Gets the connection timeout in seconds for establishing new connections.</summary>
    [YamlMember(Alias = "connectionTimeoutSeconds")]
    public int? ConnectionTimeoutSeconds { get; init; }

    /// <summary>Gets the automated backup configuration for this database.</summary>
    [YamlMember(Alias = "backup")]
    public IacBackupSpec? Backup { get; init; }

    /// <summary>
    /// Validates the database specification, yielding an error string for every violation.
    /// </summary>
    internal IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return "name is required";

        if (MaxConnections.HasValue && MaxConnections.Value is < 1 or > 1000)
            yield return "maxConnections must be between 1 and 1000";

        if (ConnectionTimeoutSeconds.HasValue && ConnectionTimeoutSeconds.Value is < 5 or > 300)
            yield return "connectionTimeoutSeconds must be between 5 and 300";

        if (Backup is { RetentionDays: < 1 or > 365 })
            yield return "backup.retentionDays must be between 1 and 365";
    }
}

/// <summary>
/// Automated backup configuration for a database, specifying strategy, cron schedule, and retention.
/// </summary>
public record IacBackupSpec
{
    /// <summary>Gets whether automated backups are active for this database.</summary>
    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; init; } = false;

    /// <summary>Gets the backup strategy to apply.</summary>
    [YamlMember(Alias = "strategy")]
    public BackupStrategy Strategy { get; init; } = BackupStrategy.Snapshot;

    /// <summary>Gets the number of days to retain backup archives before automatic deletion.</summary>
    [YamlMember(Alias = "retentionDays")]
    public int RetentionDays { get; init; } = 30;

    /// <summary>
    /// Gets the cron expression that controls when backups run (e.g., <c>"0 2 * * *"</c> = 02:00 daily).
    /// Defaults to the Coolify platform default when omitted.
    /// </summary>
    [YamlMember(Alias = "schedule")]
    public string? Schedule { get; init; }
}

// ─── Result types ─────────────────────────────────────────────────────────────

/// <summary>
/// Outcome of structurally validating an <see cref="InfrastructureTemplate"/>, accumulating
/// both hard errors (which block application) and advisory warnings.
/// </summary>
public record TemplateValidationResult
{
    /// <summary>Gets whether the template passed all validation rules with no errors.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Gets the collected validation error messages that must be resolved before applying.</summary>
    public List<string> Errors { get; init; } = [];

    /// <summary>Gets advisory warning messages that do not block application but indicate risk.</summary>
    public List<string> Warnings { get; init; } = [];

    /// <summary>Gets the template name as read from <c>metadata.name</c>, if available.</summary>
    public string? TemplateName { get; init; }
}

/// <summary>
/// Result of comparing an <see cref="InfrastructureTemplate"/> against the live Coolify state,
/// categorised into additions, modifications, removals, and unchanged resources.
/// </summary>
public record TemplateDiffResult
{
    /// <summary>Gets resources declared in the template that do not yet exist in the live environment.</summary>
    public List<TemplateDiffEntry> Added { get; init; } = [];

    /// <summary>Gets resources that exist in both the template and live environment but differ.</summary>
    public List<TemplateDiffEntry> Modified { get; init; } = [];

    /// <summary>
    /// Gets resources present in the live environment but absent from the template.
    /// These are flagged for awareness but are never automatically removed.
    /// </summary>
    public List<TemplateDiffEntry> Removed { get; init; } = [];

    /// <summary>Gets resources that are identical between the template and live state.</summary>
    public List<TemplateDiffEntry> Unchanged { get; init; } = [];

    /// <summary>Gets whether any additions or modifications require reconciliation.</summary>
    public bool HasChanges => Added.Count > 0 || Modified.Count > 0;
}

/// <summary>
/// Represents a single resource entry within a <see cref="TemplateDiffResult"/>.
/// </summary>
public record TemplateDiffEntry
{
    /// <summary>Gets the resource category, such as <c>Application</c> or <c>Database</c>.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the name of the resource.</summary>
    public required string Name { get; init; }

    /// <summary>Gets a human-readable description of the detected change, if applicable.</summary>
    public string? ChangeDescription { get; init; }
}

/// <summary>
/// Aggregated outcome of applying an <see cref="InfrastructureTemplate"/>, summarising
/// every resource-level operation attempted during the reconciliation phase.
/// </summary>
public record TemplateApplyResult
{
    /// <summary>Gets the ordered list of operations that were attempted during apply.</summary>
    public List<TemplateApplyOperation> Operations { get; init; } = [];

    /// <summary>Gets whether all attempted operations completed successfully.</summary>
    public bool Success => Operations.Count > 0 && Operations.All(o => o.Succeeded);

    /// <summary>Gets the total elapsed time from validation through to the final operation.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets the number of operations that succeeded.</summary>
    public int SucceededCount => Operations.Count(o => o.Succeeded);

    /// <summary>Gets the number of operations that failed.</summary>
    public int FailedCount => Operations.Count(o => !o.Succeeded);
}

/// <summary>
/// Describes the outcome of a single resource-level create or update operation performed
/// during a template apply phase.
/// </summary>
public record TemplateApplyOperation
{
    /// <summary>Gets the resource category that was operated on.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Gets the name of the specific resource that was operated on.</summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Gets the action label for this operation (e.g., <c>Create</c>, <c>Update</c>).
    /// </summary>
    public required string Action { get; init; }

    /// <summary>Gets whether this individual operation completed without error.</summary>
    public bool Succeeded { get; init; }

    /// <summary>Gets an optional message providing detail about the outcome.</summary>
    public string? Message { get; init; }
}
