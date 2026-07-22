#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;
using CoolifyCli.Models;

/// <summary>
/// Test suite for the <see cref="ApiResponse{T}"/> and <see cref="ApiPaginatedResponse{T}"/> classes.
/// Tests both success and failure factory paths, and data access on failure.
/// </summary>
public class ApiResponseTests
{
    /// <summary>
    /// Test data for various scenarios
    /// </summary>
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

    /// <summary>
    /// Simple test class for testing generic ApiResponse
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    // ====================================================================
    // ApiResponse<T> Factory Methods Tests
    // ====================================================================

    /// <summary>
    /// Verifies that SuccessResponse creates a successful response with correct properties.
    /// </summary>
    [Fact]
    public void SuccessResponse_WithData_CreatesSuccessfulResponse()
    {
        // Arrange
        var testData = TestData.SampleUser;
        var message = "Operation completed successfully";

        // Act
        var response = ApiResponse<User>.SuccessResponse(testData, message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().BeSameAs(testData);
        response.Message.Should().Be(message);
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
        response.TotalRecords.Should().Be(0);
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that SuccessResponse with null message uses default message.
    /// </summary>
    [Fact]
    public void SuccessResponse_WithNullMessage_UsesDefaultMessage()
    {
        // Act
        var response = ApiResponse<User>.SuccessResponse(TestData.SampleUser, null);

        // Assert
        response.Message.Should().Be("Operation completed successfully.");
    }

    /// <summary>
    /// Verifies that SuccessResponse with null data still creates valid response.
    /// </summary>
    [Fact]
    public void SuccessResponse_WithNullData_CreatesValidResponse()
    {
        // Act
        var response = ApiResponse<User?>.SuccessResponse(null, "Data is null");

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Data is null");
        response.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that ErrorResponse with list of errors creates failed response.
    /// </summary>
    [Fact]
    public void ErrorResponse_WithErrorList_CreatesFailedResponse()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var response = ApiResponse<User>.ErrorResponse(errors, 400);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().BeEquivalentTo(errors);
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("Error 1");
    }

    /// <summary>
    /// Verifies that ErrorResponse with list of errors uses default status code when not specified.
    /// </summary>
    [Fact]
    public void ErrorResponse_WithErrorList_UsesDefaultStatusCode()
    {
        // Arrange
        var errors = new List<string> { "Single error" };

        // Act
        var response = ApiResponse<User>.ErrorResponse(errors);

        // Assert
        response.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Verifies that ErrorResponse with single string creates failed response.
    /// </summary>
    [Fact]
    public void ErrorResponse_WithSingleErrorString_CreatesFailedResponse()
    {
        // Arrange
        var error = "Single error message";

        // Act
        var response = ApiResponse<User>.ErrorResponse(error, 500);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().BeEquivalentTo(new List<string> { error });
        response.StatusCode.Should().Be(500);
        response.Message.Should().Be(error);
    }

    /// <summary>
    /// Verifies that ErrorResponse with single string uses default status code when not specified.
    /// </summary>
    [Fact]
    public void ErrorResponse_WithSingleErrorString_UsesDefaultStatusCode()
    {
        // Arrange
        var error = "Validation failed";

        // Act
        var response = ApiResponse<User>.ErrorResponse(error);

        // Assert
        response.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Verifies that ErrorResponse with empty error list creates failed response.
    /// </summary>
    [Fact]
    public void ErrorResponse_WithEmptyErrorList_CreatesFailedResponse()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var response = ApiResponse<User>.ErrorResponse(errors);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().BeEmpty();
        response.Message.Should().Be("An error occurred.");
    }

    // ====================================================================
    // ApiResponse<T> Instance Methods Tests
    // ====================================================================

    /// <summary>
    /// Verifies that AddError adds error to errors list and sets Success to false.
    /// </summary>
    [Fact]
    public void AddError_AddsErrorAndSetsSuccessToFalse()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Success = true,
            Data = TestData.SampleUser,
            Errors = new List<string> { "Existing error" }
        };

        // Act
        response.AddError("New error");

        // Assert
        response.Errors.Should().HaveCount(2);
        response.Errors.Should().Contain("Existing error");
        response.Errors.Should().Contain("New error");
        response.Success.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasErrors returns true when errors list is not empty.
    /// </summary>
    [Fact]
    public void HasErrors_WithErrors_ReturnsTrue()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "Error 1", "Error 2" }
        };

        // Act & Assert
        response.HasErrors().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasErrors returns false when errors list is empty.
    /// </summary>
    [Fact]
    public void HasErrors_WithoutErrors_ReturnsFalse()
    {
        // Arrange
        var response = new ApiResponse<User>();

        // Act & Assert
        response.HasErrors().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetFirstError returns first error message.
    /// </summary>
    [Fact]
    public void GetFirstError_WithMultipleErrors_ReturnsFirstError()
    {
        // Arrange
        var response = new ApiResponse<User>
        {
            Errors = new List<string> { "First error", "Second error", "Third error" }
        };

        // Act & Assert
        response.GetFirstError().Should().Be("First error");
    }

    /// <summary>
    /// Verifies that GetFirstError returns empty string when no errors exist.
    /// </summary>
    [Fact]
    public void GetFirstError_WithoutErrors_ReturnsEmptyString()
    {
        // Arrange
        var response = new ApiResponse<User>();

        // Act & Assert
        response.GetFirstError().Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetFirstError returns empty string when errors list is empty.
    /// </summary>
    [Fact]
    public void GetFirstError_WithEmptyErrors_ReturnsEmptyString()
    {
        // Arrange
        var response = new ApiResponse<User> { Errors = new List<string>() };

        // Act & Assert
        response.GetFirstError().Should().BeEmpty();
    }

    // ====================================================================
    // ApiPaginatedResponse<T> Tests
    // ====================================================================

    /// <summary>
    /// Verifies that ApiPaginatedResponse default values are correct.
    /// </summary>
    [Fact]
    public void ApiPaginatedResponse_DefaultValues_AreCorrect()
    {
        // Act
        var response = new ApiPaginatedResponse<User>();

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull().And.BeEmpty();
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(20);
        response.TotalRecords.Should().Be(0);
        response.TotalPages.Should().Be(0);
        response.Message.Should().BeNull();
        response.Errors.Should().NotBeNull().And.BeEmpty();
        response.StatusCode.Should().Be(200);
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that CalculateTotalPages correctly calculates total pages.
    /// </summary>
    [Theory]
    [InlineData(10, 5, 2)]  // 10 items, 5 per page = 2 pages
    [InlineData(25, 10, 3)] // 25 items, 10 per page = 3 pages
    [InlineData(100, 20, 5)] // 100 items, 20 per page = 5 pages
    [InlineData(19, 20, 1)] // 19 items, 20 per page = 1 page
    [InlineData(21, 20, 2)] // 21 items, 20 per page = 2 pages
    public void CalculateTotalPages_CalculatesCorrectTotalPages(int totalRecords, int pageSize, int expectedPages)
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            TotalRecords = totalRecords,
            PageSize = pageSize
        };

        // Act
        response.CalculateTotalPages();

        // Assert
        response.TotalPages.Should().Be(expectedPages);
    }

    /// <summary>
    /// Verifies that HasNextPage returns true when there are more pages.
    /// </summary>
    [Fact]
    public void HasNextPage_WithMorePages_ReturnsTrue()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act & Assert
        response.HasNextPage().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasNextPage returns false when on last page.
    /// </summary>
    [Fact]
    public void HasNextPage_WithoutMorePages_ReturnsFalse()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalRecords = 25,
            TotalPages = 3
        };

        // Act & Assert
        response.HasNextPage().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsFirstPage returns true when on first page.
    /// </summary>
    [Fact]
    public void IsFirstPage_WhenOnFirstPage_ReturnsTrue()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User> { PageNumber = 1 };

        // Act & Assert
        response.IsFirstPage().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsFirstPage returns false when not on first page.
    /// </summary>
    [Fact]
    public void IsFirstPage_WhenNotOnFirstPage_ReturnsFalse()
    {
        // Arrange
        var response = new ApiPaginatedResponse<User> { PageNumber = 2 };

        // Act & Assert
        response.IsFirstPage().Should().BeFalse();
    }
}
