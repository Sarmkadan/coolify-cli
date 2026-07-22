#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;
using CoolifyCli.Models;

/// <summary>
/// Test suite for the <see cref="ApiResponseExtensions"/> class.
/// Tests extension methods for ApiResponse and ApiPaginatedResponse classes.
/// </summary>
public class ApiResponseExtensionsTests
{
    // Test data
    public static class TestData
    {
        public static readonly User SampleUser = new() { Id = 1, Name = "Test User", Email = "test@example.com" };
        public static readonly List<User> SampleUsers = new()
        {
            new() { Id = 1, Name = "User 1", Email = "user1@example.com" },
            new() { Id = 2, Name = "User 2", Email = "user2@example.com" },
            new() { Id = 3, Name = "User 3", Email = "user3@example.com" }
        };
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    // ====================================================================
    // Map Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that Map transforms data in successful ApiResponse.
    /// </summary>
    [Fact]
    public void Map_WithSuccessfulResponse_TransformsData()
    {
        // Arrange
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser, "User retrieved");

        // Act
        var mappedResponse = response.Map(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeTrue();
        mappedResponse.Data.Should().NotBeNull();
        mappedResponse.Data!.Id.Should().Be(TestData.SampleUser.Id);
        mappedResponse.Data.Name.Should().Be(TestData.SampleUser.Name);
        mappedResponse.Data.Email.Should().Be(TestData.SampleUser.Email);
        mappedResponse.Message.Should().Be("User retrieved");
        mappedResponse.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies that Map preserves failure state and errors in ApiResponse.
    /// </summary>
    [Fact]
    public void Map_WithFailedResponse_PreservesFailureState()
    {
        // Arrange
        var errors = new List<string> { "Database error", "Connection failed" };
        var response = ApiResponse<User>.ErrorResponse(errors, 500);

        // Act
        var mappedResponse = response.Map(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeFalse();
        mappedResponse.Data.Should().BeNull();
        mappedResponse.Errors.Should().BeEquivalentTo(errors);
        mappedResponse.StatusCode.Should().Be(500);
        mappedResponse.Message.Should().Be("Database error");
    }

    /// <summary>
    /// Verifies that Map handles null data in successful response.
    /// </summary>
    [Fact]
    public void Map_WithSuccessfulResponseAndNullData_ReturnsNullData()
    {
        // Arrange
        var response = ApiResponse<User?>.SuccessResponse(null, "No user found");

        // Act
        var mappedResponse = response.Map(u => u!.Name); // This should not be called since data is null

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeTrue();
        mappedResponse.Data.Should().BeNull();
        mappedResponse.Message.Should().Be("No user found");
    }

    /// <summary>
    /// Verifies that Map throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void Map_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse<User>? response = null;

        // Act & Assert
        Func<ApiResponse<UserDto>> act = () => response!.Map(u => new UserDto());
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }

    /// <summary>
    /// Verifies that Map throws ArgumentNullException when mapper is null.
    /// </summary>
    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser);
        Func<User, UserDto>? mapper = null;

        // Act & Assert
        Func<ApiResponse<UserDto>> act = () => response.Map(mapper!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
    }

    // ====================================================================
    // Map Method Tests for ApiPaginatedResponse
    // ====================================================================

    /// <summary>
    /// Verifies that Map transforms data in successful ApiPaginatedResponse.
    /// </summary>
    [Fact]
    public void Map_WithSuccessfulPaginatedResponse_TransformsData()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            Success = true,
            Data = TestData.SampleUsers,
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 3,
            Message = "Users retrieved"
        };
        response.CalculateTotalPages();

        // Act
        var mappedResponse = response.Map(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeTrue();
        mappedResponse.Data.Should().NotBeNull();
        mappedResponse.Data.Should().HaveCount(3);
        mappedResponse.Data[0].Id.Should().Be(TestData.SampleUsers[0].Id);
        mappedResponse.Data[0].Name.Should().Be(TestData.SampleUsers[0].Name);
        mappedResponse.Data[0].Email.Should().Be(TestData.SampleUsers[0].Email);
        mappedResponse.PageNumber.Should().Be(1);
        mappedResponse.PageSize.Should().Be(10);
        mappedResponse.TotalRecords.Should().Be(3);
        mappedResponse.TotalPages.Should().Be(1);
        mappedResponse.Message.Should().Be("Users retrieved");
    }

    /// <summary>
    /// Verifies that Map preserves failure state and errors in ApiPaginatedResponse.
    /// </summary>
    [Fact]
    public void Map_WithFailedPaginatedResponse_PreservesFailureState()
    {
        // Arrange
        var errors = new List<string> { "Service unavailable" };
        var response = new ApiPaginatedResponse<User>
        {
            Success = false,
            Errors = errors,
            StatusCode = 503,
            Message = "Service unavailable"
        };

        // Act
        var mappedResponse = response.Map(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeFalse();
        mappedResponse.Data.Should().BeEmpty();
        mappedResponse.Errors.Should().BeEquivalentTo(errors);
        mappedResponse.StatusCode.Should().Be(503);
        mappedResponse.Message.Should().Be("Service unavailable");
    }

    /// <summary>
    /// Verifies that Map handles null data in ApiPaginatedResponse.
    /// </summary>
    [Fact]
    public void Map_WithPaginatedResponseAndNullData_ReturnsEmptyList()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User?>
        {
            Success = true,
            Data = null!,
            Message = "No data"
        };

        // Act
        var mappedResponse = response.Map(u => u!.Name);

        // Assert
        mappedResponse.Should().NotBeNull();
        mappedResponse.Success.Should().BeTrue();
        mappedResponse.Data.Should().BeEmpty();
        mappedResponse.Message.Should().Be("No data");
    }

    // ====================================================================
    // Combine Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that Combine combines multiple successful responses.
    /// </summary>
    [Fact]
    public void Combine_WithMultipleSuccessfulResponses_ReturnsCombinedSuccess()
    {
        // Arrange
        var responses = new List<ApiResponse<User>>
        {
            ApiResponse<User>.SuccessResponse(TestData.SampleUsers[0], "First"),
            ApiResponse<User>.SuccessResponse(TestData.SampleUsers[1], "Second"),
            ApiResponse<User>.SuccessResponse(TestData.SampleUsers[2], "Third")
        };

        // Act
        var combined = responses.Combine();

        // Assert
        combined.Should().NotBeNull();
        combined.Success.Should().BeTrue();
        combined.Data.Should().NotBeNull();
        combined.Data.Should().HaveCount(3);
        combined.Data[0].Id.Should().Be(TestData.SampleUsers[0].Id);
        combined.Data[1].Id.Should().Be(TestData.SampleUsers[1].Id);
        combined.Data[2].Id.Should().Be(TestData.SampleUsers[2].Id);
        combined.Message.Should().Be("Operation completed successfully.");
        combined.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies that Combine with failed responses returns failure.
    /// </summary>
    [Fact]
    public void Combine_WithSomeFailedResponses_ReturnsFailure()
    {
        // Arrange
        var responses = new List<ApiResponse<User>>
        {
            ApiResponse<User>.ErrorResponse("Database error", 500),
            ApiResponse<User>.SuccessResponse(TestData.SampleUsers[0], "First"),
            ApiResponse<User>.SuccessResponse(TestData.SampleUsers[1], "Second")
        };

        // Act
        var combined = responses.Combine();

        // Assert
        combined.Should().NotBeNull();
        combined.Success.Should().BeFalse();
        combined.Data.Should().BeNull();
        combined.Errors.Should().Contain("Database error");
        combined.StatusCode.Should().Be(500); // First failed response's status code
        combined.Message.Should().Be("Database error");
    }

    /// <summary>
    /// Verifies that Combine with all failed responses returns combined errors.
    /// </summary>
    [Fact]
    public void Combine_WithAllFailedResponses_ReturnsCombinedErrors()
    {
        // Arrange
        var responses = new List<ApiResponse<User>>
        {
            ApiResponse<User>.ErrorResponse(new List<string> { "Error 1", "Error 2" }, 400),
            ApiResponse<User>.ErrorResponse(new List<string> { "Error 3" }, 401)
        };

        // Act
        var combined = responses.Combine();

        // Assert
        combined.Should().NotBeNull();
        combined.Success.Should().BeFalse();
        combined.Data.Should().BeNull();
        combined.Errors.Should().BeEquivalentTo(new List<string> { "Error 1", "Error 2", "Error 3" });
        combined.StatusCode.Should().Be(400); // First response's status code
        combined.Message.Should().Be("Error 1");
    }

    /// <summary>
    /// Verifies that Combine with empty collection returns empty success response.
    /// </summary>
    [Fact]
    public void Combine_WithEmptyCollection_ReturnsEmptySuccessResponse()
    {
        // Arrange
        var responses = new List<ApiResponse<User>>();

        // Act
        var combined = responses.Combine();

        // Assert
        combined.Should().NotBeNull();
        combined.Success.Should().BeTrue();
        combined.Data.Should().NotBeNull();
        combined.Data.Should().BeEmpty();
        combined.Message.Should().Be("Operation completed successfully.");
        combined.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies that Combine throws ArgumentNullException when responses is null.
    /// </summary>
    [Fact]
    public void Combine_WithNullResponses_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<ApiResponse<User>>? responses = null;

        // Act & Assert
        Func<ApiResponse<List<User>>> act = () => responses!.Combine();
        act.Should().Throw<ArgumentNullException>().WithParameterName("responses");
    }

    // ====================================================================
    // GetFirstErrorOrNull Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that GetFirstErrorOrNull returns first error when errors exist.
    /// </summary>
    [Fact]
    public void GetFirstErrorOrNull_WithErrors_ReturnsFirstError()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "First error", "Second error" }
        };

        // Act
        var result = response.GetFirstErrorOrNull();

        // Assert
        result.Should().Be("First error");
    }

    /// <summary>
    /// Verifies that GetFirstErrorOrNull returns null when no errors exist.
    /// </summary>
    [Fact]
    public void GetFirstErrorOrNull_WithoutErrors_ReturnsNull()
    {
        // Arrange
        var response = new ApiResponse<User>();

        // Act
        var result = response.GetFirstErrorOrNull();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetFirstErrorOrNull returns null when errors list is empty.
    /// </summary>
    [Fact]
    public void GetFirstErrorOrNull_WithEmptyErrors_ReturnsNull()
    {
        // Arrange
        var response = new ApiResponse<User> { Errors = new List<string>() };

        // Act
        var result = response.GetFirstErrorOrNull();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetFirstErrorOrNull throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void GetFirstErrorOrNull_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse<User>? response = null;

        // Act & Assert
        Func<string?> act = () => response!.GetFirstErrorOrNull();
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }

    // ====================================================================
    // ContainsError Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that ContainsError returns true when response contains specified error.
    /// </summary>
    [Fact]
    public void ContainsError_WithMatchingError_ReturnsTrue()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Validation failed", "Database timeout" }
        };

        // Act
        var result = response.ContainsError("validation failed"); // Case insensitive

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ContainsError returns true when response contains any of multiple specified errors.
    /// </summary>
    [Fact]
    public void ContainsError_WithMultipleErrorsAndOneMatch_ReturnsTrue()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Not found", "Unauthorized", "Forbidden" }
        };

        // Act
        var result = response.ContainsError("bad request", "not found", "internal server error");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ContainsError returns false when response doesn't contain specified errors.
    /// </summary>
    [Fact]
    public void ContainsError_WithNonMatchingErrors_ReturnsFalse()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Validation failed", "Database timeout" }
        };

        // Act
        var result = response.ContainsError("not found", "unauthorized");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ContainsError returns false when errorMessages array is empty.
    /// </summary>
    [Fact]
    public void ContainsError_WithEmptyErrorMessages_ReturnsFalse()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Some error" }
        };

        // Act
        var result = response.ContainsError();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ContainsError returns false when errorMessages array is null.
    /// </summary>
    [Fact]
    public void ContainsError_WithNullErrorMessages_ReturnsFalse()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Some error" }
        };
        string[]? errorMessages = null;

        // Act
        var result = response.ContainsError(errorMessages!);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ContainsError throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void ContainsError_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse<User>? response = null;

        // Act & Assert
        Func<bool> act = () => response!.ContainsError("test");
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }

    // ====================================================================
    // AddErrors Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that AddErrors adds multiple errors to response.
    /// </>
    [Fact]
    public void AddErrors_WithValidErrors_AddsErrorsToResponse()
    {
        // Arrange
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser, "Initial");
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        response.AddErrors(errors);

        // Assert
        response.Success.Should().BeFalse();
        response.Errors.Should().BeEquivalentTo(errors);
    }

    /// <summary>
    /// Verifies that AddErrors ignores null or whitespace errors.
    /// </summary>
    [Fact]
    public void AddErrors_WithNullAndWhitespaceErrors_IgnoresInvalidErrors()
    {
        // Arrange
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser, "Initial");
        var errors = new List<string> { "Valid error", "", null, "   ", "Another valid error" };

        // Act
        response.AddErrors(errors);

        // Assert
        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(2);
        response.Errors.Should().Contain("Valid error");
        response.Errors.Should().Contain("Another valid error");
    }

    /// <summary>
    /// Verifies that AddErrors throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void AddErrors_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse<User>? response = null;
        var errors = new List<string> { "Error" };

        // Act & Assert
        Action act = () => response!.AddErrors(errors);
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }

    /// <summary>
    /// Verifies that AddErrors throws ArgumentNullException when errors is null.
    /// </summary>
    [Fact]
    public void AddErrors_WithNullErrors_ThrowsArgumentNullException()
    {
        // Arrange
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser);
        IEnumerable<string>? errors = null;

        // Act & Assert
        Action act = () => response.AddErrors(errors!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
    }

    // ====================================================================
    // ToPaginatedResponse Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that ToPaginatedResponse creates correct paginated response from list.
    /// </summary>
    [Fact]
    public void ToPaginatedResponse_WithList_CreatesCorrectPaginatedResponse()
    {
        // Arrange
        var items = TestData.SampleUsers;
        int pageNumber = 2;
        int pageSize = 10;
        string? message = "Users page 2";

        // Act
        var response = items.ToPaginatedResponse(pageNumber, pageSize, message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(items);
        response.PageNumber.Should().Be(pageNumber);
        response.PageSize.Should().Be(pageSize);
        response.TotalRecords.Should().Be(items.Count);
        response.Message.Should().Be(message);
        response.TotalPages.Should().Be(1); // 3 items with page size 10 = 1 page
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that ToPaginatedResponse calculates total pages correctly.
    /// </summary>
    [Theory]
    [InlineData(0, 20, 0)]   // 0 items, 20 per page = 0 pages
    [InlineData(5, 10, 1)]   // 5 items, 10 per page = 1 page
    [InlineData(10, 10, 1)]  // 10 items, 10 per page = 1 page
    [InlineData(15, 10, 2)]  // 15 items, 10 per page = 2 pages
    [InlineData(25, 10, 3)]  // 25 items, 10 per page = 3 pages
    public void ToPaginatedResponse_CalculatesTotalPagesCorrectly(int itemCount, int pageSize, int expectedPages)
    {
        // Arrange
        var items = Enumerable.Range(1, itemCount).Select(i => new User { Id = i }).ToList();

        // Act
        var response = items.ToPaginatedResponse(pageSize: pageSize);

        // Assert
        response.TotalPages.Should().Be(expectedPages);
    }

    /// <summary>
    /// Verifies that ToPaginatedResponse throws ArgumentNullException when items is null.
    /// </summary>
    [Fact]
    public void ToPaginatedResponse_WithNullItems_ThrowsArgumentNullException()
    {
        // Arrange
        List<User>? items = null;

        // Act & Assert
        Func<ApiPaginatedResponse<User>> act = () => items!.ToPaginatedResponse();
        act.Should().Throw<ArgumentNullException>().WithParameterName("items");
    }

    // ====================================================================
    // GetNextPageNumber Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that GetNextPageNumber returns next page when available.
    /// </summary>
    [Fact]
    public void GetNextPageNumber_WithNextPageAvailable_ReturnsNextPage()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act
        var nextPage = response.GetNextPageNumber();

        // Assert
        nextPage.Should().Be(3);
    }

    /// <summary>
    /// Verifies that GetNextPageNumber returns current page when no next page.
    /// </summary>
    [Fact]
    public void GetNextPageNumber_WithoutNextPageAvailable_ReturnsCurrentPage()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act
        var nextPage = response.GetNextPageNumber();

        // Assert
        nextPage.Should().Be(3);
    }

    /// <summary>
    /// Verifies that GetNextPageNumber throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void GetNextPageNumber_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiPaginatedResponse<User>? response = null;

        // Act & Assert
        Func<int> act = () => response!.GetNextPageNumber();
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }

    // ====================================================================
    // GetPreviousPageNumber Method Tests
    // ====================================================================

    /// <summary>
    /// Verifies that GetPreviousPageNumber returns previous page when available.
    /// </summary>
    [Fact]
    public void GetPreviousPageNumber_WithPreviousPageAvailable_ReturnsPreviousPage()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act
        var previousPage = response.GetPreviousPageNumber();

        // Assert
        previousPage.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetPreviousPageNumber returns 1 when on first page.
    /// </summary>
    [Fact]
    public void GetPreviousPageNumber_WhenOnFirstPage_ReturnsOne()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act
        var previousPage = response.GetPreviousPageNumber();

        // Assert
        previousPage.Should().Be(1);
    }

    /// <summary>
    /// Verifies that GetPreviousPageNumber throws ArgumentNullException when response is null.
    /// </summary>
    [Fact]
    public void GetPreviousPageNumber_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiPaginatedResponse<User>? response = null;

        // Act & Assert
        Func<int> act = () => response!.GetPreviousPageNumber();
        act.Should().Throw<ArgumentNullException>().WithParameterName("response");
    }
}