#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Computes and formats deployment diffs by fetching the live configuration from
/// the API and comparing it against a proposed configuration.
/// </summary>
public class DeploymentDiffService
{
    private readonly ApplicationService _appService;
    private readonly ILogger _logger;

    public DeploymentDiffService(ApplicationService appService, ILogger logger)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches the current live configuration for <paramref name="applicationId"/> and
    /// computes a diff against <paramref name="proposed"/>.
    /// </summary>
    /// <param name="applicationId">The ID of the application to compare.</param>
    /// <param name="proposed">The proposed configuration to apply.</param>
    /// <returns>
    /// An <see cref="ApiResponse{T}"/> containing the <see cref="DeploymentDiff"/>,
    /// or an error response if the live configuration could not be retrieved.
    /// </returns>
    public async Task<ApiResponse<DeploymentDiff>> ComputeDiffAsync(
        int applicationId, ApplicationDeployment proposed)
    {
        _logger.Info($"Computing deployment diff for application {applicationId}");

        var currentResult = await _appService.GetApplicationAsync(applicationId);
        if (!currentResult.Success || currentResult.Data is null)
        {
            _logger.Error($"Failed to fetch current deployment: {currentResult.Message}");
            return ApiResponse<DeploymentDiff>.ErrorResponse(
                $"Could not fetch current deployment: {currentResult.Message}", 404);
        }

        var diff = DeploymentDiff.Compute(currentResult.Data, proposed);
        _logger.Info($"Diff computed: {diff.Changes.Count} change(s) detected");

        return ApiResponse<DeploymentDiff>.SuccessResponse(diff);
    }

    /// <summary>
    /// Renders the diff to the console with color-coded output.
    /// Changed properties are highlighted; unchanged properties are omitted unless
    /// <paramref name="showUnchanged"/> is true.
    /// </summary>
    /// <param name="diff">The diff to render.</param>
    /// <param name="showUnchanged">When true, all properties are shown regardless of change.</param>
    public void RenderDiff(DeploymentDiff diff, bool showUnchanged = false)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Deployment Diff — {diff.ApplicationName} (id: {diff.ApplicationId})");
        Console.WriteLine($"Computed at: {diff.ComputedAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine(new string('─', 72));
        Console.ResetColor();

        if (!diff.HasChanges)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  No changes detected. Current configuration is up-to-date.");
            Console.ResetColor();
            return;
        }

        var categories = diff.Entries
            .Where(e => showUnchanged || e.HasChange)
            .GroupBy(e => e.Category)
            .OrderBy(g => g.Key);

        foreach (var group in categories)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  [{group.Key}]");
            Console.ResetColor();

            foreach (var entry in group)
            {
                if (!entry.HasChange)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"    {entry.Property,-40} (unchanged)");
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  - {entry.Property,-38} {entry.CurrentValue}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  + {entry.Property,-38} {entry.ProposedValue}");
                Console.ResetColor();
            }
        }

        Console.WriteLine();

        if (diff.IsHighRisk)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠  High-risk changes detected (repository, environment, or port modifications).");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n  Summary: {diff.Changes.Count} change(s) across {diff.Changes.Select(c => c.Category).Distinct().Count()} categorie(s).");
        Console.ResetColor();
    }
}
