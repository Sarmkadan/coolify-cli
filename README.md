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

## TuiStateTests

The `TuiStateTests` class provides unit tests for the `TuiState` class, which manages the state of a terminal user interface for navigating and selecting applications. These tests verify navigation behavior (moving up and down), boundary conditions, selection retrieval, and scroll management to ensure the TUI state transitions correctly across different scenarios.

Here's an example of how to use the tested methods to manage application selection in a terminal UI:

```csharp
// Create a TUI state with some applications
var state = new TuiState
{
    Applications = new List<ApplicationDeployment>
    {
        new() { Id = 1, Name = "web-app" },
        new() { Id = 2, Name = "api-service" },
        new() { Id = 3, Name = "worker-process" },
        new() { Id = 4, Name = "database" },
        new() { Id = 5, Name = "cache-layer" }
    },
    SelectedIndex = 0,
    ScrollOffset = 0
};

// Navigate down through the list
state.MoveDown(state.Applications.Count);
state.SelectedIndex.Should().Be(1); // Moves to second item

state.MoveDown(state.Applications.Count);
state.SelectedIndex.Should().Be(2); // Moves to third item

// Navigate up
state.MoveUp();
state.SelectedIndex.Should().Be(1); // Moves back to second item

// Get the currently selected application
var selectedApp = state.GetSelectedApp();
selectedApp.Should().NotBeNull();
selectedApp!.Name.Should().Be("api-service");

// Reset selection to initial state
state.ResetSelection();
state.SelectedIndex.Should().Be(0);
state.ScrollOffset.Should().Be(0);

// Get visible apps for rendering (with a window size of 3)
var visibleApps = state.GetVisibleApps(3);
visibleApps.Should().HaveCount(3);
visibleApps[0].Name.Should().Be("web-app");
visibleApps[1].Name.Should().Be("api-service");
visibleApps[2].Name.Should().Be("worker-process");
```

## MemoryCacheProviderTests

The `MemoryCacheProviderTests` class provides unit tests for the `MemoryCacheProvider` class, which handles in-memory caching with support for expiration, atomic operations, and factory-based value creation. These tests verify core CRUD operations, expiration handling, concurrency, and cache management to ensure that cached data is reliably stored, retrieved, and cleaned up as expected.

Here's an example of how to use the tested methods to manage cached data:

```csharp
// Initialize the memory cache provider
// Use a reasonable cleanup interval for the test
using var cache = new MemoryCacheProvider(cleanupInterval: TimeSpan.FromMinutes(1));

// Store a value
cache.Set("key1", "value1");

// Verify existence and retrieve
if (cache.Exists("key1"))
{
    var value = cache.Get<string>("key1");
    // value is "value1"
}

// Try to retrieve a value safely
if (cache.TryGet("key1", out string? retrievedValue))
{
    // retrievedValue is "value1"
}

// Remove an entry
cache.Remove("key1");

// Clear all entries
cache.Clear();

// Efficiently get or add a value (invokes factory if key is missing)
var result = cache.GetOrAdd("key2", () => "computedValue");
// result is "computedValue"

// Verify count
var count = cache.Count;
// count is 1
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides unit tests for the DateTime extension methods in the `CoolifyCli.Extensions` namespace. It tests relative time formatting, duration formatting, date manipulation, and business day calculations to ensure these utility methods work correctly across different time ranges and scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Format a recent timestamp as relative time
var justNow = DateTime.UtcNow.AddMilliseconds(-500).ToRelativeTime();
justNow.Should().Be("just now");

// Format a timestamp from 30 seconds ago
var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30).ToRelativeTime();
thirtySecondsAgo.Should().Be("30 seconds ago");

// Format a timestamp from 3 hours ago
var threeHoursAgo = DateTime.UtcNow.AddHours(-3).ToRelativeTime();
threeHoursAgo.Should().Be("3 hours ago");

// Format a duration with hours, minutes, and seconds
var duration = new TimeSpan(2, 30, 15).ToReadableDuration();
duration.Should().Be("2h 30m 15s");

// Get the start of the current day
dateTime.StartOfDay().Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));

// Get the end of the current day
dateTime.EndOfDay().Should().Be(new DateTime(2024, 6, 15, 23, 59, 59, 999));

// Get the start of the current week (Monday)
var monday = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday);

// Get the start of the current month
var monthStart = DateTime.UtcNow.StartOfMonth();

// Get the end of the current month
var monthEnd = DateTime.UtcNow.EndOfMonth();

// Calculate business days between two dates
var businessDays = DateTime.UtcNow.StartOfWeek().BusinessDaysBetween(DateTime.UtcNow.EndOfWeek());
```

## StringExtensionsTests

The `StringExtensionsTests` class provides unit tests for string extension methods in the `CoolifyCli.Extensions` namespace. It tests text transformation utilities including Pascal case conversion, string truncation with ellipsis, sensitive data masking, and whitespace handling to ensure these common string operations work correctly across different input scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Convert a hyphen-delimited string to PascalCase
var deploymentName = "deploy-my-service".ToPascalCase();
deploymentName.Should().Be("DeployMyService");

// Truncate a long string with ellipsis when exceeding max length
var longText = "This is a very long deployment description that needs to be shortened";
var truncated = longText.Truncate(20);
truncated.Should().Be("This is a very lon...");

// Truncate preserves the original when within max length
var shortText = "Hi";
var preserved = shortText.Truncate(10);
preserved.Should().Be("Hi");

// Mask sensitive data, showing only edge characters
var apiKey = "sk_live_1234567890abcdef";
var maskedKey = apiKey.MaskSensitive(showChars: 4);
maskedKey.Should().Be("sk_l_************def");

// Split and trim whitespace-padded strings
var services = " api , web , worker ".SplitTrimmed(',');
services.Should().Equal("api", "web", "worker");
```

## CollectionExtensionsTests

The `CollectionExtensionsTests` class provides unit tests for collection extension methods in the `CoolifyCli.Extensions` namespace. It tests various utility methods for working with collections including batching operations, filtering, transformation, and dictionary manipulation to ensure these extension methods work correctly across different scenarios.

Here's an example of how to use some of the tested methods:

```csharp
// Test if a collection is null or empty
var emptyList = new List<string>();
emptyList.IsNullOrEmpty().Should().BeTrue();

var populatedList = new[] { "item1", "item2", "item3" };
populatedList.IsNullOrEmpty().Should().BeFalse();

// Split a collection into batches of 2 items each
var numbers = Enumerable.Range(1, 6).ToList();
var batches = numbers.Batch(2).ToList();
batches.Should().HaveCount(3);
batches[0].Should().BeEquivalentTo(new[] { 1, 2 });
batches[1].Should().BeEquivalentTo(new[] { 3, 4 });
batches[2].Should().BeEquivalentTo(new[] { 5, 6 });

// Filter out null values from a collection
var itemsWithNulls = new string?[] { "a", null, "b", null, "c" };
var nonNullItems = itemsWithNulls.WhereNotNull().ToList();
nonNullItems.Should().BeEquivalentTo(new[] { "a", "b", "c" });

// Split a collection into two based on a predicate
var (evens, odds) = Enumerable.Range(1, 6).Split(n => n % 2 == 0);
evens.Should().BeEquivalentTo(new[] { 2, 4, 6 });
odds.Should().BeEquivalentTo(new[] { 1, 3, 5 });

// Flatten nested collections
var nested = new List<List<int>>
{
    new() { 1, 2 },
    new() { 3 },
    new() { 4, 5, 6 }
};
var flattened = nested.Flatten().ToList();
flattened.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6 });

// Find items with max/min values based on a key selector
var words = new[] { "cat", "elephant", "dog" };
var longest = words.MaxBy(w => w.Length);
longest.Should().Be("elephant");

var shortest = words.MinBy(w => w.Length);
shortest.Should().Be("cat");

// Merge two dictionaries, with second dictionary values overwriting first
var first = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
var second = new Dictionary<string, int> { { "b", 99 }, { "c", 3 } };
var merged = first.Merge(second);
merged["a"].Should().Be(1);
merged["b"].Should().Be(99);
merged["c"].Should().Be(3);

// Convert a dictionary to a query string
var queryParams = new Dictionary<string, string>
{
    { "env", "prod" },
    { "region", "us-east-1" }
};
var queryString = queryParams.ToQueryString();
queryString.Should().Contain("env=prod");
queryString.Should().Contain("region=us-east-1");

// Group consecutive items based on a condition
var numbersForGrouping = new[] { 1, 2, 3, 10, 11, 20 };
var groups = numbersForGrouping.GroupConsecutive((a, b) => b - a <= 1).ToList();
groups.Should().HaveCount(3);
groups[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
groups[1].Should().BeEquivalentTo(new[] { 10, 11 });
groups[2].Should().BeEquivalentTo(new[] { 20 });
```
