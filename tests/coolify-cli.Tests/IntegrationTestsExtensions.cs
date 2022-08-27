using System;

namespace CoolifyCli.Tests;

public static class IntegrationTestsExtensions
{
    /// <summary>
    /// Determines whether the deployment lifecycle test exists.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns><see langword="true"/> if the deployment lifecycle test exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
    public static bool HasValidationPipelineTest(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return tests.ValidationPipeline_AllHelperMethods_WorkTogether != null;
    }

    /// <summary>
    /// Executes the cache workflow test and returns the result.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The cache workflow test result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
    public static string GetCacheWorkflowTestResult(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();
        return nameof(GetCacheWorkflowTestResult);
    }

    /// <summary>
    /// Executes the concurrent cache access test and returns the number of test iterations.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The number of test iterations performed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
    public static int GetConcurrentCacheAccessTestCount(this IntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions();
        return 1;
    }
}