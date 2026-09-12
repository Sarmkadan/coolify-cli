#nullable enable
namespace CoolifyCli.Infrastructure;

/// <summary>
/// Base exception for all Coolify CLI specific errors.
/// </summary>
public class CoolifyException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, string> ContextData { get; set; } = new();

    /// <summary>
/// Initialises a new instance of the <see cref="CoolifyException"/> class with a specified error message and optional error code.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="errorCode">An optional error code associated with the exception.</param>
    public CoolifyException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
/// Initialises a new instance of the <see cref="CoolifyException"/> class with a specified error message, inner exception, and optional error code.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
/// <param name="errorCode">An optional error code associated with the exception.</param>
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
    /// <summary>
/// Initialises a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
    public ConfigurationException(string message) : base(message, "CONFIG_ERROR") { }
/// <summary>
/// Initialises a new instance of the <see cref="ConfigurationException"/> class with a specified error message and inner exception.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException, "CONFIG_ERROR") { }
}

/// <summary>
/// Exception raised when API communication fails.
/// </summary>
public class ApiCommunicationException : CoolifyException
{
    public int? HttpStatusCode { get; set; }

    /// <summary>
/// Initialises a new instance of the <see cref="ApiCommunicationException"/> class with a specified error message and optional HTTP status code.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="statusCode">The HTTP status code associated with the exception, if available.</param>
    public ApiCommunicationException(string message, int? statusCode = null)
        : base(message, "API_COMMUNICATION_ERROR")
    {
        HttpStatusCode = statusCode;
    }

/// <summary>
/// Initialises a new instance of the <see cref="ApiCommunicationException"/> class with a specified error message, inner exception, and optional HTTP status code.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
/// <param name="statusCode">The HTTP status code associated with the exception, if available.</param>
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

    /// <summary>
/// Initialises a new instance of the <see cref="ApiException"/> class with a specified error message, HTTP status code, and optional API error code.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="statusCode">The HTTP status code returned by the API.</param>
/// <param name="apiErrorCode">The optional API-specific error code.</param>
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
    /// <summary>
    /// Initialises a new instance of the <see cref="ApplicationNotFoundException"/> class with the specified application identifier.
    /// </summary>
    /// <param name="applicationId">The identifier of the application that was not found.</param>
    public ApplicationNotFoundException(int applicationId)
        : base($"Application {applicationId} not found.", 404, "NOT_FOUND") { }
}

/// <summary>
/// Exception raised when a database is not found.
/// </summary>
public class DatabaseNotFoundException : ApiException
{
    /// <summary>
    /// Initialises a new instance of the <see cref="DatabaseNotFoundException"/> class with the specified database identifier.
    /// </summary>
    /// <param name="databaseId">The identifier of the database that was not found.</param>
    public DatabaseNotFoundException(int databaseId)
        : base($"Database {databaseId} not found.", 404, "NOT_FOUND") { }
}

/// <summary>
/// Exception raised when a deployment operation fails.
/// </summary>
public class DeploymentException : CoolifyException
{
    public string? DeploymentId { get; set; }

    /// <summary>
/// Initialises a new instance of the <see cref="DeploymentException"/> class with a specified error message and optional deployment identifier.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="deploymentId">The optional identifier of the deployment that failed.</param>
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

    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR") { }

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

// ─── Infrastructure-as-code exceptions ───────────────────────────────────────

/// <summary>
/// Exception raised when an infrastructure template file cannot be located at the
/// specified path or within the configured search paths.
/// </summary>
public class TemplateNotFoundException : CoolifyException
{
    /// <summary>Gets the file path that was searched.</summary>
    public string FilePath { get; }

    /// <summary>
    /// Initialises the exception with the path that could not be resolved.
    /// </summary>
    /// <param name="filePath">The path that was searched.</param>
    public TemplateNotFoundException(string filePath)
        : base($"Infrastructure template not found: '{filePath}'", "TEMPLATE_NOT_FOUND")
    {
        FilePath = filePath;
        AddContextData("filePath", filePath);
    }
}

/// <summary>
/// Exception raised when a template document fails structural or semantic validation and
/// cannot be applied. Contains every validation error so the caller can report them all
/// without re-running validation.
/// </summary>
public class TemplateValidationException : CoolifyException
{
    /// <summary>Gets the complete list of validation errors that triggered this exception.</summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Initialises the exception from a non-empty list of validation errors.
    /// </summary>
    /// <param name="errors">The validation errors accumulated during template validation.</param>
    public TemplateValidationException(IReadOnlyList<string> errors)
        : base(
            $"Template validation failed with {errors.Count} error(s): " +
            string.Join("; ", errors),
            "TEMPLATE_VALIDATION_FAILED")
    {
        ValidationErrors = errors;
        AddContextData("errorCount", errors.Count.ToString());
    }
}

/// <summary>
/// Exception raised when one or more resource operations fail during a template apply
/// phase. Contains the template name and the number of failed operations so the caller
/// can emit a precise summary.
/// </summary>
public class TemplateApplyException : CoolifyException
{
    /// <summary>Gets the name of the template whose apply phase produced failures.</summary>
    public string TemplateName { get; }

    /// <summary>Gets the count of operations that did not succeed.</summary>
    public int FailedOperations { get; }

    /// <summary>
    /// Initialises the exception with context about the failed apply run.
    /// </summary>
    /// <param name="templateName">The <c>metadata.name</c> of the template being applied.</param>
    /// <param name="failedOperations">The number of resource operations that failed.</param>
    /// <param name="message">Human-readable summary of the failure.</param>
    public TemplateApplyException(string templateName, int failedOperations, string message)
        : base(message, "TEMPLATE_APPLY_FAILED")
    {
        TemplateName     = templateName;
        FailedOperations = failedOperations;
        AddContextData("templateName",     templateName);
        AddContextData("failedOperations", failedOperations.ToString());
    }
}
