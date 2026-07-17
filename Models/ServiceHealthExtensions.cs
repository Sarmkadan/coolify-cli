using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoolifyCli.Models
{
    /// <summary>
    /// Extension methods for <see cref="ServiceHealth"/>.
    /// </summary>
    public static class ServiceHealthExtensions
    {
        /// <summary>
        /// Gets the elapsed time since the last successful health check.
        /// </summary>
        /// <param name="health">The <see cref="ServiceHealth"/> instance.</param>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the time elapsed since <see cref="ServiceHealth.LastSuccessfulCheck"/>,
        /// or <c>null</c> if no successful check has been recorded.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="health"/> is <c>null</c>.</exception>
        public static TimeSpan? TimeSinceLastSuccess(this ServiceHealth health)
        {
            ArgumentNullException.ThrowIfNull(health);
            return health.LastSuccessfulCheck.HasValue
                ? DateTime.UtcNow - health.LastSuccessfulCheck.Value
                : null;
        }

        /// <summary>
        /// Returns a concise, human-readable summary of the service health.
        /// </summary>
        /// <param name="health">The <see cref="ServiceHealth"/> instance.</param>
        /// <returns>A formatted string containing key health metrics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="health"/> is <c>null</c>.</exception>
        public static string ToSummary(this ServiceHealth health)
        {
            ArgumentNullException.ThrowIfNull(health);
            return string.Format(
                CultureInfo.InvariantCulture,
                "Service '{0}' (ID: {1}) – Status: {2}, Healthy: {3}, Response: {4:F1} ms, CPU: {5:F1} %, Memory: {6:F1} MiB, Errors: {7:F1} %, Connections: {8}",
                health.ServiceId,
                health.Id,
                health.Status,
                health.IsHealthy,
                health.ResponseTimeMs,
                health.CpuUsagePercent,
                health.MemoryUsageMb,
                health.ErrorRatePercent,
                health.ActiveConnections);
        }

        /// <summary>
        /// Retrieves the warnings associated with the health record as a read-only list.
        /// </summary>
        /// <param name="health">The <see cref="ServiceHealth"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of warning messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="health"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetWarnings(this ServiceHealth health)
        {
            ArgumentNullException.ThrowIfNull(health);
            return health.Warnings.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the service health should be considered critical.
        /// </summary>
        /// <param name="health">The <see cref="ServiceHealth"/> instance.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="ServiceHealth.Status"/> is <see cref="HealthStatus.Critical"/>,
        /// or if the <see cref="ServiceHealth.FailureCount"/> exceeds five consecutive failures,
        /// or if the <see cref="ServiceHealth.ErrorRatePercent"/> exceeds 50%; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="health"/> is <c>null</c>.</exception>
        public static bool IsCritical(this ServiceHealth health)
        {
            ArgumentNullException.ThrowIfNull(health);
            return health.Status is HealthStatus.Critical
                || health.FailureCount > 5
                || health.ErrorRatePercent > 50.0;
        }
    }
}
