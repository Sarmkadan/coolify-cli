using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using CoolifyCli.Services;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the EnvironmentVariableService class.
/// </summary>
public class EnvironmentVariableServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly EnvironmentVariableService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentVariableServiceTests"/> class.
    /// </summary>
    public EnvironmentVariableServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.coolify.io") };

        var apiClient = new CoolifyApiClient(_httpClient, "https://api.coolify.io", "test-key");
        _service = new EnvironmentVariableService(apiClient, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that GetApplicationVariablesAsync returns environment variables when the API returns success.
    /// </summary>
    [Fact]
    public async Task GetApplicationVariablesAsync_ShouldReturnEnvironmentVariables_WhenApiReturnsSuccess()
    {
        // Arrange
        var applicationId = "app-123";
        var variables = new List<EnvironmentVariable> { new EnvironmentVariable { Key = "MY_VAR", Value = "test-value", ApplicationId = applicationId } };
        var json = JsonSerializer.Serialize(variables);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.GetApplicationVariablesAsync(applicationId);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data![0].Key.Should().Be("MY_VAR");
        result.Data![0].Value.Should().Be("test-value");
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetApplicationVariablesAsync returns error when application ID is null.
    /// </summary>
    [Fact]
    public async Task GetApplicationVariablesAsync_ShouldReturnError_WhenApplicationIdIsNull()
    {
        // Arrange
        string? nullApplicationId = null;

        // Act
        var result = await _service.GetApplicationVariablesAsync(nullApplicationId);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetApplicationVariablesAsync returns error when application ID is empty.
    /// </summary>
    [Fact]
    public async Task GetApplicationVariablesAsync_ShouldReturnError_WhenApplicationIdIsEmpty()
    {
        // Arrange
        string emptyApplicationId = "";

        // Act
        var result = await _service.GetApplicationVariablesAsync(emptyApplicationId);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetApplicationVariablesAsync returns error when application ID is whitespace.
    /// </summary>
    [Fact]
    public async Task GetApplicationVariablesAsync_ShouldReturnError_WhenApplicationIdIsWhitespace()
    {
        // Arrange
        string whitespaceApplicationId = "   ";

        // Act
        var result = await _service.GetApplicationVariablesAsync(whitespaceApplicationId);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetApplicationVariablesAsync returns error when API returns error.
    /// </summary>
    [Fact]
    public async Task GetApplicationVariablesAsync_ShouldReturnError_WhenApiReturnsError()
    {
        // Arrange
        var applicationId = "app-123";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal server error")
            });

        // Act
        var result = await _service.GetApplicationVariablesAsync(applicationId);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(500);
    }

    /// <summary>
    /// Tests that GetVariableAsync returns environment variable when the API returns success.
    /// </summary>
    [Fact]
    public async Task GetVariableAsync_ShouldReturnEnvironmentVariable_WhenApiReturnsSuccess()
    {
        // Arrange
        var variableId = 42;
        var variable = new EnvironmentVariable { Id = variableId, Key = "SECRET_KEY", Value = "secret-value", ApplicationId = "app-123" };
        var json = JsonSerializer.Serialize(variable);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.GetVariableAsync(variableId);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(variableId);
        result.Data.Key.Should().Be("SECRET_KEY");
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetVariableAsync throws ArgumentOutOfRangeException when variable ID is not positive.
    /// </summary>
    [Fact]
    public async Task GetVariableAsync_ShouldThrow_WhenVariableIdIsNotPositive()
    {
        // Arrange
        int invalidVariableId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.GetVariableAsync(invalidVariableId));
    }

    /// <summary>
    /// Tests that CreateVariableAsync returns created variable when validation passes.
    /// </summary>
    [Fact]
    public async Task CreateVariableAsync_ShouldReturnCreatedVariable_WhenValidationPasses()
    {
        // Arrange
        var applicationId = "app-123";
        var variable = new EnvironmentVariable { Key = "NEW_VAR", Value = "new-value", ApplicationId = applicationId };
        var createdVariable = new EnvironmentVariable { Id = 1, Key = variable.Key, Value = variable.Value, ApplicationId = applicationId };
        var json = JsonSerializer.Serialize(createdVariable);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.CreateVariableAsync(applicationId, variable);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CreateVariableAsync returns error when application ID is null.
    /// </summary>
    [Fact]
    public async Task CreateVariableAsync_ShouldReturnError_WhenApplicationIdIsNull()
    {
        // Arrange
        string? nullApplicationId = null;
        var variable = new EnvironmentVariable { Key = "NEW_VAR", Value = "new-value" };

        // Act
        var result = await _service.CreateVariableAsync(nullApplicationId, variable);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that CreateVariableAsync returns error when variable validation fails.
    /// </summary>
    [Fact]
    public async Task CreateVariableAsync_ShouldReturnError_WhenVariableValidationFails()
    {
        // Arrange
        var applicationId = "app-123";
        var variable = new EnvironmentVariable { Key = "invalid-key!", Value = "val", ApplicationId = applicationId };

        // Act
        var result = await _service.CreateVariableAsync(applicationId, variable);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that UpdateVariableAsync returns updated variable when validation passes.
    /// </summary>
    [Fact]
    public async Task UpdateVariableAsync_ShouldReturnUpdatedVariable_WhenValidationPasses()
    {
        // Arrange
        var variableId = 42;
        var variable = new EnvironmentVariable { Key = "UPDATED_VAR", Value = "updated-value", ApplicationId = "app-123" };
        var updatedVariable = new EnvironmentVariable { Id = variableId, Key = variable.Key, Value = variable.Value, ApplicationId = "app-123" };
        var json = JsonSerializer.Serialize(updatedVariable);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.UpdateVariableAsync(variableId, variable);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(variableId);
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that UpdateVariableAsync returns error when variable validation fails.
    /// </summary>
    [Fact]
    public async Task UpdateVariableAsync_ShouldReturnError_WhenVariableValidationFails()
    {
        // Arrange
        var variableId = 42;
        var variable = new EnvironmentVariable { Key = "invalid-key!", Value = "val", ApplicationId = "app-123" };

        // Act
        var result = await _service.UpdateVariableAsync(variableId, variable);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that DeleteVariableAsync returns success when API returns success.
    /// </summary>
    [Fact]
    public async Task DeleteVariableAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var variableId = 42;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.DeleteVariableAsync(variableId);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that DeleteVariableAsync throws ArgumentOutOfRangeException when variable ID is not positive.
    /// </summary>
    [Fact]
    public async Task DeleteVariableAsync_ShouldThrow_WhenVariableIdIsNotPositive()
    {
        // Arrange
        int invalidVariableId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.DeleteVariableAsync(invalidVariableId));
    }

    /// <summary>
    /// Tests that BulkUpdateVariablesAsync returns success when validation passes.
    /// </summary>
    [Fact]
    public async Task BulkUpdateVariablesAsync_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var applicationId = "app-123";
        var variables = new List<EnvironmentVariable> {
            new EnvironmentVariable { Key = "VAR1", Value = "value1", ApplicationId = applicationId },
            new EnvironmentVariable { Key = "VAR2", Value = "value2", ApplicationId = applicationId }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.BulkUpdateVariablesAsync(applicationId, variables);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that BulkUpdateVariablesAsync returns error when application ID is null.
    /// </summary>
    [Fact]
    public async Task BulkUpdateVariablesAsync_ShouldReturnError_WhenApplicationIdIsNull()
    {
        // Arrange
        string? nullApplicationId = null;
        var variables = new List<EnvironmentVariable> { new EnvironmentVariable { Key = "VAR1", Value = "value1" } };

        // Act
        var result = await _service.BulkUpdateVariablesAsync(nullApplicationId, variables);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that BulkUpdateVariablesAsync returns error when variables list is null.
    /// </summary>
    [Fact]
    public async Task BulkUpdateVariablesAsync_ShouldReturnError_WhenVariablesListIsNull()
    {
        // Arrange
        var applicationId = "app-123";
        List<EnvironmentVariable>? nullVariables = null;

        // Act
        var result = await _service.BulkUpdateVariablesAsync(applicationId, nullVariables);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that BulkUpdateVariablesAsync returns error when variables list is empty.
    /// </summary>
    [Fact]
    public async Task BulkUpdateVariablesAsync_ShouldReturnError_WhenVariablesListIsEmpty()
    {
        // Arrange
        var applicationId = "app-123";
        var emptyVariables = new List<EnvironmentVariable>();

        // Act
        var result = await _service.BulkUpdateVariablesAsync(applicationId, emptyVariables);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetVariablesByScopeAsync returns variables when the API returns success.
    /// </summary>
    [Fact]
    public async Task GetVariablesByScopeAsync_ShouldReturnVariables_WhenApiReturnsSuccess()
    {
        // Arrange
        var applicationId = "app-123";
        var scope = "production";
        var variables = new List<EnvironmentVariable> { new EnvironmentVariable { Key = "PROD_VAR", Value = "prod-value", ApplicationId = applicationId, EnvironmentScope = scope } };
        var json = JsonSerializer.Serialize(variables);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.GetVariablesByScopeAsync(applicationId, scope);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data![0].EnvironmentScope.Should().Be(scope);
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetVariablesByScopeAsync returns error when application ID is null.
    /// </summary>
    [Fact]
    public async Task GetVariablesByScopeAsync_ShouldReturnError_WhenApplicationIdIsNull()
    {
        // Arrange
        string? nullApplicationId = null;
        var scope = "production";

        // Act
        var result = await _service.GetVariablesByScopeAsync(nullApplicationId, scope);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetVariablesByScopeAsync returns error when scope is null.
    /// </summary>
    [Fact]
    public async Task GetVariablesByScopeAsync_ShouldReturnError_WhenScopeIsNull()
    {
        // Arrange
        var applicationId = "app-123";
        string? nullScope = null;

        // Act
        var result = await _service.GetVariablesByScopeAsync(applicationId, nullScope);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that RotateSecretsAsync returns success when API returns success.
    /// </summary>
    [Fact]
    public async Task RotateSecretsAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var applicationId = "app-123";
        var variableIds = new List<int> { 1, 2, 3 };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.RotateSecretsAsync(applicationId, variableIds);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that RotateSecretsAsync throws ArgumentException when application ID is null.
    /// </summary>
    [Fact]
    public async Task RotateSecretsAsync_ShouldThrow_WhenApplicationIdIsNull()
    {
        // Arrange
        string? nullApplicationId = null;
        var variableIds = new List<int> { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RotateSecretsAsync(nullApplicationId, variableIds));
    }

    /// <summary>
    /// Tests that RotateSecretsAsync throws ArgumentException when variable IDs list is null.
    /// </summary>
    [Fact]
    public async Task RotateSecretsAsync_ShouldThrow_WhenVariableIdsIsNull()
    {
        // Arrange
        var applicationId = "app-123";
        List<int>? nullVariableIds = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RotateSecretsAsync(applicationId, nullVariableIds));
    }

    /// <summary>
    /// Tests that ValidateVariablesAsync returns success when API returns success.
    /// </summary>
    [Fact]
    public async Task ValidateVariablesAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var applicationId = "app-123";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.ValidateVariablesAsync(applicationId);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetChangeHistoryAsync returns change history when API returns success.
    /// </summary>
    [Fact]
    public async Task GetChangeHistoryAsync_ShouldReturnChangeHistory_WhenApiReturnsSuccess()
    {
        // Arrange
        var variableId = 42;
        var limit = 10;
        var changes = new List<object> { new { Id = 1, Action = "create", Timestamp = DateTime.UtcNow } };
        var json = JsonSerializer.Serialize(changes);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.GetChangeHistoryAsync(variableId, limit);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetChangeHistoryAsync returns error when limit is less than 1.
    /// </summary>
    [Fact]
    public async Task GetChangeHistoryAsync_ShouldReturnError_WhenLimitIsLessThan1()
    {
        // Arrange
        var variableId = 42;
        int invalidLimit = 0;

        // Act
        var result = await _service.GetChangeHistoryAsync(variableId, invalidLimit);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Tests that GetChangeHistoryAsync returns error when limit is greater than 100.
    /// </summary>
    [Fact]
    public async Task GetChangeHistoryAsync_ShouldReturnError_WhenLimitIsGreaterThan100()
    {
        // Arrange
        var variableId = 42;
        int invalidLimit = 101;

        // Act
        var result = await _service.GetChangeHistoryAsync(variableId, invalidLimit);

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}
