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

## InfrastructureTemplateEngineExtensions

The `InfrastructureTemplateEngineExtensions` class provides extension methods for working with infrastructure templates. It allows you to validate templates, check for changes, and get resource summaries. For example, you can use the `ValidateOrThrowAsync` method to validate a template and throw an exception if it is invalid:

## CsvFormatterExtensions

The `CsvFormatterExtensions` class provides methods to convert collections to and from CSV-formatted strings. It supports formatting with or without headers, and parsing CSV data into strongly-typed lists.

### Example usage:
