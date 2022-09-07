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

## DeploymentDiffTests

The `DeploymentDiffTests` class validates the behavior of the `DeploymentDiff` class, which compares two `ApplicationDeployment` configurations and produces a detailed diff of changes. It detects property differences, flags high-risk changes (like repository URL changes), and tracks environment variable additions/removals. The tests also verify that the `DeploymentDiffEntry` class correctly identifies when values have changed.

Here's an example of how to use the deployment diff functionality:

```csharp
// Create two deployment configurations
var current = new ApplicationDeployment
{
    Id = 1,
    Name = "my-service",
    Repository = "https://github.com/org/my-service",
    Branch = "main",
    EnvironmentId = "env-prod",
    BuildCommand = "dotnet publish",
    StartCommand = "dotnet run",
    Ports = new List<string> { "8080" },
    HealthCheckIntervalSeconds = 30,
    EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "info" }
};

var proposed = new ApplicationDeployment
{
    Id = 1,
    Name = "my-service",
    Repository = "https://github.com/org/my-service",
    Branch = "release/v2",  // Changed from "main"
    EnvironmentId = "env-prod",
    BuildCommand = "dotnet publish",
    StartCommand = "dotnet run",
    Ports = new List<string> { "8080" },
    HealthCheckIntervalSeconds = 30,
    EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "info" }
};

// Compute the deployment diff
var diff = DeploymentDiff.Compute(current, proposed);

// Verify changes were detected
diff.HasChanges.Should().BeTrue();
diff.IsHighRisk.Should().BeFalse(); // Branch change is not high risk

// Check specific changes
diff.Changes.Should().ContainSingle(e => e.Property == "Branch");
var branchChange = diff.Changes.Single(e => e.Property == "Branch");
branchChange.CurrentValue.Should().Be("main");
branchChange.ProposedValue.Should().Be("release/v2");

// Check application metadata
diff.ApplicationId.Should().Be(1);
diff.ApplicationName.Should().Be("my-service");
```
