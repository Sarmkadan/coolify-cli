# DeploymentTests

`DeploymentTests` is a test suite within the `coolify-cli` project that validates the behavior of deployment-related logic. It covers input validation, state transitions for deployment statuses, failure accumulation, attention-required thresholds, and cache provider delegation. The class ensures that the core deployment components behave correctly under both nominal and edge-case conditions.

## API

### `public void Validate_WhenNameIsEmpty_IncludesNameRequiredError`
Verifies that validating a deployment configuration with an empty name produces an error collection that explicitly includes a name-required error.  
**Purpose:** Ensures the validation layer enforces mandatory fields.  
**Parameters:** None (parameterless test method).  
**Return value:** `void`.  
**Throws:** Assertion failures if the expected error is absent.

### `public void Validate_WithCompleteValidConfiguration_ReturnsNoErrors`
Confirms that a fully populated, valid deployment configuration passes validation without any errors.  
**Purpose:** Acts as a positive test to guarantee that correct input yields a clean validation result.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Assertion failures if any error is present in the result.

### `public void MarkAsDeployed_AfterPreviousFailures_ResetsFailureStateAndSetsTimestamp`
Tests that invoking `MarkAsDeployed` on a deployment that previously recorded failures clears the failure count and message, and stamps the deployment with a current timestamp.  
**Purpose:** Validates the state-reset semantics of a successful deployment after failures.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Assertion failures if the failure state is not cleared or the timestamp is not updated.

### `public void MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage`
Exercises calling `MarkAsFailed` multiple times and asserts that the failure count increments with each call and that only the most recent failure message is retained.  
**Purpose:** Confirms correct accumulation behavior and message replacement logic.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Assertion failures if the count does not match the number of calls or the message does not reflect the latest failure.

### `public void RequiresAttention_WhenFailureCountReachesThreshold_ReturnsTrue`
Verifies that the `RequiresAttention` flag returns `true` exactly when the accumulated failure count meets or exceeds a defined threshold.  
**Purpose:** Ensures the attention mechanism triggers at the correct boundary.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Assertion failures if the flag does not match the expected boolean value for the given failure count.

### `public void CacheProvider_GetOrAdd_WhenKeyAbsent_DelegatesValueCreationToFactory`
Tests that a cache provider’s `GetOrAdd` method, when the requested key is absent, invokes the supplied factory delegate to create the value and then stores it.  
**Purpose:** Validates correct delegation and caching behavior for missing keys.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Assertion failures if the factory is not called or the returned/stored value is incorrect.

## Usage

```csharp
// Example 1: Validating a deployment configuration end-to-end
var test = new DeploymentTests();

// Should flag missing name
test.Validate_WhenNameIsEmpty_IncludesNameRequiredError();

// Should pass cleanly with full config
test.Validate_WithCompleteValidConfiguration_ReturnsNoErrors();
```

```csharp
// Example 2: Exercising deployment state transitions and cache behavior
var test = new DeploymentTests();

// Simulate repeated failures, then a successful deploy
test.MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage();
test.RequiresAttention_WhenFailureCountReachesThreshold_ReturnsTrue();
test.MarkAsDeployed_AfterPreviousFailures_ResetsFailureStateAndSetsTimestamp();

// Verify cache provider delegates to factory on cache miss
test.CacheProvider_GetOrAdd_WhenKeyAbsent_DelegatesValueCreationToFactory();
```

## Notes

- **Edge cases:** `MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage` implicitly covers overflow scenarios only if the test harness supplies a large number of calls; the underlying implementation must handle counter increments safely. `RequiresAttention_WhenFailureCountReachesThreshold_ReturnsTrue` should be verified exactly at the threshold boundary (off-by-one checks).
- **Thread-safety:** The test methods themselves are synchronous and single-threaded by nature. However, the production components they exercise—particularly the cache provider and deployment state—may be accessed concurrently in real usage. The tests do not validate thread safety; additional concurrency-focused tests would be required to guarantee correctness under parallel access.
- **State leakage:** Each test method should operate on isolated instances to prevent state from leaking between tests. The signatures imply no shared static state, but test runners should enforce isolation.
