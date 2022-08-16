# DeploymentDiffEntryExtensions

This static class provides extension methods for the `DeploymentDiffEntry` type, enabling quick classification and transformation of deployment diff entries. It is used to determine the nature of a change (critical, sensitive, resource-related), format a human-readable description, and create deep copies.

## API

### `IsCriticalChange(this DeploymentDiffEntry entry)`

- **Purpose**: Determines whether the diff entry represents a critical change that may require special attention or approval.
- **Parameters**: `entry` – The deployment diff entry to evaluate.
- **Returns**: `true` if the entry is considered critical; otherwise `false`.
- **Throws**: `ArgumentNullException` if `entry` is `null`.

### `FormatChange(this DeploymentDiffEntry entry)`

- **Purpose**: Returns a human-readable string describing the change represented by the diff entry.
- **Parameters**: `entry` – The deployment diff entry to format.
- **Returns**: A `string` containing the formatted change description.
- **Throws**: `ArgumentNullException` if `entry` is `null`.

### `IsSensitiveChange(this DeploymentDiffEntry entry)`

- **Purpose**: Indicates whether the diff entry involves sensitive data (e.g., secrets, passwords) that should not be logged or displayed in plain text.
- **Parameters**: `entry` – The deployment diff entry to evaluate.
- **Returns**: `true` if the entry is sensitive; otherwise `false`.
- **Throws**: `ArgumentNullException` if `entry` is `null`.

### `DeepCopy(this DeploymentDiffEntry entry)`

- **Purpose**: Creates a deep copy of the deployment diff entry, producing a new independent instance with the same property values.
- **Parameters**: `entry` – The deployment diff entry to copy.
- **Returns**: A new `DeploymentDiffEntry` instance that is a deep copy of the original.
- **Throws**: `ArgumentNullException` if `entry` is `null`.

### `IsResourceChange(this DeploymentDiffEntry entry)`

- **Purpose**: Determines whether the diff entry corresponds to a change in a resource (e.g., a container, volume, network) rather than a configuration or metadata change.
- **Parameters**: `entry` – The deployment diff entry to evaluate.
- **Returns**: `true` if the entry is a resource change; otherwise `false`.
- **Throws**: `ArgumentNullException` if `entry` is `null`.

## Usage

### Example 1: Classifying and formatting diff entries

```csharp
using CoolifyCLI.Models;

var diffEntries = GetDeploymentDiff(); // hypothetical method returning IEnumerable<DeploymentDiffEntry>

foreach (var entry in diffEntries)
{
    if (entry.IsCriticalChange())
    {
        Console.WriteLine($"CRITICAL: {entry.FormatChange()}");
    }
    else if (entry.IsSensitiveChange())
    {
        Console.WriteLine($"SENSITIVE: {entry.FormatChange()}");
    }
    else if (entry.IsResourceChange())
    {
        Console.WriteLine($"RESOURCE: {entry.FormatChange()}");
    }
    else
    {
        Console.WriteLine($"INFO: {entry.FormatChange()}");
    }
}
```

### Example 2: Creating a deep copy for auditing

```csharp
using CoolifyCLI.Models;

DeploymentDiffEntry original = GetOriginalEntry(); // hypothetical method
DeploymentDiffEntry copy = original.DeepCopy();

// Modify the copy without affecting the original
copy.SomeProperty = "modified";

// The original remains unchanged
Console.WriteLine(original.SomeProperty); // original value
```

## Notes

- All extension methods throw `ArgumentNullException` if the `entry` parameter is `null`. Always validate the input before calling these methods.
- The classification methods (`IsCriticalChange`, `IsSensitiveChange`, `IsResourceChange`) are read-only and do not modify the entry. They are safe to call from multiple threads concurrently on the same instance, provided the underlying `DeploymentDiffEntry` type is immutable or its state is not being mutated concurrently.
- `DeepCopy` creates a new instance and does not share any mutable references with the original. The copy is independent and thread-safe relative to the original.
- `FormatChange` returns a string; the formatting logic may depend on the internal state of the entry. If the entry is modified concurrently, the returned string may be inconsistent. For thread safety, ensure the entry is not mutated during the call.
- These extension methods are intended for use with `DeploymentDiffEntry` objects obtained from the `coolify-cli` deployment diff pipeline. Behavior with entries from other sources is undefined.
