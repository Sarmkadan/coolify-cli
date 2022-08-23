#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Provides validation and verification methods for <see cref="ServiceHealth"/> instances.
/// </summary>
public static class ServiceHealthValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceHealth"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The service health instance to validate.</param>
    /// <returns>An immutable list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.ServiceId))
        {
            problems.Add("ServiceId is required and cannot be null or whitespace.");
        }

        // Validate numeric ranges
        if (value.ResponseTimeMs < 0)
        {
            problems.Add("ResponseTimeMs cannot be negative.");
        }

        if (value.HttpStatusCode < 0)
        {
            problems.Add("HttpStatusCode cannot be negative.");
        }

        if (value.CpuUsagePercent < 0 || value.CpuUsagePercent > 100)
        {
            problems.Add("CpuUsagePercent must be between 0 and 100 inclusive.");
        }

        if (value.MemoryUsageMb < 0)
        {
            problems.Add("MemoryUsageMb cannot be negative.");
        }

        if (value.ActiveConnections < 0)
        {
            problems.Add("ActiveConnections cannot be negative.");
        }

        if (value.ErrorRatePercent < 0 || value.ErrorRatePercent > 100)
        {
            problems.Add("ErrorRatePercent must be between 0 and 100 inclusive.");
        }

        if (value.FailureCount < 0)
        {
            problems.Add("FailureCount cannot be negative.");
        }

        // Validate dates
        if (value.CheckedAt == default)
        {
            problems.Add("CheckedAt must be set to a valid DateTime.");
        }

        if (value.LastSuccessfulCheck == default)
        {
            problems.Add("LastSuccessfulCheck should be set for successful checks.");
        }

        // Validate status consistency
        if (value.Status == HealthStatus.Unknown && value.FailureCount == 0 && value.ResponseTimeMs == 0)
        {
            problems.Add("Status is Unknown but no failure or metrics indicate this is expected.");
        }

        // Validate failure reason consistency
        if (value.Status != HealthStatus.Critical && !string.IsNullOrEmpty(value.FailureReason))
        {
            problems.Add("FailureReason should only be set when Status is Critical.");
        }

        // Validate warnings list
        if (value.Warnings == null)
        {
            problems.Add("Warnings collection cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceHealth"/> instance is valid.
    /// </summary>
    /// <param name="value">The service health instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceHealth value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceHealth"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The service health instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ServiceHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ServiceHealth instance is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                }",
                nameof(value));
        }
    }
}