#nullable enable

using CoolifyCli.Models;

namespace CoolifyCli.Extensions;

/// <summary>
/// Extension methods for <see cref="HealthSummary"/> to provide additional health status utilities.
/// </summary>
public static class HealthSummaryExtensions
{
    /// <summary>
    /// Returns an emoji representing the overall health status based on service counts.
    /// </summary>
    /// <param name="summary">The health summary to evaluate.</param>
    /// <returns>
    /// 🔴 if any critical services exist,
    /// 🟠 if any unhealthy services exist (but no critical),
    /// 🟡 if any degraded services exist (but no unhealthy/critical),
    /// 🟢 if all services are healthy,
    /// ⚪ if no services or unknown state.
    /// </returns>
    public static string ToStatusEmoji(this HealthSummary summary)
    {
        if (summary.CriticalCount > 0)
            return "🔴";
        if (summary.UnhealthyCount > 0)
            return "🟠";
        if (summary.DegradedCount > 0)
            return "🟡";
        if (summary.HealthyCount > 0 && summary.TotalServices == summary.HealthyCount)
            return "🟢";
        return "⚪";
    }

    /// <summary>
    /// Determines if there are any critical services in the health summary.
    /// </summary>
    /// <param name="summary">The health summary to evaluate.</param>
    /// <returns>True if CriticalCount is greater than zero; otherwise, false.</returns>
    public static bool IsCritical(this HealthSummary summary) => summary.CriticalCount > 0;
}