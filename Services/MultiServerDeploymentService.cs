#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Services;

using System.Collections.Concurrent;
using CoolifyCli.Models;

/// <summary>
/// Orchestrates parallel deployments across multiple Coolify server instances.
/// A bounded semaphore limits simultaneous connections while per-server HTTP
/// timeouts prevent a single unresponsive node from stalling the entire rollout.
/// Failed servers are isolated — their errors are recorded without aborting
/// deployments to healthy servers.
/// </summary>
public class MultiServerDeploymentService
{
    private readonly ILogger _logger;
    private readonly int _maxConcurrency;
    private readonly int _perServerTimeoutSeconds;

    /// <summary>
    /// Initializes the service with concurrency and timeout configuration.
    /// </summary>
    /// <param name="logger">Application logger.</param>
    /// <param name="maxConcurrency">Maximum simultaneous server deployments (default: 5).</param>
    /// <param name="perServerTimeoutSeconds">Per-server HTTP timeout in seconds (default: 300).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxConcurrency"/> or <paramref name="perServerTimeoutSeconds"/> is not positive.
    /// </exception>
    public MultiServerDeploymentService(
        ILogger logger,
        int maxConcurrency = 5,
        int perServerTimeoutSeconds = 300)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (maxConcurrency <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Must be greater than zero.");

        if (perServerTimeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(perServerTimeoutSeconds), "Must be greater than zero.");

        _maxConcurrency = maxConcurrency;
        _perServerTimeoutSeconds = perServerTimeoutSeconds;
    }

    /// <summary>
    /// Deploys an application to all active servers in <paramref name="servers"/> in parallel.
    /// Inactive servers (IsActive == false) are silently skipped.
    /// The method always returns a result — it does not throw on partial failures.
    /// </summary>
    /// <param name="servers">Target servers to deploy to.</param>
    /// <param name="context">
    /// Shared deployment context forwarded to each server. Populate
    /// <see cref="DeploymentContext.EnvironmentVariables"/> before calling to satisfy
    /// per-server validation requirements.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels servers still waiting to acquire the concurrency slot.
    /// Already-running server deployments complete or time out independently.
    /// </param>
    /// <returns>
    /// <see cref="MultiServerDeploymentResult"/> containing per-server outcomes and aggregate metrics.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="servers"/> or <paramref name="context"/> is null.
    /// </exception>
    public async Task<MultiServerDeploymentResult> DeployToServersAsync(
        IEnumerable<ServerTarget> servers,
        DeploymentContext context,
        CancellationToken cancellationToken = default)
    {
        if (servers is null) throw new ArgumentNullException(nameof(servers));
        if (context is null) throw new ArgumentNullException(nameof(context));

        var targets = servers.Where(s => s.IsActive).ToList();
        var runResult = new MultiServerDeploymentResult();

        _logger.Info(
            $"Multi-server deployment [{runResult.DeploymentRunId}] targeting " +
            $"{targets.Count} active server(s), max concurrency: {_maxConcurrency}");

        var validationErrors = CollectValidationErrors(targets);
        if (validationErrors.Count > 0)
        {
            _logger.Error(
                $"Multi-server deployment aborted — {validationErrors.Count} validation error(s): " +
                string.Join("; ", validationErrors));

            foreach (var target in targets)
            {
                var skipped = new ServerDeploymentResult { Server = target };
                skipped.LogEvent("Skipped: validation failed before deployment started");
                runResult.Results.Add(skipped);
            }

            runResult.CompletedAt = DateTime.UtcNow;
            return runResult;
        }

        var semaphore = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
        var bag = new ConcurrentBag<ServerDeploymentResult>();

        var tasks = targets.Select(async target =>
        {
            var serverResult = new ServerDeploymentResult { Server = target };
            var semaphoreAcquired = false;

            try
            {
                await semaphore.WaitAsync(cancellationToken);
                semaphoreAcquired = true;
                serverResult = await DeploySingleServerAsync(target, context, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                serverResult.ErrorMessage = "Operation cancelled before execution started";
                serverResult.LogEvent(serverResult.ErrorMessage);
                serverResult.CompletedAt = DateTime.UtcNow;
            }
            finally
            {
                if (semaphoreAcquired)
                    semaphore.Release();

                bag.Add(serverResult);
            }
        });

        await Task.WhenAll(tasks);

        runResult.Results.AddRange(bag.OrderBy(r => r.StartedAt));
        runResult.CompletedAt = DateTime.UtcNow;

        _logger.Info(
            $"Multi-server deployment [{runResult.DeploymentRunId}] complete — " +
            $"{runResult.SucceededCount}/{runResult.TotalServers} succeeded " +
            $"in {runResult.GetTotalDuration().TotalSeconds:F1}s");

        return runResult;
    }

    /// <summary>
    /// Executes the full deployment lifecycle against a single server using a dedicated
    /// <see cref="HttpClient"/> and <see cref="CoolifyApiClient"/> scoped to that server.
    /// </summary>
    /// <param name="target">Server to deploy to.</param>
    /// <param name="parentContext">Shared deployment context from the caller.</param>
    /// <param name="parentToken">Parent cancellation token checked between API calls.</param>
    /// <returns>
    /// <see cref="ServerDeploymentResult"/> describing the outcome for this server.
    /// Never throws — all exceptions are caught and recorded in the result.
    /// </returns>
    private async Task<ServerDeploymentResult> DeploySingleServerAsync(
        ServerTarget target,
        DeploymentContext parentContext,
        CancellationToken parentToken)
    {
        var result = new ServerDeploymentResult { Server = target };
        result.LogEvent($"Starting deployment on '{target.Name}' (app {target.ApplicationId})");
        _logger.Debug($"Deploying to server '{target.Name}' [{target.ServerId}]");

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_perServerTimeoutSeconds)
        };

        try
        {
            var apiClient = new CoolifyApiClient(httpClient, target.ApiUrl, target.ApiKey);
            var appService = new ApplicationService(apiClient, _logger);

            result.LogEvent("Verifying application exists on target server");
            var appCheck = await appService.GetApplicationAsync(target.ApplicationId);

            if (!appCheck.Success || appCheck.Data is null)
            {
                result.Success = false;
                result.ErrorMessage = $"Application {target.ApplicationId} not found: {appCheck.Message}";
                result.LogEvent($"Aborted — {result.ErrorMessage}");
                return result;
            }

            parentToken.ThrowIfCancellationRequested();

            var serverContext = new DeploymentContext
            {
                Application = appCheck.Data,
                EnvironmentVariables = parentContext.EnvironmentVariables,
                TargetStatus = parentContext.TargetStatus,
                RequiresApproval = parentContext.RequiresApproval,
                RollbackToVersion = parentContext.RollbackToVersion
            };

            result.LogEvent("Triggering deployment via Coolify API");
            var deployResponse = await appService.DeployApplicationAsync(target.ApplicationId, serverContext);

            result.Success = deployResponse.Success;
            result.DeploymentId = deployResponse.Data?.DeploymentId;
            result.Message = deployResponse.Message;

            result.LogEvent(deployResponse.Success
                ? $"Accepted — deployment id: {result.DeploymentId}"
                : $"Rejected — {deployResponse.Message}");

            if (!result.Success)
                result.ErrorMessage = deployResponse.Message;
        }
        catch (OperationCanceledException) when (!parentToken.IsCancellationRequested)
        {
            result.Success = false;
            result.ErrorMessage = $"Timed out after {_perServerTimeoutSeconds}s";
            result.LogEvent(result.ErrorMessage);
            _logger.Warn($"Server '{target.Name}' deployment timed out");
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "Deployment cancelled by caller";
            result.LogEvent(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.LogEvent($"Unexpected error: {ex.Message}");
            _logger.Error(ex, $"Deployment to server '{target.Name}' failed unexpectedly");
        }
        finally
        {
            result.CompletedAt = DateTime.UtcNow;
            httpClient.Dispose();
        }

        return result;
    }

    /// <summary>
    /// Aggregates validation errors across all server targets, prefixing each with the server name.
    /// </summary>
    /// <param name="targets">Targets to validate.</param>
    /// <returns>Flat list of validation error messages.</returns>
    private static List<string> CollectValidationErrors(List<ServerTarget> targets) =>
        targets
            .SelectMany(t => t.Validate().Select(e => $"[{t.Name}] {e}"))
            .ToList();
}
