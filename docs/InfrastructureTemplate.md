# InfrastructureTemplate

`InfrastructureTemplate` is a declarative configuration type used in `coolify-cli` to define the infrastructure and runtime characteristics of a deployable workload. It encapsulates metadata, application specifications, database configurations, and validation logic required to provision and manage cloud-native resources programmatically.

## API

### `ApiVersion`
- **Type**: `string`
- **Purpose**: Specifies the API version of the template schema. Used for versioning and compatibility checks.
- **Constraints**: Must conform to semantic versioning conventions (e.g., `"v1"`).

### `Kind`
- **Type**: `string`
- **Purpose**: Identifies the template type (e.g., `"Infrastructure"`). Used for discriminating between different template kinds in the system.

### `Metadata`
- **Type**: `IacTemplateMetadata` (required)
- **Purpose**: Contains core metadata about the template, such as identifiers, ownership, and lifecycle attributes.
- **Constraints**: Must not be `null`.

### `Applications`
- **Type**: `List<IacTemplateApplication>`
- **Purpose**: Defines a list of application workloads to be deployed. Each entry specifies runtime requirements, build/start commands, and exposed ports.
- **Default**: Empty list if not provided.

### `Databases`
- **Type**: `List<IacTemplateDatabase>`
- **Purpose**: Defines a list of database services to be provisioned alongside applications. Includes configuration for engine type, versions, and persistence.
- **Default**: Empty list if not provided.

### `Validate`
- **Type**: `IEnumerable<string>`
- **Purpose**: Validates the template configuration and yields human-readable error messages for invalid fields.
- **Returns**: An enumerable of validation errors. Empty if validation succeeds.
- **Behavior**: Performs cross-field validation (e.g., ensuring `Ports` are within valid ranges). Throws no exceptions; errors are surfaced via the return value.

### `Name`
- **Type**: `string` (required)
- **Purpose**: The unique identifier for the template. Used for referencing and logging.
- **Constraints**: Must not be `null` or whitespace.

### `Description`
- **Type**: `string?`
- **Purpose**: Optional human-readable description of the template’s purpose or functionality.
- **Default**: `null`.

### `Environment`
- **Type**: `string?`
- **Purpose**: Specifies the target environment (e.g., `"production"`, `"staging"`). Influences runtime behavior and resource allocation.
- **Default**: `null`.

### `Version`
- **Type**: `string?`
- **Purpose**: The version of the template, following semantic versioning conventions.
- **Default**: `null`.

### `Labels`
- **Type**: `Dictionary<string, string>`
- **Purpose**: Key-value pairs for attaching arbitrary metadata (e.g., `"team": "backend"`). Used for filtering, grouping, or policy enforcement.
- **Default**: Empty dictionary if not provided.

### `Repository`
- **Type**: `string` (required)
- **Purpose**: The source code repository URL (e.g., GitHub) where the application code resides.
- **Constraints**: Must be a valid URI.

### `Branch`
- **Type**: `string`
- **Purpose**: The Git branch to deploy. Defaults to the repository’s default branch if not specified.
- **Default**: `null`.

### `Runtime`
- **Type**: `RuntimeEnvironment`
- **Purpose**: Specifies the runtime environment (e.g., `.NET`, `Node.js`, `Python`). Determines build tooling and runtime dependencies.

### `EnvironmentId`
- **Type**: `string?`
- **Purpose**: A unique identifier for the target environment instance. Used for multi-environment deployments (e.g., ephemeral preview environments).
- **Default**: `null`.

### `BuildCommand`
- **Type**: `string?`
- **Purpose**: The command to execute during the build phase (e.g., `"dotnet publish"`). If `null`, the runtime’s default build process is used.
- **Default**: `null`.

### `StartCommand`
- **Type**: `string?`
- **Purpose**: The command to start the application (e.g., `"dotnet MyApp.dll"`). Required for runtimes that do not auto-detect entry points.
- **Default**: `null`.

### `Ports`
- **Type**: `List<int>`
- **Purpose**: List of TCP ports exposed by the application. Used for service discovery and ingress configuration.
- **Constraints**: Ports must be in the range `1-65535`. Duplicate ports are ignored.
- **Default**: Empty list if not provided.

### `HealthCheck`
- **Type**: `IacHealthCheckSpec?`
- **Purpose**: Defines health check configuration (e.g., endpoint, interval, timeout). Used for liveness/readiness probes.
- **Default**: `null`.

## Usage

### Example 1: Minimal Template for a .NET Application
