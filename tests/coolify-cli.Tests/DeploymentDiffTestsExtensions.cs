using CoolifyCli.Tests;
using Xunit;

/// <summary>
/// Extension methods for <see cref="DeploymentDiffTests"/> that provide test discovery and execution capabilities.
/// </summary>
public static class DeploymentDiffTestsExtensions
{
	/// <summary>
	/// Determines whether the <see cref="DeploymentDiffTests"/> class contains compute-related test methods.
	/// </summary>
	/// <param name="tests">The test instance to check.</param>
	/// <returns><see langword="true"/> if compute tests are present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static bool HasComputeTests(this DeploymentDiffTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		return tests.GetType().GetMethods().Any(m =>
			m.Name.StartsWith("Compute_") &&
			m.GetParameters().Length == 0 &&
			m.ReturnType == typeof(void));
	}

	/// <summary>
	/// Determines whether the <see cref="DeploymentDiffTests"/> class contains DeploymentDiffEntry test methods.
	/// </summary>
	/// <param name="tests">The test instance to check.</param>
	/// <returns><see langword="true"/> if DeploymentDiffEntry tests are present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static bool HasDeploymentDiffEntryTests(this DeploymentDiffTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		return tests.GetType().GetMethods().Any(m =>
			m.Name.StartsWith("DeploymentDiffEntry_") &&
			m.Name.EndsWith("_HasChange") &&
			m.GetParameters().Length == 2 &&
			m.ReturnType == typeof(bool));
	}

	/// <summary>
	/// Executes all compute-related test methods on the provided test instance.
	/// </summary>
	/// <param name="tests">The test instance to execute.</param>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static void RunAllComputeTests(this DeploymentDiffTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		tests.Compute_WhenBothConfigurationsIdentical_ReportsNoChanges();
		tests.Compute_WhenBranchChanged_DetectsOneBranchChange();
		tests.Compute_WhenRepositoryChanged_FlagsHighRisk();
		tests.Compute_WhenOnlyBuildCommandChanged_IsNotHighRisk();
		tests.Compute_WhenEnvVarAdded_IncludesEnvVarChange();
		tests.Compute_WhenEnvVarRemoved_IncludesDeletionChange();
		tests.Compute_SetsApplicationIdAndName();
	}

	/// <summary>
	/// Executes all DeploymentDiffEntry test methods on the provided test instance.
	/// </summary>
	/// <param name="tests">The test instance to execute.</param>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static void RunAllDeploymentDiffEntryTests(this DeploymentDiffTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		tests.DeploymentDiffEntry_HasChange_ReturnsFalseForIdenticalValues();
		tests.DeploymentDiffEntry_HasChange_ReturnsTrueForDifferentValues();
	}
}