// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Guide

Deep dive into Coolify CLI's design, patterns, and infrastructure.

## Design Principles

1. **Separation of Concerns**: Each layer has a single responsibility
2. **Dependency Injection**: Loose coupling through interfaces
3. **Repository Pattern**: Abstract data access implementation
4. **SOLID Principles**: Code is maintainable, testable, and extensible
5. **Fail-Fast**: Validate early, error loudly
6. **Performance**: Optimize for common use cases

## Layered Architecture

### Layer 1: Presentation (Program.cs)

Entry point for the CLI. Uses System.CommandLine for command parsing and routing.

**Responsibilities**:
- Parse command-line arguments
- Invoke appropriate command handlers
- Format and display output to user
- Handle exit codes

**Key Components**:
- Root command definition
- Command hierarchy (app, db, logs)
- Global options (--verbose, --help)
- Output formatting selection

```
User Input (ARGV)
       ↓
   Program.cs
       ↓
System.CommandLine Parser
       ↓
Command Handler Invocation
```

### Layer 2: Command Processing (Commands/)

Implements CLI commands with business logic.

**Directories**:
- `CommandBase.cs`: Base class for all commands
- `AdvancedAppCommands.cs`: Complex app operations
- `DatabaseManagementCommands.cs`: Database management
- `MonitoringCommands.cs`: Health and monitoring

**Design Pattern**: Command Pattern

Each command encapsulates a request as an object with:
- Execution context
- Parameter validation
- Error handling
- Result formatting

### Layer 3: Service Layer (Services/)

Orchestrates business logic and cross-cutting concerns.

**Core Services**:

1. **CoolifyApiClient** - HTTP communication
   - Manages HttpClient
   - Handles authentication headers
   - Implements retry logic
   - Manages timeouts

2. **ApplicationService** - Application operations
   - List/get applications
   - Manage environment variables
   - Deploy applications
   - Check deployment status

3. **DatabaseService** - Database operations
   - List databases
   - Health checks
   - Backup management
   - Configuration

4. **LogService** - Log retrieval
   - Stream logs
   - Filter logs
   - Parse log entries
   - Format output

5. **HealthCheckService** - System monitoring
   - API connectivity
   - System health
   - Performance metrics
   - Alert generation

6. **DeploymentOrchestrator** - Deployment workflows
   - Pre-deployment validation
   - Strategy selection
   - Progress tracking
   - Rollback handling

7. **EnvironmentVariableService** - Configuration management
   - Encrypt/decrypt secrets
   - Variable scope management
   - Validation rules

**Pattern**: Service Locator with Dependency Injection

### Layer 4: Middleware (Middleware/)

Cross-cutting concerns applied to all operations.

**Middleware Chain**:
```
Request
   ↓
AuthenticationMiddleware (API key validation)
   ↓
RateLimitingMiddleware (Quota enforcement)
   ↓
LoggingMiddleware (Request/response logging)
   ↓
ErrorHandlingMiddleware (Exception handling)
   ↓
Service Execution
   ↓
Response
```

**Implementations**:

1. **AuthenticationMiddleware**
   - Validates API key presence
   - Injects auth headers
   - Handles token refresh

2. **RateLimitingMiddleware**
   - Enforces API rate limits
   - Implements exponential backoff
   - Queues requests

3. **LoggingMiddleware**
   - Logs request/response
   - Tracks performance
   - Records errors

4. **ErrorHandlingMiddleware**
   - Catches exceptions
   - Formats error messages
   - Returns appropriate exit codes

### Layer 5: Data Access (Data/)

Repository pattern for data operations.

**Interfaces**:
```csharp
IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

**Implementations**:
- `ApplicationRepository`: Application data operations
- `DatabaseRepository`: Database data operations

**Benefits**:
- Testable with mock repositories
- Cacheable results
- Queryable results
- Transaction support

### Layer 6: Infrastructure (Infrastructure/)

Configuration, exceptions, and constants.

**Components**:

1. **CoolifyConfiguration**
   - Reads environment variables
   - Validates settings
   - Provides defaults
   - Supports .env files

2. **CoolifyExceptions**
   - Custom exception hierarchy
   - Semantic error types
   - Exit code mapping

3. **Constants**
   - Application version
   - Author information
   - Exit codes
   - API endpoints

### Layer 7: Supporting Components

**Caching (Caching/)**
- Memory-based response cache
- TTL support
- Invalidation patterns

**Formatters (Formatters/)**
- Table output formatting
- JSON serialization
- CSV export
- Plain text formatting

**Extensions (Extensions/)**
- String manipulation
- DateTime formatting
- Enum utilities
- Collection helpers

**Integration (Integration/)**
- HTTP client factory
- Webhook handling
- External API integration

## Data Flow

### Application Deployment Flow

```
User Command
    ↓
CLI Parser (Program.cs)
    ↓
DeployCommand Handler
    ↓
ApplicationService.DeployApplicationAsync()
    ↓
DeploymentOrchestrator
    ├→ Pre-flight validation
    ├→ Health checks
    ├→ Strategy selection
    └→ Execute deployment
    ↓
CoolifyApiClient (HTTP POST)
    ↓
Coolify API Server
    ↓
Deployment Execution
    ↓
Health Verification
    ↓
Response to CLI
    ↓
User Output
```

### Log Streaming Flow

```
User: logs 1 --follow
    ↓
LogService.StreamLogsAsync()
    ↓
CoolifyApiClient (WebSocket/SSE)
    ↓
API Log Stream
    ↓
Log Entry Parsing
    ↓
Console Output with Formatting
    ↓
Stream continues until Ctrl+C
```

## Design Patterns

### 1. Repository Pattern

Abstracts data access:

```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
}

public class ApplicationRepository : IRepository<Application>
{
    private readonly CoolifyApiClient _client;
    
    public async Task<Application> GetByIdAsync(int id)
    {
        return await _client.GetAsync<Application>($"/apps/{id}");
    }
}
```

### 2. Service Locator

Centralized service access:

```csharp
var appService = new ApplicationService(apiClient, logger);
var dbService = new DatabaseService(apiClient, logger);
var logService = new LogService(apiClient, logger);
```

### 3. Strategy Pattern

Different deployment strategies:

```csharp
public interface IDeploymentStrategy
{
    Task<DeploymentResult> ExecuteAsync(Application app);
}

public class BlueGreenDeployment : IDeploymentStrategy { }
public class CanaryDeployment : IDeploymentStrategy { }
public class RollingDeployment : IDeploymentStrategy { }
```

### 4. Factory Pattern

Create HTTP clients consistently:

```csharp
public class HttpClientFactory
{
    public static HttpClient CreateClient(CoolifyConfiguration config)
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        return client;
    }
}
```

### 5. Middleware Chain

Apply cross-cutting concerns:

```csharp
public interface ICommandMiddleware
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
}

public class AuthenticationMiddleware : ICommandMiddleware
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        // Validate auth
        // Execute operation
        // Handle errors
    }
}
```

## Error Handling

### Exception Hierarchy

```
Exception
└── CoolifyException (Base)
    ├── ConfigurationException
    ├── ApiCommunicationException
    ├── ApiException
    ├── DeploymentException
    └── ValidationException
```

### Error Recovery

1. **Validation Errors**: Fail immediately with clear message
2. **Transient Errors**: Retry with exponential backoff
3. **Authentication Errors**: Prompt for new credentials
4. **API Errors**: Display API error message and suggest fixes
5. **Network Errors**: Retry or suggest connectivity check

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Configuration error |
| 3 | API communication error |
| 4 | Deployment error |
| 5 | Validation error |

## Performance Optimization

### 1. Response Caching

Cache GET requests with TTL:

```csharp
public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    
    public async Task<T> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan ttl)
    {
        if (_cache.TryGetValue(key, out T? cached))
            return cached;
            
        var value = await factory();
        _cache.Set(key, value, ttl);
        return value;
    }
}
```

### 2. Batch Operations

Process multiple items efficiently:

```csharp
// Instead of:
foreach (var id in ids)
    await GetApplicationAsync(id);  // N requests

// Use:
var apps = await GetApplicationsBatchAsync(ids);  // 1 request
```

### 3. Connection Pooling

HTTP client reuse:

```csharp
private static readonly HttpClient _httpClient = new();

public CoolifyApiClient(HttpClient httpClient)
{
    _httpClient = httpClient;  // Reuse across requests
}
```

### 4. Lazy Loading

Load data only when needed:

```csharp
public class Application
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Only loaded when accessed
    public Lazy<Task<List<LogEntry>>> Logs { get; set; }
}
```

## Security Considerations

### 1. API Key Management

- Never log API keys
- Support environment variable and .env file
- Validate key format on startup
- Implement key rotation support

### 2. HTTPS Enforcement

- Require HTTPS for API communication
- Validate SSL certificates
- Support certificate pinning

### 3. Secret Handling

- Encrypt secrets in transit
- Never expose in logs
- Support secret masking in debug output
- Implement secure key storage

### 4. Input Validation

- Validate all CLI arguments
- Sanitize database queries
- Prevent command injection

## Testing Strategy

### Unit Tests

Test individual services in isolation:

```csharp
[TestClass]
public class ApplicationServiceTests
{
    [TestMethod]
    public async Task GetApplicationAsync_WithValidId_ReturnsApplication()
    {
        // Arrange
        var mockClient = new Mock<ICoolifyApiClient>();
        var service = new ApplicationService(mockClient.Object, logger);
        
        // Act
        var result = await service.GetApplicationAsync(1);
        
        // Assert
        Assert.IsNotNull(result);
    }
}
```

### Integration Tests

Test end-to-end workflows with test Coolify instance.

### Performance Tests

Benchmark critical paths:
- API response times
- Log streaming performance
- Large batch operations

## Extension Points

### Add New Command

1. Create class inheriting `CommandBase`
2. Implement command logic
3. Register in `Program.cs`

### Add New Service

1. Create interface `INewService`
2. Implement service class
3. Inject into commands

### Add New Middleware

1. Implement `ICommandMiddleware`
2. Add to middleware chain
3. Test isolation

## Deployment Architecture

```
Development Environment
    ↓
Build (dotnet build)
    ↓
Test (dotnet test)
    ↓
Publish (dotnet publish)
    ↓
Package (Docker/Zip)
    ↓
Release (GitHub Releases)
    ↓
User Installation
    ↓
Production Use
```

## Technology Stack

- **Language**: C# 13 with latest features
- **Framework**: .NET 10
- **CLI Framework**: System.CommandLine
- **HTTP Client**: HttpClient with retry policies
- **Logging**: Console and file logging
- **JSON**: Newtonsoft.Json
- **Configuration**: Environment variables + .env
- **Caching**: Memory cache
- **Dependency Injection**: Manual (can upgrade to DI container)

## Scalability Considerations

1. **Rate Limiting**: Handle API throttling gracefully
2. **Batch Operations**: Support bulk actions
3. **Caching Strategy**: Cache frequent requests
4. **Connection Management**: Reuse HTTP connections
5. **Async/Await**: Non-blocking I/O throughout
6. **Memory Usage**: Stream large responses
7. **Retry Strategy**: Exponential backoff for failures

## Future Architecture Enhancements

1. **Plugin System**: Load extensions dynamically
2. **Webhook Server**: Receive events from Coolify
3. **Configuration Profiles**: Multiple environment configs
4. **Interactive Mode**: TUI for guided operations
5. **Workspace Management**: Project-level configurations
6. **Authentication Plugins**: OAuth, OIDC support
7. **Data Persistence**: Local cache of resources
8. **Metrics Export**: Prometheus metrics integration

---

For implementation examples, see the code structure in each layer directory.
