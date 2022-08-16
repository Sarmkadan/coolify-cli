# ApiResponseExtensions

Extension methods for transforming and working with `ApiResponse<T>` and `ApiPaginatedResponse<T>` types, providing common mapping, error handling, and pagination utilities.

## API

### `Map<TSource, TTarget>(this ApiResponse<TSource> source)`
Maps the source response data to a new type using a default mapping strategy.

- **Parameters**
  - `source`: The source API response to map.
- **Return Value**
  Returns a new `ApiResponse<TTarget>` with the mapped data.
- **Throws**
  Throws `ArgumentNullException` if `source` is `null`.

---

### `Map<TSource, TTarget>(this ApiPaginatedResponse<TSource> source)`
Maps the paginated source response data to a new type using a default mapping strategy.

- **Parameters**
  - `source`: The paginated source API response to map.
- **Return Value**
  Returns a new `ApiPaginatedResponse<TTarget>` with the mapped data.
- **Throws**
  Throws `ArgumentNullException` if `source` is `null`.

---

### `Combine<T>(this IEnumerable<ApiResponse<T>> responses)`
Combines multiple API responses into a single response containing a list of all successful results.

- **Parameters**
  - `responses`: The collection of responses to combine.
- **Return Value**
  Returns an `ApiResponse<List<T>>` where the data is a list of all non-error responses' data. Errors are ignored.
- **Throws**
  Throws `ArgumentNullException` if `responses` is `null`.

---

### `GetFirstErrorOrNull<T>(this ApiResponse<T> response)`
Extracts the first error message from the response, if any.

- **Parameters**
  - `response`: The API response to inspect.
- **Return Value**
  Returns the first error message as a string, or `null` if no errors exist.
- **Throws**
  Throws `ArgumentNullException` if `response` is `null`.

---

### `ContainsError<T>(this ApiResponse<T> response)`
Checks whether the response contains any error messages.

- **Parameters**
  - `response`: The API response to check.
- **Return Value**
  Returns `true` if the response has one or more errors; otherwise, `false`.
- **Throws**
  Throws `ArgumentNullException` if `response` is `null`.

---
### `AddErrors<T>(this ApiResponse<T> response, IEnumerable<string> errors)`
Appends additional error messages to the response.

- **Parameters**
  - `response`: The API response to modify.
  - `errors`: The collection of error messages to add.
- **Throws**
  Throws `ArgumentNullException` if `response` or `errors` is `null`.

---
### `ToPaginatedResponse<T>(this ApiResponse<List<T>> response, int pageNumber, int pageSize, int totalCount)`
Converts a non-paginated response into a paginated response.

- **Parameters**
  - `response`: The source response containing the data to paginate.
  - `pageNumber`: The current page number (1-based).
  - `pageSize`: The number of items per page.
  - `totalCount`: The total number of items across all pages.
- **Return Value**
  Returns a new `ApiPaginatedResponse<T>` with pagination metadata.
- **Throws**
  Throws `ArgumentNullException` if `response` is `null`.
  Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1, or if `totalCount` is negative.

---
### `GetNextPageNumber<T>(this ApiPaginatedResponse<T> response)`
Calculates the next page number based on the current pagination state.

- **Parameters**
  - `response`: The paginated response to evaluate.
- **Return Value**
  Returns the next page number (1-based), or `0` if there are no more pages.
- **Throws**
  Throws `ArgumentNullException` if `response` is `null`.

---
### `GetPreviousPageNumber<T>(this ApiPaginatedResponse<T> response)`
Calculates the previous page number based on the current pagination state.

- **Parameters**
  - `response`: The paginated response to evaluate.
- **Return Value**
  Returns the previous page number (1-based), or `0` if there is no previous page.
- **Throws**
  Throws `ArgumentNullException` if `response` is `null`.

## Usage

### Mapping a paginated response
