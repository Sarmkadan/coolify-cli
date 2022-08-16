# InfrastructureTemplateEngineExtensions

Static extension methods that provide asynchronous utilities for working with the infrastructure template engine, including change detection, resource summarization, and validation.

## API

### HasChangesAsync
```csharp
public static async Task<bool> HasChangesAsync
```
- **Purpose**: Determines whether the infrastructure template engine currently has pending changes.
- **Parameters**: None (aside from the implicit `this` parameter representing the template engine instance).
- **Return Value**: A `Task<bool>` that completes with `true` if changes are detected; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` if the template engine is not properly initialized; may propagate any exception thrown by underlying async operations.

### GetResourceSummary
```csharp
public static string GetResourceSummary
```
- **Purpose**: Produces a human‑readable summary of the resources managed by the template engine.
- **Parameters**: None (aside from the implicit `this` parameter).
- **Return Value**: A `string` containing the summary; returns an empty string if no resources are present.
- **Exceptions**: May throw `ArgumentNullException` if the template engine instance is `null`.

### ValidateOrThrowAsync
```csharp
public static async Task ValidateOrThrowAsync
```
- **Purpose**: Asynchronously validates the current state of the template engine and throws if validation fails.
- **Parameters**: None (aside from the implicit `this` parameter).
- **Return Value**: A `Task` that completes when validation succeeds.
- **Exceptions**: Throws a `ValidationException` (or a derived type) containing details of the first validation error encountered; may also throw `OperationCanceledException` if a cancellation token is triggered, or any exception from underlying validation logic.

### Incomplete Signatures
The following members appear in the source with incomplete signatures; their names, parameters, and full return types cannot be determined from the available information, so no further details are provided:

```csharp
public static async Task<
public static async Task<
public static async Task<
public static async Task<
```

## Usage

### Detecting Changes and Validating
```csharp
using Coolify.Cli.Infrastructure;

// Assuming `engine` is an instance of the infrastructure template engine
bool hasChanges = await engine.HasChangesAsync();
if (hasChanges)
{
    await engine.ValidateOrThrowAsync();
    // Proceed with deployment or further processing
}
```

### Retrieving a Resource Summary
```csharp
using Coolify.Cli.Infrastructure;

// `engine` is the template engine instance
string summary = engine.GetResourceSummary();
Console.WriteLine($"Current resources: {summary}");
```

## Notes
- All extension methods are designed to be called on an instance of the infrastructure template engine type; invoking them on a `null` instance will result in a `NullReferenceException` unless the method explicitly checks for null (as `GetResourceSummary` does).
- The methods are stateless and thread‑safe with respect to their own logic; however, thread safety depends on the underlying template engine instance. Concurrent calls that mutate the engine’s state may lead to race conditions.
- The incomplete signatures suggest additional asynchronous members exist, but without complete metadata their behavior, parameters, and exception contracts cannot be documented here. Consumers should refer to the source code or IntelliSense for full details.
