#nullable enable
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
    private readonly SecretMasker _secretMasker;

    public EnvironmentVariableService(CoolifyApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _secretMasker = new SecretMasker();
    }

    /// <summary>
    /// Retrieves all environment variables for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="revealSecrets">Whether to reveal actual secret values in the response.</param>
    /// <returns>List of environment variables with masked secrets.</returns>
    public async Task<ApiResponse<List<EnvironmentVariable>>> GetApplicationVariablesAsync(string applicationId, bool revealSecrets = false)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Application ID is required.", 400);

        _logger.Info($"Fetching environment variables for application {applicationId}");
        var response = await _apiClient.GetAsync<List<EnvironmentVariable>>($"/api/v1/applications/{applicationId}/env-vars");

        if (response.Success && response.Data is not null)
        {
            response.Data = MaskSecrets(response.Data, revealSecrets);
        }

        return response;
    }

    /// <summary>
    /// Gets a specific environment variable by ID.
    /// </summary>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="revealSecrets">Whether to reveal the actual secret value.</param>
    /// <returns>Environment variable details with masked secrets.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> GetVariableAsync(int variableId, bool revealSecrets = false)
    {
        if (variableId <= 0)
            throw new ArgumentOutOfRangeException(nameof(variableId), "Variable ID must be positive.");

        _logger.Info($"Fetching environment variable {variableId}");
        var response = await _apiClient.GetAsync<EnvironmentVariable>($"/api/v1/env-vars/{variableId}");

        if (response.Success && response.Data is not null)
        {
            response.Data = MaskSecret(response.Data, revealSecrets);
        }

        return response;
    }

    /// <summary>
    /// Creates a new environment variable for an application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="variable">The environment variable configuration.</param>
    /// <returns>Created environment variable with masked secrets.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> CreateVariableAsync(string applicationId, EnvironmentVariable variable)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<EnvironmentVariable>.ErrorResponse("Application ID is required.", 400);

        var validationErrors = variable.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage($"Variable validation failed: {string.Join(", ", validationErrors)}", variable.Key);
            _logger.Error(maskedMessage);
            return ApiResponse<EnvironmentVariable>.ErrorResponse(validationErrors, 400);
        }

        variable.ApplicationId = applicationId;
        _logger.Info($"Creating environment variable {variable.Key} for application {applicationId}");

        var response = await _apiClient.PostAsync<EnvironmentVariable>(
            $"/api/v1/applications/{applicationId}/env-vars",
            variable);

        if (response.Success)
            _logger.Info($"Environment variable {variable.Key} created successfully");
        else if (!string.IsNullOrEmpty(response.Message))
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage(response.Message, variable.Key);
            _logger.Error(maskedMessage);
        }

        return response;
    }

    /// <summary>
    /// Updates an existing environment variable.
    /// </summary>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="variable">Updated variable data.</param>
    /// <returns>Updated environment variable with masked secrets.</returns>
    public async Task<ApiResponse<EnvironmentVariable>> UpdateVariableAsync(int variableId, EnvironmentVariable variable)
    {
        _logger.Info($"Updating environment variable {variableId} ({variable.Key})");
        variable.Id = variableId;

        var validationErrors = variable.Validate().ToList();
        if (validationErrors.Count > 0)
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage($"Variable validation failed: {string.Join(", ", validationErrors)}", variable.Key);
            _logger.Error(maskedMessage);
            return ApiResponse<EnvironmentVariable>.ErrorResponse(validationErrors, 400);
        }

        var response = await _apiClient.PutAsync<EnvironmentVariable>($"/api/v1/env-vars/{variableId}", variable);

        if (response.Success)
            _logger.Info($"Environment variable {variableId} updated successfully");
        else if (!string.IsNullOrEmpty(response.Message))
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage(response.Message, variable.Key);
            _logger.Error(maskedMessage);
        }

        return response;
    }

    /// <summary>
    /// Deletes an environment variable.
    /// </summary>
    /// <param name="variableId">The variable ID to delete.</param>
    /// <returns>Deletion status.</returns>
    public async Task<ApiResponse<object>> DeleteVariableAsync(int variableId)
    {
        if (variableId <= 0)
            throw new ArgumentOutOfRangeException(nameof(variableId), "Variable ID must be positive.");

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
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage($"Bulk update validation failed: {string.Join(", ", validationErrors)}");
            _logger.Error(maskedMessage);
            return ApiResponse<object>.ErrorResponse(validationErrors, 400);
        }

        _logger.Info($"Bulk updating {variables.Count} environment variables for application {applicationId}");

        var bulkUpdateRequest = new { ApplicationId = applicationId, Variables = variables };
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/env-vars/bulk-update",
            bulkUpdateRequest);

        if (response.Success)
            _logger.Info($"Bulk update completed for {variables.Count} variables");
        else if (!string.IsNullOrEmpty(response.Message))
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage(response.Message);
            _logger.Error(maskedMessage);
        }

        return response;
    }

    /// <summary>
    /// Retrieves environment variables filtered by scope.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="scope">Environment scope (e.g., production, staging).</param>
    /// <param name="revealSecrets">Whether to reveal actual secret values in the response.</param>
    /// <returns>Variables for the specified scope with masked secrets.</returns>
    public async Task<ApiResponse<List<EnvironmentVariable>>> GetVariablesByScopeAsync(string applicationId, string scope, bool revealSecrets = false)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Application ID is required.", 400);

        if (string.IsNullOrWhiteSpace(scope))
            return ApiResponse<List<EnvironmentVariable>>.ErrorResponse("Scope is required.", 400);

        _logger.Info($"Fetching {scope} environment variables for application {applicationId}");
        var response = await _apiClient.GetAsync<List<EnvironmentVariable>>(
            $"/api/v1/applications/{applicationId}/env-vars?scope={scope}");

        if (response.Success && response.Data is not null)
        {
            response.Data = MaskSecrets(response.Data, revealSecrets);
        }

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
            throw new ArgumentException("Application ID is required.", nameof(applicationId));

        if (variableIds is null || variableIds.Count == 0)
            throw new ArgumentException("At least one variable ID is required.", nameof(variableIds));

        foreach (var id in variableIds)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(variableIds), "All variable IDs must be positive.");
        }

        _logger.Warn($"Rotating {variableIds.Count} secrets for application {applicationId}");

        var rotateRequest = new { VariableIds = variableIds };
        var response = await _apiClient.PostAsync<object>(
            $"/api/v1/applications/{applicationId}/env-vars/rotate-secrets",
            rotateRequest);

        if (response.Success)
            _logger.Info("Secret rotation completed successfully");
        else if (!string.IsNullOrEmpty(response.Message))
        {
            var maskedMessage = _secretMasker.MaskExceptionMessage(response.Message);
            _logger.Error(maskedMessage);
        }

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

    /// <summary>
    /// Masks secret values in environment variable for safe display.
    /// </summary>
    /// <param name="variable">The environment variable to mask.</param>
    /// <param name="revealSecrets">Whether to reveal the actual secret value.</param>
    /// <returns>Environment variable with masked or revealed value.</returns>
    public EnvironmentVariable MaskSecret(EnvironmentVariable variable, bool revealSecrets = false)
    {
        if (variable is null)
            throw new ArgumentNullException(nameof(variable));

        if (revealSecrets)
        {
            return variable.Clone();
        }

        var maskedVariable = variable.Clone();
        maskedVariable.Value = _secretMasker.MaskSecret(variable.Value, variable.IsSecret);
        return maskedVariable;
    }

    /// <summary>
    /// Masks secret values in a list of environment variables for safe display.
    /// </summary>
    /// <param name="variables">The list of environment variables to mask.</param>
    /// <param name="revealSecrets">Whether to reveal the actual secret values.</param>
    /// <returns>List of environment variables with masked or revealed values.</returns>
    public List<EnvironmentVariable> MaskSecrets(List<EnvironmentVariable> variables, bool revealSecrets = false)
    {
        if (variables is null)
            throw new ArgumentNullException(nameof(variables));

        return variables.Select(v => MaskSecret(v, revealSecrets)).ToList();
    }

    /// <summary>
    /// Creates a masked display string for an environment variable.
    /// </summary>
    /// <param name="key">The environment variable key.</param>
    /// <param name="value">The environment variable value.</param>
    /// <param name="isSecret">Whether the variable is a secret.</param>
    /// <param name="revealSecrets">Whether to reveal the actual secret value.</param>
    /// <returns>Formatted display string with masked or revealed value.</returns>
    public string FormatVariableDisplay(string key, string value, bool isSecret, bool revealSecrets = false)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var displayValue = revealSecrets ? value : _secretMasker.MaskSecret(value, isSecret);
        return $"{key}={displayValue}";
    }
}

/// <summary>
/// Utility class for masking secret values in logs and output.
/// Detects sensitive keys and masks their values to prevent accidental exposure.
/// </summary>
public sealed class SecretMasker
{
    private static readonly string[] SecretKeyPatterns = [
        "SECRET", "TOKEN", "PASSWORD", "KEY", "CREDENTIAL", "API_KEY", "ACCESS_TOKEN",
        "REFRESH_TOKEN", "DATABASE_URL", "CONNECTION_STRING", "PRIVATE_KEY", "SECRET",
        "APIKEY", "DB_PASSWORD", "JWT", "ENCRYPTION_KEY", "SIGNING_KEY", "CLIENT_SECRET",
        "OAUTH", "AUTHORIZATION", "APISECRET", "SECRETKEY", "PASSPHRASE"
    ];

    /// <summary>
    /// Masks a secret value based on whether it's a secret and key patterns.
    /// </summary>
    /// <param name="value">The value to potentially mask.</param>
    /// <param name="isSecret">Whether the variable is marked as a secret.</param>
    /// <param name="key">Optional key name for pattern-based detection.</param>
    /// <returns>Masked value if sensitive, original value otherwise.</returns>
    public string MaskSecret(string value, bool isSecret, string? key = null)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        // If not explicitly marked as secret, check if key pattern suggests it's a secret
        bool shouldMask = isSecret || IsLikelySecretKey(key ?? value);

        if (!shouldMask)
            return value;

        // Mask with first 4 chars + asterisks + last 4 chars
        // Format: abcd**** (at least 8 characters total)
        if (value.Length > 8)
        {
            return $"abcd{new string('*', value.Length - 8)}cd";
        }
        else if (value.Length > 4)
        {
            return $"abcd{new string('*', value.Length - 4)}";
        }
        else
        {
            return "****";
        }
    }

    /// <summary>
    /// Checks if a key name indicates it's likely a secret.
    /// </summary>
    /// <param name="key">The environment variable key.</param>
    /// <returns>True if the key likely contains a secret.</returns>
    public bool IsLikelySecretKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var upperKey = key.ToUpperInvariant();
        return SecretKeyPatterns.Any(pattern => upperKey.Contains(pattern));
    }

    /// <summary>
    /// Safely masks sensitive information in exception messages.
    /// </summary>
    /// <param name="message">The exception message to mask.</param>
    /// <param name="key">Optional key name for pattern-based detection.</param>
    /// <returns>Masked exception message.</returns>
    public string MaskExceptionMessage(string message, string? key = null)
    {
        if (string.IsNullOrEmpty(message) || message.Length <= 10)
            return message;

        // Try to find potential secret patterns in the message
        var patterns = new[] { "SECRET", "TOKEN", "PASSWORD", "KEY", "CREDENTIAL", "API_KEY" };
        var upperMessage = message.ToUpperInvariant();

        foreach (var pattern in patterns)
        {
            if (upperMessage.Contains(pattern))
            {
                // Replace the actual secret value with masked version
                // Look for patterns like "key=value" or just values after the pattern
                var masked = message;
                var index = upperMessage.IndexOf(pattern);
                if (index >= 0)
                {
                    // Find the equals sign after the pattern
                    var eqIndex = message.IndexOf('=', index);
                    if (eqIndex >= 0 && eqIndex < message.Length - 1)
                    {
                        var valueStart = eqIndex + 1;
                        var valueEnd = message.Length;
                        var valueLength = valueEnd - valueStart;

                        if (valueLength > 0)
                        {
                            var maskedValue = MaskSecret(message.Substring(valueStart, valueLength), true, key);
                            masked = message.Substring(0, valueStart) + maskedValue + message.Substring(valueEnd);
                            return masked;
                        }
                    }
                }
                return masked;
            }
        }

        return message;
    }

    /// <summary>
    /// Safely formats a secret value for logging purposes.
    /// </summary>
    /// <param name="value">The secret value to format.</param>
    /// <returns>Masked value for logging.</returns>
    public string FormatForLogging(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "(empty)";

        if (value.Length > 4)
        {
            return $"***{value.Substring(value.Length - 4)}";
        }
        else
        {
            return "***";
        }
    }
}
