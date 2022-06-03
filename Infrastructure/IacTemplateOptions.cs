// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Infrastructure;

/// <summary>
/// Configuration options that govern how the infrastructure template engine loads, validates,
/// computes diffs, and applies declarative YAML templates to a live Coolify environment.
/// </summary>
public sealed class IacTemplateOptions
{
    /// <summary>
    /// Gets or sets whether the engine operates in dry-run mode.
    /// When <see langword="true"/> every operation is simulated and logged, but no API mutations
    /// are actually performed. Useful for previewing changes before committing them.
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the engine skips the interactive confirmation prompt that is shown
    /// before applying changes. Set to <see langword="true"/> in unattended CI/CD environments
    /// where <c>stdin</c> is unavailable or not monitored.
    /// </summary>
    public bool AutoApprove { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to bypass the local structural validation step that runs before the
    /// apply phase. Not recommended for production workloads; intended for rapid development
    /// iteration where partial templates are intentional.
    /// </summary>
    public bool SkipValidation { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the apply phase halts immediately on the first resource-level failure
    /// (<see langword="true"/>) or continues to attempt all remaining operations and collects all
    /// errors before returning (<see langword="false"/>).
    /// </summary>
    public bool FailFast { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum wall-clock time allowed for the complete apply phase.
    /// The engine cancels any in-flight operation that exceeds this budget.
    /// </summary>
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum number of resource operations to execute concurrently during
    /// the apply phase. Applications and databases within the same template may be provisioned
    /// in parallel up to this limit; higher values reduce total apply time at the cost of
    /// increased API load.
    /// </summary>
    public int MaxConcurrentOperations { get; set; } = 3;

    /// <summary>
    /// Gets or sets the console output format for diff and apply reports.
    /// Accepted values are <c>"text"</c> (human-readable, default) and <c>"json"</c>
    /// (machine-parseable, suitable for CI log scrapers).
    /// </summary>
    public string OutputFormat { get; set; } = "text";

    /// <summary>
    /// Gets or sets whether to emit a full diff summary to the console before the apply phase
    /// begins. When <see langword="false"/> the diff is computed but not printed, which reduces
    /// noise in automated pipelines.
    /// </summary>
    public bool ShowDiff { get; set; } = true;

    /// <summary>
    /// Gets or sets the search paths used to resolve relative template file paths.
    /// The current working directory is always searched first, followed by entries in this list.
    /// </summary>
    public List<string> TemplateSearchPaths { get; set; } = [];

    // ─── Pre-built option profiles ────────────────────────────────────────────

    /// <summary>Gets the default interactive options suitable for developer workstations.</summary>
    public static IacTemplateOptions Default => new();

    /// <summary>
    /// Gets options pre-configured for unattended CI/CD pipeline execution:
    /// auto-approves the confirmation prompt, enables fail-fast, suppresses the diff summary,
    /// and switches output to JSON for log parsing.
    /// </summary>
    public static IacTemplateOptions CiMode => new()
    {
        AutoApprove = true,
        FailFast = true,
        ShowDiff = false,
        OutputFormat = "json"
    };

    /// <summary>
    /// Gets options that perform a complete simulation of every operation without mutating any
    /// live state. The diff is always printed so the caller can inspect what <em>would</em>
    /// have happened.
    /// </summary>
    public static IacTemplateOptions DryRunMode => new()
    {
        DryRun = true,
        AutoApprove = true,
        ShowDiff = true
    };

    /// <summary>
    /// Gets options suitable for validating templates in a pull-request gate: validation runs,
    /// the diff is computed, but nothing is applied.
    /// </summary>
    public static IacTemplateOptions ValidateOnly => new()
    {
        DryRun = true,
        AutoApprove = true,
        ShowDiff = true,
        OutputFormat = "json"
    };
}
