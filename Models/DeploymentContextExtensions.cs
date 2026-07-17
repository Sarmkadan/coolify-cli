namespace CoolifyCli.Models;

/// <summary>
/// Extension methods for <see cref="DeploymentContext"/> that provide common deployment state checks and formatting.
/// </summary>
public static class DeploymentContextExtensions
{
    /// <summary>
    /// Checks if the deployment has completed.
    /// </summary>
    /// <param name="context">The deployment context.</param>
    /// <returns>True if the deployment has completed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool HasCompleted(this DeploymentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.CompletedAt.HasValue;
    }

    /// <summary>
    /// Gets the duration of the deployment in a human-readable format.
    /// </summary>
    /// <param name="context">The deployment context.</param>
    /// <returns>A string representing the duration of the deployment in format "Xh Ym Zs".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string GetDurationString(this DeploymentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var duration = context.GetDuration();
        var hours = duration.Hours;
        var minutes = duration.Minutes;
        var seconds = duration.Seconds;

        return $"{hours}h {minutes}m {seconds}s";
    }

    /// <summary>
    /// Checks if the deployment requires approval and has not been approved yet.
    /// </summary>
    /// <param name="context">The deployment context.</param>
    /// <returns>True if the deployment requires approval and has not been approved yet; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool IsPendingApproval(this DeploymentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.RequiresApproval && string.IsNullOrEmpty(context.ApprovedBy);
    }
}
