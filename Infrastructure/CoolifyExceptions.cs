// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Infrastructure;

/// <summary>
/// Base exception for all Coolify CLI specific errors.
/// </summary>
public class CoolifyException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, string> ContextData { get; set; } = new();

    public CoolifyException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public CoolifyException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public void AddContextData(string key, string value)
    {
        ContextData[key] = value;
    }
}

/// <summary>
/// Exception raised when API configuration is invalid or missing.
/// </summary>
public class ConfigurationException : CoolifyException
{
    public ConfigurationException(string message) : base(message, "CONFIG_ERROR") { }
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException, "CONFIG_ERROR") { }
}

/// <summary>
/// Exception raised when API communication fails.
/// </summary>
public class ApiCommunicationException : CoolifyException
{
    public int? HttpStatusCode { get; set; }

    public ApiCommunicationException(string message, int? statusCode = null)
        : base(message, "API_COMMUNICATION_ERROR")
    {
        HttpStatusCode = statusCode;
    }

    public ApiCommunicationException(string message, Exception innerException, int? statusCode = null)
        : base(message, innerException, "API_COMMUNICATION_ERROR")
    {
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Exception raised when API returns an error response.
/// </summary>
public class ApiException : CoolifyException
{
    public int StatusCode { get; set; }
    public string? ApiErrorCode { get; set; }

    public ApiException(string message, int statusCode, string? apiErrorCode = null)
        : base(message, "API_ERROR")
    {
        StatusCode = statusCode;
        ApiErrorCode = apiErrorCode;
    }
}

/// <summary>
/// Exception raised when an application is not found.
/// </summary>
public class ApplicationNotFoundException : ApiException
{
    public ApplicationNotFoundException(int applicationId)
        : base($"Application {applicationId} not found.", 404, "NOT_FOUND") { }
}

/// <summary>
/// Exception raised when a database is not found.
/// </summary>
public class DatabaseNotFoundException : ApiException
{
    public DatabaseNotFoundException(int databaseId)
        : base($"Database {databaseId} not found.", 404, "NOT_FOUND") { }
}

/// <summary>
/// Exception raised when a deployment operation fails.
/// </summary>
public class DeploymentException : CoolifyException
{
    public string? DeploymentId { get; set; }

    public DeploymentException(string message, string? deploymentId = null)
        : base(message, "DEPLOYMENT_ERROR")
    {
        DeploymentId = deploymentId;
    }
}

/// <summary>
/// Exception raised when an operation times out.
/// </summary>
public class OperationTimeoutException : CoolifyException
{
    public TimeSpan Timeout { get; set; }

    public OperationTimeoutException(string message, TimeSpan timeout)
        : base(message, "OPERATION_TIMEOUT")
    {
        Timeout = timeout;
    }
}

/// <summary>
/// Exception raised when input validation fails.
/// </summary>
public class ValidationException : CoolifyException
{
    public List<string> ValidationErrors { get; set; } = new();

    public ValidationException(string message, List<string> errors)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationErrors = errors;
    }

    public ValidationException(List<string> errors)
        : base("Validation failed.", "VALIDATION_ERROR")
    {
        ValidationErrors = errors;
    }
}
