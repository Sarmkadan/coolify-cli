#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Services;

using CoolifyCli.Models;

/// <summary>
/// Orchestrates complex deployment workflows including pre-deployment checks,
/// database migrations, and rollback procedures.
/// </summary>
public class DeploymentOrchestrator
{
    private readonly ApplicationService _applicationService;
    private readonly DatabaseService _databaseService;
    private readonly LogService _logService;
    private readonly HealthCheckService _healthService;
    private readonly ILogger _logger;

    public DeploymentOrchestrator(
        ApplicationService applicationService,
        DatabaseService databaseService,
        LogService logService,
        HealthCheckService healthService,
        ILogger logger)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a safe deployment with pre-checks, deployment, and verification.
    /// </summary>
    /// <param name="applicationId">Application to deploy.</param>
    /// <param name="linkedDatabaseIds">Optional database IDs to include.</param>
    /// <returns>Deployment result with status.</returns>
    public async Task<DeploymentResult> ExecuteSafeDeploymentAsync(
        int applicationId,
        List<int>? linkedDatabaseIds = null)
    {
        var result = new DeploymentResult { ApplicationId = applicationId };

        try
        {
            _logger.Info($"Starting safe deployment for application {applicationId}");
            result.LogEvent("Deployment orchestration started");

            // Step 1: Pre-deployment validation
            result.LogEvent("Performing pre-deployment checks");
            var preCheckResult = await PerformPreDeploymentChecksAsync(applicationId, linkedDatabaseIds);

            if (!preCheckResult.Success)
            {
                result.Success = false;
                result.CompletedAt = DateTime.UtcNow;
                result.LogEvent($"Pre-deployment checks failed: {string.Join(", ", preCheckResult.Errors)}");
                return result;
            }

            // Step 2: Health check
            result.LogEvent("Checking current application health");
            var healthResult = await _healthService.CheckApplicationHealthAsync(applicationId);

            if (healthResult.Success && healthResult.Data is not null)
            {
                result.PreDeploymentHealth = healthResult.Data;
                result.LogEvent($"Current health status: {healthResult.Data.Status}");
            }

            // Step 3: Create backup snapshot (for databases)
            if (linkedDatabaseIds is not null && linkedDatabaseIds.Count > 0)
            {
                result.LogEvent($"Creating backups for {linkedDatabaseIds.Count} linked databases");
                await BackupDatabasesAsync(linkedDatabaseIds, result);
            }

            // Step 4: Execute deployment
            result.LogEvent("Initiating application deployment");
            var deploymentContext = new DeploymentContext { Application = new ApplicationDeployment { Id = applicationId } };
            var deployResult = await _applicationService.DeployApplicationAsync(applicationId, deploymentContext);

            if (!deployResult.Success)
            {
                result.Success = false;
                result.CompletedAt = DateTime.UtcNow;
                result.LogEvent($"Deployment failed: {deployResult.Message}");
                return result;
            }

            result.LogEvent("Deployment completed successfully");

            // Step 5: Verification
            result.LogEvent("Waiting for application to stabilize");
            await Task.Delay(5000); // Allow time for deployment to complete

            var postHealthResult = await _healthService.CheckApplicationHealthAsync(applicationId);
            if (postHealthResult.Success && postHealthResult.Data is not null)
            {
                result.PostDeploymentHealth = postHealthResult.Data;
                result.LogEvent($"Post-deployment health status: {postHealthResult.Data.Status}");

                if (postHealthResult.Data.IsHealthy())
                {
                    result.Success = true;
                    result.LogEvent("Deployment successful and application is healthy");
                }
                else
                {
                    result.Success = false;
                    result.LogEvent("Application is not healthy after deployment");
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Deployment orchestration error");
            result.Success = false;
            result.CompletedAt = DateTime.UtcNow;
            result.LogEvent($"Critical error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Performs pre-deployment validation checks.
    /// </summary>
    /// <param name="applicationId">Application to check.</param>
    /// <param name="linkedDatabaseIds">Database IDs to validate.</param>
    /// <returns>Pre-check validation result.</returns>
    private async Task<ApiResponse<object>> PerformPreDeploymentChecksAsync(int applicationId, List<int>? linkedDatabaseIds)
    {
        var errors = new List<string>();

        // Check application exists and is valid
        var appResult = await _applicationService.GetApplicationAsync(applicationId);
        if (!appResult.Success || appResult.Data is null)
        {
            errors.Add($"Application {applicationId} not found");
            return ApiResponse<object>.ErrorResponse(errors, 404);
        }

        var validationErrors = appResult.Data.Validate().ToList();
        errors.AddRange(validationErrors);

        // Check linked databases
        if (linkedDatabaseIds is not null && linkedDatabaseIds.Count > 0)
        {
            foreach (var dbId in linkedDatabaseIds)
            {
                var dbResult = await _databaseService.GetDatabaseAsync(dbId);
                if (!dbResult.Success)
                    errors.Add($"Database {dbId} not found or inaccessible");

                // Check database health
                var healthResult = await _databaseService.CheckDatabaseHealthAsync(dbId);
                if (healthResult.Success && healthResult.Data is not null && !healthResult.Data.IsHealthy())
                    errors.Add($"Database {dbId} is not healthy");
            }
        }

        return errors.Count > 0
            ? ApiResponse<object>.ErrorResponse(errors, 400)
            : ApiResponse<object>.SuccessResponse(new { });
    }

    /// <summary>
    /// Creates backup snapshots for specified databases.
    /// </summary>
    /// <param name="databaseIds">Database IDs to backup.</param>
    /// <param name="result">Deployment result to log to.</param>
    private async Task BackupDatabasesAsync(List<int> databaseIds, DeploymentResult result)
    {
        foreach (var dbId in databaseIds)
        {
            try
            {
                result.LogEvent($"Backing up database {dbId}");
                var backupResult = await _databaseService.BackupDatabaseAsync(dbId);

                if (backupResult.Success)
                {
                    result.DatabaseBackups.Add(dbId);
                    result.LogEvent($"Database {dbId} backup completed");
                }
                else
                {
                    result.LogEvent($"Database {dbId} backup failed: {backupResult.Message}");
                }
            }
            catch (Exception ex)
            {
                result.LogEvent($"Error backing up database {dbId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Executes a rollback to a previous version.
    /// </summary>
    /// <param name="applicationId">Application to rollback.</param>
    /// <param name="previousVersion">Version to rollback to.</param>
    /// <returns>Rollback result.</returns>
    public async Task<RollbackResult> ExecuteRollbackAsync(int applicationId, string previousVersion)
    {
        var result = new RollbackResult { ApplicationId = applicationId, TargetVersion = previousVersion };

        try
        {
            _logger.Info($"Executing rollback for application {applicationId} to version {previousVersion}");
            result.LogEvent("Rollback process initiated");

            // Get current health before rollback
            var currentHealth = await _healthService.CheckApplicationHealthAsync(applicationId);
            result.PreRollbackHealth = currentHealth.Data;

            // Execute rollback
            result.LogEvent($"Rolling back to version {previousVersion}");
            var rollbackResult = await _applicationService.RollbackApplicationAsync(applicationId, previousVersion);

            if (!rollbackResult.Success)
            {
                result.Success = false;
                result.LogEvent($"Rollback failed: {rollbackResult.Message}");
                return result;
            }

            // Wait for application to stabilize
            await Task.Delay(3000);

            // Verify health after rollback
            var postRollbackHealth = await _healthService.CheckApplicationHealthAsync(applicationId);
            result.PostRollbackHealth = postRollbackHealth.Data;

            result.Success = postRollbackHealth.Success && postRollbackHealth.Data?.IsHealthy() == true;
            result.LogEvent($"Rollback completed. Status: {(result.Success ? "Successful" : "Failed")}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Rollback orchestration error");
            result.Success = false;
            result.LogEvent($"Rollback error: {ex.Message}");
            return result;
        }
    }
}

/// <summary>
/// Result object for deployment orchestration.
/// </summary>
public class DeploymentResult
{
    public int ApplicationId { get; set; }
    public bool Success { get; set; }
    public List<string> Events { get; set; } = new();
    public ServiceHealth? PreDeploymentHealth { get; set; }
    public ServiceHealth? PostDeploymentHealth { get; set; }
    public List<int> DatabaseBackups { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public void LogEvent(string message)
    {
        Events.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }

    public TimeSpan GetDuration() => (CompletedAt ?? DateTime.UtcNow) - StartedAt;
}

/// <summary>
/// Result object for rollback orchestration.
/// </summary>
public class RollbackResult
{
    public int ApplicationId { get; set; }
    public string? TargetVersion { get; set; }
    public bool Success { get; set; }
    public List<string> Events { get; set; } = new();
    public ServiceHealth? PreRollbackHealth { get; set; }
    public ServiceHealth? PostRollbackHealth { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public void LogEvent(string message)
    {
        Events.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
}
