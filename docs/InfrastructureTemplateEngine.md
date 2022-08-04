# InfrastructureTemplateEngine

A utility class that provides infrastructure-as-code templating capabilities for the coolify-cli project. It enables loading, validating, comparing, applying, and exporting infrastructure templates in YAML format, primarily used for managing cloud infrastructure state.

## API

### `InfrastructureTemplateEngine`

The primary class exposing infrastructure templating operations. All operations return an `ApiResponse<T>` to encapsulate both success and failure states with detailed error information.

### `async Task<ApiResponse<InfrastructureTemplate>> LoadTemplateAsync(string templatePath)`

Loads an infrastructure template from the specified file path.

- **Parameters**:
  - `templatePath` (string): The filesystem path to the YAML template file.
- **Return value**: An `ApiResponse<InfrastructureTemplate>` containing the deserialized template on success, or error details on failure.
- **Exceptions**: Throws `ArgumentNullException` if `templatePath` is null or empty. Throws `FileNotFoundException` if the file does not exist. Throws `YamlException` if the file contains invalid YAML.

---

### `Task<ApiResponse<TemplateValidationResult>> ValidateTemplateAsync(InfrastructureTemplate template)`

Validates the structure and content of an infrastructure template.

- **Parameters**:
  - `template` (InfrastructureTemplate): The template to validate.
- **Return value**: An `ApiResponse<TemplateValidationResult>` with validation results including warnings and errors.
- **Exceptions**: Throws `ArgumentNullException` if `template` is null.

---

### `async Task<ApiResponse<TemplateDiffResult>> ComputeDiffAsync(InfrastructureTemplate currentState, InfrastructureTemplate desiredState)`

Computes the difference between the current infrastructure state and a desired state defined by a template.

- **Parameters**:
  - `currentState` (InfrastructureTemplate): The current infrastructure state.
  - `desiredState` (InfrastructureTemplate): The target infrastructure state.
- **Return value**: An `ApiResponse<TemplateDiffResult>` containing the computed diff, including additions, changes, and deletions.
- **Exceptions**: Throws `ArgumentNullException` if either parameter is null.

---

### `async Task<ApiResponse<TemplateApplyResult>> ApplyTemplateAsync(InfrastructureTemplate template, string targetEnvironment)`

Applies the specified infrastructure template to the target environment.

- **Parameters**:
  - `template` (InfrastructureTemplate): The template defining the desired infrastructure state.
  - `targetEnvironment` (string): Identifier for the environment where changes will be applied (e.g., "production", "staging").
- **Return value**: An `ApiResponse<TemplateApplyResult>` with results of the apply operation, including applied changes and any errors.
- **Exceptions**: Throws `ArgumentNullException` if `template` or `targetEnvironment` is null or empty.

---
### `async Task<ApiResponse<InfrastructureTemplate>> ExportCurrentStateAsync(string targetEnvironment)`

Exports the current state of the infrastructure for the specified environment.

- **Parameters**:
  - `targetEnvironment` (string): Identifier for the environment to export (e.g., "production", "staging").
- **Return value**: An `ApiResponse<InfrastructureTemplate>` containing the exported infrastructure state.
- **Exceptions**: Throws `ArgumentNullException` if `targetEnvironment` is null or empty. Throws `InvalidOperationException` if the export operation fails due to missing permissions or unavailable resources.

---
### `static string SerializeToYaml(InfrastructureTemplate template)`

Serializes an infrastructure template to a YAML string.

- **Parameters**:
  - `template` (InfrastructureTemplate): The template to serialize.
- **Return value**: A string containing the YAML representation of the template.
- **Exceptions**: Throws `ArgumentNullException` if `template` is null. Throws `YamlException` if serialization fails due to unsupported types or circular references.

## Usage

### Example 1: Load, Validate, and Apply a Template
