# DeploymentTestsExtensions

The `DeploymentTestsExtensions` class provides a set of static helper methods designed to facilitate automated testing of deployment workflows within the `coolify-cli` tool. These methods encapsulate common validation, simulation, and failure scenarios, allowing test suites to verify the correctness and resilience of deployment pipelines without duplicating boilerplate logic. Each method is intended to be called directly from unit or integration tests.

## API

### `RunAllValidationTests`

```csharp
public static void RunAllValidationTests()
```

Runs a comprehensive suite of validation checks against the current deployment configuration. This method executes all predefined validation rules (e.g., schema conformance, required field presence, value range checks) and aggregates any failures. It does not return a value; instead, it throws an exception if any validation fails.

**Parameters**  
None.

**Returns**  
`void`.

**Exceptions**  
- `DeploymentValidationException` – Thrown when one or more validation rules are violated. The exception message contains details of all failures.

---

### `SimulateDeploymentScenario`

```csharp
public static void SimulateDeploymentScenario(string scenarioName)
```

Simulates a specific deployment scenario identified by `scenarioName`. The method loads the corresponding test data and executes the deployment steps in a sandboxed environment, recording outcomes for later assertion.

**Parameters**  
- `scenarioName` – A non-null, non-empty string that identifies the scenario to simulate (e.g., `"RollingUpdate"`, `"BlueGreen"`).

**Returns**  
`void`.

**Exceptions**  
- `ArgumentNullException` – Thrown if `scenarioName` is `null`.  
- `ArgumentException` – Thrown if `scenarioName` is empty or consists only of whitespace.  
- `ScenarioNotFoundException` – Thrown if no scenario with the given name is registered.

---

### `ExecuteCacheProviderTest`

```csharp
public static void ExecuteCacheProviderTest()
```

Executes a predefined test that validates the behavior of the active cache provider (e.g., `MemoryCacheProvider`). This method verifies basic CRUD operations, expiration, and concurrency handling. It is intended to be called after the cache provider has been initialized in the test context.

**Parameters**  
None.

**Returns**  
`void`.

**Exceptions**  
- `CacheProviderTestFailedException` – Thrown if any cache operation does not meet expected results. The exception includes details of the failing operation.

---

### `RunFailureSequence`

```csharp
public static void RunFailureSequence()
```

Triggers a sequence of simulated failures (e.g., network timeouts, service unavailability, invalid responses) to test the resilience and error-handling logic of the deployment pipeline. The method does not require any setup beyond the current test environment.

**Parameters**  
None.

**Returns**  
`void`.

**Exceptions**  
- `FailureSequenceException` – Thrown when the deployment pipeline fails to handle one or more of the injected failures correctly. The exception message describes which failure scenarios were not properly mitigated.

## Usage

### Example 1: Validating a deployment configuration before a release

```csharp
using CoolifyCli.Tests;

public class DeploymentTests
{
    [Fact]
    public void ValidateProductionDeployment()
    {
        // Arrange – configuration is already loaded by test fixture

        // Act
        DeploymentTestsExtensions.RunAllValidationTests();

        // Assert – if no exception is thrown, validation passed
    }
}
```

### Example 2: Testing resilience with a failure sequence

```csharp
using CoolifyCli.Tests;

public class ResilienceTests
{
    [Fact]
    public void PipelineShouldRecoverFromTransientFailures()
    {
        // Arrange – set up mock services that can simulate failures

        // Act
        DeploymentTestsExtensions.RunFailureSequence();

        // Assert – exception indicates a failure was not handled
    }
}
```

## Notes

- All methods are static and do not rely on instance state. They are thread-safe as long as the underlying test infrastructure (e.g., configuration, service mocks) is also thread-safe.  
- `SimulateDeploymentScenario` requires that the scenario data be registered prior to invocation; calling it with an unregistered name throws `ScenarioNotFoundException`.  
- `ExecuteCacheProviderTest` assumes a cache provider has already been initialized. If no provider is active, the method may throw an `InvalidOperationException` (not documented above but possible in certain implementations).  
- The methods are designed for use in test fixtures and should not be called in production code. They may modify global state (e.g., temporary files, environment variables) and are not intended to be re-entrant without proper cleanup.  
- When using `RunAllValidationTests`, ensure that all required configuration sources are available; otherwise, validation may fail with misleading errors.
