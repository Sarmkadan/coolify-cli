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

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `COOLIFY_API_KEY` | - | Required API token |
| `COOLIFY_API_URL` | - | Coolify instance URL |
| `COOLIFY_VERBOSE` | `false` | Enable verbose logging |
| `COOLIFY_TIMEOUT` | `30` | Request timeout in seconds |

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
