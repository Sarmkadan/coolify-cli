# CLAUDE.md

.NET 10 CLI for the Coolify API (System.CommandLine + YamlDotNet): deploy/manage apps and databases, view logs, monitor resources, and apply declarative YAML infrastructure templates.

## Build

- `dotnet build -c Release` (or `make build`)
- `dotnet publish -c Release -o ./publish` (or `make publish`)
- `make docker-build` - builds `sarmkadan/coolify-cli` image via `Dockerfile`
- SDK pinned in `global.json` (10.0.100, rollForward latestMinor)

## Test

- `dotnet test -c Release --verbosity minimal` (or `make test`)
- Test project: `tests/coolify-cli.Tests/` (xunit, FluentAssertions, Moq)
- Benchmarks: `benchmarks/` (BenchmarkDotNet)
- `TestValidation/Program.cs` - standalone validation harness, not part of the test suite

## Lint / Format

- `make lint` - `dotnet build /p:TreatWarningsAsErrors=true`
- `make format` - runs `dotnet csharp-format` over all `.cs` files
- Style rules live in `.editorconfig` (4-space indent, LF, file-scoped namespaces)

## Layout

- `coolify-cli.csproj` - single main project; `tests/`, `benchmarks/`, `examples/` are excluded from its compile
- `Commands/` - System.CommandLine command groups; `CommandBase` is the abstract base; `InfrastructureCommands.cs` / `ResourceMonitorCommands.cs` contain `Main` entry points
- `Services/` - API-facing logic (`CoolifyApiClient`, `ApplicationService`, `DatabaseService`, `InfrastructureTemplateEngine`, `LogService`, `TuiService`)
- `Models/` - DTOs, enums, `ApiResponse`
- `Infrastructure/` - config, options, constants, exceptions, `ResilientHttpHandler`
- `Http/` - `CircuitBreaker`, `ResiliencePolicy`
- `Formatters/` - table/json/csv/text output, `OutputFormatterFactory`
- `Caching/`, `Extensions/`, `Utilities/` - cache provider, extension methods, helpers
- `docs/` - per-class markdown docs; `configs/`, `examples/`, `scripts/` - sample config, IaC templates, install scripts
- CI: `.github/workflows/` (build, codeql, docker, nuget-publish, publish, release)

## Conventions

- Namespaces: `CoolifyCli.<Folder>` (file-scoped), `#nullable enable`, implicit usings on
- One class per file; companion files by suffix: `*Extensions.cs`, `*JsonExtensions.cs`, `*Validation.cs`
- Tests mirror source names: `<Class>Tests.cs`
- Warnings are not errors by default (`Directory.Build.props`); `make lint` enforces them
- XML doc comments enabled (`GenerateDocumentationFile`), CS1591 suppressed
- Do not commit `.aider*`, `bin/`, `obj/`, `BenchmarkDotNet.Artifacts/`, `config.json`
