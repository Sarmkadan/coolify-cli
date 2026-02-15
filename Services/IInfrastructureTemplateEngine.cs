// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

using CoolifiCli.Infrastructure;
using CoolifiCli.Models;

/// <summary>
/// Contract for an engine that loads, validates, diffs, and applies declarative YAML
/// infrastructure templates against a live Coolify environment.
/// Implementations are responsible for coordinating API calls and reporting per-resource outcomes.
/// </summary>
public interface IInfrastructureTemplateEngine
{
    /// <summary>
    /// Reads the YAML file at <paramref name="filePath"/> and deserialises it into an
    /// <see cref="InfrastructureTemplate"/> object graph.
    /// </summary>
    /// <param name="filePath">
    /// Absolute or working-directory-relative path to the <c>.yaml</c> template file.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <returns>
    /// A successful <see cref="ApiResponse{T}"/> containing the parsed template, or an error
    /// response when the file is missing, empty, or the YAML is malformed.
    /// </returns>
    Task<ApiResponse<InfrastructureTemplate>> LoadTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the structural integrity and semantic correctness of <paramref name="template"/>
    /// without contacting the Coolify API.  Produces both hard errors (which block apply) and
    /// advisory warnings.
    /// </summary>
    /// <param name="template">The template document to validate.</param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <returns>
    /// A <see cref="TemplateValidationResult"/> that aggregates all errors and warnings found.
    /// The outer <see cref="ApiResponse{T}"/> is always successful unless an unhandled exception
    /// occurs; check <see cref="TemplateValidationResult.IsValid"/> for the actual outcome.
    /// </returns>
    Task<ApiResponse<TemplateValidationResult>> ValidateTemplateAsync(
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the current live state from Coolify and compares it against
    /// <paramref name="template"/>, classifying every resource as added, modified, removed,
    /// or unchanged.
    /// </summary>
    /// <param name="template">The desired-state template to diff against live state.</param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <returns>
    /// A <see cref="TemplateDiffResult"/> categorising every resource change.
    /// Returns an error response when the live-state API calls fail.
    /// </returns>
    Task<ApiResponse<TemplateDiffResult>> ComputeDiffAsync(
        InfrastructureTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconciles <paramref name="template"/> with the live Coolify environment by creating or
    /// updating resources as needed.  Removed resources (present in live state but absent from the
    /// template) are surfaced in the diff but never deleted automatically.
    /// </summary>
    /// <param name="template">The desired-state template to apply.</param>
    /// <param name="options">
    /// Execution options controlling dry-run mode, concurrency, auto-approval, and failure
    /// strategy.  Defaults to <see cref="IacTemplateOptions.Default"/> when <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <returns>
    /// A <see cref="TemplateApplyResult"/> containing the outcome of every resource operation
    /// attempted during reconciliation.
    /// </returns>
    Task<ApiResponse<TemplateApplyResult>> ApplyTemplateAsync(
        InfrastructureTemplate template,
        IacTemplateOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Snapshots the current live Coolify environment and serialises it as a new
    /// <see cref="InfrastructureTemplate"/>.  The exported document can be saved to disk
    /// to seed a declarative workflow for an existing deployment.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <returns>
    /// An <see cref="InfrastructureTemplate"/> reflecting the live environment at this instant.
    /// Returns an error response when the live-state API calls fail.
    /// </returns>
    Task<ApiResponse<InfrastructureTemplate>> ExportCurrentStateAsync(
        CancellationToken cancellationToken = default);
}
