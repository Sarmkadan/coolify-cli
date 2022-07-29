# DeploymentContext
The `DeploymentContext` class in the `coolify-cli` project provides a centralized context for managing deployments. It encapsulates various aspects of a deployment, including the deployment ID, application deployment details, environment variables, linked databases, deployment status, and logs. This class serves as a single source of truth for deployment-related information, enabling efficient management and tracking of deployments throughout their lifecycle.

## API
The `DeploymentContext` class exposes the following public members:
* `DeploymentId`: A string representing the unique identifier of the deployment.
* `Application`: An `ApplicationDeployment` object containing details about the application being deployed.
* `EnvironmentVariables`: A list of `EnvironmentVariable` objects representing the environment variables associated with the deployment.
* `LinkedDatabases`: A list of `DatabaseConfiguration` objects representing the databases linked to the deployment.
* `TargetStatus`: A `DeploymentStatus` enum value indicating the target status of the deployment.
* `StartedAt`: A `DateTime` object representing the timestamp when the deployment started.
* `CompletedAt`: A nullable `DateTime` object representing the timestamp when the deployment completed, if applicable.
* `DeploymentLogs`: A list of `LogEntry` objects containing logs related to the deployment.
* `Artifacts`: A dictionary of string key-value pairs representing artifacts associated with the deployment.
* `RequiresApproval`: A boolean indicating whether the deployment requires approval.
* `ApprovedBy`: A nullable string representing the user who approved the deployment, if applicable.
* `RollbackToVersion`: A nullable string representing the version to roll back to, if applicable.
* `LogEvent`: A method to log an event related to the deployment. Parameters: none. Return value: none. Throws: not specified.
* `MarkAsCompleted`: A method to mark the deployment as completed. Parameters: none. Return value: none. Throws: not specified.
* `GetDuration`: A method to calculate the duration of the deployment. Parameters: none. Return value: `TimeSpan`. Throws: not specified.
* `AddArtifact`: A method to add an artifact to the deployment. Parameters: not specified. Return value: none. Throws: not specified.
* `LoadEnvironmentVariables`: A method to load environment variables for the deployment. Parameters: none. Return value: none. Throws: not specified.
* `Validate`: A method to validate the deployment context. Parameters: none. Return value: `IEnumerable<string>` containing validation errors. Throws: not specified.

## Usage
Here are two examples of using the `DeploymentContext` class:
```csharp
// Example 1: Creating a new deployment context
var deploymentContext = new DeploymentContext();
deploymentContext.DeploymentId = "DEP-123";
deploymentContext.Application = new ApplicationDeployment { Name = "My App" };
deploymentContext.EnvironmentVariables.Add(new EnvironmentVariable { Name = "VAR1", Value = "val1" });
deploymentContext.LogEvent();

// Example 2: Retrieving deployment logs
var deploymentContext = new DeploymentContext();
deploymentContext.LoadEnvironmentVariables();
foreach (var logEntry in deploymentContext.DeploymentLogs)
{
    Console.WriteLine(logEntry.Message);
}
```

## Notes
When using the `DeploymentContext` class, consider the following edge cases and thread-safety remarks:
* The `CompletedAt` property may be null if the deployment has not completed yet.
* The `ApprovedBy` and `RollbackToVersion` properties may be null if approval or rollback is not applicable.
* The `LogEvent` and `MarkAsCompleted` methods may throw exceptions if the deployment is not in a valid state.
* The `GetDuration` method may return an invalid `TimeSpan` if the deployment has not started or completed yet.
* The `AddArtifact` method may throw an exception if the artifact already exists.
* The `LoadEnvironmentVariables` method may throw an exception if the environment variables cannot be loaded.
* The `Validate` method may return an empty enumerable if the deployment context is valid.
* The `DeploymentContext` class is not thread-safe by default. If accessing the class from multiple threads, consider implementing synchronization mechanisms to ensure data integrity.
