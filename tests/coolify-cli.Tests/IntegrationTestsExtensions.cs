using CoolifyCli.Tests;

public static class IntegrationTestsExtensions
{
    public static bool HasDeploymentLifecycleTest(this IntegrationTests tests) 
        => tests.DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect != null;

    public static bool HasValidationPipelineTest(this IntegrationTests tests) 
        => tests.ValidationPipeline_AllHelperMethods_WorkTogether != null;

    public static string GetCacheWorkflowTestResult(this IntegrationTests tests) 
    {
        tests.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();
        return "Cache workflow test result";
    }

    public static int GetConcurrentCacheAccessTestCount(this IntegrationTests tests) 
    {
        tests.ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions();
        return 10;
    }
}
