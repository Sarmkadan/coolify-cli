# Coolify CLI

Coolify CLI is a command-line interface for working with infrastructure as code (IaC) templates.

## IacTemplateOptions

The `IacTemplateOptions` class provides configuration options for working with infrastructure as code (IaC) templates. It allows you to customize the behavior of the template engine, such as enabling dry run mode, auto-approving changes, and skipping validation.

## CoolifyConfiguration

The `CoolifyConfiguration` class provides configuration options for the Coolify API client. It allows you to customize the behavior of the client, such as setting the API URL, API key, and request timeout.

### Example usage:

## MonitoringValidationException

The `MonitoringValidationException` class represents an exception that occurs when validation of monitoring-related resources fails. It provides a way to handle validation errors in a centralized manner.

### Example usage:

## CoolifyException

The `CoolifyException` class serves as the base exception for all Coolify CLI-specific errors. It provides a structured way to handle and categorize exceptions through the `ErrorCode` property and allows attaching contextual data via the `ContextData` dictionary. This base class can be extended to create more specific exception types for different error scenarios.

### Example usage:

```csharp
try
{
    // Some operation that might throw
}
catch (CoolifyException ex)
{
    // Add additional context data to the exception
    ex.AddContextData("requestId", Guid.NewGuid().ToString());
    ex.AddContextData("userId", "user-123");
    
    // Re-throw with additional context
    throw;
}
```

## TemplateBenchmarks

The `TemplateBenchmarks` class provides methods for setting up and tearing down a benchmarking environment. It includes methods for logging at different levels and expanding a template.


Example usage:

```csharp
var benchmarks = new TemplateBenchmarks();
benchmarks.Setup();
var (template, files) = benchmarks.ExpandTemplate("template-name", new List<string> { "file1.txt", "file2.txt" });
benchmarks.Debug("Debug message");
benchmarks.Info("Info message");
benchmarks.Warn("Warning message");
benchmarks.Error("Error message");
benchmarks.Fatal("Fatal message");
```

## InfrastructureTemplateEngineExtensions

The `InfrastructureTemplateEngineExtensions` class provides extension methods for working with infrastructure templates. It allows you to validate templates, check for changes, and get resource summaries. For example, you can use the `ValidateOrThrowAsync` method to validate a template and throw an exception if it is invalid:

## CsvFormatterExtensions

The `CsvFormatterExtensions` class provides methods to convert collections to and from CSV-formatted strings. It supports formatting with or without headers, and parsing CSV data into strongly-typed lists.

### Example usage:
