# ApplicationService
The `ApplicationService` class provides asynchronous methods for interacting with the Coolify API to manage applications, deployments, and related operations. It is intended to be instantiated directly or supplied via dependency injection and wraps HTTP calls in `ApiResponse<T>` objects for consistent error handling.

## API
### ApplicationService()
Public parameterless constructor. Creates a new instance ready to make API calls. No state is maintained internally beyond any configured HTTP client.

### GetAllApplicationsAsync()
Purpose: Retrieves a list of all applications known to the Coolify server.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<List<ApplicationDeployment>>>` containing the list on success or error details on failure.  
Throws: May throw `OperationCanceledException` if the request is cancelled; other exceptions are wrapped in the `ApiResponse` error payload.

### GetApplicationAsync()
Purpose: Obtains details for a single application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` with the application data or error information.  
Throws: Same as above; network failures or invalid responses are reflected in the returned `ApiResponse`.

### CreateApplicationAsync()
Purpose: Submits a request to create a new application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` containing the created application or error details.  
Throws: Exceptions are captured in the `ApiResponse`; cancellation results in `OperationCanceledException`.

### UpdateApplicationAsync()
Purpose: Updates an existing application’s configuration.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` with the updated application or error info.  
Throws: Errors are reported via the `ApiResponse`; cancellation throws `OperationCanceledException`.

### DeployApplicationAsync()
Purpose: Triggers a deployment for an application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<DeploymentContext>>` describing the deployment or error details.  
Throws: Network or server errors are encapsulated in the `ApiResponse`; cancellation throws `OperationCanceledException`.

### RollbackApplicationAsync()
Purpose: Rolls back an application to a previous deployment.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` with the application state after rollback or error info.  
Throws: Errors are wrapped in the `ApiResponse`; cancellation throws `OperationCanceledException`.

### DeleteApplicationAsync()
Purpose: Deletes an application from the Coolify server.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<object>>` indicating success or failure.  
Throws: Any HTTP or serialization errors are placed in the `ApiResponse`; cancellation throws `OperationCanceledException`.

### GetDeploymentHistoryAsync()
Purpose: Retrieves the deployment history for an application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<List<DeploymentContext>>>` containing the history or error details.  
Throws: Errors are reported via the `ApiResponse`; cancellation throws `OperationCanceledException`.

### StartApplicationAsync()
Purpose: Starts a stopped application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` with the running application or error info.  
Throws: Errors are captured in the `ApiResponse`; cancellation throws `OperationCanceledException`.

### StopApplicationAsync()
Purpose: Stops a running application.  
Parameters: None (as per the displayed signature).  
Return Value: `Task<ApiResponse<ApplicationDeployment>>` with the stopped application or error details.  
Throws: Errors are wrapped in the `ApiResponse`; cancellation throws `OperationCanceledException`.

## Usage
```csharp
using CoolifyCli.Services;

// Instantiate the service (could also be injected via DI)
var appService = new ApplicationService();

// List all applications
var listResponse = await appService.GetAllApplicationsAsync();
if (listResponse.Success)
{
    foreach (var app in listResponse.Data)
    {
        Console.WriteLine($"{app.Id}: {app.Name}");
    }
}
else
{
    Console.Error.WriteLine($"Failed to list apps: {listResponse.ErrorMessage}");
}
```

```csharp
using CoolifyCli.Services;

var appService = new ApplicationService();

// Create a new application
var createResponse = await appService.CreateApplicationAsync();
if (!createResponse.Success)
{
    Console.Error.WriteLine($"Create failed: {createResponse.ErrorMessage}");
    return;
}

var newApp = createResponse.Data;

// Deploy the newly created application
var deployResponse = await appService.DeployApplicationAsync();
if (deployResponse.Success)
{
    Console.WriteLine($"Deployment started, ID: {deployResponse.Data.Id}");
}
else
{
    Console.Error.WriteLine($"Deploy failed: {deployResponse.ErrorMessage}");
}
```

## Notes
- All methods are asynchronous and return `ApiResponse<T>`; callers should inspect the `Success` property before accessing `Data`.
- The service does not maintain mutable state; instances are thread-safe with respect to their own members, but thread safety of underlying HTTP transport depends on the implementation used (typically `HttpClient` is safe for concurrent use).
- Passing `null` or invalid values for any hidden parameters (not shown in the signature) will result in an error response rather than a thrown exception.
- Cancellation tokens, if supported by the caller, should be supplied via the method’s overload (if available); otherwise, use `CancellationTokenSource` to trigger cancellation, which will surface as an `OperationCanceledException`.
- Error details are contained within the `ApiResponse` instance; exceptions are only thrown for task cancellation or severe failures preventing response deserialization.
