#nullable enable

using CoolifyCli.Infrastructure;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Extension methods for <see cref="InfrastructureTemplateEngine"/> that provide
/// convenient helper methods for common template operations.
/// </summary>
public static class InfrastructureTemplateEngineExtensions
{
    /// <summary>
    /// Validates the template and returns a tuple with the validation result and the template itself.
    /// Useful when you need both the validation outcome and the template for subsequent operations.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the validation result and the original template.
    /// If validation fails, the template is still returned for inspection.
    /// </returns>
    public static async Task<(ApiResponse<TemplateValidationResult> validation, InfrastructureTemplate template)>
        ValidateWithTemplateAsync(
            this InfrastructureTemplateEngine engine,
            InfrastructureTemplate template,
            CancellationToken cancellationToken = default)
    {
        var validation = await engine.ValidateTemplateAsync(template, cancellationToken);
        return (validation, template);
    }

    /// <summary>
    /// Computes the diff and returns a tuple with the diff result and the template.
    /// Useful when you need both the diff and the template for subsequent operations.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to compute diff for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the diff result and the original template.
    /// If diff computation fails, the template is still returned for inspection.
    /// </returns>
    public static async Task<(ApiResponse<TemplateDiffResult> diff, InfrastructureTemplate template)>
        ComputeDiffWithTemplateAsync(
            this InfrastructureTemplateEngine engine,
            InfrastructureTemplate template,
            CancellationToken cancellationToken = default)
    {
        var diff = await engine.ComputeDiffAsync(template, cancellationToken);
        return (diff, template);
    }

    /// <summary>
    /// Applies the template with the given options and returns a tuple with the apply result and the template.
    /// Useful when you need both the apply result and the template for subsequent operations.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to apply.</param>
    /// <param name="options">Application options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the apply result and the original template.
    /// If apply fails, the template is still returned for inspection.
    /// </returns>
    public static async Task<(ApiResponse<TemplateApplyResult> apply, InfrastructureTemplate template)>
        ApplyWithTemplateAsync(
            this InfrastructureTemplateEngine engine,
            InfrastructureTemplate template,
            IacTemplateOptions? options = null,
            CancellationToken cancellationToken = default)
    {
        var apply = await engine.ApplyTemplateAsync(template, options, cancellationToken);
        return (apply, template);
    }

    /// <summary>
    /// Validates, computes diff, and applies the template in a single operation.
    /// This is a convenience method that chains the three operations together.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to validate, diff, and apply.</param>
    /// <param name="options">Application options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing validation result, diff result, apply result, and the original template.
    /// If any operation fails, subsequent operations are skipped but the template is still returned.
    /// </returns>
    public static async Task<
        (
            ApiResponse<TemplateValidationResult> validation,
            ApiResponse<TemplateDiffResult> diff,
            ApiResponse<TemplateApplyResult> apply,
            InfrastructureTemplate template
        )
    > ValidateDiffAndApplyAsync(
        this InfrastructureTemplateEngine engine,
        InfrastructureTemplate template,
        IacTemplateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Validate
        var validation = await engine.ValidateTemplateAsync(template, cancellationToken);
        if (!validation.Success)
        {
            return (validation,
                   ApiResponse<TemplateDiffResult>.ErrorResponse("Skipped due to validation failure", 422),
                   ApiResponse<TemplateApplyResult>.ErrorResponse("Skipped due to validation failure", 422),
                   template);
        }

        // Compute diff
        var diffResult = await engine.ComputeDiffAsync(template, cancellationToken);
        if (!diffResult.Success)
        {
            return (validation,
                   diffResult,
                   ApiResponse<TemplateApplyResult>.ErrorResponse("Skipped due to diff computation failure", 500),
                   template);
        }

        // Apply
        var applyResult = await engine.ApplyTemplateAsync(template, options, cancellationToken);

        return (validation, diffResult, applyResult, template);
    }

    /// <summary>
    /// Exports the current state and serializes it to YAML in a single operation.
    /// Useful when you need to quickly get the current state as a YAML string.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the export result and the YAML string representation.
    /// </returns>
    public static async Task<(ApiResponse<InfrastructureTemplate> export, string yaml)>
        ExportToYamlAsync(
            this InfrastructureTemplateEngine engine,
            CancellationToken cancellationToken = default)
    {
        var export = await engine.ExportCurrentStateAsync(cancellationToken);
        if (!export.Success)
        {
            return (export, string.Empty);
        }

        var yaml = InfrastructureTemplateEngine.SerializeToYaml(export.Data!);
        return (export, yaml);
    }

    /// <summary>
    /// Checks if the template has any changes compared to the live environment.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True if the template has changes compared to live environment; otherwise false.
    /// </returns>
    public static async Task<bool> HasChangesAsync(
        this InfrastructureTemplateEngine engine,
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default)
    {
        var diffResult = await engine.ComputeDiffAsync(template, cancellationToken);
        return diffResult.Success && diffResult.Data?.HasChanges == true;
    }

    /// <summary>
    /// Gets a summary of the template's resources (applications and databases).
    /// Useful for logging and quick inspection.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to summarize.</param>
    /// <returns>
    /// A formatted string containing the resource summary.
    /// </returns>
    public static string GetResourceSummary(this InfrastructureTemplateEngine engine, InfrastructureTemplate template)
    {
        var appCount = template.Applications.Count;
        var dbCount = template.Databases.Count;
        var total = appCount + dbCount;

        return total == 0
            ? "No resources defined"
            : $"Template '{template.Metadata?.Name ?? "unnamed"}' contains {total} resource(s): " +
              $"{appCount} application(s), {dbCount} database(s)";
    }

    /// <summary>
    /// Validates the template and throws an exception if validation fails.
    /// Useful for scenarios where you want to fail fast if the template is invalid.
    /// </summary>
    /// <param name="engine">The template engine instance.</param>
    /// <param name="template">The template to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
    public static async Task ValidateOrThrowAsync(
        this InfrastructureTemplateEngine engine,
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default)
    {
        var validation = await engine.ValidateTemplateAsync(template, cancellationToken);
        if (!validation.Success)
        {
            throw new InvalidOperationException(
                $"Template validation failed: {validation.Message}");
        }

        if (validation.Data is { IsValid: false })
        {
            var errors = string.Join("; ", validation.Data.Errors);
            throw new InvalidOperationException(
                $"Template '{template.Metadata?.Name ?? "unnamed"}' has validation errors: {errors}");
        }
    }
}