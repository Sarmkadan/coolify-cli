// existing content ...

// ...

## ValidationHelperTests

The `ValidationHelperTests` class provides a set of unit tests for the `ValidationHelper` utility class. 
These tests verify the correctness of various validation methods, such as ID, email, port, and semantic version validation.

Here's an example of how to use some of the tested methods:

```csharp
var isValidId = ValidationHelper.IsValidId(123); // true
var isValidEmail = ValidationHelper.IsValidEmail("test@example.com"); // true
var isValidPort = ValidationHelper.IsValidPort("8080"); // true
var isValidSemanticVersion = ValidationHelper.IsValidSemanticVersion("1.2.3"); // true
var isValidCommitHash = ValidationHelper.IsValidCommitHash("a3f1b8c2d4e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3"); // true
var isValidDatabaseName = ValidationHelper.IsValidDatabaseName("my_database"); // true
var isValidResourceName = ValidationHelper.IsValidResourceName("my-resource"); // true
```