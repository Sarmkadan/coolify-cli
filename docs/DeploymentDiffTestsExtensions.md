# DeploymentDiffTestsExtensions

Utility class providing extension methods for running and checking deployment-diff related test suites in the coolify-cli project. It centralizes logic for detecting and executing compute and deployment-diff entry test collections, ensuring consistent behavior across test runners and CI pipelines.

## API

### `HasComputeTests`
Determines whether the current test context contains any compute-related tests.

- **Returns**
  `true` if compute tests are present; otherwise, `false`.

### `HasDeploymentDiffEntryTests`
Determines whether the current test context contains any deployment-diff entry tests.

- **Returns**
  `true` if deployment-diff entry tests are present; otherwise, `false`.

### `RunAllComputeTests`
Executes all compute-related tests in the current context.

- **Throws**
  `InvalidOperationException` if no compute tests are detected (`HasComputeTests` returns `false`).

### `RunAllDeploymentDiffEntryTests`
Executes all deployment-diff entry tests in the current context.

- **Throws**
  `InvalidOperationException` if no deployment-diff entry tests are detected (`HasDeploymentDiffEntryTests` returns `false`).

## Usage

```csharp
// Example 1: Conditional execution based on test presence
if (DeploymentDiffTestsExtensions.HasComputeTests)
{
    DeploymentDiffTestsExtensions.RunAllComputeTests();
}

// Example 2: Fail-fast if expected tests are missing
if (!DeploymentDiffTestsExtensions.HasDeploymentDiffEntryTests)
{
    throw new InvalidOperationException("Deployment-diff entry tests are required.");
}
DeploymentDiffTestsExtensions.RunAllDeploymentDiffEntryTests();
```

## Notes

- Methods are stateless and thread-safe; no shared mutable state is accessed.
- `RunAllComputeTests` and `RunAllDeploymentDiffEntryTests` will throw if no matching tests are found, avoiding silent no-op behavior.
- Detection methods (`HasComputeTests`, `HasDeploymentDiffEntryTests`) are idempotent and safe to call multiple times.
