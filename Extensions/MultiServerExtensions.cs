// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Extensions;

using CoolifiCli.Models;
using CoolifiCli.Services;

/// <summary>
/// Extension methods that simplify multi-server deployment workflows —
/// filtering server targets, formatting results, and creating service instances.
/// </summary>
public static class MultiServerExtensions
{
    /// <summary>
    /// Filters a server collection to those tagged with the specified environment (case-insensitive).
    /// </summary>
    /// <param name="servers">Source server collection.</param>
    /// <param name="environment">Environment tag to match, e.g. "production" or "staging".</param>
    /// <returns>Servers whose <see cref="ServerTarget.Environment"/> matches.</returns>
    public static IEnumerable<ServerTarget> ForEnvironment(
        this IEnumerable<ServerTarget> servers,
        string environment) =>
        servers.Where(s => s.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Filters a server collection to those that are currently active.
    /// </summary>
    /// <param name="servers">Source server collection.</param>
    /// <returns>Servers where <see cref="ServerTarget.IsActive"/> is true.</returns>
    public static IEnumerable<ServerTarget> Active(this IEnumerable<ServerTarget> servers) =>
        servers.Where(s => s.IsActive);

    /// <summary>
    /// Produces a human-readable, multi-line summary of a multi-server deployment result
    /// suitable for printing to the console or writing to a log.
    /// </summary>
    /// <param name="result">The result to summarise.</param>
    /// <returns>Formatted summary string.</returns>
    public static string ToDeploymentSummary(this MultiServerDeploymentResult result)
    {
        var overallStatus = result.AllSucceeded ? "SUCCESS"
            : result.AnySucceeded ? "PARTIAL"
            : "FAILED";

        var lines = new List<string>
        {
            $"Deployment Run : {result.DeploymentRunId}",
            $"Servers        : {result.TotalServers} targeted, {result.SucceededCount} succeeded, {result.FailedCount} failed",
            $"Duration       : {result.GetTotalDuration().TotalSeconds:F1}s",
            $"Status         : {overallStatus}"
        };

        foreach (var failure in result.GetFailures())
            lines.Add($"  ! [{failure.Server.Name}] {failure.ErrorMessage}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Wraps a <see cref="MultiServerDeploymentResult"/> in a standard
    /// <see cref="ApiResponse{T}"/> for consistent handling by command handlers.
    /// Returns HTTP 200 on full success, 207 on partial success, and 500 on total failure.
    /// </summary>
    /// <param name="result">The multi-server deployment result to wrap.</param>
    /// <returns>
    /// <see cref="ApiResponse{T}"/> representing the aggregate deployment outcome.
    /// </returns>
    public static ApiResponse<MultiServerDeploymentResult> ToApiResponse(
        this MultiServerDeploymentResult result)
    {
        if (result.AllSucceeded)
            return ApiResponse<MultiServerDeploymentResult>.SuccessResponse(
                result,
                $"All {result.TotalServers} server(s) deployed successfully.");

        var errors = result.GetFailures()
            .Select(f => $"[{f.Server.Name}] {f.ErrorMessage}")
            .ToList();

        var statusCode = result.AnySucceeded ? 207 : 500;
        return ApiResponse<MultiServerDeploymentResult>.ErrorResponse(errors, statusCode);
    }

    /// <summary>
    /// Creates a <see cref="MultiServerDeploymentService"/> using the logger as the factory receiver,
    /// matching the manual dependency injection pattern used throughout the CLI.
    /// </summary>
    /// <param name="logger">Application logger passed to the new service instance.</param>
    /// <param name="maxConcurrency">Maximum simultaneous server deployments (default: 5).</param>
    /// <param name="perServerTimeoutSeconds">Per-server HTTP timeout in seconds (default: 300).</param>
    /// <returns>A configured <see cref="MultiServerDeploymentService"/> ready for use.</returns>
    public static MultiServerDeploymentService CreateMultiServerDeploymentService(
        this ILogger logger,
        int maxConcurrency = 5,
        int perServerTimeoutSeconds = 300) =>
        new(logger, maxConcurrency, perServerTimeoutSeconds);
}
