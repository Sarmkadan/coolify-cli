# Coolify CLI

A powerful .NET 10 command-line interface for managing Coolify infrastructure directly from the terminal. Deploy, manage applications, databases, and view logs with ease.

## Features

- **Application Management**: List, deploy, and manage applications
- **Database Operations**: Create, configure, and manage databases with backup support
- **Health Monitoring**: Real-time health checks and performance metrics
- **Log Viewing**: Stream and search application and database logs
- **Safe Deployments**: Pre-deployment checks, rollback support, and health verification
- **Environment Variables**: Manage secure environment variables with scope support
- **Automated Backups**: Configure and manage database backups

## Prerequisites

- .NET 10 SDK or later
- Coolify API credentials (API key)

## Installation

```bash
git clone https://github.com/Vladyslav-Zaiets/coolify-cli.git
cd coolify-cli
dotnet build
dotnet publish -c Release -o ./publish
```

## Configuration

Set environment variables before running:

```bash
export COOLIFY_API_KEY="your-api-key"
export COOLIFY_API_URL="https://api.coolify.io"
export COOLIFY_VERBOSE="true"  # Optional: enable verbose logging
export COOLIFY_TIMEOUT="30"    # Optional: request timeout in seconds
```

Or create a `.env` file in the application directory.

## Usage

### Application Commands

List all applications:
```bash
coolify-cli app list
```

Get application details:
```bash
coolify-cli app get 123
```

Deploy an application:
```bash
coolify-cli app deploy 123
```

### Database Commands

List all databases:
```bash
coolify-cli db list
```

Check database health:
```bash
coolify-cli db health 456
```

### Logs

View application logs:
```bash
coolify-cli logs 123 --lines 50
```

### System Health

Check system and API connectivity:
```bash
coolify-cli health
```

### Version

Display version information:
```bash
coolify-cli version
```

## Project Structure

```
coolify-cli/
├── Models/                    # Domain models and entities
│   ├── ApplicationDeployment.cs
│   ├── DatabaseConfiguration.cs
│   ├── LogEntry.cs
│   ├── EnvironmentVariable.cs
│   ├── ServiceHealth.cs
│   ├── ApiResponse.cs
│   ├── DeploymentContext.cs
│   └── Enums.cs
├── Services/                  # Business logic services
│   ├── CoolifyApiClient.cs
│   ├── ApplicationService.cs
│   ├── DatabaseService.cs
│   ├── LogService.cs
│   ├── HealthCheckService.cs
│   ├── EnvironmentVariableService.cs
│   ├── DeploymentOrchestrator.cs
│   └── ILogger.cs
├── Data/                      # Data access layer
│   ├── IRepository.cs
│   ├── ApplicationRepository.cs
│   └── DatabaseRepository.cs
├── Infrastructure/            # Configuration and exceptions
│   ├── CoolifyConfiguration.cs
│   ├── CoolifyExceptions.cs
│   └── Constants.cs
├── Utilities/                 # Helper utilities
│   └── CliHelpers.cs
├── Program.cs                 # CLI entry point
├── coolify-cli.csproj        # Project file
└── README.md                  # This file
```

## Architecture

### Layered Architecture

1. **Presentation Layer (Program.cs)**: CLI commands using System.CommandLine
2. **Service Layer**: Business logic orchestration and API coordination
3. **Data Access Layer**: Repository pattern for data operations
4. **Infrastructure Layer**: Configuration, exceptions, and utilities
5. **Domain Layer**: Entity models and enumerations

### Key Services

- **CoolifyApiClient**: Core HTTP client for API communication
- **ApplicationService**: Application lifecycle management
- **DatabaseService**: Database provisioning and management
- **LogService**: Log retrieval and streaming
- **HealthCheckService**: Health monitoring and metrics
- **DeploymentOrchestrator**: Complex deployment workflows
- **EnvironmentVariableService**: Environment variable management

## Error Handling

The application implements custom exception hierarchy:

- `CoolifyException`: Base exception for all CLI errors
- `ConfigurationException`: Configuration-related errors
- `ApiCommunicationException`: API communication failures
- `ApiException`: API error responses
- `DeploymentException`: Deployment operation failures
- `ValidationException`: Input validation errors

## Logging

Logging levels supported:
- DEBUG: Detailed diagnostic information
- INFO: General informational messages
- WARNING: Warning messages for potentially problematic situations
- ERROR: Error messages
- FATAL: Critical errors

Enable verbose logging with `--verbose` flag or `COOLIFY_VERBOSE=true`.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See LICENSE file for details.

## Author

**Vladyslav Zaiets**
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com

## Support

For issues, feature requests, or contributions, please visit the project repository.
