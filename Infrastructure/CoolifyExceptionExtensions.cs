using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Provides extension methods for <see cref="CoolifyException"/> and its derived types
/// to facilitate common error handling patterns and diagnostics.
/// </summary>
public static class CoolifyExceptionExtensions
{
    /// <summary>
    /// Creates a comprehensive error message that includes the exception's error context,
    /// HTTP status code (if available), and all context data.
    /// </summary>
    /// <param name="exception">The exception to format. Must not be null.</param>
    /// <returns>A formatted error message suitable for logging or user display.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string ToDetailedErrorMessage(this CoolifyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = new System.Text.StringBuilder();
        message.AppendLine(exception.Message);

        if (!string.IsNullOrEmpty(exception.ErrorCode))
        {
            message.Append("Error Code: ").AppendLine(exception.ErrorCode);
        }

        if (exception is ApiCommunicationException apiCommEx && apiCommEx.HttpStatusCode.HasValue)
        {
            message.Append("HTTP Status: ").AppendLine(apiCommEx.HttpStatusCode.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (exception is ApiException apiEx && apiEx.StatusCode > 0)
        {
            message.Append("API Status: ").AppendLine(apiEx.StatusCode.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(apiEx.ApiErrorCode))
            {
                message.Append("API Error Code: ").AppendLine(apiEx.ApiErrorCode);
            }
        }

        if (exception is DeploymentException deploymentEx && !string.IsNullOrEmpty(deploymentEx.DeploymentId))
        {
            message.Append("Deployment ID: ").AppendLine(deploymentEx.DeploymentId);
        }

        if (exception is OperationTimeoutException timeoutEx)
        {
            message.Append("Timeout: ").AppendLine(timeoutEx.Timeout.ToString());
        }

        if (exception.ContextData.Count > 0)
        {
            message.AppendLine("Context Data:");
            foreach (var kvp in exception.ContextData)
            {
                message.Append("  ").Append(kvp.Key).Append(": ").AppendLine(kvp.Value);
            }
        }

        if (exception.InnerException != null)
        {
            message.AppendLine("Inner Exception:");
            message.Append("  ").AppendLine(exception.InnerException.ToString());
        }

        return message.ToString().Trim();
    }

    /// <summary>
    /// Determines whether the exception represents a client error (4xx HTTP status).
    /// </summary>
    /// <param name="exception">The exception to check. Must not be null.</param>
    /// <returns>True if the exception indicates a client error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsClientError(this CoolifyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ApiCommunicationException apiCommEx => apiCommEx.HttpStatusCode >= 400 && apiCommEx.HttpStatusCode < 500,
            ApiException apiEx => apiEx.StatusCode >= 400 && apiEx.StatusCode < 500,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the exception represents a server error (5xx HTTP status).
    /// </summary>
    /// <param name="exception">The exception to check. Must not be null.</param>
    /// <returns>True if the exception indicates a server error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsServerError(this CoolifyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ApiCommunicationException apiCommEx => apiCommEx.HttpStatusCode >= 500,
            ApiException apiEx => apiEx.StatusCode >= 500,
            _ => false
        };
    }

    /// <summary>
    /// Adds multiple context data entries from a dictionary in a single call.
    /// </summary>
    /// <param name="exception">The exception to add context to. Must not be null.</param>
    /// <param name="data">The dictionary of context data to add. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
    public static void AddContextData(this CoolifyException exception, Dictionary<string, string> data)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(data);

        foreach (var kvp in data)
        {
            exception.AddContextData(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Gets the combined error code and status information as a single string identifier.
    /// Useful for error categorization and metrics.
    /// </summary>
    /// <param name="exception">The exception to analyze. Must not be null.</param>
    /// <returns>A string combining error code and status, or just the error code if no status available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetErrorIdentifier(this CoolifyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ApiCommunicationException apiCommEx when apiCommEx.HttpStatusCode.HasValue
                => $"API_COMMUNICATION_{apiCommEx.HttpStatusCode.Value}",
            ApiException apiEx => $"API_{apiEx.StatusCode}_{apiEx.ApiErrorCode ?? "UNKNOWN"}",
            DeploymentException => $"DEPLOYMENT_{exception.ErrorCode}",
            OperationTimeoutException => $"TIMEOUT_{exception.ErrorCode}",
            ValidationException => $"VALIDATION_{exception.ErrorCode}",
            _ => exception.ErrorCode ?? "UNKNOWN_ERROR"
        };
    }
}