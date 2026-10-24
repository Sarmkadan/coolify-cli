# Contributing to coolify-cli

Thanks for contributing! Bug fixes, features, and docs improvements are all welcome.

## Prerequisites

- **.NET SDK 10.0.100** or later in the 10.0 band. The version is pinned in [`global.json`](global.json) (`rollForward: latestMinor`).
- Git

Check your setup with `dotnet --version`.

## Cloning

```bash
git clone https://github.com/sarmkadan/coolify-cli.git
cd coolify-cli
```

To contribute, fork the repository on GitHub and clone your fork instead.

## Building

```bash
dotnet restore
dotnet build -c Release        # or: make build
```

`make lint` builds with `TreatWarningsAsErrors=true`. Run it before opening a PR if you touched C# code.

## Running Tests

```bash
dotnet test -c Release --verbosity minimal    # or: make test
```

Tests live in `tests/coolify-cli.Tests/` (xUnit, FluentAssertions, Moq). Add or update a `<Class>Tests.cs` file next to the class you change, following the existing names.

## Running the CLI Locally

The CLI needs a Coolify instance to talk to. Set these first (use your own values; never commit them):

```bash
export COOLIFY_API_URL=https://coolify.example.com
export COOLIFY_API_KEY=<your-api-token>
```

```bash
dotnet run --project coolify-cli.csproj -c Release -- --help
dotnet run --project coolify-cli.csproj -c Release -- resources --help
```

Everything after `--` is passed to the CLI. The command fails with a configuration error if `COOLIFY_API_KEY` is missing. `make run ARGS="..."` runs the same thing.

## Project Layout

| Path | Contents |
|------|----------|
| `Commands/` | System.CommandLine command groups (`CommandBase` is the base class) |
| `Services/` | API-facing logic (`CoolifyApiClient`, `ApplicationService`, `DatabaseService`, ...) |
| `Models/` | DTOs, enums, `ApiResponse` |
| `Infrastructure/` | Configuration, options, constants, exceptions, `ResilientHttpHandler` |
| `Http/` | `CircuitBreaker`, `ResiliencePolicy` |
| `Formatters/` | Table, JSON, CSV, and text output |
| `Caching/`, `Extensions/`, `Utilities/` | Cache provider, extension methods, helpers |
| `docs/` | Per-class markdown docs |

Repository conventions (namespaces, one class per file, companion `*Extensions.cs` / `*Validation.cs` files) are described in [`CLAUDE.md`](CLAUDE.md). Formatting is defined in `.editorconfig` (4-space indent, LF, file-scoped namespaces). Run `make format` to apply it.

## Branch Naming

Branch from an up-to-date `main` and use a short prefix that matches the change type:

- `feature/<short-name>`: new functionality
- `fix/<short-name>`: bug fixes
- `docs/<short-name>`: documentation only
- `chore/<short-name>`: maintenance, dependencies, tooling

```bash
git checkout main && git pull
git checkout -b fix/dispose-http-response
```

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/): `type(scope): summary`.

- Types: `feat`, `fix`, `docs`, `test`, `ci`, `chore`, `refactor`
- Scope is optional and usually the class or folder changed, e.g. `fix(EnvironmentVariableService): mask secret values in output`
- Use the imperative mood in the summary and keep it under about 72 characters
- Keep each commit focused on one logical change

## Pull Request Process

1. Open the PR **against `main`**.
2. **CI must pass.** The `CI` workflow (`.github/workflows/ci.yml`) runs on every PR to `main`: it restores, runs `dotnet build --no-restore -c Release`, and runs `dotnet test --no-build -c Release`. Fix failures before asking for review.
3. Describe **what** changed and **why** in the PR body. Note how you tested it and link related issues with `Closes #<number>`.
4. Keep PRs small enough to review in one sitting. Split unrelated changes into separate PRs.
5. Update `README.md` or `docs/` when you change user-facing behavior or public APIs.

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/coolify-cli/issues). For bugs, include:

- Steps to reproduce
- Expected and actual behavior
- Your OS and .NET version (`dotnet --version`)

Do not paste API keys, tokens, or other secrets into issues or PRs.

## License

By contributing you agree that your changes are licensed under the [MIT License](LICENSE).
