using System;

namespace CoolifyCli.Tests;

/// <summary>
/// Provides extension methods for inspecting and executing integration tests.
/// </summary>
public static class IntegrationTestsExtensions
{
    /// <summary>
    /// Determines whether the deployment lifecycle test exists.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns><see langword="true"/> if the deployment lifecycle test exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static bool HasDeploymentLifecycleTest(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return tests.DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect != null;
    }

    /// <summary>
    /// Determines whether the validation pipeline test exists.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns><see langword="true"/> if the validation pipeline test exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static bool HasValidationPipelineTest(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return tests.ValidationPipeline_AllHelperMethods_WorkTogether != null;
    }

    /// <summary>
    /// Executes the cache workflow test and returns the name of the test method.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The name of the cache workflow test method.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static string GetCacheWorkflowTestResult(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();
        return nameof(GetCacheWorkflowTestResult);
    }

    /// <summary>
    /// Executes the concurrent cache access test and returns a fixed count of iterations.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>A fixed value of <c>1</c> representing the number of test iterations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static int GetConcurrentCacheAccessTestCount(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions();
        return 1;
    }
}
