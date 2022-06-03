// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to Coolify CLI are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-27

### Added
- Stable production release
- Full deployment strategy suite: blue-green, canary, rolling
- Multi-server deployment orchestration via `MultiServerDeploymentService`
- Resource quota management and enforcement
- Background health monitoring via `StatusCheckWorker`
- Native AOT publish support for sub-50 ms cold start
- NuGet packaging and `dotnet tool install` support

### Changed
- Promoted middleware chain to default pipeline (auth → rate-limit → logging → error)
- Improved error messages with actionable suggestions throughout all commands
- Finalised NuGet metadata (`PackageId`, `RepositoryUrl`, `PackageReadmeFile`)
- Updated documentation: added QUICKSTART.md, PERFORMANCE.md, and ROADMAP.md

### Fixed
- Race condition in concurrent deployments
- Memory leak in long-running log stream sessions
- API key validation rejecting keys that contain special characters

## [0.9.0] - 2025-09-22

### Added
- `RateLimitingMiddleware` with configurable per-environment limits
- `ResourceQuotaManager` for tracking CPU, memory, and connection quotas
- Code coverage reporting in CI (`dotnet test /p:CollectCoverage=true`)
- `StringExtensionsTests`, `ValidationHelperTests`, and `DeploymentTests` test suites

### Changed
- Refactored `DeploymentOrchestrator` to support pluggable strategy implementations
- Improved test isolation: source files compiled directly in test project to avoid circular dependencies

### Fixed
- Timezone handling in log timestamps
- Pagination offset error in large result sets

## [0.8.0] - 2025-08-25

### Added
- `NotificationService` for Slack and email deployment alerts
- `WebhookHandler` for publishing deployment events to external endpoints
- `EventPublisher` for internal event distribution
- `MultiServerExtensions` helper methods
- Docker image and `docker-compose.yml` for containerised deployment

### Changed
- `CoolifyApiClient` now uses `IHttpClientFactory` for connection pooling
- Log streaming switched to async enumerable pattern
- Reduced default retry delay from 2 000 ms to 1 000 ms

### Fixed
- Connection pool exhaustion under high concurrency
- Intermittent timeouts on slow or high-latency networks

## [0.7.0] - 2025-07-21

### Added
- GitHub Actions workflow for NuGet publish (`nuget-publish.yml`)
- CodeQL security analysis workflow
- Dependabot configuration for NuGet packages and GitHub Actions
- `examples/cicd-integration.yml` — ready-made GitHub Actions deployment template
- `docs/GITHUB_ACTIONS.md` integration guide

### Changed
- Build workflow updated to `ubuntu-latest` with .NET 10 SDK
- Test workflow now uploads coverage artefacts

### Fixed
- Missing `IncludeAssets` on xunit runner package reference

## [0.6.0] - 2025-06-16

### Added
- Output formatters: `TableFormatter`, `JsonFormatter`, `CsvFormatter`, `TextFormatter`
- `--format` flag on all list commands (`table` | `json` | `csv`)
- `CollectionExtensions`, `DateTimeExtensions`, `EnumExtensions` helper classes
- `docs/ARCHITECTURE.md` with layered architecture diagram

### Changed
- Default output switched from plain text to aligned table format
- `ConsoleLogger` now respects `COOLIFY_VERBOSE` at runtime without restart

## [0.5.0] - 2025-05-12

### Added
- `MemoryCacheProvider` with configurable TTL (`COOLIFY_CACHE_TTL`)
- `AuthenticationMiddleware` for API key injection and validation
- `LoggingMiddleware` for structured request/response logging
- `ErrorHandlingMiddleware` with documented exit-code mapping
- `CommandBase` providing shared middleware execution pipeline

### Changed
- Configuration loading extracted to `CoolifyConfiguration` and `ConfigurationHelper`
- All HTTP requests now share a single `HttpClient` instance

### Fixed
- Unhandled exception on malformed `COOLIFY_API_URL`
- Duplicate log output when `--verbose` was combined with log streaming commands

## [0.4.0] - 2025-04-07

### Added
- Real-time system health check (`coolify-cli health`)
- `HealthCheckService` with response-time and error-rate metrics
- `MonitoringCommands` for application and infrastructure health
- `ServiceHealth` model with CPU, memory, and active connection fields
- `docs/MONITORING.md` guide

### Changed
- `app deploy` now runs post-deployment health verification before returning
- Exit code table documented in README (0 success through 5 validation error)

## [0.3.0] - 2025-03-03

### Added
- Deployment strategies: blue-green, canary, rolling (`DeploymentStrategy.cs`)
- Pre-deployment validation checks and automatic rollback on failure
- `DeploymentContext` and `ApplicationDeployment` models
- `DeploymentOrchestrator` coordinating the multi-step deployment lifecycle
- `examples/production-deployment.sh` and `examples/multi-environment-deploy.sh`

### Changed
- `app deploy` command now accepts an optional `--strategy` flag
- Deployment timeout increased to 600 s

## [0.2.0] - 2025-02-03

### Added
- `DatabaseService` with list, health check, backup, and restore operations
- `DatabaseManagementCommands` (`db list`, `db health`, `db backup`, `db logs`)
- `DatabaseRepository` backed by `CoolifyApiClient`
- `DatabaseConfiguration` and `LogEntry` models
- `examples/backup-databases.sh`

### Changed
- `CoolifyApiClient` refactored to reuse `HttpClient` across commands
- Improved error output on API 4xx / 5xx responses

### Fixed
- `app list` crashing when Coolify returns an empty application array

## [0.1.0] - 2025-01-06

### Added
- Initial release of Coolify CLI
- `app list`, `app get`, `app deploy`, `app stop`, `app restart` commands
- `app env get`, `app env set`, `app env delete` for environment variable management
- `logs` command with `--lines` and `--follow` flags
- `version` and `config` commands
- `CoolifyApiClient` HTTP client using `System.Net.Http`
- `ApplicationService` and `ApplicationRepository`
- `EnvironmentVariableService` for managing per-app configuration
- `LogService` with async log streaming
- Configuration via environment variables (`COOLIFY_API_KEY`, `COOLIFY_API_URL`)
- `.env` file loading at startup
- Colour-coded console output via `ConsoleLogger`
- `ValidationHelper` and `CliHelpers` utility classes
- MIT licence, README, and CONTRIBUTING guide
- GitHub Actions CI workflows (`build.yml`, `test.yml`)
- `.editorconfig` for consistent code style
- Cross-platform support: Linux, macOS, Windows

---

## Contributors

- **Vladyslav Zaiets** (@Sarmkadan) — Author, CTO & Software Architect

## License

MIT License — see [LICENSE](LICENSE) for details.

---

Repository: https://github.com/Sarmkadan/coolify-cli  
Website: https://sarmkadan.com  
Issues: https://github.com/Sarmkadan/coolify-cli/issues
