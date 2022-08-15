# StringExtensionsTestsExtensions

The `StringExtensionsTestsExtensions` static class provides a set of helper methods for unit testing the `StringExtensions` class. It simplifies common assertion patterns for string transformation utilities such as PascalCase conversion, truncation, masking of sensitive data, and trimmed splitting. The class also includes a factory method to create a standard test instance of `StringExtensionsTests`.

## API

### `CreateTestInstance`

```csharp
public static StringExtensionsTests CreateTestInstance()
```

Creates and returns a new instance of `StringExtensionsTests` with default configuration. This instance can be used to invoke the assertion methods defined in this class.

- **Returns**: A new `StringExtensionsTests` object.
- **Throws**: None.

### `AssertToPascalCase`

```csharp
public static void AssertToPascalCase(this StringExtensionsTests test, string input, string expected)
```

Asserts that the `ToPascalCase` method of `StringExtensions` transforms the given `input` string into the `expected` PascalCase result.

- **Parameters**:
  - `test` – The test instance on which the assertion is performed.
  - `input` – The string to convert.
  - `expected` – The expected PascalCase output.
- **Throws**: `AssertFailedException` if the actual result does not match `expected`.

### `AssertTruncate`

```csharp
public static void AssertTruncate(this StringExtensionsTests test, string input, int maxLength, string expected)
```

Asserts that the `Truncate` method of `StringExtensions` shortens the `input` string to at most `maxLength` characters, producing the `expected` result.

- **Parameters**:
  - `test` – The test instance.
  - `input` – The string to truncate.
  - `maxLength` – The maximum allowed length.
  - `expected` – The expected truncated string.
- **Throws**: `AssertFailedException` if the actual result differs from `expected`.  
  Also throws `ArgumentOutOfRangeException` if `maxLength` is negative (when the underlying `Truncate` method enforces that constraint).

### `AssertMaskSensitive`

```csharp
public static void AssertMaskSensitive(this StringExtensionsTests test, string input, string maskChar, int visibleChars, string expected)
```

Asserts that the `MaskSensitive` method of `StringExtensions` replaces all but the last `visibleChars` characters of the `input` with the `maskChar` character, yielding the `expected` string.

- **Parameters**:
  - `test` – The test instance.
  - `input` – The sensitive string to mask.
  - `maskChar` – The character used for masking (e.g., `"*"`).
  - `visibleChars` – The number of characters to leave unmasked at the end.
  - `expected` – The expected masked string.
- **Throws**: `AssertFailedException` if the actual result does not match `expected`.  
  May throw `ArgumentNullException` if `input` or `maskChar` is `null`, or `ArgumentOutOfRangeException` if `visibleChars` is negative.

### `AssertSplitTrimmed`

```csharp
public static void AssertSplitTrimmed(this StringExtensionsTests test, string input, char separator, string[] expected)
```

Asserts that the `SplitTrimmed` method of `StringExtensions` splits the `input` string by the given `separator` and trims each resulting segment, producing the `expected` array of strings.

- **Parameters**:
  - `test` – The test instance.
  - `input` – The string to split and trim.
  - `separator` – The character used as the delimiter.
  - `expected` – The expected array of trimmed substrings.
- **Throws**: `AssertFailedException` if the actual array does not match `expected` in length or content.  
  Throws `ArgumentNullException` if `input` is `null`.

## Usage

The following examples demonstrate typical usage of `StringExtensionsTestsExtensions` in a unit test project using a framework such as MSTest or NUnit.

### Example 1: Testing PascalCase conversion and truncation

```csharp
[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void ToPascalCase_And_Truncate_WorkCorrectly()
    {
        var test = StringExtensionsTestsExtensions.CreateTestInstance();

        test.AssertToPascalCase("hello world", "HelloWorld");
        test.AssertTruncate("A long string that needs shortening", 20, "A long string that n");
    }
}
```

### Example 2: Testing masking and split-trimmed

```csharp
[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void MaskSensitive_And_SplitTrimmed_HandleEdgeCases()
    {
        var test = StringExtensionsTestsExtensions.CreateTestInstance();

        test.AssertMaskSensitive("1234567890", "*", 4, "******7890");
        test.AssertSplitTrimmed("  one , two , three ", ',', new[] { "one", "two", "three" });
    }
}
```

## Notes

- **Edge cases**:  
  - `AssertToPascalCase` with an empty string should expect an empty string.  
  - `AssertTruncate` with `maxLength` equal to the input length returns the original string; with `maxLength` of zero returns `string.Empty`.  
  - `AssertMaskSensitive` with `visibleChars` greater than the input length returns the input unchanged.  
  - `AssertSplitTrimmed` with an empty input returns an empty array; with no separator present returns a single-element array containing the trimmed input.

- **Thread safety**: All methods in this class are static and do not modify any shared state. They are inherently thread-safe when used with immutable input parameters. However, the underlying `StringExtensions` methods being tested should also be thread-safe for reliable parallel test execution.
