# Testing Guide

Comprehensive guide for writing, running, and maintaining tests for Coolify CLI.

## Test Structure

Tests are organized by type and location:

```
coolify-cli/
├── Services.Tests/
│   ├── ApplicationServiceTests.cs
│   ├── DatabaseServiceTests.cs
│   └── DeploymentOrchestratorTests.cs
├── Integration.Tests/
│   ├── ApiClientTests.cs
│   └── DeploymentTests.cs
└── Performance.Tests/
    └── PerformanceTests.cs
```

## Running Tests

### Quick Start

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ApplicationServiceTests"

# Run with detailed output
dotnet test --verbosity detailed

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

### Using Test Script

```bash
# Run all tests
./scripts/test.sh all

# Run unit tests only
./scripts/test.sh unit

# Run with coverage
./scripts/test.sh coverage

# Run specific test class
./scripts/test.sh class ApplicationServiceTests

# Check code quality
./scripts/test.sh quality

# Run performance tests
./scripts/test.sh performance

# Generate test report
./scripts/test.sh report

# List available tests
./scripts/test.sh list
```

## Writing Tests

### Unit Test Example

```csharp
[TestClass]
public class ApplicationServiceTests
{
    private Mock<IApplicationRepository> _mockRepository;
    private Mock<ILogger> _mockLogger;
    private ApplicationService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IApplicationRepository>();
        _mockLogger = new Mock<ILogger>();
        _service = new ApplicationService(_mockRepository.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetApplicationAsync_WithValidId_ReturnsApplication()
    {
        // Arrange
        var applicationId = 1;
        var expectedApp = new Application { Id = applicationId, Name = "TestApp" };
        _mockRepository.Setup(r => r.GetByIdAsync(applicationId))
            .ReturnsAsync(expectedApp);

        // Act
        var result = await _service.GetApplicationAsync(applicationId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedApp.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(applicationId), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(CoolifyException))]
    public async Task GetApplicationAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var applicationId = -1;

        // Act
        await _service.GetApplicationAsync(applicationId);
    }
}
```

### Integration Test Example

```csharp
[TestClass]
[TestCategory("Integration")]
public class ApiClientTests
{
    private CoolifyApiClient _apiClient;

    [TestInitialize]
    public void Setup()
    {
        var config = new CoolifyConfiguration
        {
            ApiUrl = "http://localhost:3000",
            ApiKey = "test-key"
        };
        _apiClient = new CoolifyApiClient(config);
    }

    [TestMethod]
    public async Task GetApplications_WithValidKey_ReturnsApplications()
    {
        // Arrange & Act
        var applications = await _apiClient.GetApplicationsAsync();

        // Assert
        Assert.IsNotNull(applications);
        Assert.IsTrue(applications.Count > 0);
    }
}
```

### Performance Test Example

```csharp
[TestClass]
[TestCategory("Performance")]
public class PerformanceTests
{
    [TestMethod]
    public void ListApplications_ShouldCompleteInUnder500ms()
    {
        // Arrange
        var service = new ApplicationService(/* dependencies */);
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var applications = service.ListApplications();
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }
}
```

## Test Categories

### Unit Tests

**Purpose**: Test individual components in isolation

**Location**: `Services.Tests/`, `Models.Tests/`

**Characteristics**:
- Fast execution
- No external dependencies
- Mock all dependencies
- Single responsibility per test

**Examples**:
- Service method logic
- Utility function behavior
- Model validation

### Integration Tests

**Purpose**: Test component interactions

**Location**: `Integration.Tests/`

**Characteristics**:
- Slower than unit tests
- Use real or realistic dependencies
- Test end-to-end workflows
- Database/API interaction

**Examples**:
- API client communication
- Deployment workflows
- Database operations

### Performance Tests

**Purpose**: Verify performance characteristics

**Location**: `Performance.Tests/`

**Characteristics**:
- Measure execution time
- Test under load
- Verify resource usage
- Establish baselines

**Examples**:
- Deployment speed
- Memory usage
- Response time

### End-to-End Tests

**Purpose**: Test complete scenarios

**Characteristics**:
- Real Coolify instance
- Full deployment workflow
- Production-like environment
- Slow but comprehensive

## Test Best Practices

### 1. Naming Conventions

```csharp
// Format: MethodName_Scenario_ExpectedResult
[TestMethod]
public void DeployApplication_WithValidId_ReturnsSuccessfulDeployment()
{
    // Test implementation
}
```

### 2. Arrange-Act-Assert Pattern

```csharp
[TestMethod]
public async Task HealthCheck_WhenApiIsHealthy_ReturnsTrue()
{
    // Arrange
    var service = new HealthCheckService(/* mocks */);
    _mockApiClient.Setup(a => a.PingAsync()).ReturnsAsync(true);

    // Act
    var result = await service.CheckAsync();

    // Assert
    Assert.IsTrue(result);
}
```

### 3. One Assertion Per Test (Prefer)

```csharp
// Good
[TestMethod]
public void ServiceInitialization_ShouldSetProperties()
{
    var service = new MyService("test");
    Assert.AreEqual("test", service.Name);
}

[TestMethod]
public void ServiceInitialization_ShouldEnableLogging()
{
    var service = new MyService();
    Assert.IsTrue(service.LoggingEnabled);
}

// Avoid
[TestMethod]
public void ServiceInitialization_ShouldSetPropertiesAndEnableLogging()
{
    var service = new MyService("test");
    Assert.AreEqual("test", service.Name);  // Multiple assertions
    Assert.IsTrue(service.LoggingEnabled);
}
```

### 4. Use Mocking Effectively

```csharp
[TestInitialize]
public void Setup()
{
    _mockRepository = new Mock<IApplicationRepository>();
    
    // Setup default behavior
    _mockRepository
        .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
        .ReturnsAsync((int id) => new Application { Id = id });
}

[TestMethod]
public async Task DeployApplication_WithValidId_InvokesRepository()
{
    // Act
    await _service.DeployAsync(1);

    // Assert
    _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
}
```

### 5. Test Exceptions Properly

```csharp
[TestMethod]
[ExpectedException(typeof(CoolifyException))]
public async Task GetApplication_WithInvalidId_ThrowsException()
{
    await _service.GetApplicationAsync(-1);
}

// Or using try-catch
[TestMethod]
public async Task GetApplication_WithInvalidId_ThrowsCorrectException()
{
    try
    {
        await _service.GetApplicationAsync(-1);
        Assert.Fail("Expected CoolifyException");
    }
    catch (CoolifyException ex)
    {
        Assert.AreEqual(ErrorCode.InvalidId, ex.ErrorCode);
    }
}
```

## Code Coverage

### Generate Coverage Report

```bash
./scripts/test.sh coverage
```

### Coverage Targets

| Component | Target |
|-----------|--------|
| Services | 80%+ |
| Models | 70%+ |
| Utilities | 75%+ |
| Controllers | 70%+ |

### View Coverage Report

```bash
# Open HTML report
open coverage/report/index.html  # macOS
xdg-open coverage/report/index.html  # Linux
start coverage/report/index.html  # Windows
```

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Push to main/develop branches
- Pull requests
- Scheduled nightly runs

**Test Matrix**:
- Ubuntu, macOS, Windows
- .NET 10.x

### Local CI Simulation

```bash
# Run exact CI tests locally
dotnet test --configuration Release --verbosity detailed
dotnet format --verify-no-changes
dotnet list package --vulnerable
```

## Troubleshooting Tests

### Tests Timeout

```csharp
[TestMethod]
[Timeout(5000)]  // 5 seconds
public async Task LongRunningTest()
{
    // Test implementation
}
```

### Async Test Issues

```csharp
// Correct async test
[TestMethod]
public async Task AsyncOperation_ReturnsExpectedValue()
{
    var result = await _service.PerformAsync();
    Assert.IsNotNull(result);
}

// Avoid
[TestMethod]
public void AsyncOperation_ReturnsExpectedValue()
{
    var result = _service.PerformAsync().Result;  // Can deadlock
    Assert.IsNotNull(result);
}
```

### Test Isolation Issues

```csharp
[TestClass]
public class MyTests
{
    [TestInitialize]  // Runs before each test
    public void Setup()
    {
        // Initialize test state
    }

    [TestCleanup]  // Runs after each test
    public void Cleanup()
    {
        // Clean up resources
    }

    [ClassInitialize]  // Runs once per class
    public static void ClassSetup(TestContext context)
    {
        // Class-level setup
    }

    [ClassCleanup]  // Runs once per class
    public static void ClassCleanup()
    {
        // Class-level cleanup
    }
}
```

## Test Data Management

### Using Test Fixtures

```csharp
public class ApplicationTestData
{
    public static Application GetTestApplication()
    {
        return new Application
        {
            Id = 1,
            Name = "TestApp",
            Status = "running",
            Environment = "test"
        };
    }

    public static List<Application> GetTestApplications(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Application { Id = i, Name = $"App{i}" })
            .ToList();
    }
}

// Usage in tests
[TestMethod]
public void ValidateApplication_WithTestData_Succeeds()
{
    var app = ApplicationTestData.GetTestApplication();
    Assert.IsTrue(_validator.IsValid(app));
}
```

## Performance Benchmarking

```csharp
[SimpleJob(warmupCount: 3, targetCount: 5)]
[MemoryDiagnoser]
public class DeploymentBenchmarks
{
    private DeploymentService _service;

    [GlobalSetup]
    public void Setup()
    {
        _service = new DeploymentService(/* dependencies */);
    }

    [Benchmark]
    public async Task DeployApplication()
    {
        await _service.DeployAsync(1);
    }
}

// Run with: dotnet run -c Release
```

## Resources

- [Microsoft Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development)
