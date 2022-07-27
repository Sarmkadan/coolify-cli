# ResourceUsageTests

The `ResourceUsageTests` class serves as a test suite within the `coolify-cli` project, designed to validate the logic governing resource consumption metrics and alerting thresholds. It specifically verifies the correctness of memory percentage calculations, ensures alert severity levels are accurately assigned based on CPU and memory utilization boundaries, and confirms that summary output formatting includes essential identification data such as application ID and name.

## API

### `MemoryPercent_WhenLimitIsZero_ReturnsZero`
Validates the behavior of memory percentage calculation when the defined memory limit is zero. This test ensures that the system handles division-by-zero scenarios gracefully by returning a zero percent value rather than throwing an exception or returning an undefined number.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the calculated percentage is not exactly zero.

### `MemoryPercent_WhenHalfOfLimit_ReturnsFiftyPercent`
Verifies that the memory percentage calculation returns exactly 50% when the current usage is half of the defined limit. This confirms the linear proportionality of the metric calculation logic.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the result deviates from 50%.

### `GetAlertSeverity_WhenAllMetricsNormal_ReturnsNull`
Ensures that the alert severity evaluation logic returns `null` when both CPU and memory metrics are within acceptable operational ranges (below warning thresholds).
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the returned severity is not `null`.

### `GetAlertSeverity_WhenCpuAboveEighty_ReturnsWarning`
Confirms that a "Warning" severity level is triggered when CPU utilization exceeds the 80% threshold, provided other critical thresholds are not met.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the severity is not "Warning".

### `GetAlertSeverity_WhenCpuAboveNinetyFive_ReturnsCritical`
Confirms that a "Critical" severity level is triggered when CPU utilization exceeds the 95% threshold, taking precedence over lower severity warnings.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the severity is not "Critical".

### `GetAlertSeverity_WhenMemoryPercentAboveEightyFive_ReturnsWarning`
Validates that a "Warning" severity level is assigned when memory usage percentage exceeds 85%.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the severity is not "Warning".

### `GetAlertSeverity_WhenMemoryPercentAboveNinetyFive_ReturnsCritical`
Validates that a "Critical" severity level is assigned when memory usage percentage exceeds 95%.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the severity is not "Critical".

### `ToSummaryLine_IncludesApplicationIdAndName`
Verifies that the generated summary line string correctly incorporates the specific Application ID and Application Name, ensuring traceability in logs or output.
*   **Parameters**: None (Test method).
*   **Return Value**: `void` (Assertion result).
*   **Throws**: Throws an assertion failure if the output string lacks the required identifiers.

## Usage

The following examples demonstrate how the logic verified by `ResourceUsageTests` is typically consumed in the `coolify-cli` codebase.

**Example 1: Calculating Memory Percentage with Safety Checks**
This example illustrates the calculation logic that ensures a zero limit does not cause runtime errors, a behavior verified by `MemoryPercent_WhenLimitIsZero_ReturnsZero`.

```csharp
public static double CalculateMemoryPercent(long currentUsage, long limit)
{
    if (limit == 0)
    {
        // Logic verified by MemoryPercent_WhenLimitIsZero_ReturnsZero
        return 0.0;
    }

    return (double)currentUsage / limit * 100.0;
}

// Usage context
long usage = 512;
long limit = 1024;
double percent = CalculateMemoryPercent(usage, limit); 
// Expected: 50.0
```

**Example 2: Determining Alert Severity Based on Thresholds**
This example shows the evaluation chain for alert severities, reflecting the conditions tested in `GetAlertSeverity_WhenCpuAboveEighty_ReturnsWarning` and related methods.

```csharp
public static string? GetAlertSeverity(double cpuPercent, double memoryPercent)
{
    // Critical checks (verified by ...WhenCpuAboveNinetyFive... and ...MemoryPercentAboveNinetyFive...)
    if (cpuPercent > 95.0 || memoryPercent > 95.0)
    {
        return "Critical";
    }

    // Warning checks (verified by ...WhenCpuAboveEighty... and ...MemoryPercentAboveEightyFive...)
    if (cpuPercent > 80.0 || memoryPercent > 85.0)
    {
        return "Warning";
    }

    // Normal state (verified by ...WhenAllMetricsNormal...)
    return null;
}

// Usage context
var severity = GetAlertSeverity(82.5, 60.0); 
// Expected: "Warning"
```

## Notes

*   **Edge Cases**: The implementation explicitly handles the edge case where the memory limit is zero. Instead of causing a `DivideByZeroException`, the logic defaults the percentage to zero. This is critical for containers or processes where limits may not be enforced or defined.
*   **Threshold Precedence**: The alert severity logic implies a strict precedence where Critical thresholds (>95%) override Warning thresholds (>80% CPU or >85% Memory). Tests verify these boundaries individually; in production, the evaluation order must ensure higher severities are checked first.
*   **Thread Safety**: As this class represents a suite of unit tests validating stateless calculation logic, the underlying methods being tested are expected to be pure functions. Consequently, the logic is inherently thread-safe as it relies solely on input parameters without modifying shared static state or instance fields during execution.
*   **Formatting Dependencies**: The `ToSummaryLine` verification assumes that the Application ID and Name are non-null and populated prior to the summary generation call. If these fields are missing, the test will fail, indicating a data integrity issue upstream rather than a formatting error.
