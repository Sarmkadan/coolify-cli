// ... (rest of the file content remains the same)

## CoolifyApiClientOptions

The `CoolifyApiClientOptions` class provides a set of configuration options for the Coolify API client. It allows you to customize the timeout settings for different HTTP methods.

### Usage Example

```csharp
var options = CoolifyApiClientOptions.FromConfiguration;
options.GetTimeoutSeconds = 30;
options.PostTimeoutSeconds = 60;
options.PutTimeoutSeconds = 30;
options.DeleteTimeoutSeconds = 15;

// Use the configured options to create a client
var client = new CoolifyApiClient(options);
```

## ApiResponseExtensions

The `ApiResponseExtensions` class provides a set of extension methods for working with API responses. These methods enable you to easily map responses, combine multiple responses, and extract error information.

### Usage Examples

Here's an example of using `Map` to transform an API response:

```csharp
var apiResponse = new ApiResponse<MyDto> { Data = new MyDto { Id = 1, Name = "John" } };
var mappedResponse = apiResponse.Map<MyDto, MyOtherDto>(dto => new MyOtherDto { Id = dto.Id, FullName = dto.Name });
```

You can combine multiple API responses into a single response using `Combine`:

```csharp
var response1 = new ApiResponse<MyDto> { Data = new MyDto { Id = 1, Name = "John" } };
var response2 = new ApiResponse<MyDto> { Data = new MyDto { Id = 2, Name = "Jane" } };
var combinedResponse = ApiResponseExtensions.Combine(new[] { response1, response2 });
```

To extract the first error from an API response, use `GetFirstErrorOrNull`:

```csharp
var apiResponse = new ApiResponse<MyDto> { Errors = new[] { "Error 1", "Error 2" } };
var firstError = apiResponse.GetFirstErrorOrNull();
```

## DeploymentDiffEntryExtensions

The `DeploymentDiffEntryExtensions` class offers a collection of extension methods for `DeploymentDiffEntry` objects, helping you quickly determine the significance of a deployment change. It includes helpers to check if a change is critical, sensitive, or resource‑related, to format the change description, and to create a deep copy of the entry.

### Usage Example

```csharp
var diffEntry = new DeploymentDiffEntry
{
    // Populate properties as needed
};

bool isCritical   = diffEntry.IsCriticalChange();
bool isSensitive  = diffEntry.IsSensitiveChange();
bool isResource   = diffEntry.IsResourceChange();

string formatted  = diffEntry.FormatChange();

DeploymentDiffEntry copy = diffEntry.DeepCopy();
```

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
```