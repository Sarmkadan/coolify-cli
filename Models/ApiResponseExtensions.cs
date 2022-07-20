#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolifyCli.Models;

/// <summary>
/// Extension methods for ApiResponse and ApiPaginatedResponse classes.
/// Provides convenient utility methods for working with API responses.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Converts an ApiResponse to a new ApiResponse with a different data type.
    /// Useful for chaining operations that change the response type.
    /// </summary>
    /// <typeparam name="TSource">Source data type.</typeparam>
    /// <typeparam name="TTarget">Target data type.</typeparam>
    /// <param name="response">Source API response.</param>
    /// <param name="mapper">Function to map source data to target data.</param>
    /// <returns>New ApiResponse with mapped data.</returns>
    public static ApiResponse<TTarget> Map<TSource, TTarget>(this ApiResponse<TSource> response, Func<TSource, TTarget> mapper)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (mapper == null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        return new ApiResponse<TTarget>
        {
            Success = response.Success,
            Data = response.Data != null ? mapper(response.Data) : default,
            Message = response.Message,
            Errors = new List<string>(response.Errors),
            StatusCode = response.StatusCode,
            TotalRecords = response.TotalRecords,
            Timestamp = response.Timestamp
        };
    }

    /// <summary>
    /// Converts an ApiPaginatedResponse to a new ApiPaginatedResponse with a different data type.
    /// Useful for transforming paginated data while preserving pagination metadata.
    /// </summary>
    /// <typeparam name="TSource">Source data type.</typeparam>
    /// <typeparam name="TTarget">Target data type.</typeparam>
    /// <param name="response">Source paginated API response.</param>
    /// <param name="mapper">Function to map each source item to target item.</param>
    /// <returns>New ApiPaginatedResponse with mapped data.</returns>
    public static ApiPaginatedResponse<TTarget> Map<TSource, TTarget>(this ApiPaginatedResponse<TSource> response, Func<TSource, TTarget> mapper)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (mapper == null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        return new ApiPaginatedResponse<TTarget>
        {
            Success = response.Success,
            Data = response.Data?.Select(mapper).ToList() ?? new List<TTarget>(),
            PageNumber = response.PageNumber,
            PageSize = response.PageSize,
            TotalRecords = response.TotalRecords,
            TotalPages = response.TotalPages,
            Message = response.Message,
            Errors = new List<string>(response.Errors),
            StatusCode = response.StatusCode,
            Timestamp = response.Timestamp
        };
    }

    /// <summary>
    /// Combines multiple ApiResponse objects into a single ApiResponse.
    /// All responses must be successful for the combined result to be successful.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="responses">Collection of API responses to combine.</param>
    /// <returns>Combined API response.</returns>
    public static ApiResponse<List<T>> Combine<T>(this IEnumerable<ApiResponse<T>> responses)
    {
        if (responses == null)
        {
            throw new ArgumentNullException(nameof(responses));
        }

        var responseList = responses.ToList();
        if (responseList.Count == 0)
        {
            return ApiResponse<List<T>>.SuccessResponse(new List<T>());
        }

        var allSuccessful = responseList.All(r => r.Success);
        var combinedErrors = new List<string>();
        var combinedData = new List<T>();
        var firstStatusCode = responseList.First().StatusCode;

        foreach (var response in responseList)
        {
            if (!response.Success)
            {
                combinedErrors.AddRange(response.Errors);
            }
            else if (response.Data != null)
            {
                combinedData.Add(response.Data);
            }
        }

        if (!allSuccessful)
        {
            return ApiResponse<List<T>>.ErrorResponse(combinedErrors, firstStatusCode);
        }

        return ApiResponse<List<T>>.SuccessResponse(combinedData);
    }

    /// <summary>
    /// Gets the first error message from the response, or null if no errors exist.
    /// This is a convenience method that returns null instead of empty string for nullable scenarios.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="response">API response to check.</param>
    /// <returns>First error message or null.</returns>
    public static string? GetFirstErrorOrNull<T>(this ApiResponse<T> response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return response.Errors.FirstOrDefault();
    }

    /// <summary>
    /// Determines whether the API response contains any of the specified error messages.
    /// Useful for checking against known error patterns.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="response">API response to check.</param>
    /// <param name="errorMessages">Error messages to search for.</param>
    /// <returns>True if any of the specified errors are found.</returns>
    public static bool ContainsError<T>(this ApiResponse<T> response, params string[] errorMessages)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (errorMessages == null || errorMessages.Length == 0)
        {
            return false;
        }

        return response.Errors.Any(error => errorMessages.Contains(error, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds multiple error messages to the response at once.
    /// Useful for batch error reporting.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="response">API response to add errors to.</param>
    /// <param name="errors">Error messages to add.</param>
    public static void AddErrors<T>(this ApiResponse<T> response, IEnumerable<string> errors)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (errors == null)
        {
            return;
        }

        foreach (var error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                response.Errors.Add(error);
            }
        }

        if (response.Errors.Count > 0)
        {
            response.Success = false;
        }
    }

    /// <summary>
    /// Creates a new successful paginated response from a list of items.
    /// Calculates pagination metadata automatically based on the list count.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="items">Items to include in the response.</param>
    /// <param name="pageNumber">Current page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>Paginated API response.</returns>
    public static ApiPaginatedResponse<T> ToPaginatedResponse<T>(this List<T> items, int pageNumber = 1, int pageSize = 20, string? message = null)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var paginatedResponse = new ApiPaginatedResponse<T>
        {
            Success = true,
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = items.Count,
            Message = message ?? "Data retrieved successfully.",
            Timestamp = DateTime.UtcNow
        };

        paginatedResponse.CalculateTotalPages();
        return paginatedResponse;
    }

    /// <summary>
    /// Gets the next page number for pagination.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="response">Paginated API response.</param>
    /// <returns>Next page number, or current page number if no next page exists.</returns>
    public static int GetNextPageNumber<T>(this ApiPaginatedResponse<T> response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return response.HasNextPage() ? response.PageNumber + 1 : response.PageNumber;
    }

    /// <summary>
    /// Gets the previous page number for pagination.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="response">Paginated API response.</param>
    /// <returns>Previous page number, or 1 if on first page.</returns>
    public static int GetPreviousPageNumber<T>(this ApiPaginatedResponse<T> response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return response.IsFirstPage() ? 1 : response.PageNumber - 1;
    }
}