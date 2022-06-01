# Contributing to Coolify CLI

Thank you for your interest in contributing to Coolify CLI! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

Be respectful, inclusive, and professional. We're building a community together.

## Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **Git** for version control
- **A GitHub account** for submitting pull requests

### Setting Up Development Environment

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/coolify-cli.git
cd coolify-cli

# Add upstream remote
git remote add upstream https://github.com/Sarmkadan/coolify-cli.git

# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## Development Workflow

### 1. Create a Feature Branch

```bash
# Update main branch
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/your-feature-name
```

Use descriptive names like:
- `feature/add-backup-restore`
- `fix/handle-timeout-errors`
- `docs/improve-installation-guide`
- `refactor/simplify-http-client`

### 2. Make Your Changes

Follow these guidelines:

- **One feature per branch** - Keep changes focused
- **Write tests** for new functionality
- **Update documentation** as needed
- **Follow code style** conventions (see below)
- **Commit often** with clear messages

### 3. Commit Messages

Write clear, descriptive commit messages:

```bash
# Good
git commit -m "Add blue-green deployment strategy

Implement blue-green deployment strategy with health
validation and automatic rollback on failure.

- Add DeploymentStrategy.BlueGreen class
- Add HealthCheckService integration
- Add integration tests for strategy
- Update deployment documentation"

# Avoid
git commit -m "fix stuff"
git commit -m "WIP"
```

### 4. Push and Create Pull Request

```bash
# Push your branch
git push origin feature/your-feature-name

# Create PR on GitHub
# Use the PR template and describe your changes
```

## Code Standards

### C# Conventions

```csharp
// Class names: PascalCase
public class ApplicationService { }

// Method names: PascalCase
public async Task<Application> GetApplicationAsync(int id) { }

// Local variables: camelCase
var applicationList = new List<Application>();

// Constants: UPPER_SNAKE_CASE
private const string DEFAULT_TIMEOUT = "30";

// Private fields: _camelCase
private readonly ILogger _logger;

// Properties: PascalCase with auto-properties preferred
public string Name { get; set; }
```

### Documentation

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Deploys an application with pre-flight checks and rollback support.
/// </summary>
/// <param name="applicationId">The application ID to deploy</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>Deployment result with status and metadata</returns>
/// <exception cref="CoolifyException">Thrown when deployment fails</exception>
public async Task<DeploymentResult> DeployAsync(int applicationId, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### File Organization

Each .cs file should:

1. Start with the author header (see above)
2. Include using statements (ordered alphabetically)
3. Define a single public class (with private nested types if needed)
4. Keep methods under 50 lines when possible
5. Use dependency injection for testability

### Error Handling

Use custom exception types from `Infrastructure/CoolifyExceptions.cs`:

```csharp
if (application == null)
{
    throw new CoolifyException("Application not found", ErrorCode.NotFound);
}

try
{
    await _apiClient.DeployAsync(applicationId);
}
catch (HttpRequestException ex)
{
    throw new CoolifyException("API communication failed", ErrorCode.ApiError, ex);
}
```

## Testing

### Writing Tests

- Write unit tests for all public methods
- Use descriptive test names following Arrange-Act-Assert pattern
- Test both success and error scenarios
- Mock external dependencies

```csharp
[TestClass]
public class ApplicationServiceTests
{
    [TestMethod]
    public async Task DeployAsync_WithValidId_ReturnsSuccessfulDeployment()
    {
        // Arrange
        var service = new ApplicationService(/* mocks */);
        var applicationId = 1;

        // Act
        var result = await service.DeployAsync(applicationId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("deployed", result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(CoolifyException))]
    public async Task DeployAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var service = new ApplicationService(/* mocks */);

        // Act
        await service.DeployAsync(-1);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "ApplicationServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura

# Run tests with live log output
dotnet test --logger "console;verbosity=detailed"
```

## Documentation Contributions

### Updating Docs

Documentation files are in the `docs/` directory:

- `GETTING_STARTED.md` - Quick start guide
- `ARCHITECTURE.md` - System design and patterns
- `API_REFERENCE.md` - Complete API documentation
- `DEPLOYMENT.md` - Deployment procedures
- `FAQ.md` - Frequently asked questions

Guidelines:

- Use clear, concise language
- Include code examples for complex topics
- Keep line width under 100 characters
- Use headings and subheadings properly
- Update Table of Contents when adding sections

### README Updates

The main README.md should include:
- Overview of the change
- Usage examples if applicable
- Configuration options if added
- Links to detailed documentation

## Pull Request Process

### Before Submitting

1. **Run tests** - Ensure all tests pass
   ```bash
   dotnet test
   ```

2. **Check formatting** - Verify code style
   ```bash
   dotnet format
   ```

3. **Update documentation** - Add/update relevant docs
4. **Update CHANGELOG.md** - Add entry for your change

### Pull Request Template

```markdown
## Description
Brief description of the change

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation
- [ ] Refactoring

## How Has This Been Tested?
Describe the testing approach

## Screenshots (if applicable)
Include screenshots for UI changes

## Checklist
- [ ] Tests pass
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] No breaking changes
```

### Review Process

1. Code review by maintainers
2. Automated tests must pass
3. Documentation review if applicable
4. Approval and merge

## Release Process

Releases follow semantic versioning (MAJOR.MINOR.PATCH):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes

### Release Checklist

1. Update version in `coolify-cli.csproj`
2. Update `CHANGELOG.md`
3. Create git tag: `git tag v1.2.0`
4. Push tag: `git push upstream v1.2.0`
5. GitHub Actions automatically builds and publishes

## Reporting Bugs

When reporting bugs, include:

1. **Description** - Clear explanation of the issue
2. **Reproduction steps** - Detailed steps to reproduce
3. **Expected behavior** - What should happen
4. **Actual behavior** - What actually happens
5. **Environment** - OS, .NET version, CLI version
6. **Logs** - Any relevant error messages

Example:

```
Title: Deployment fails with timeout on large applications

Description:
When deploying large applications (>500MB), the CLI times out
before the deployment completes.

Steps to Reproduce:
1. Create a large application (>500MB)
2. Run: coolify-cli app deploy 1
3. Wait for deployment to progress

Expected: Deployment completes successfully
Actual: "Request timeout" error after 30 seconds

Environment:
- OS: Ubuntu 22.04
- .NET: 10.0
- CLI Version: 1.0.0

Logs:
[ERROR] Request timeout after 30000ms
```

## Feature Requests

When suggesting features:

1. **Clear title** - Concise feature description
2. **Use case** - Why this feature is needed
3. **Proposed solution** - How you'd like it to work
4. **Alternatives** - Other approaches considered
5. **Examples** - Usage examples if applicable

## Community

### Getting Help

- **Discussions** - Ask questions in GitHub Discussions
- **Issues** - Report bugs or request features
- **Telegram** - Connect with maintainer
- **Email** - Contact for security issues

### Recognition

Contributors are recognized in:
- CHANGELOG.md
- GitHub Contributors page
- Project documentation

## License

By contributing to Coolify CLI, you agree that your contributions
will be licensed under the MIT License.

---

Thank you for contributing to Coolify CLI!
