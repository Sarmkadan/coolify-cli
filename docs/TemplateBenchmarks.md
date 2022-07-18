# TemplateBenchmarks

The `TemplateBenchmarks` class serves as a utility component within the `coolify-cli` project, designed to facilitate the performance evaluation and diagnostic logging of template expansion operations. It provides a structured environment for setting up benchmark contexts, executing template expansions with timing capabilities, and reporting outcomes through a hierarchical logging system ranging from debug traces to fatal errors.

## API

### `Setup`
Initializes the internal state required for running benchmarks. This method prepares the necessary context, such as clearing previous results or configuring default parameters for subsequent template operations.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May throw initialization exceptions if the underlying benchmarking infrastructure is misconfigured or unavailable.

### `ExpandTemplate`
Executes the core logic for expanding a template string against a provided set of variables or arguments. This method is the primary subject of the benchmarking process.
*   **Parameters**: Implicitly accepts a template string and a collection of arguments (specific signature details depend on the internal overload resolution, but logically processes a template and a list of strings).
*   **Returns**: A tuple containing a `string` (the expanded result) and a `List<string>` (likely representing metadata, warnings, or intermediate steps generated during expansion).
*   **Throws**: Throws an exception if the template syntax is invalid, if required variables are missing, or if the expansion process encounters a runtime error.

### `Debug(string message)`
Records a detailed diagnostic message intended for deep troubleshooting. These logs are typically verbose and only visible when the logging level is set to Debug.
*   **Parameters**: `message` (`string`) – The diagnostic text to log.
*   **Returns**: `void`.
*   **Throws**: Generally does not throw unless the logging sink is corrupted.

### `Info(string message)`
Logs an informational message indicating normal operational progress, such as the start or completion of a benchmark run.
*   **Parameters**: `message` (`string`) – The informational text to log.
*   **Returns**: `void`.
*   **Throws**: Generally does not throw unless the logging sink is corrupted.

### `Warn(string message)`
Records a warning message indicating a potential issue or non-critical anomaly detected during template processing or benchmarking.
*   **Parameters**: `message` (`string`) – The warning text to log.
*   **Returns**: `void`.
*   **Throws**: Generally does not throw unless the logging sink is corrupted.

### `Error(string message)`
Logs an error message indicating a recoverable failure or a specific issue encountered during execution that does not halt the entire application but marks the current operation as failed.
*   **Parameters**: `message` (`string`) – The error description to log.
*   **Returns**: `void`.
*   **Throws**: Generally does not throw unless the logging sink is corrupted.

### `Error`
Represents an error logging capability, potentially an overload or a specific state accessor for error reporting without an immediate message payload.
*   **Parameters**: None (in this specific signature context).
*   **Returns**: `void` (assuming invocation as an action) or returns an error handler delegate depending on implementation.
*   **Throws**: Depends on usage; if invoked as a method without parameters, it may trigger a default error state or throw if misused.

### `Fatal(string message)`
Logs a critical failure message indicating an unrecoverable error that typically necessitates the termination of the benchmarking process or the application.
*   **Parameters**: `message` (`string`) – The critical failure description.
*   **Returns**: `void`.
*   **Throws**: May throw a `FatalException` or trigger an application shutdown sequence immediately after logging.

## Usage

### Example 1: Basic Benchmark Execution
The following example demonstrates initializing the benchmark suite, running a template expansion, and logging the outcome based on the result.

```csharp
using System;
using System.Collections.Generic;

public class BenchmarkRunner
{
    public void Run()
    {
        var benchmarks = new TemplateBenchmarks();
        
        try
        {
            benchmarks.Setup();
            benchmarks.Info("Starting template expansion benchmark...");
            
            var template = "Hello, {{name}}! You have {{count}} new messages.";
            // Assuming ExpandTemplate takes the template and a list of replacement values
            var result = benchmarks.ExpandTemplate(template, new List<string> { "User", "5" });
            
            benchmarks.Debug($"Expanded result: {result.Item1}");
            benchmarks.Info("Benchmark completed successfully.");
        }
        catch (Exception ex)
        {
            benchmarks.Error($"Benchmark failed: {ex.Message}");
        }
    }
}
```

### Example 2: Error Handling and Fatal Conditions
This example illustrates handling scenarios where template expansion fails critically, utilizing the warning and fatal logging methods.

```csharp
using System;
using System.Collections.Generic;

public class RobustBenchmarkRunner
{
    public void RunWithValidation()
    {
        var benchmarks = new TemplateBenchmarks();
        benchmarks.Setup();

        string invalidTemplate = "Invalid {{ syntax";
        
        try
        {
            benchmarks.Warn("Attempting to process potentially malformed template.");
            var result = benchmarks.ExpandTemplate(invalidTemplate, new List<string>());
            
            if (string.IsNullOrEmpty(result.Item1))
            {
                benchmarks.Error("Expansion returned empty result.");
            }
        }
        catch (Exception ex)
        {
            benchmarks.Error($"Recoverable error caught: {ex.Message}");
            
            // Determine if the error is critical
            if (ex.Message.Contains("Critical Syntax Failure"))
            {
                benchmarks.Fatal("Unrecoverable template syntax error detected. Aborting.");
                // Execution typically stops here or propagates up
            }
        }
    }
}
```

## Notes

*   **Thread Safety**: The presence of stateful methods like `Setup` combined with logging methods suggests that `TemplateBenchmarks` is likely not thread-safe. Concurrent calls to `Setup` and `ExpandTemplate` from multiple threads without external synchronization may lead to race conditions or corrupted benchmark data. Instances should be confined to a single thread or protected by a lock.
*   **Exception Propagation**: While the logging methods (`Debug`, `Info`, `Warn`, `Error`, `Fatal`) are designed to record events, `ExpandTemplate` is the primary source of operational exceptions. Callers must be prepared to catch exceptions arising from invalid template syntax or missing data.
*   **Logging Side Effects**: The `Fatal` method may have side effects beyond simple logging, such as terminating the process or throwing a terminating exception. It should not be called if the intention is to continue execution flow.
*   **Return Value Structure**: The `ExpandTemplate` method returns a tuple. Consumers must deconstruct or access `Item1` (the string result) and `Item2` (the list of strings) appropriately. The contents of the list are context-dependent and should be inspected for warnings or diagnostic details returned by the expansion engine.
