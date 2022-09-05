# coolify-cli

A .NET 10 CLI for managing [Coolify](https://coolify.io) from the terminal. Deploy apps, manage databases, stream logs, and apply infrastructure-as-code templates.

![Build](https://github.com/sarmkadan/coolify-cli/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/coolify-cli)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

## Installation

### From source

```bash
git clone https://github.com/sarmkadan/coolify-cli.git
cd coolify-cli
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./out
sudo mv out/coolify-cli /usr/local/bin/
```

## Quick Start

```bash
# Configure access
export COOLIFY_API_KEY="your-api-token"
export COOLIFY_API_URL="https://your-coolify-instance.com"

# Check connectivity
coolify-cli health

# List applications
coolify-cli app list
```

## Examples

You can find code examples showcasing how to use the underlying services in the `examples/` directory:

- `BasicUsage.cs`: Demonstrates listing applications.
- `AdvancedUsage.cs`: Shows deployment with error handling and custom configuration.
- `IntegrationExample.cs`: Illustrates how to wire services into an ASP.NET Core DI container.

## Docker

You can run the CLI inside Docker or use `docker-compose` to manage health monitoring and automated tasks.

### Running with Docker

```bash
docker build -t coolify-cli .
docker run --rm -e COOLIFY_API_KEY="your-api-token" coolify-cli --help
```

### Using docker-compose

The project includes `docker-compose.yml` for running the CLI along with health monitoring and scheduler services.

```bash
# Create a .env file
cp configs/prod.env.example .env
# Fill in your API Key and URL in .env

# Start services
docker-compose up -d
```

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `COOLIFY_API_KEY` | - | Required API token |
| `COOLIFY_API_URL` | - | Coolify instance URL |
| `COOLIFY_VERBOSE` | `false` | Enable verbose logging |
| `COOLIFY_TIMEOUT` | `30` | Request timeout in seconds |

## StringExtensionsTestsExtensions

The `StringExtensionsTestsExtensions` class provides helper methods for testing string extension methods in the CLI. It includes assertions for validating common string operations like PascalCase conversion, truncation, sensitive data masking, and split/trim logic.

Example usage in a test:
```csharp
public class StringExtensionsTests
{
    [Fact]
    public void TestStringExtensions()
    {
        var testInstance = StringExtensionsTestsExtensions.CreateTestInstance();
        StringExtensionsTestsExtensions.AssertToPascalCase(testInstance, "ExpectedPascalCase");
        StringExtensionsTestsExtensions.AssertTruncate(testInstance, 10, "Truncated...");
        StringExtensionsTestsExtensions.AssertMaskSensitive(testInstance, "****masked****");
        StringExtensionsTestsExtensions.AssertSplitTrimmed(testInstance, new[] { "Split", "Trimmed" });
    }
}
```

## EnumExtensionsTestsExtensions

The `EnumExtensionsTestsExtensions` class provides extension methods for testing enum-related extension methods in the CLI. It offers utilities for parsing enums, validating display strings, checking enum value mappings, and verifying CLI format consistency.

Example usage in a test:
```csharp
public class TestEnumExtensions
{
[Fact]
public void TestEnumParsing()
{
// Parse enum from string
var status = "Active".ParseTestEnum<ApplicationStatus>();
status.Should().Be(ApplicationStatus.Active);

// Try parse enum from string (returns null on failure)
var invalidStatus = "InvalidStatus".TryParseTestEnum<ApplicationStatus>();
invalidStatus.Should().BeNull();

// Get display string map for all enum values
var displayMap = EnumExtensionsTestsExtensions.GetEnumValueDisplayMap<ApplicationStatus>();
displayMap.Should().ContainKey(ApplicationStatus.Active).WhoseValue.Should().NotBeEmpty();

// Verify all display strings are non-empty
var allNonEmpty = EnumExtensionsTestsExtensions.AllDisplayStringsNonEmpty<ApplicationStatus>();
allNonEmpty.Should().BeTrue();

// Get integer value map
var intMap = EnumExtensionsTestsExtensions.GetEnumValueIntMap<ApplicationStatus>();
intMap.Should().ContainKey(ApplicationStatus.Active).WhoseValue.Should().BeGreaterThan(0);

// Get test cases for all enum values
var testCases = EnumExtensionsTestsExtensions.GetEnumTestCases<ApplicationStatus>();
testCases.Should().HaveCountGreaterThan(0);

// Verify enum values are in expected order
var expectedOrder = new[] { ApplicationStatus.Pending, ApplicationStatus.Active, ApplicationStatus.Stopped };
var isInOrder = expectedOrder.AreEnumValuesInOrder();
isInOrder.Should().BeTrue();

// Get CLI format map
var cliFormatMap = EnumExtensionsTestsExtensions.GetEnumCliFormatMap<ApplicationStatus>();
cliFormatMap.Should().ContainKey(ApplicationStatus.Active);

// Verify all CLI formats are unique
var allUnique = EnumExtensionsTestsExtensions.AllCliFormatsUnique<ApplicationStatus>();
allUnique.Should().BeTrue();
}
}
```

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
