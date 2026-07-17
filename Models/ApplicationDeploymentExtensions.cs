namespace CoolifyCli.Models;

/// <summary>
/// Provides extension methods for <see cref="ApplicationDeployment"/>.
/// </summary>
public static class ApplicationDeploymentExtensions
{
	/// <summary>
	/// Determines whether the application deployment is active and has a valid health check URL.
	/// </summary>
	/// <param name="deployment">The application deployment to check.</param>
	/// <returns><c>true</c> if the deployment is active and has a valid health check URL; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="deployment"/> is <c>null</c>.</exception>
	public static bool IsHealthy(this ApplicationDeployment deployment)
	{
		ArgumentNullException.ThrowIfNull(deployment);

		return deployment.IsActive && !string.IsNullOrEmpty(deployment.HealthCheckUrl);
	}

	/// <summary>
	/// Gets a human-readable string representation of the deployment's status.
	/// </summary>
	/// <param name="deployment">The application deployment to get the status string for.</param>
	/// <returns>A human-readable string representation of the deployment's status.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="deployment"/> is <c>null</c>.</exception>
	public static string GetStatusString(this ApplicationDeployment deployment)
	{
		ArgumentNullException.ThrowIfNull(deployment);

		return deployment.Status switch
		{
			DeploymentStatus.Deployed => "Deployed",
			DeploymentStatus.Failed => $"Failed ({deployment.FailureCount} failures)",
			_ => deployment.Status.ToString(),
		};
	}

	/// <summary>
	/// Gets the environment variables for the deployment as a read-only dictionary.
	/// </summary>
	/// <param name="deployment">The application deployment to get the environment variables for.</param>
	/// <returns>A read-only dictionary containing the deployment's environment variables.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="deployment"/> is <c>null</c>.</exception>
	public static IReadOnlyDictionary<string, string> GetEnvironmentVariables(this ApplicationDeployment deployment)
	{
		ArgumentNullException.ThrowIfNull(deployment);

		return deployment.EnvironmentVariables.AsReadOnly();
	}
}