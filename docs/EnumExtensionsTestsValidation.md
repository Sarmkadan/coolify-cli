# EnumExtensionsTestsValidation

A static utility class providing validation and verification methods for enum types within the coolify-cli project. This class offers a consistent interface for checking enum validity, retrieving validation error messages, and ensuring enum values meet expected constraints through a set of overloaded methods designed to work with various enum types.

## API

### Validate Methods

#### `Validate(MyEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(AnotherEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(ThirdEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(FourthEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(FifthEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(SixthEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

#### `Validate(SeventhEnum value)`
Returns a read-only list of validation error messages for the specified enum value. Returns an empty list if the value is valid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `IReadOnlyList<string>` - Collection of error messages, empty if valid

**Exceptions:** None

### IsValid Methods

#### `IsValid(MyEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(AnotherEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(ThirdEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(FourthEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(FifthEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(SixthEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

#### `IsValid(SeventhEnum value)`
Determines whether the specified enum value is valid according to the validation rules.

**Parameters:**
- `value` - The enum value to check

**Returns:** `bool` - True if valid, false otherwise

**Exceptions:** None

### EnsureValid Methods

#### `EnsureValid(MyEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(AnotherEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(ThirdEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(FourthEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(FifthEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(SixthEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

#### `EnsureValid(SeventhEnum value)`
Validates the specified enum value and throws an exception if invalid.

**Parameters:**
- `value` - The enum value to validate

**Returns:** `void`

**Exceptions:** `InvalidOperationException` - Thrown when the enum value is not valid

## Usage

```csharp
using Coolify.CLI.Enums;

// Basic validation check
var status = DeploymentStatus.Pending;
if (EnumExtensionsTestsValidation.IsValid(status))
{
    Console.WriteLine("Status is valid");
}
else
{
    var errors = EnumExtensionsTestsValidation.Validate(status);
    Console.WriteLine($"Invalid status: {string.Join(", ", errors)}");
}
```

```csharp
using Coolify.CLI.Enums;

// Using EnsureValid for fail-fast validation
try
{
    var configType = ConfigurationType.Custom;
    EnumExtensionsTestsValidation.EnsureValid(configType);
    
    // Proceed with configuration logic
    ApplyConfiguration(configType);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration validation failed: {ex.Message}");
}
```

## Notes

The validation methods handle edge cases where enum values may represent undefined or out-of-range states, particularly when dealing with enums that have explicit integer assignments or when values are cast from integers. The `Validate` methods return empty collections rather than null for valid values, ensuring safe enumeration without null checks.

All methods are static and stateless, making them inherently thread-safe. No instance data is modified during execution, and the validation logic does not rely on mutable shared state. The methods can be safely called concurrently from multiple threads without synchronization concerns.
