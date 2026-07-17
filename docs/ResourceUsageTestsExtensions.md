# ResourceUsageTestsExtensions

Utility class providing extension methods for creating and inspecting `ResourceUsage` objects in test scenarios. Designed to simplify the construction of resource usage test cases and the evaluation of their severity levels.

## API

### `WithUsage(ResourceUsage, ResourceType, decimal)`

Creates a new `ResourceUsage` instance with the specified resource type and usage value, copying all other properties from the source instance.

- **Parameters**
  - `source` (`ResourceUsage`): The source instance to copy properties from.
  - `resourceType` (`ResourceType`): The resource type to set in the new instance.
  - `usage` (`decimal`): The usage value to set in the new instance.
- **Return Value**
  - A new `ResourceUsage` instance with the updated resource type and usage value.
- **Throws**
  - `ArgumentNullException`: If `source` is `null`.

---

### `WithApplication(ResourceUsage, string)`

Creates a new `ResourceUsage` instance with the specified application name, copying all other properties from the source instance.

- **Parameters**
  - `source` (`ResourceUsage`): The source instance to copy properties from.
  - `application` (`string`): The application name to set in the new instance.
- **Return Value**
  - A new `ResourceUsage` instance with the updated application name.
- **Throws**
  - `ArgumentNullException`: If `source` is `null`.
  - `ArgumentException`: If `application` is `null` or whitespace.

---

### `GenerateLoadScenario(int, int, int)`

Generates a list of `ResourceUsage` objects simulating a load scenario with varying resource usage levels.

- **Parameters**
  - `normalCount` (`int`): The number of normal severity resource usages to generate.
  - `warningCount` (`int`): The number of warning severity resource usages to generate.
  - `criticalCount` (`int`): The number of critical severity resource usages to generate.
- **Return Value**
  - An `IReadOnlyList<ResourceUsage>` containing the generated resource usages.
- **Throws**
  - `ArgumentOutOfRangeException`: If any of the count parameters is negative.

---
### `HasNormalSeverity(IEnumerable<ResourceUsage>)`

Determines whether any `ResourceUsage` in the collection has normal severity.

- **Parameters**
  - `usages` (`IEnumerable<ResourceUsage>`): The collection of resource usages to check.
- **Return Value**
  - `true` if any usage has normal severity; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `usages` is `null`.

---
### `HasWarningSeverity(IEnumerable<ResourceUsage>)`

Determines whether any `ResourceUsage` in the collection has warning severity.

- **Parameters**
  - `usages` (`IEnumerable<ResourceUsage>`): The collection of resource usages to check.
- **Return Value**
  - `true` if any usage has warning severity; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `usages` is `null`.

---
### `HasCriticalSeverity(IEnumerable<ResourceUsage>)`

Determines whether any `ResourceUsage` in the collection has critical severity.

- **Parameters**
  - `usages` (`IEnumerable<ResourceUsage>`): The collection of resource usages to check.
- **Return Value**
  - `true` if any usage has critical severity; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `usages` is `null`.

## Usage

### Example 1: Creating a custom resource usage for testing
