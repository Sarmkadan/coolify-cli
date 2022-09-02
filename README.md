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

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
