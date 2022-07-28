# EnvironmentVariable

The `EnvironmentVariable` class represents a configurable key-value pair associated with a specific application within the Coolify ecosystem. It encapsulates metadata such as creation timestamps, scope, and security flags (indicating if the value is a secret), while providing built-in mechanisms for validation, cloning, and change tracking. This type serves as the primary data transfer object for managing environment configurations across different deployment scopes.

## API

### Properties

#### `public int Id`
Gets the unique numerical identifier for this environment variable record within the database.

#### `public string ApplicationId`
Gets the identifier of the application to which this environment variable belongs. This links the variable to a specific service or deployment target.

#### `public string Key`
Gets or sets the name of the environment variable (e.g., `DATABASE_URL`). This is the identifier used by the runtime to retrieve the value.

#### `public string Value`
Gets or sets the actual content of the environment variable. If `IsSecret` is true, this value should be handled with increased security precautions during serialization or logging.

#### `public bool IsSecret`
Gets or sets a flag indicating whether the variable contains sensitive data. When true, the system may mask the output in logs or UI displays.

#### `public string? Description`
Gets or sets an optional human-readable description explaining the purpose or expected format of this variable.

#### `public string EnvironmentScope`
Gets or sets the scope in which this variable is active (e.g., "production", "staging", "development"). This determines when the variable is injected into the runtime environment.

#### `public DateTime CreatedAt`
Gets the timestamp indicating when this record was initially created.

#### `public DateTime UpdatedAt`
Gets the timestamp indicating when this record was last modified.

#### `public string? CreatedBy`
Gets the identifier (usually a user ID or service name) of the entity that created this record.

#### `public string? UpdatedBy`
Gets the identifier of the entity that last modified this record.

#### `public bool IsActive`
Gets or sets a flag determining whether this variable is currently enabled. Inactive variables are typically ignored during deployment injection.

### Methods

#### `public IEnumerable<string> Validate`
Validates the current state of the `EnvironmentVariable` instance.
*   **Returns**: An enumerable collection of strings, where each string represents a validation error message. If the collection is empty, the instance is considered valid.
*   **Remarks**: Validation typically checks for null/empty keys, invalid characters in the key, or missing values when required.

#### `public string GetDisplayValue`
Retrieves the value suitable for display in logs or user interfaces.
*   **Returns**: The raw `Value` if `IsSecret` is false; otherwise, returns a masked string (e.g., `********`) to prevent leakage of sensitive data.
*   **Throws**: No specific exceptions expected under normal operation.

#### `public EnvironmentVariable Clone`
Creates a deep copy of the current `EnvironmentVariable` instance.
*   **Returns**: A new instance of `EnvironmentVariable` with identical property values.
*   **Remarks**: Useful for creating draft versions of variables or undo buffers without modifying the original tracked entity.

#### `public void MarkAsUpdated`
Updates the audit fields of the instance to reflect a recent modification.
*   **Parameters**: None.
*   **Behavior**: Sets `UpdatedAt` to the current UTC time and optionally updates the `UpdatedBy` field based on the current execution context.
*   **Remarks**: This method should be called immediately after modifying mutable properties (`Key`, `Value`, `IsActive`, etc.) before persisting changes.

## Usage

### Example 1: Creating and Validating a New Variable
This example demonstrates instantiating a new secret variable, assigning it to a scope, and validating its integrity before saving.

```csharp
var dbUrlVar = new EnvironmentVariable
{
    ApplicationId = "app_12345",
    Key = "DATABASE_CONNECTION_STRING",
    Value = "Host=db.example.com;Port=5432;User=admin",
    IsSecret = true,
    EnvironmentScope = "production",
    Description = "Primary production database connection",
    IsActive = true
};

// Validate the configuration
var errors = dbUrlVar.Validate();
if (errors.Any())
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation failed: {error}");
    }
    return;
}

// Safely log the display value (will be masked because IsSecret is true)
Console.WriteLine($"Configured value: {dbUrlVar.GetDisplayValue()}");

// Mark as updated to set timestamps before persistence
dbUrlVar.MarkAsUpdated();
```

### Example 2: Cloning for Staging Promotion
This example shows how to clone a production variable to create a staging version, modifying the scope and value safely.

```csharp
// Assume 'prodVar' is an existing active production variable
EnvironmentVariable stagingVar = prodVar.Clone();

// Modify specific properties for the new scope
stagingVar.EnvironmentScope = "staging";
stagingVar.Value = "Host=staging-db.internal;Port=5432;User=staging_admin";
stagingVar.Description = "Cloned from production for staging environment";

// Reset audit tracking for the new entry
stagingVar.MarkAsUpdated();

// The original 'prodVar' remains unchanged
Console.WriteLine($"Original Scope: {prodVar.EnvironmentScope}"); 
Console.WriteLine($"New Scope: {stagingVar.EnvironmentScope}");
```

## Notes

*   **Thread Safety**: The `EnvironmentVariable` class is not thread-safe. Mutable properties (`Value`, `Key`, `IsActive`, etc.) can be modified concurrently if accessed from multiple threads without external synchronization. The `MarkAsUpdated` method modifies state and should be protected by a lock if the instance is shared across threads.
*   **Secret Handling**: Always use `GetDisplayValue()` when logging or displaying variable contents to the user. Directly accessing the `Value` property in log statements may inadvertently expose secrets if `IsSecret` is true.
*   **Validation Logic**: The `Validate` method returns a deferred execution enumerable (`IEnumerable<string>`). Ensure you materialize the result (e.g., via `.ToList()` or `.Any()`) immediately if the underlying state of the object might change before enumeration.
*   **Cloning Behavior**: The `Clone` method performs a member-wise copy. While value types and strings are safely duplicated, ensure that if any future properties reference mutable collections, they are also deeply copied to prevent unintended side effects between the original and the clone.
*   **Audit Fields**: `CreatedAt` and `CreatedBy` are typically set only once upon initial persistence. Calling `MarkAsUpdated` affects only `UpdatedAt` and `UpdatedBy`; it does not alter creation metadata.
