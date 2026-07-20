#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Computes and formats deployment diffs by fetching the live configuration from
/// the API and comparing it against a proposed configuration.
/// </summary>
public sealed class DeploymentDiffService
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
    /// <param name="ignoreKeys">
    /// Optional collection of property keys or JSON paths that should be ignored when
    /// computing the diff (e.g., timestamps, generated IDs). Keys are compared case‑insensitively
    /// and support simple dot‑notation for nested properties.
    /// </param>
    /// <returns>
    /// An <see cref="ApiResponse{T}"/> containing the <see cref="DeploymentDiff"/>,
    /// or an error response if the live configuration could not be retrieved.
    /// </returns>
    public async Task<ApiResponse<DeploymentDiff>> ComputeDiffAsync(
        int applicationId,
        ApplicationDeployment proposed,
        IEnumerable<string>? ignoreKeys = null)
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

        // Apply ignore list if supplied
        if (ignoreKeys != null && ignoreKeys.Any())
        {
            var ignoreSet = new HashSet<string>(ignoreKeys, StringComparer.OrdinalIgnoreCase);

            // Filter entries that match any ignored key or path.
            // Supports simple dot‑notation (e.g., "metadata.createdAt").
            var filtered = diff.Entries
                .Where(e => !ignoreSet.Contains(e.Property) &&
                            !ignoreSet.Any(ik => e.Property.StartsWith($"{ik}.", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // If the underlying type exposes a mutable collection, replace it.
            // Otherwise, attempt to set via reflection (covers read‑only IEnumerable cases).
            if (diff.Entries is List<DeploymentDiffEntry> mutableEntries)
            {
                mutableEntries.Clear();
                mutableEntries.AddRange(filtered);
            }
            else
            {
                var entriesProp = diff.GetType().GetProperty("Entries");
                if (entriesProp?.CanWrite == true)
                {
                    entriesProp.SetValue(diff, filtered);
                }
            }

            // Re‑evaluate HasChanges and Changes after filtering.
            // Assuming DeploymentDiff recomputes these lazily, we simply log the new count.
            _logger.Info($"After ignoring keys, {filtered.Count} entry(ies) remain.");
        }

        return ApiResponse<DeploymentDiff>.SuccessResponse(diff);
    }

    /// <summary>
    /// Renders the diff to the console with color‑coded output.
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
