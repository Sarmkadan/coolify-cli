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

## DeploymentTests

The `DeploymentTests` class provides unit tests that verify the behavior of the `ApplicationDeployment` class, focusing on validation, deployment state management, failure tracking, and caching functionality. These tests ensure that deployment configurations are properly validated, state transitions work correctly, failure states are tracked accurately, and cached deployments are retrieved and updated as expected.

Here's an example of how to use the deployment tests to verify common scenarios:

```csharp
// Create a deployment configuration
var deployment = new ApplicationDeployment
{
Name = "my-service",
Repository = "https://github.com/user/repo",
EnvironmentId = "env-prod",
BuildCommand = "npm run build",
Ports = new List<string> { "3000" }
};

// Test 1: Validate that a complete deployment configuration passes validation
var errors = deployment.Validate().ToList();
errors.Should().BeEmpty(); // No validation errors for complete configuration

// Test 2: Mark a deployment as deployed after previous failures
// This resets the failure state and sets the deployment timestamp
deployment.MarkAsFailed("build timeout");
deployment.MarkAsFailed("health check failed");
deployment.MarkAsDeployed();

// Verify state was reset
deployment.Status.Should().Be(DeploymentStatus.Deployed);
deployment.FailureCount.Should().Be(0);
deployment.LastErrorMessage.Should().BeNull();
deployment.LastDeployedAt.Should().NotBeNull();

// Test 3: Track failure accumulation
// Each failure increases the failure count and updates the error message
deployment.MarkAsFailed("timeout on step 1");
deployment.MarkAsFailed("timeout on step 2");

// Verify failure tracking
deployment.FailureCount.Should().Be(2);
deployment.LastErrorMessage.Should().Be("timeout on step 2");
deployment.Status.Should().Be(DeploymentStatus.Failed);

// Test 4: Check when attention is required
// When failure count reaches a threshold, the deployment requires attention
deployment.MarkAsFailed("error");
deployment.RequiresAttention().Should().BeTrue();

// Test 5: Verify cache provider behavior
// When a deployment is not in cache, the factory method is called to create it
var mockCache = new Mock<ICacheProvider>();
mockCache
.Setup(c => c.GetOrAdd<ApplicationDeployment>(
"deployment:42",
It.IsAny<Func<ApplicationDeployment>>(),
It.IsAny<TimeSpan?>()))
.Returns<string, Func<ApplicationDeployment>, TimeSpan?>((_, factory, __) => factory());

var cachedDeployment = mockCache.Object.GetOrAdd<ApplicationDeployment>(
"deployment:42",
() => new ApplicationDeployment { Id = 42, Name = "cached-service" });

// Verify the cached deployment
cachedDeployment.Id.Should().Be(42);
cachedDeployment.Name.Should().Be("cached-service");
```
