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
/// Tests for the ApplicationService class.
/// </summary>
public class ApplicationServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ApplicationService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationServiceTests"/> class.
    /// </summary>
    public ApplicationServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        // baseUrl and apiKey are required by CoolifyApiClient constructor
        var apiClient = new CoolifyApiClient(_httpClient, "https://api.coolify.io", "test-key");
        _service = new ApplicationService(apiClient, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that GetAllApplicationsAsync returns applications when the API returns success.
    /// </summary>
    [Fact]
    public async Task GetAllApplicationsAsync_ShouldReturnApplications_WhenApiReturnsSuccess()
    {
        // Arrange
        var applications = new List<ApplicationDeployment> { new ApplicationDeployment { Id = 1, Name = "app1" } };
        var json = JsonSerializer.Serialize(applications);
        
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
        var result = await _service.GetAllApplicationsAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data![0].Name.Should().Be("app1");
    }

    /// <summary>
    /// Tests that GetApplicationAsync returns an application when the API returns success.
    /// </summary>
    [Fact]
    public async Task GetApplicationAsync_ShouldReturnApplication_WhenApiReturnsSuccess()
    {
        // Arrange
        var application = new ApplicationDeployment { Id = 1, Name = "app1" };
        var json = JsonSerializer.Serialize(application);
        
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
        var result = await _service.GetApplicationAsync(1);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("app1");
    }
}
