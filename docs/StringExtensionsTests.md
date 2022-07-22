# StringExtensionsTests

The `StringExtensionsTests` class serves as the unit test suite for the `StringExtensions` utility within the `coolify-cli` project. It validates the correctness of string manipulation methods used throughout the CLI, specifically focusing on formatting, truncation, data masking, and parsing logic. By enforcing strict behavioral contracts for these extension methods, this class ensures that command-line output remains consistent, sensitive data is properly obscured in logs, and user input is parsed reliably across different environments.

## API

### `ToPascalCase_WithHyphenDelimitedWords_ReturnsConcatenatedPascalWords`
Verifies the behavior of converting hyphen-delimited strings (kebab-case) into PascalCase.
*   **Purpose**: Ensures that input strings containing hyphens are split, capitalized appropriately, and concatenated without delimiters.
*   **Parameters**: None (uses internal test data fixtures).
*   **Return Value**: `void` (asserts conditions via test framework).
*   **Throws**: Throws an assertion exception if the resulting string does not match the expected PascalCase format or if null/empty inputs are mishandled.

### `Truncate_WhenInputExceedsMaxLength_TruncatesAndAddsEllipsis`
Validates the truncation logic when the input string length exceeds the specified maximum limit.
*   **Purpose**: Confirms that long strings are cut to the defined maximum length and appended with an ellipsis (`...`) to indicate omission.
*   **Parameters**: None (uses internal test data fixtures).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the output length exceeds the limit, if the ellipsis is missing, or if the truncation cuts off in the middle of a logical unit where prohibited.

### `Truncate_WhenInputWithinMaxLength_ReturnsOriginalString`
Ensures that the truncation method acts as a pass-through when the input string is already within the allowed length.
*   **Purpose**: Guarantees that no modification, including the addition of an ellipsis, occurs when the input length is less than or equal to the maximum threshold.
*   **Parameters**: None (uses internal test data fixtures).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the returned string differs from the original input.

### `MaskSensitive_WithLongApiKey_ExposesOnlyEdgeCharacters`
Tests the security masking functionality for sensitive strings like API keys or tokens.
*   **Purpose**: Verifies that long sensitive strings are obscured, exposing only a specific number of characters at the start and end while replacing the middle section with masking characters.
*   **Parameters**: None (uses internal test data fixtures).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the masked output reveals too many characters, fails to mask the center, or throws an overflow error on boundary conditions.

### `SplitTrimmed_WithWhitespacePaddedParts_ReturnsCleanSegments`
Validates the splitting of strings by delimiters while simultaneously trimming whitespace from the resulting segments.
*   **Purpose**: Ensures that when a string is split, any leading or trailing whitespace around the delimiters or within the segments is removed, returning clean, non-empty segments.
*   **Parameters**: None (uses internal test data fixtures).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the resulting collection contains empty strings, nulls, or segments with retained whitespace.

## Usage

The following examples demonstrate how the tested logic behaves in practical scenarios within the `coolify-cli` codebase.

### Example 1: Formatting Command Arguments
Converting user-provided kebab-case arguments into PascalCase for internal reflection or display purposes.

```csharp
using System;

public class ArgumentFormatter
{
    public void FormatArgument()
    {
        string userInput = "deploy-production-server";
        
        // Logic tested by ToPascalCase_WithHyphenDelimitedWords_ReturnsConcatenatedPascalWords
        // Expected output: "DeployProductionServer"
        string formatted = userInput.ToPascalCase(); 
        
        Console.WriteLine($"Normalized Command: {formatted}");
    }
}
```

### Example 2: Secure Logging of Configuration
Masking sensitive API keys before writing them to diagnostic logs to prevent credential leakage.

```csharp
using System;

public class SecureLogger
{
    public void LogConfiguration(string apiKey)
    {
        // Logic tested by MaskSensitive_WithLongApiKey_ExposesOnlyEdgeCharacters
        // If apiKey is "sk_live_1234567890abcdef", output might be "sk_l...cdef"
        string safeKey = apiKey.MaskSensitive(); 
        
        Console.WriteLine($"Using API Key: {safeKey}");
        
        // Logic tested by Truncate_WhenInputExceedsMaxLength_TruncatesAndAddsEllipsis
        string longMessage = "This is a very verbose debug message that exceeds the standard console width limit...";
        string truncatedMessage = longMessage.Truncate(50);
        
        Console.WriteLine(truncatedMessage);
    }
}
```

## Notes

*   **Edge Cases**: The `Truncate` tests explicitly distinguish between inputs that exactly match the maximum length and those that exceed it. Implementations must ensure that the ellipsis is not appended if the string fits exactly. Similarly, `MaskSensitive` must handle strings shorter than the masking threshold gracefully, potentially returning the original string or a fully masked version depending on the specific implementation rules, rather than throwing an index out of range exception.
*   **Whitespace Handling**: The `SplitTrimmed` method implies a robust handling of multiple consecutive delimiters and varying whitespace types (spaces, tabs). Consumers should expect that empty segments resulting from consecutive delimiters are filtered out unless the underlying implementation specifies otherwise.
*   **Thread Safety**: As this class represents a suite of unit tests for static or extension methods operating on immutable `string` types, the tested methods are inherently thread-safe. Strings in C# are immutable, and the operations described (splitting, truncating, casing) do not maintain internal state, allowing safe concurrent execution across multiple CLI threads.
