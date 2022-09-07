// existing content ...

## IntegrationTests

The `IntegrationTests` class provides a set of integration tests that exercise multiple components together, verifying end-to-end workflows, concurrency safety, and configuration combinations described in the project README.

Here's an example of how to use some of the tested methods:

```csharp
var tests = new IntegrationTests();

// Test deployment lifecycle
tests.DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect();

// Test validation pipeline
tests.ValidationPipeline_AllHelperMethods_WorkTogether();

// Test cache workflow
tests.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();

// Test collection and string pipeline
tests.CollectionAndStringPipeline_BatchAndFormatDeploymentNames_ProducesExpectedOutput();

// Test concurrent cache access
tests.ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions();

// Test concurrent deployment state updates
await tests.ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent();

// Test validation with invalid field combinations
tests.Validate_AllInvalidFieldCombinations_ReturnsAllExpectedErrors();

// Test validation with only start command
tests.Validate_WithOnlyStartCommand_PassesBuildCommandCheck();

// Test validation with invalid port in list
tests.Validate_WithInvalidPortInList_ReportsSpecificPort();

// Test datetime and enum pipeline
tests.DateTimeAndEnumPipeline_FormatDeploymentTimestamp_ProducesHumanReadableOutput();
