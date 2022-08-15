# EnumExtensionsTestsExtensions

`EnumExtensionsTestsExtensions` provides a suite of utility methods for testing and validating enum types in unit tests. These extensions simplify common validation tasks such as parsing enums, checking display mappings, and verifying CLI formatting rules, particularly useful in test scenarios where enum behavior needs to be validated against expected values.

## API

### `ParseTestEnum<TEnum>`
Parses a string value into an enum of type `TEnum` using the same rules as `Enum.Parse`. This method is primarily intended for test assertions where you need to validate that a string can be correctly converted to an enum value.

- **Parameters**:
  - `value`: The string representation of the enum value to parse.
- **Return value**: The parsed enum value of type `TEnum`.
- **Exceptions**:
  - Throws `ArgumentException` if the string cannot be parsed into a valid enum value.

### `TryParseTestEnum<TEnum>`
Attempts to parse a string value into an enum of type `TEnum`. This method is useful for testing scenarios where you need to handle invalid enum strings gracefully.

- **Parameters**:
  - `value`: The string representation of the enum value to parse.
- **Return value**: The parsed enum value if successful; otherwise, `null`.
- **Exceptions**: None.

### `GetEnumValueDisplayMap<TEnum>`
Generates a dictionary mapping each enum value to its display string representation. This is useful for validating that all enum values have associated display strings in test assertions.

- **Parameters**: None.
- **Return value**: A `Dictionary<TEnum, string>` where each key is an enum value and the corresponding value is its display string.
- **Exceptions**: None.

### `AllDisplayStringsNonEmpty<TEnum>`
Checks whether all enum values have non-empty display strings. This method is useful for validating that no enum value is missing a display string in test scenarios.

- **Parameters**: None.
- **Return value**: `true` if all display strings are non-empty; otherwise, `false`.
- **Exceptions**: None.

### `GetEnumValueIntMap<TEnum>`
Generates a dictionary mapping each enum value to its underlying integer value. This is useful for validating that enum values correspond to expected integer values in test assertions.

- **Parameters**: None.
- **Return value**: A `Dictionary<TEnum, int>` where each key is an enum value and the corresponding value is its underlying integer.
- **Exceptions**: None.

### `GetEnumValueLongMap<TEnum>`
Generates a dictionary mapping each enum value to its underlying long value. This is useful for validating that enum values correspond to expected long values in test scenarios, particularly when dealing with large enum values.

- **Parameters**: None.
- **Return value**: A `Dictionary<TEnum, long>` where each key is an enum value and the corresponding value is its underlying long value.
- **Exceptions**: None.

### `GetEnumTestCases<TEnum>`
Generates a sequence of tuples containing each enum value paired with its display string. This is useful for generating test cases for enum-based tests, particularly when testing display formatting or CLI argument parsing.

- **Parameters**: None.
- **Return value**: An `IEnumerable<(TEnum Value, string Display)>` containing each enum value and its display string.
- **Exceptions**: None.

### `AreEnumValuesInOrder<TEnum>`
Checks whether the enum values are defined in ascending order. This method is useful for validating that enum values follow a predictable ordering convention in test scenarios.

- **Parameters**: None.
- **Return value**: `true` if the enum values are in ascending order; otherwise, `false`.
- **Exceptions**: None.

### `GetEnumCliFormatMap<TEnum>`
Generates a dictionary mapping each enum value to its CLI format string. This is useful for validating that all enum values have associated CLI format strings in test assertions.

- **Parameters**: None.
- **Return value**: A `Dictionary<TEnum, string>` where each key is an enum value and the corresponding value is its CLI format string.
- **Exceptions**: None.

### `AllCliFormatsUnique<TEnum>`
Checks whether all enum values have unique CLI format strings. This method is useful for validating that no two enum values share the same CLI format in test scenarios.

- **Parameters**: None.
- **Return value**: `true` if all CLI format strings are unique; otherwise, `false`.
- **Exceptions**: None.

## Usage

```csharp
// Example 1: Validating enum parsing and display strings
var testEnum = EnumExtensionsTestsExtensions.ParseTestEnum<TestEnum>("ValueTwo");
var displayMap = EnumExtensionsTestsExtensions.GetEnumValueDisplayMap<TestEnum>();
Assert.True(EnumExtensionsTestsExtensions.AllDisplayStringsNonEmpty<TestEnum>());

// Example 2: Checking CLI format uniqueness and generating test cases
var cliFormatMap = EnumExtensionsTestsExtensions.GetEnumCliFormatMap<TestEnum>();
Assert.True(EnumExtensionsTestsExtensions.AllCliFormatsUnique<TestEnum>());
var testCases = EnumExtensionsTestsExtensions.GetEnumTestCases<TestEnum>();
foreach (var (value, display) in testCases)
{
    Console.WriteLine($"{value}: {display}");
}
```

## Notes

- **Thread safety**: All methods are stateless and thread-safe. They do not modify shared state and rely only on the enum type `TEnum`, which is immutable at runtime.
- **Performance**: Methods such as `GetEnumValueDisplayMap` and `GetEnumCliFormatMap` use reflection to inspect the enum type. While this is acceptable for test utilities, avoid calling these methods in performance-critical paths.
- **Edge cases**: Methods such as `ParseTestEnum` and `TryParseTestEnum` handle case-sensitive parsing by default. If case-insensitive parsing is required, the caller should normalize the input string before invoking these methods.
- **Generic constraints**: The generic type parameter `TEnum` must be an enum type. Passing a non-enum type will result in a compile-time error.
