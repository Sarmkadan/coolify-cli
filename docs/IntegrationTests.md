# IntegrationTests
The `IntegrationTests` class is designed to validate the functionality of various components within the `coolify-cli` project. It contains a suite of tests that verify the correct behavior of different pipelines, validation mechanisms, and concurrent access scenarios. These tests ensure that the project's core features operate as expected, providing a foundation for reliable and efficient execution.

## API
The `IntegrationTests` class provides the following public members:
* `DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect`: Verifies that the deployment lifecycle correctly transitions through its states when a deployment fails.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `ValidationPipeline_AllHelperMethods_WorkTogether`: Tests that all helper methods in the validation pipeline work together seamlessly.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls`: Validates that the cache workflow persists deployment data between calls.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `CollectionAndStringPipeline_BatchAndFormatDeploymentNames_ProducesExpectedOutput`: Verifies that the collection and string pipeline produces the expected output when batching and formatting deployment names.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions`: Tests that concurrent cache access by multiple threads does not result in exceptions.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent`: Asynchronously verifies that the failure count remains consistent when multiple threads update the deployment state.
	+ Parameters: None
	+ Return value: Task
	+ Throws: None
* `Validate_AllInvalidFieldCombinations_ReturnsAllExpectedErrors`: Tests that the validation mechanism returns all expected errors for various invalid field combinations.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `Validate_WithOnlyStartCommand_PassesBuildCommandCheck`: Verifies that the validation passes the build command check when only the start command is provided.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `Validate_WithInvalidPortInList_ReportsSpecificPort`: Tests that the validation reports a specific port when an invalid port is provided in a list.
	+ Parameters: None
	+ Return value: None
	+ Throws: None
* `DateTimeAndEnumPipeline_FormatDeploymentTimestamp_ProducesHumanReadableOutput`: Verifies that the date and time pipeline produces human-readable output when formatting deployment timestamps.
	+ Parameters: None
	+ Return value: None
	+ Throws: None

## Usage
The following examples demonstrate how to utilize the `IntegrationTests` class:
```csharp
// Example 1: Running a single test
IntegrationTests tests = new IntegrationTests();
tests.DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect();

// Example 2: Running multiple tests concurrently
IntegrationTests tests1 = new IntegrationTests();
IntegrationTests tests2 = new IntegrationTests();
Task task1 = tests1.ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent();
tests2.CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls();
task1.Wait();
```

## Notes
When using the `IntegrationTests` class, consider the following edge cases and thread-safety remarks:
* The `ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions` and `ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent` tests are designed to verify thread safety. However, they may still throw exceptions if the underlying cache or deployment state mechanisms are not properly synchronized.
* The `Validate_AllInvalidFieldCombinations_ReturnsAllExpectedErrors` test may take longer to execute due to the large number of invalid field combinations being tested.
* The `DateTimeAndEnumPipeline_FormatDeploymentTimestamp_ProducesHumanReadableOutput` test relies on the correct formatting of date and time values. Any changes to the formatting logic may affect the test's outcome.
