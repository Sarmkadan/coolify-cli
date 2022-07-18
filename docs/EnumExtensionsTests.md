# EnumExtensionsTests

The `EnumExtensionsTests` class serves as the comprehensive unit test suite for the `EnumExtensions` utility within the `coolify-cli` project. It validates the correctness of extension methods designed to handle enum parsing, formatting, description retrieval, and value conversion, ensuring robust behavior across valid inputs, edge cases, and invalid data scenarios.

## API

### GetDescription_WithNoDescriptionAttribute_ReturnsMemberName
Verifies that when an enum member lacks a `DescriptionAttribute`, the extension method returns the literal name of the enum member as the description.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the returned string does not match the member name.

### ToDisplayString_SimpleEnumValue_ReturnsFormattedString
Validates that a simple enum value is converted into a human-readable formatted string using default display rules.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the formatted string is incorrect or empty.

### ToDisplayString_AllDeploymentStatuses_ReturnNonEmptyStrings
Iterates through all defined `DeploymentStatus` enum values to ensure `ToDisplayString` produces a non-empty result for every possible status.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if any deployment status yields an empty or null string.

### ParseEnum_WithExactMatch_ReturnsCorrectValue
Confirms that `ParseEnum` successfully parses a string that exactly matches an enum member name (case-sensitive) into the corresponding enum value.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the parsed value does not match the expected enum member.

### ParseEnum_CaseInsensitive_ReturnsCorrectValue
Ensures that `ParseEnum` correctly identifies and returns the enum value even when the input string differs in case from the member name.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if case-insensitive matching fails.

### ParseEnum_WithInvalidValue_ThrowsArgumentException
Verifies that `ParseEnum` throws an `ArgumentException` when provided with a string that does not correspond to any defined enum member.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if an exception is not thrown or if the exception type is incorrect.

### ParseEnum_WithEmptyString_ThrowsArgumentException
Confirms that passing an empty string to `ParseEnum` results in an `ArgumentException`.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the method does not throw the expected exception.

### TryParseEnum_WithValidString_ReturnsValue
Tests the `TryParseEnum` method to ensure it returns `true` and outputs the correct enum value when given a valid string representation.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the method returns `false` or the output value is incorrect.

### TryParseEnum_WithInvalidString_ReturnsNull
Validates that `TryParseEnum` returns `false` and sets the output value to default (null for nullable enums) when the input string is invalid.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the method indicates success or returns a non-default value.

### TryParseEnum_WithNullString_ReturnsNull
Ensures that `TryParseEnum` handles a `null` input gracefully by returning `false` and setting the output to default without throwing an exception.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if an exception is thrown or the return value is `true`.

### GetAllValues_ReturnsAllDefinedEnumMembers
Checks that the `GetAllValues` extension returns a collection containing every member defined in the target enum type.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the count of returned values differs from the defined members or if any member is missing.

### GetValueDescriptionMap_ContainsEntryForEachEnumMember
Verifies that `GetValueDescriptionMap` generates a dictionary where every enum member is mapped to its corresponding description or name.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the map is missing entries or contains incorrect mappings.

### ToCliFormat_CamelCaseValue_ProducesKebabCase
Tests the conversion of CamelCase enum values (e.g., `BuildFailed`) into kebab-case strings (e.g., `build-failed`) suitable for CLI arguments.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the output string is not in valid kebab-case format.

### ToCliFormat_SingleWordValue_ReturnsLowercased
Ensures that single-word enum values are converted to lowercase strings by `ToCliFormat` without adding hyphens.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the result is not strictly lowercase.

### ToInt_ReturnsUnderlyingIntegerValue
Validates that the `ToInt` extension method correctly retrieves the underlying integer representation of an enum value.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the returned integer does not match the enum's defined value.

### ToLong_ReturnsUnderlyingLongValue
Similar to `ToInt`, this verifies that `ToLong` returns the underlying value cast as a `long`, supporting enums with larger underlying types.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the returned long value is incorrect.

### EqualsIgnoreCase_WithMatchingName_ReturnsTrue
Tests the `EqualsIgnoreCase` helper to confirm it returns `true` when comparing an enum value to a string name that matches ignoring case.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the comparison returns `false`.

### EqualsIgnoreCase_WithDifferentName_ReturnsFalse
Ensures `EqualsIgnoreCase` returns `false` when the string name does not match the enum member, even when ignoring case.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the comparison returns `true`.

### GetDisplayStrings_ReturnsOneStringPerEnumMember
Confirms that `GetDisplayStrings` returns a collection of strings where the count equals the number of enum members, with one display string per member.
*   **Parameters**: None (Test context).
*   **Return Value**: Void (Assertion).
*   **Throws**: Fails the test if the collection count mismatches or contains nulls.

## Usage

### Example 1: Safe Parsing with Fallback
The following example demonstrates using `TryParseEnum` to safely convert user input from a CLI argument into a `DeploymentStatus` enum, providing a default value if the input is invalid.

```csharp
using System;
using Coolify.Cli.Extensions;

public class DeploymentRunner
{
    public void RunDeployment(string userInput)
    {
        // Attempt to parse the user input; defaults to 'Pending' if invalid or null
        if (!userInput.TryParseEnum(out DeploymentStatus status))
        {
            status = DeploymentStatus.Pending;
            Console.WriteLine($"Invalid status '{userInput}'. Defaulting to {status}.");
        }
        
        Console.WriteLine($"Starting deployment with status: {status.ToDisplayString()}");
    }
}
```

### Example 2: Generating CLI Help Options
This example illustrates how to generate a list of valid kebab-cased options for a command-line help menu using `ToCliFormat` and `GetAllValues`.

```csharp
using System;
using System.Linq;
using Coolify.Cli.Extensions;

public class HelpGenerator
{
    public void PrintLogLevelOptions()
    {
        var options = EnumExtensions.GetAllValues<LogLevel>()
            .Select(level => level.ToCliFormat())
            .ToList();

        Console.WriteLine("Available log levels: " + string.Join(", ", options));
        // Output example: "Available log levels: debug, info, warning, error"
    }
}
```

## Notes

*   **Case Sensitivity**: The `ParseEnum` method supports case-insensitive matching, whereas standard `Enum.Parse` behavior may vary depending on overload usage. The `EqualsIgnoreCase` method explicitly abstracts this logic for boolean checks.
*   **Null and Empty Handling**: The `TryParseEnum` implementation is designed to handle `null` and empty strings gracefully by returning `false` rather than throwing exceptions, making it safe for unvalidated user input. Conversely, `ParseEnum` will throw an `ArgumentException` in these scenarios.
*   **Formatting Consistency**: The `ToCliFormat` method assumes CamelCase input conventions for multi-word enums to produce kebab-case output. Enums defined as single words or already containing separators may produce unexpected formatting if they deviate from standard PascalCase/CamelCase conventions.
*   **Thread Safety**: As this class consists entirely of static extension methods operating on immutable enum values and strings, all methods are inherently thread-safe and do not maintain internal state.
*   **Underlying Types**: While `ToInt` is sufficient for most scenarios, `ToLong` should be used when working with enums explicitly defined with a `long` underlying type to prevent overflow or truncation issues.
