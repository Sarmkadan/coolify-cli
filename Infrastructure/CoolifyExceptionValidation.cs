using System.Globalization;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="CoolifyException"/> and its derived exception types.
/// </summary>
public static class CoolifyExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="CoolifyException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate. Cannot be null.</param>
    /// <returns>An immutable list of validation problems; empty if the exception is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CoolifyException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ErrorCode
        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode must not be null, empty, or whitespace.");
        }

        // Validate ContextData
        if (value.ContextData is null)
        {
            problems.Add("ContextData dictionary must not be null.");
        }
        else if (value.ContextData.Count > 100)
        {
            problems.Add("ContextData dictionary contains more than 100 entries; consider reducing the size.");
        }

        // Validate derived exception properties
        if (value is ApiCommunicationException apiCommEx)
        {
            ValidateApiCommunicationException(apiCommEx, problems);
        }
        else if (value is ApiException apiEx)
        {
            ValidateApiException(apiEx, problems);
        }
        else if (value is DeploymentException deploymentEx)
        {
            ValidateDeploymentException(deploymentEx, problems);
        }
        else if (value is OperationTimeoutException timeoutEx)
        {
            ValidateOperationTimeoutException(timeoutEx, problems);
        }
        else if (value is ValidationException validationEx)
        {
            ValidateValidationException(validationEx, problems);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="CoolifyException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate. Cannot be null.</param>
    /// <returns>True if the exception is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CoolifyException value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CoolifyException"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The exception to validate. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the exception contains validation problems.</exception>
    public static void EnsureValid(this CoolifyException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CoolifyException validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    private static void ValidateApiCommunicationException(ApiCommunicationException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (exception.HttpStatusCode.HasValue)
        {
            if (exception.HttpStatusCode < 100 || exception.HttpStatusCode > 599)
            {
                problems.Add("ApiCommunicationException.HttpStatusCode must be a valid HTTP status code (100-599).");
            }
        }
    }

    private static void ValidateApiException(ApiException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (exception.StatusCode < 100 || exception.StatusCode > 599)
        {
            problems.Add("ApiException.StatusCode must be a valid HTTP status code (100-599).");
        }

        if (string.IsNullOrWhiteSpace(exception.ApiErrorCode))
        {
            problems.Add("ApiException.ApiErrorCode must not be null, empty, or whitespace.");
        }
    }

    private static void ValidateDeploymentException(DeploymentException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (string.IsNullOrWhiteSpace(exception.DeploymentId))
        {
            problems.Add("DeploymentException.DeploymentId must not be null, empty, or whitespace.");
        }
    }

    private static void ValidateOperationTimeoutException(OperationTimeoutException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (exception.Timeout <= TimeSpan.Zero)
        {
            problems.Add("OperationTimeoutException.Timeout must be a positive time span greater than zero.");
        }
    }

    private static void ValidateValidationException(ValidationException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (exception.ValidationErrors is null)
        {
            problems.Add("ValidationException.ValidationErrors list must not be null.");
        }
        else if (exception.ValidationErrors.Count == 0)
        {
            problems.Add("ValidationException.ValidationErrors list must not be empty.");
        }
        else
        {
            for (int i = 0; i < exception.ValidationErrors.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(exception.ValidationErrors[i]))
                {
                    problems.Add($"ValidationException.ValidationErrors[{i}] must not be null, empty, or whitespace.");
                }
            }
        }
    }
}