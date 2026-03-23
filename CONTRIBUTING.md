# Contributing to coolify-cli

Thank you for taking the time to contribute! Every bug fix, feature, and documentation improvement helps.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Building Locally](#building-locally)
- [Running Tests](#running-tests)
- [Code Style](#code-style)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Reporting Issues](#reporting-issues)

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git

## Building Locally

```bash
# Clone your fork
git clone https://github.com/your-username/coolify-cli.git
cd coolify-cli

# Restore dependencies
dotnet restore

# Build in Release configuration
dotnet build --configuration Release

# Run the CLI
dotnet run -- --help
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output and generate a TRX report
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/coolify-cli.Tests/coolify-cli.Tests.csproj
```

## Code Style

This project uses an `.editorconfig` to enforce consistent formatting. Before submitting a PR, verify formatting:

```bash
dotnet format --verify-no-changes
```

Key conventions:
- **File-scoped namespaces** (`namespace Foo;`)
- **Explicit types** over `var` unless the type is obvious from the right-hand side
- **XML documentation** on all public types, methods, and properties
- **Author headers** — keep existing file headers intact; do not remove them

## Pull Request Guidelines

1. Create a branch from `main`: `git checkout -b feature/your-feature-name`
2. Keep commits focused and use conventional commit prefixes:
   - `feat:` — new feature
   - `fix:` — bug fix
   - `docs:` — documentation only
   - `test:` — adding or updating tests
   - `ci:` — CI/CD changes
   - `chore:` — maintenance
3. Ensure all tests pass before opening a PR: `dotnet test`
4. Fill in the PR description — explain *what* changed and *why*
5. Link any related issues with `Closes #<issue-number>`

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/coolify-cli/issues) to report bugs or suggest features. When reporting a bug, include:

- Steps to reproduce
- Expected behaviour
- Actual behaviour
- Your OS and .NET version (`dotnet --version`)

## License

By contributing you agree that your changes will be licensed under the [MIT License](LICENSE).
