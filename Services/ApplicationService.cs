#nullable enable
namespace CoolifyCli.Services;

using CoolifyCli.Models;

/// <summary>
/// Service for managing application deployments in Coolify.
/// Orchestrates deployment lifecycle, scaling, and rollback operations.
/// </summary>
public class ApplicationService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public ApplicationService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all applications for the current environment.
    /// </summary>
    /// <returns>List of applications.</returns>
    public async Task<ApiResponse<List<ApplicationDeployment>>> GetAllApplicationsAsync()
    {
        _logger.Info("Fetching all applications");
        var response = await _apiClient.GetAsync<List<ApplicationDeployment>>("/api/v1/applications");
        return response;
    }

    /// <summary>
    /// Retrieves a specific application by ID.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Application details.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> GetApplicationAsync(int applicationId)
    {
        _logger.Info($"Fetching application with ID: {applicationId}");
        var response = await _apiClient.GetAsync<ApplicationDeployment>($"/api/v1/applications/{applicationId}");
        return response;
    }

    /// <summary>
    /// Creates a new application deployment.
    /// </summary>
    /// <param name="application">Application configuration.</param>
    /// <returns>Created application with assigned ID.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> CreateApplicationAsync(ApplicationDeployment application)
    {
        var validationErrors = application.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            _logger.Error($"Application validation failed: {string.Join(", ", validationErrors)}");
            return ApiResponse<ApplicationDeployment>.ErrorResponse(validationErrors, 400);
        }

        _logger.Info($"Creating new application: {application.Name}");
        var response = await _apiClient.PostAsync<ApplicationDeployment>("/api/v1/applications", application);

        if (response.Success)
            _logger.Info($"Application created successfully with ID: {response.Data?.Id}");
        else
            _logger.Error($"Failed to create application: {response.Message}");

        return response;
    }

    /// <summary>
    /// Updates an existing application configuration.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="application">Updated application data.</param>
    /// <returns>Updated application.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> UpdateApplicationAsync(int applicationId, ApplicationDeployment application)
    {
        _logger.Info($"Updating application {applicationId}");
        application.Id = applicationId;

        var validationErrors = application.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            return ApiResponse<ApplicationDeployment>.ErrorResponse(validationErrors, 400);
        }

        var response = await _apiClient.PutAsync<ApplicationDeployment>($"/api/v1/applications/{applicationId}", application);

        if (response.Success)
            _logger.Info($"Application {applicationId} updated successfully");
        else
            _logger.Error($"Failed to update application: {response.Message}");

        return response;
    }

    /// <summary>
    /// Deploys an application to the specified environment.
    /// </summary>
    /// <param name="applicationId">The application ID to deploy.</param>
    /// <param name="deploymentContext">Deployment configuration context.</param>
    /// <returns>Deployment status result.</returns>
    public async Task<ApiResponse<DeploymentContext>> DeployApplicationAsync(int applicationId, DeploymentContext deploymentContext)
    {
        _logger.Info($"Initiating deployment for application {applicationId}");

        var validationErrors = deploymentContext.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            return ApiResponse<DeploymentContext>.ErrorResponse(validationErrors, 400);
        }

        try
        {
            deploymentContext.LogEvent("Starting deployment process", LogLevel.Info);
            var response = await _apiClient.PostAsync<DeploymentContext>(
                $"/api/v1/applications/{applicationId}/deploy",
                deploymentContext);

            if (response.Success)
            {
                _logger.Info($"Deployment initiated successfully for application {applicationId}");
            }
            else
            {
                _logger.Error($"Deployment failed: {response.Message}");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception during deployment: {ex.Message}");
            deploymentContext.LogEvent($"Deployment error: {ex.Message}", LogLevel.Fatal);
            return ApiResponse<DeploymentContext>.ErrorResponse($"Deployment error: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Rolls back an application to a previous version.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="version">Version to rollback to.</param>
    /// <returns>Rollback status.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> RollbackApplicationAsync(int applicationId, string version)
    {
        _logger.Info($"Initiating rollback for application {applicationId} to version {version}");

        if (string.IsNullOrWhiteSpace(version))
        {
            return ApiResponse<ApplicationDeployment>.ErrorResponse("Target version is required for rollback.", 400);
        }

        var rollbackRequest = new { TargetVersion = version };
        var response = await _apiClient.PostAsync<ApplicationDeployment>(
            $"/api/v1/applications/{applicationId}/rollback",
            rollbackRequest);

        if (response.Success)
            _logger.Info($"Rollback completed for application {applicationId}");
        else
            _logger.Error($"Rollback failed: {response.Message}");

        return response;
    }

    /// <summary>
    /// Deletes an application and all associated resources.
    /// </summary>
    /// <param name="applicationId">The application ID to delete.</param>
    /// <returns>Deletion status.</returns>
    public async Task<ApiResponse<object>> DeleteApplicationAsync(int applicationId)
    {
        _logger.Warn($"Deleting application {applicationId}");
        var response = await _apiClient.DeleteAsync<object>($"/api/v1/applications/{applicationId}");

        if (response.Success)
            _logger.Info($"Application {applicationId} deleted successfully");
        else
            _logger.Error($"Failed to delete application: {response.Message}");

        return response;
    }

    /// <summary>
    /// Retrieves deployment history for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="limit">Number of recent deployments to fetch.</param>
    /// <returns>List of past deployments.</returns>
    public async Task<ApiResponse<List<DeploymentContext>>> GetDeploymentHistoryAsync(int applicationId, int limit = 10)
    {
        _logger.Info($"Fetching deployment history for application {applicationId} (limit: {limit})");
        var response = await _apiClient.GetAsync<List<DeploymentContext>>($"/api/v1/applications/{applicationId}/deployments?limit={limit}");
        return response;
    }

    /// <summary>
    /// Starts a stopped application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Updated application status.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> StartApplicationAsync(int applicationId)
    {
        _logger.Info($"Starting application {applicationId}");
        var response = await _apiClient.PostAsync<ApplicationDeployment>(
            $"/api/v1/applications/{applicationId}/start",
            new { });

        if (response.Success)
            _logger.Info($"Application {applicationId} started successfully");

        return response;
    }

    /// <summary>
    /// Stops a running application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Updated application status.</returns>
    public async Task<ApiResponse<ApplicationDeployment>> StopApplicationAsync(int applicationId)
    {
        _logger.Info($"Stopping application {applicationId}");
        var response = await _apiClient.PostAsync<ApplicationDeployment>(
            $"/api/v1/applications/{applicationId}/stop",
            new { });

        if (response.Success)
            _logger.Info($"Application {applicationId} stopped successfully");

        return response;
    }
}
