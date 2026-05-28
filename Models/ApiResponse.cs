#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Generic API response wrapper for standardized communication with Coolify API.
/// Provides consistent error handling and data serialization across all endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; } = 200;
    public long TotalRecords { get; set; } = 0;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful API response with data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>Successful API response.</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully.",
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a failed API response with error details.
    /// </summary>
    /// <param name="errors">List of error messages.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>Failed API response.</returns>
    public static ApiResponse<T> ErrorResponse(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Errors = errors,
            StatusCode = statusCode,
            Message = errors.FirstOrDefault() ?? "An error occurred."
        };
    }

    /// <summary>
    /// Creates a failed API response with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>Failed API response.</returns>
    public static ApiResponse<T> ErrorResponse(string error, int statusCode = 400)
    {
        return ErrorResponse(new List<string> { error }, statusCode);
    }

    /// <summary>
    /// Adds an error message to the response errors list.
    /// </summary>
    /// <param name="error">Error message to add.</param>
    public void AddError(string error)
    {
        Errors.Add(error);
        Success = false;
    }

    /// <summary>
    /// Checks if the response contains errors.
    /// </summary>
    /// <returns>True if errors list is not empty.</returns>
    public bool HasErrors() => Errors.Count > 0;

    /// <summary>
    /// Returns the first error message or empty string if no errors.
    /// </summary>
    /// <returns>First error message.</returns>
    public string GetFirstError() => Errors.FirstOrDefault() ?? string.Empty;
}

/// <summary>
/// Generic paginated API response for list endpoints.
/// </summary>
public class ApiPaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public long TotalRecords { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; } = 200;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates total pages based on total records and page size.
    /// </summary>
    public void CalculateTotalPages()
    {
        TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    /// <summary>
    /// Checks if there are more pages available.
    /// </summary>
    /// <returns>True if current page is not the last page.</returns>
    public bool HasNextPage() => PageNumber < TotalPages;

    /// <summary>
    /// Checks if current page is the first page.
    /// </summary>
    /// <returns>True if on first page.</returns>
    public bool IsFirstPage() => PageNumber == 1;
}
