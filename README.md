# coolify-cli

![CI](https://github.com/sarmkadan/coolify-cli/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/coolify-cli)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

A .NET 10 CLI for managing [Coolify](https://coolify.io) from the terminal. Deploy apps, manage databases, stream logs, and apply infrastructure-as-code templates.

## Install

**From source:**

```bash
git clone https://github.com/Sarmkadan/coolify-cli.git
cd coolify-cli
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./out
sudo mv out/coolify-cli /usr/local/bin/
```

**Pre-built binaries** are available on the [Releases](https://github.com/sarmkadan/coolify-cli/releases) page for Linux, macOS, and Windows.

## Configuration

```bash
export COOLIFY_API_KEY="your-api-token"
export COOLIFY_API_URL="https://your-coolify-instance.com"
```

Optional:

| Variable | Default | Description |
|----------|---------|-------------|
| `COOLIFY_VERBOSE` | `false` | Enable verbose logging |
| `COOLIFY_TIMEOUT` | `30` | Request timeout in seconds |

## Usage

```bash
# Check connectivity
coolify-cli health

# Applications
coolify-cli app list
coolify-cli app get <id>
coolify-cli app deploy <id>
coolify-cli app stop <id>
coolify-cli app restart <id>

# Environment variables
coolify-cli env list <id>
coolify-cli env set <id> KEY VALUE
coolify-cli env sync <id> --file .env

# Logs
coolify-cli logs <id>
coolify-cli logs <id> --follow
coolify-cli logs <id> --lines 200

# Databases
coolify-cli db list
coolify-cli db health <id>
coolify-cli db backup create <id>

# Infrastructure as code
coolify-cli iac validate coolify.yaml
coolify-cli iac diff coolify.yaml
coolify-cli iac apply coolify.yaml

# Resource monitoring
coolify-cli resources show
coolify-cli resources watch <id>

# Interactive TUI
coolify-cli tui
```

## Building from source

```bash
dotnet build
dotnet test
```

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
