# ValidationHelperTests

`ValidationHelperTests` is a test class that contains unit tests for the validation logic encapsulated in the `ValidationHelper` utility. Each test method verifies a specific validation rule (identifier, email, port, semantic version, commit hash, database name, resource name) against a set of representative inputs and asserts the expected Boolean outcome.

## API

### `public void IsValidId_WithVariousInputs_ReturnsExpectedResult`
- **Purpose**: Validates that the `ValidationHelper.IsValidId` method returns correct results for a variety of identifier strings, including valid formats and common invalid patterns.
- **Parameters**: None.
- **Return Value**: `void`. The method signals success by completing without throwing an exception; failure is indicated by an thrown assertion exception from the test framework.
- **Throws**: May throw an exception (e.g., `AssertionException`) if any test case does not produce the expected result.

### `public void IsValidEmail_WithVariousAddresses_ReturnsExpectedResult`
- **Purpose**: Checks that `ValidationHelper.IsValidEmail` correctly accepts well‑formed email addresses and rejects malformed ones, covering typical edge cases such as missing `@`, multiple `@` symbols, and invalid domain parts.
- **Parameters**: None.
- **Return Value**: `void`. Success is indicated by a clean run; failures raise an assertion exception.
- **Throws**: May throw an assertion exception when an email validation result deviates from the expectation.

### `public void IsValidPort_WithBoundaryAndInvalidValues_ReturnsExpectedResult`
- **Purpose**: Ensures `ValidationHelper.IsValidPort` treats the boundary values `1` and `65535` as valid, values outside this range as invalid, and non‑numeric input as invalid.
- **Parameters**: None.
- **Return Value**: `void`. Completion without exception denotes all assertions passed.
- **Throws**: May throw an assertion exception if any port validation yields an unexpected result.

### `public void IsValidSemanticVersion_WithVariousVersionStrings_ReturnsExpectedResult`
- **Purpose**: Verifies that `ValidationHelper.IsValidSemanticVersion` accepts strings conforming to the SemVer specification (including optional pre‑release and build metadata) and rejects strings that violate the format.
- **Parameters**: None.
- **Return Value**: `void`. A clean execution indicates all test cases passed.
- **Throws**: May throw an assertion exception on any mismatch between expected and actual validation outcomes.

### `public void IsValidCommitHash_WithFortyLowercaseHexCharacters_ReturnsTrue`
- **Purpose**: Confirms that `ValidationHelper.IsValidCommitHash` returns `true` for a string consisting of exactly forty lowercase hexadecimal characters (the canonical Git commit hash) and `false` for any deviation.
- **Parameters**: None.
- **Return Value**: `void`. Success is signaled by the absence of thrown assertions.
- **Throws**: May throw an assertion exception if the method does not return the expected Boolean value for the supplied test cases.

### `public void IsValidDatabaseName_WithNameStartingWithDigit_ReturnsFalse`
- **Purpose**: Tests that `ValidationHelper.IsValidDatabaseName` rejects database names that begin with a digit, while accepting names that start with a letter or underscore and contain only alphanumerics and underscores.
- **Parameters**: None.
- **Return Value**: `void`. Completion without exception indicates all assertions succeeded.
- **Throws**: May throw an assertion exception if the validation logic does not behave as specified.

### `public void IsValidResourceName_WithTrailingHyphen_ReturnsFalse`
- **Purpose**: Ensures `ValidationHelper.IsValidResourceName` treats a trailing hyphen as invalid, while allowing hyphens in interior positions and enforcing the overall naming convention (alphanumerics and hyphens, not starting or ending with a hyphen).
- **Parameters**: None.
- **Return Value**: `void`. A clean run signals that all test assertions passed.
- **Throws**: May throw an assertion exception when the method’s output diverges from the expected result.

## Usage

```csharp
// Example 1: Running the full test suite with the dotnet test command.
// Assuming the project is built and the test assembly is available:
dotnet test coolify-cli.Tests.dll --filter "FullyQualifiedName~ValidationHelperTests"
```

```csharp
// Example 2: Invoking a single test method manually in a custom test harness.
using NUnit.Framework;
using coolify-cli.Tests; // namespace containing ValidationHelperTests

[TestFixture]
public class CustomTestRunner
{
    [Test]
    public void RunIdValidationTest()
    {
        var testInstance = new ValidationHelperTests();
        // The test method is public and returns void; any assertion failure will surface as an exception.
        testInstance.IsValidId_WithVariousInputs_ReturnsExpectedResult();
        // If no exception is thrown, the test passed.
    }
}
```

## Notes

- The test methods contain no mutable state; they rely only on the static `ValidationHelper` methods and local test data. Consequently, they are thread‑safe and can be executed in parallel without interference.
- Edge cases covered by the tests include:
  - Identifier strings with leading/trailing whitespace, special characters, and varying lengths.
  - Email addresses with subdomains, plus‑sign addressing, and invalid top‑level domains.
  - Port values at the extremes of the allowed range (`1` and `65535`), as well as zero, negative numbers, and values exceeding `65535`.
  - Semantic version strings with missing components, leading zeros, and illegal characters in pre‑release or build metadata.
  - Commit hashes that are not exactly forty hexadecimal characters, contain uppercase letters, or include non‑hexadecimal symbols.
  - Database names that start with a digit, contain hyphens, or include spaces.
  - Resource names that begin or end with a hyphen, contain consecutive hyphens, or include unsupported characters.
- Because the test class does not expose any properties or fields, there is no need for disposal or cleanup after use. Instances can be safely discarded after the test method completes.
