# IacTemplateOptions
The `IacTemplateOptions` type in the `coolify-cli` project provides a set of options for customizing the behavior of Infrastructure as Code (IaC) template operations. These options allow users to control aspects such as validation, approval, and output formatting, enabling flexible and tailored usage of IaC templates in various scenarios.

## API
The `IacTemplateOptions` type exposes the following public members:
- `DryRun`: A boolean indicating whether to perform a dry run, which means the operation will be simulated without actually applying any changes.
- `AutoApprove`: A boolean that determines whether to automatically approve the operation without prompting for user confirmation.
- `SkipValidation`: A boolean specifying whether to bypass validation checks for the template.
- `FailFast`: A boolean controlling whether the operation should fail immediately upon encountering an error, rather than attempting to continue.
- `OperationTimeout`: A `TimeSpan` value representing the maximum time allowed for the operation to complete before it is considered timed out.
- `MaxConcurrentOperations`: An integer defining the maximum number of operations that can be executed concurrently.
- `OutputFormat`: A string specifying the format in which the output should be presented.
- `ShowDiff`: A boolean indicating whether to display the differences resulting from the operation.
- `TemplateSearchPaths`: A list of strings representing the paths where the system should search for templates.

## Usage
Here are two examples demonstrating how to use `IacTemplateOptions` in C#:
```csharp
// Example 1: Basic usage with dry run
var options = new IacTemplateOptions
{
    DryRun = true,
    AutoApprove = true,
    OutputFormat = "json"
};
// Proceed with the operation using the specified options

// Example 2: Customizing validation and concurrency
var advancedOptions = new IacTemplateOptions
{
    SkipValidation = false,
    FailFast = true,
    MaxConcurrentOperations = 5,
    TemplateSearchPaths = new List<string> { "/path/to/templates" }
};
// Use the advanced options for a more controlled operation
```

## Notes
When using `IacTemplateOptions`, consider the following edge cases and thread-safety remarks:
- Setting `DryRun` to `true` will not modify any actual resources but may still consume system resources for simulation.
- `AutoApprove` should be used cautiously, as it bypasses user confirmation and could lead to unintended changes.
- `SkipValidation` can significantly speed up operations but at the risk of applying invalid or incompatible templates.
- The `OperationTimeout` should be set based on the expected duration of the operation and the system's capacity to handle timeouts.
- `MaxConcurrentOperations` must be tuned according to the system's capabilities to avoid overloading.
- The `OutputFormat` and `ShowDiff` options are primarily for user convenience and do not affect the operational outcome.
- `TemplateSearchPaths` should include all relevant locations to ensure templates are found correctly.
- `IacTemplateOptions` instances are not inherently thread-safe; if accessed or modified by multiple threads, appropriate synchronization mechanisms should be employed.
