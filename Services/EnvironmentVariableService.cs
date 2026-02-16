#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Services;

using CoolifyCli.Models;

/// <summary>
/// Service for managing environment variables across applications.
/// Supports secure secret management, environment-specific values, and change tracking.
/// </summary>
public class EnvironmentVariableService
{
    private readonly CoolifyApiClient _apiClient;
    private readonly ILogger _logger;

    public EnvironmentVariableService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all environment variables for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>List of environment variables.</returns>
    public async Task<ApiResponse<List<EnvironmentVariable>>> GetApplicationVariablesAsync(string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Application ID is required.", 400);

        _logger.Info($"Fetching environment variables for application {applicationId}");
        var response = await _apiClient.GetAsync<List<EnvironmentVariable>>($"/api/v1/applications/{applicationId}/env-vars");

        return response;
    }

    /// <summary>
    /// Gets a specific environment variable by ID.
    /// </summary>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>Environment variable details.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> GetVariableAsync(int variableId)
    {
        _logger.Info($"Fetching environment variable {variableId}");
        var response = await _apiClient.GetAsync<EnvironmentVariable>($"/api/v1/env-vars/{variableId}");

        return response;
    }

    /// <summary>
    /// Creates a new environment variable for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="variable">The environment variable configuration.</param>
    /// <returns>Created environment variable.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> CreateVariableAsync(string applicationId, EnvironmentVariable variable)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<EnvironmentVariable>.ErrorResponse("Application ID is required.", 400);

        var validationErrors = variable.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            _logger.Error($"Variable validation failed: {string.Join(", ", validationErrors)}");
            return ApiResponse<EnvironmentVariable>.ErrorResponse(validationErrors, 400);
        }

        variable.ApplicationId = applicationId;
        _logger.Info($"Creating environment variable {variable.Key} for application {applicationId}");

        var response = await _apiClient.PostAsync<EnvironmentVariable>(
            $"/api/v1/applications/{applicationId}/env-vars",
            variable);

        if (response.Success)
            _logger.Info($"Environment variable {variable.Key} created successfully");

        return response;
    }

    /// <summary>
    /// Updates an existing environment variable.
    /// </summary>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="variable">Updated variable data.</param>
    /// <returns>Updated environment variable.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> UpdateVariableAsync(int variableId, EnvironmentVariable variable)
    {
        _logger.Info($"Updating environment variable {variableId} ({variable.Key})");
        variable.Id = variableId;

        var validationErrors = variable.Validate().ToList();
        if (validationErrors.Count > 0)
            return ApiResponse<EnvironmentVariable>.ErrorResponse(validationErrors, 400);

        var response = await _apiClient.PutAsync<EnvironmentVariable>($"/api/v1/env-vars/{variableId}", variable);

        if (response.Success)
            _logger.Info($"Environment variable {variableId} updated successfully");

        return response;
    }

    /// <summary>
    /// Deletes an environment variable.
    /// </summary>
    /// <param name="variableId">The variable ID to delete.</param>
    /// <returns>Deletion status.</returns>
    public async Task<ApiResponse<object>> DeleteVariableAsync(int variableId)
    {
        _logger.Warn($"Deleting environment variable {variableId}");
        var response = await _apiClient.DeleteAsync<object>($"/api/v1/env-vars/{variableId}");

        if (response.Success)
            _logger.Info($"Environment variable {variableId} deleted successfully");

        return response;
    }

    /// <summary>
    /// Bulk updates multiple environment variables.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="variables">List of variables to update.</param>
    /// <returns>Update result.</returns>
    public async Task<ApiResponse<object>> BulkUpdateVariablesAsync(string applicationId, List<EnvironmentVariable> variables)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<object>.ErrorResponse("Application ID is required.", 400);

        if (variables is null || variables.Count == 0)
            return ApiResponse<object>.ErrorResponse("At least one variable is required.", 400);

        var validationErrors = new List<string>();
        foreach (var variable in variables)
        {
            var errors = variable.Validate().ToList();
            validationErrors.AddRange(errors);
        }

        if (validationErrors.Count > 0)
            return ApiResponse<object>.ErrorResponse(validationErrors, 400);

        _logger.Info($"Bulk updating {variables.Count} environment variables for application {applicationId}");

        var bulkUpdateRequest = new { ApplicationId = applicationId, Variables = variables };
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/env-vars/bulk-update",
            bulkUpdateRequest);

        if (response.Success)
            _logger.Info($"Bulk update completed for {variables.Count} variables");

        return response;
    }

    /// <summary>
    /// Retrieves environment variables filtered by scope.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="scope">Environment scope (e.g., production, staging).</param>
    /// <returns>Variables for the specified scope.</returns>
    public async Task<ApiResponse<List<EnvironmentVariable>>> GetVariablesByScopeAsync(string applicationId, string scope)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Application ID is required.", 400);

        if (string.IsNullOrWhiteSpace(scope))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Scope is required.", 400);

        _logger.Info($"Fetching {scope} environment variables for application {applicationId}");
        var response = await _apiClient.GetAsync<List<EnvironmentVariable>>(
            $"/api/v1/applications/{applicationId}/env-vars?scope={scope}");

        return response;
    }

    /// <summary>
    /// Rotates secret values for sensitive variables.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="variableIds">IDs of variables to rotate.</param>
    /// <returns>Rotation result with new values.</returns>
    public async Task<ApiResponse<object>> RotateSecretsAsync(string applicationId, List<int> variableIds)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<object>.ErrorResponse("Application ID is required.", 400);

        if (variableIds is null || variableIds.Count == 0)
            return ApiResponse<object>.ErrorResponse("At least one variable ID is required.", 400);

        _logger.Warn($"Rotating {variableIds.Count} secrets for application {applicationId}");

        var rotateRequest = new { VariableIds = variableIds };
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/env-vars/rotate-secrets",
            rotateRequest);

        if (response.Success)
            _logger.Info("Secret rotation completed successfully");

        return response;
    }

    /// <summary>
    /// Retrieves environment variable change history.
    /// </summary>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="limit">Number of changes to retrieve.</param>
    /// <returns>List of historical changes.</returns>
    public async Task<ApiResponse<List<object>>> GetChangeHistoryAsync(int variableId, int limit = 20)
    {
        if (limit < 1 || limit > 100)
            return ApiResponse<List<object>>.ErrorResponse("Limit must be between 1 and 100.", 400);

        _logger.Info($"Fetching change history for variable {variableId}");
        var response = await _apiClient.GetAsync<List<object>>($"/api/v1/env-vars/{variableId}/history?limit={limit}");

        return response;
    }

    /// <summary>
    /// Validates all environment variables for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <returns>Validation results.</returns>
    public async Task<ApiResponse<object>> ValidateVariablesAsync(string applicationId)
    {
        _logger.Info($"Validating environment variables for application {applicationId}");
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/env-vars/validate",
            new { });

        return response;
    }
}
