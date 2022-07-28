# ApplicationDeployment

Represents a deployment configuration for an application managed by the coolify-cli tool, including metadata, build instructions, environment variables, and health monitoring settings.

## API

### `public int Id`
Unique identifier for the deployment. Read-only; assigned by the system. Throws no exceptions.

### `public string Name`
Human-readable name of the application deployment. Required for identification and display purposes. Throws no exceptions.

### `public string Description`
Optional descriptive text about the application deployment. May be null or empty. Throws no exceptions.

### `public string Repository`
URL of the source code repository (e.g., GitHub, GitLab) from which the application is built. Required for deployment. Throws no exceptions.

### `public string Branch`
Name of the Git branch to deploy (e.g., `main`, `develop`). Must match a valid branch in the repository. Throws no exceptions.

### `public string EnvironmentId`
Identifier of the environment where the application is deployed (e.g., `production`, `staging`). Links to a deployment environment. Throws no exceptions.

### `public DeploymentStatus Status`
Current status of the deployment (e.g., `Pending`, `Deployed`, `Failed`). Reflects the latest deployment state. Throws no exceptions.

### `public DateTime CreatedAt`
Timestamp when the deployment configuration was first created. Read-only; set by the system. Throws no exceptions.

### `public DateTime UpdatedAt`
Timestamp when the deployment configuration was last modified. Updated automatically on changes. Throws no exceptions.

### `public DateTime? LastDeployedAt`
Timestamp of the most recent successful deployment. Null if never deployed. Throws no exceptions.

### `public int FailureCount`
Number of consecutive failed deployment attempts. Reset to zero on successful deployment. Throws no exceptions.

### `public string? LastErrorMessage`
Error message from the most recent failed deployment attempt. Null if no failures or last attempt succeeded. Throws no exceptions.

### `public Dictionary<string, string> EnvironmentVariables`
Key-value pairs of environment variables to inject into the application runtime. Keys are variable names; values are their assigned values. Throws no exceptions.

### `public List<string> Ports`
List of network ports exposed by the application (e.g., `["80", "443"]`). Used for routing and health checks. Throws no exceptions.

### `public string BuildCommand`
Shell command to execute during the build phase of deployment (e.g., `npm run build`). May be empty if no build step is required. Throws no exceptions.

### `public string StartCommand`
Shell command to execute to start the application (e.g., `npm start`). Required for runtime. Throws no exceptions.

### `public bool IsActive`
Indicates whether the deployment is currently active and should be monitored/managed. When false, deployments are paused. Throws no exceptions.

### `public string? HealthCheckUrl`
Optional URL endpoint to query for application health status (e.g., `/health`). If null, no health checks are performed. Throws no exceptions.

### `public int HealthCheckIntervalSeconds`
Interval in seconds between health check requests. Must be a positive integer if `HealthCheckUrl` is set. Throws no exceptions.

### `public IEnumerable<string> Validate()`
Validates the deployment configuration for correctness and completeness. Returns an enumerable of validation error messages (empty if valid). Does not throw exceptions; returns errors as strings.

## Usage
