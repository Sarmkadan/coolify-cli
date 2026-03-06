#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Formatters;

using System.Text;
using System.Text.Json;
using CoolifiCli.Infrastructure;
using CoolifiCli.Models;

/// <summary>
/// Renders <see cref="TemplateDiffResult"/>, <see cref="TemplateApplyResult"/>, and
/// <see cref="TemplateValidationResult"/> objects as either a human-readable text table or a
/// structured JSON document.  The output format is controlled by
/// <see cref="IacTemplateOptions.OutputFormat"/>: <c>"text"</c> (default) or <c>"json"</c>.
/// All public methods write the rendered output to <see cref="Console"/> and also return it
/// as a string for testing and logging.
/// </summary>
public static class TemplateDiffFormatter
{
    private const int RulerWidth = 64;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ─── Public surface ───────────────────────────────────────────────────────

    /// <summary>
    /// Formats <paramref name="diff"/> and writes it to <see cref="Console"/>.
    /// Each resource change category is annotated with an ASCII sigil:
    /// <c>+</c> added, <c>~</c> modified, <c>!</c> orphaned, <c>=</c> in sync.
    /// </summary>
    /// <param name="diff">The diff result to render.</param>
    /// <param name="options">Options controlling the output format (<c>text</c> or <c>json</c>).</param>
    /// <returns>The rendered string that was written to the console.</returns>
    public static string FormatDiff(TemplateDiffResult diff, IacTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(diff);
        ArgumentNullException.ThrowIfNull(options);

        var rendered = IsJson(options) ? RenderDiffJson(diff) : RenderDiffText(diff);
        Console.Write(rendered);
        return rendered;
    }

    /// <summary>
    /// Formats <paramref name="result"/> and writes it to <see cref="Console"/>.
    /// Each operation is listed with its action label, resource type, name, and outcome.
    /// </summary>
    /// <param name="result">The apply result to render.</param>
    /// <param name="options">Options controlling the output format.</param>
    /// <returns>The rendered string that was written to the console.</returns>
    public static string FormatApplyResult(TemplateApplyResult result, IacTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);

        var rendered = IsJson(options)
            ? RenderApplyResultJson(result)
            : RenderApplyResultText(result);
        Console.Write(rendered);
        return rendered;
    }

    /// <summary>
    /// Formats <paramref name="validation"/> and writes it to <see cref="Console"/>.
    /// Hard errors are prefixed with <c>✗</c> and advisory warnings with <c>!</c>.
    /// </summary>
    /// <param name="validation">The validation result to render.</param>
    /// <param name="options">Options controlling the output format.</param>
    /// <returns>The rendered string that was written to the console.</returns>
    public static string FormatValidationResult(
        TemplateValidationResult validation,
        IacTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(validation);
        ArgumentNullException.ThrowIfNull(options);

        var rendered = IsJson(options)
            ? RenderValidationJson(validation)
            : RenderValidationText(validation);
        Console.Write(rendered);
        return rendered;
    }

    // ─── Text renderers ───────────────────────────────────────────────────────

    private static string RenderDiffText(TemplateDiffResult diff)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("  Infrastructure Diff");
        sb.AppendLine(new string('─', RulerWidth));

        AppendDiffSection(sb, diff.Added,     "+", "ADDED    ");
        AppendDiffSection(sb, diff.Modified,  "~", "MODIFIED ");
        AppendDiffSection(sb, diff.Removed,   "!", "ORPHAN   ");
        AppendDiffSection(sb, diff.Unchanged, "=", "IN SYNC  ");

        sb.AppendLine(new string('─', RulerWidth));
        sb.AppendLine(
            $"  +{diff.Added.Count} added  " +
            $"~{diff.Modified.Count} modified  " +
            $"!{diff.Removed.Count} orphaned  " +
            $"={diff.Unchanged.Count} unchanged");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string RenderApplyResultText(TemplateApplyResult result)
    {
        var sb   = new StringBuilder();
        var icon = result.Success ? "✓" : "✗";

        sb.AppendLine();
        sb.AppendLine($"  Apply Result  {icon}  {result.Duration.TotalSeconds:F1}s");
        sb.AppendLine(new string('─', RulerWidth));

        foreach (var op in result.Operations)
        {
            var status = op.Succeeded ? "✓" : "✗";
            sb.AppendLine(
                $"  {status}  [{op.Action,-6}]  {op.ResourceType,-13} {op.ResourceName}");

            if (!string.IsNullOrWhiteSpace(op.Message))
                sb.AppendLine($"             {op.Message}");
        }

        sb.AppendLine(new string('─', RulerWidth));
        sb.AppendLine($"  {result.SucceededCount} succeeded  {result.FailedCount} failed");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string RenderValidationText(TemplateValidationResult validation)
    {
        var sb     = new StringBuilder();
        var status = validation.IsValid ? "✓ PASSED" : "✗ FAILED";

        sb.AppendLine();
        sb.AppendLine($"  Validation: {validation.TemplateName}  —  {status}");
        sb.AppendLine(new string('─', RulerWidth));

        foreach (var err in validation.Errors)
            sb.AppendLine($"  ✗  {err}");

        foreach (var warn in validation.Warnings)
            sb.AppendLine($"  !  {warn}");

        if (validation.Errors.Count == 0 && validation.Warnings.Count == 0)
            sb.AppendLine("  Template is valid with no warnings.");

        sb.AppendLine(new string('─', RulerWidth));
        sb.AppendLine(
            $"  {validation.Errors.Count} error(s)  {validation.Warnings.Count} warning(s)");
        sb.AppendLine();

        return sb.ToString();
    }

    // ─── JSON renderers ───────────────────────────────────────────────────────

    private static string RenderDiffJson(TemplateDiffResult diff) =>
        JsonSerializer.Serialize(new
        {
            diff.Added,
            diff.Modified,
            diff.Removed,
            diff.Unchanged,
            summary = new
            {
                added      = diff.Added.Count,
                modified   = diff.Modified.Count,
                orphaned   = diff.Removed.Count,
                unchanged  = diff.Unchanged.Count,
                hasChanges = diff.HasChanges
            }
        }, JsonOptions) + System.Environment.NewLine;

    private static string RenderApplyResultJson(TemplateApplyResult result) =>
        JsonSerializer.Serialize(new
        {
            result.Success,
            durationSeconds  = Math.Round(result.Duration.TotalSeconds, 3),
            result.SucceededCount,
            result.FailedCount,
            result.Operations
        }, JsonOptions) + System.Environment.NewLine;

    private static string RenderValidationJson(TemplateValidationResult validation) =>
        JsonSerializer.Serialize(new
        {
            validation.IsValid,
            validation.TemplateName,
            validation.Errors,
            validation.Warnings
        }, JsonOptions) + System.Environment.NewLine;

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static void AppendDiffSection(
        StringBuilder sb,
        List<TemplateDiffEntry> entries,
        string sigil,
        string label)
    {
        foreach (var entry in entries)
        {
            sb.Append($"  {sigil}  {label}  {entry.ResourceType,-13} {entry.Name}");
            if (!string.IsNullOrWhiteSpace(entry.ChangeDescription))
                sb.Append($"  ({entry.ChangeDescription})");
            sb.AppendLine();
        }
    }

    private static bool IsJson(IacTemplateOptions options) =>
        options.OutputFormat.Equals("json", StringComparison.OrdinalIgnoreCase);
}
