# ApiResponse

`ApiResponse` is a generic response wrapper used throughout the `coolify-cli` project to standardize the structure of data returned from API calls. It encapsulates success state, payload data, error details, HTTP status codes, and pagination metadata, providing a consistent contract for both successful and failed operations.

## API

### `ApiResponse<T>`

#### Properties

| Member | Type | Description |
|---|---|---|
| `Success` | `bool` | Indicates whether the operation completed successfully. `true` when no errors occurred; `false` otherwise. |
| `Data` | `T?` | The primary payload of a successful response. `null` when `Success` is `false` or no data is available. |
| `Message` | `string?` | A human-readable summary message, typically set on success or as a general informational note. May be `null`. |
| `Errors` | `List<string>` | A collection of error descriptions accumulated during a failed operation. Empty when `Success` is `true`. |
| `StatusCode` | `int` | The HTTP status code associated with the response (e.g., 200, 400, 500). |
| `TotalRecords` | `long` | The total number of records available for the query, used in paginated responses. |
| `Timestamp` | `DateTime` | The UTC timestamp at which the response was constructed. |

#### Static Factory Methods

| Member | Signature | Description |
|---|---|---|
| `SuccessResponse` | `static ApiResponse<T> SuccessResponse(T data, string? message = null, int statusCode = 200)` | Creates a new `ApiResponse<T>` marked as successful with the given payload, optional message, and status code. Returns the constructed instance. |
| `ErrorResponse` | `static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)` | Creates a new `ApiResponse<T>` marked as failed with the given error message and status code. The error is added to the `Errors` list. Returns the constructed instance. |
| `ErrorResponse` | `static ApiResponse<T> ErrorResponse(List<string> errors, int statusCode = 400)` | Creates a new `ApiResponse<T>` marked as failed with a pre-existing list of errors and status code. Returns the constructed instance. |

#### Instance Methods

| Member | Signature | Description |
|---|---|---|
| `AddError` | `void AddError(string error)` | Appends an error string to the `Errors` list. Does not automatically set `Success` to `false`; callers must manage that state if needed. |
| `HasErrors` | `bool HasErrors` | Returns `true` if the `Errors` list contains at least one entry; `false` otherwise. |
| `GetFirstError` | `string GetFirstError()` | Returns the first error string from the `Errors` list, or `string.Empty` if the list is empty. Does not throw. |

### `ApiResponse<T>` (Paginated Variant)

When `T` is a collection type, the response exposes additional pagination members.

| Member | Type | Description |
|---|---|---|
| `Success` | `bool` | Same as the standard variant. |
| `Data` | `List<T>` | The paginated subset of items for the current page. |
| `PageNumber` | `int` | The current page number (1-based). |
| `PageSize` | `int` | The maximum number of items per page. |
| `TotalRecords` | `long` | The total number of records across all pages. |
| `TotalPages` | `int` | The total number of pages available, computed from `TotalRecords` and `PageSize`. |
| `Message` | `string?` | A human-readable summary message. May be `null`. |

## Usage

### Example 1: Basic Success and Error Handling

```csharp
// Successful operation
var user = await FetchUserAsync(userId);
var response = ApiResponse<User>.SuccessResponse(user, "User retrieved successfully");

if (response.Success)
{
    Console.WriteLine(response.Message);
    ProcessUser(response.Data);
}
else
{
    Console.WriteLine($"Request failed: {response.GetFirstError()}");
}

// Error accumulation during validation
var validationResponse = ApiResponse<Order>.ErrorResponse("Validation failed");
validationResponse.AddError("Customer name is required.");
validationResponse.AddError("Order must contain at least one item.");

if (validationResponse.HasErrors)
{
    foreach (var error in validationResponse.Errors)
    {
        Log.Error(error);
    }
}
```

### Example 2: Paginated Response

```csharp
// Fetching a paginated list of resources
var productsResponse = await GetProductsAsync(page: 2, pageSize: 20);

if (productsResponse.Success)
{
    Console.WriteLine($"Showing page {productsResponse.PageNumber} of {productsResponse.TotalPages}");
    Console.WriteLine($"Items: {productsResponse.Data.Count} of {productsResponse.TotalRecords} total");

    foreach (var product in productsResponse.Data)
    {
        RenderProduct(product);
    }
}
else
{
    Console.WriteLine($"Failed to load products: {productsResponse.Message}");
}
```

## Notes

- **Nullability of `Data`**: When `Success` is `false`, `Data` is expected to be `null`. Callers should always check `Success` before dereferencing `Data` to avoid null-reference exceptions.
- **`AddError` does not toggle `Success`**: Calling `AddError` on an existing response does not automatically set `Success` to `false`. If you are building an error response incrementally, use `ErrorResponse` factory methods or manually set `Success = false` after adding errors.
- **`GetFirstError` fallback**: Returns `string.Empty` when the `Errors` list is empty. It never throws, making it safe for logging or display without null checks.
- **Thread safety**: `ApiResponse<T>` is not designed for concurrent mutation. The `Errors` list and property setters are not synchronized. Instances should be constructed, populated, and then shared read-only across threads. Do not call `AddError` or modify properties from multiple threads simultaneously.
- **Pagination fields**: The paginated variant computes `TotalPages` from `TotalRecords` and `PageSize`. If `PageSize` is zero or negative, `TotalPages` may be zero or negative; callers should validate `PageSize` before constructing paginated responses.
- **Timestamp**: Set at construction time via the factory methods. Not updated on subsequent mutations.
