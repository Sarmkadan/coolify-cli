// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with Coolify CLI

This guide will walk you through installing and using Coolify CLI for the first time.

## Prerequisites

Before you begin, ensure you have:

- **.NET 10 SDK** installed ([download](https://dotnet.microsoft.com/download))
- **API Key** from your Coolify instance (find in Admin Dashboard → API Keys)
- **API URL** of your Coolify instance (e.g., `https://coolify.example.com`)

Verify .NET installation:

```bash
dotnet --version
# Output: .NET 10.0.xxx
```

## Installation

### Option 1: Build from Source

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/coolify-cli.git
cd coolify-cli

# Build the project
dotnet build -c Release

# Run directly
dotnet run -- app list

# Or publish and install globally
dotnet publish -c Release -o ./publish
sudo ln -s $(pwd)/publish/coolify-cli /usr/local/bin/coolify-cli

# Verify installation
coolify-cli version
```

### Option 2: Use Pre-built Binary

Download from [GitHub Releases](https://github.com/Sarmkadan/coolify-cli/releases):

```bash
# Download latest release
wget https://github.com/Sarmkadan/coolify-cli/releases/download/v1.0.0/coolify-cli-linux-x64.zip

# Extract and install
unzip coolify-cli-linux-x64.zip
sudo mv coolify-cli /usr/local/bin/

# Make executable
chmod +x /usr/local/bin/coolify-cli
```

### Option 3: Docker

```bash
# Pull image
docker pull sarmkadan/coolify-cli:latest

# Create alias for convenience
alias coolify-cli='docker run --rm -e COOLIFY_API_KEY -e COOLIFY_API_URL sarmkadan/coolify-cli:latest'

# Use as normal
coolify-cli version
```

## Configuration

### 1. Get Your API Key

1. Log in to your Coolify dashboard
2. Navigate to **Settings → API Keys**
3. Click **New API Key**
4. Copy the generated key (starts with `sk_` or `pk_`)

### 2. Set Environment Variables

Create a `.env` file in your project directory:

```bash
# Required
COOLIFY_API_KEY=sk_prod_1234567890abcdefghijk
COOLIFY_API_URL=https://coolify.example.com

# Optional but recommended
COOLIFY_VERBOSE=true
COOLIFY_TIMEOUT=30
COOLIFY_CACHE_ENABLED=true
```

Or export directly in your shell:

```bash
export COOLIFY_API_KEY="sk_prod_1234567890abcdefghijk"
export COOLIFY_API_URL="https://coolify.example.com"
```

### 3. Verify Configuration

```bash
# Test connection
coolify-cli health

# Expected output:
# ✓ Connected to Coolify API
# ✓ System health check passed
```

If you see errors, check:

1. API key is correct (no spaces or special characters)
2. API URL is accessible (try `curl` to test)
3. Network connection (ping the server)
4. Firewall rules (ensure outbound HTTPS is allowed)

## Your First Commands

### List Applications

```bash
coolify-cli app list
```

This shows all applications:

```
ID    Name                      Status       Deployed At
──────────────────────────────────────────────────────
1     production-api            running      2026-05-04 14:30:22
2     staging-web               stopped      2026-05-02 09:15:45
```

### Get Application Details

```bash
# Replace 1 with your app ID
coolify-cli app get 1
```

Output shows:
- Application name and repository
- Current status and environment
- Deployment history
- Health check configuration
- Exposed ports

### List Databases

```bash
coolify-cli db list
```

### Check System Health

```bash
coolify-cli health --verbose
```

## Common Workflows

### Deploy an Application

```bash
# 1. Find the application ID
coolify-cli app list

# 2. Deploy
coolify-cli app deploy 1

# 3. Monitor status
coolify-cli app status 1

# 4. View logs
coolify-cli logs 1 --lines 50
```

### Monitor Application Logs

```bash
# View last 100 lines
coolify-cli logs 1 --lines 100

# Stream logs continuously
coolify-cli logs 1 --follow

# Filter by log level
coolify-cli logs 1 --filter ERROR
```

### Manage Environment Variables

```bash
# View current environment variables
coolify-cli app env get 1

# Set a new variable
coolify-cli app env set 1 DATABASE_URL "postgresql://user:pass@host/db"

# Delete a variable
coolify-cli app env delete 1 OLD_VAR
```

### Database Operations

```bash
# List all databases
coolify-cli db list

# Check health of a database
coolify-cli db health 1

# List backups
coolify-cli db backup list 1

# Create a backup
coolify-cli db backup create 1

# Restore from backup
coolify-cli db backup restore 1 backup_id
```

## Scripting and Automation

### Simple Deployment Script

```bash
#!/bin/bash
# deploy.sh - Deploy all apps in sequence

set -e  # Exit on error

# Load environment
source .env

echo "Starting deployments..."
for app_id in 1 2 3; do
    echo "Deploying app $app_id..."
    coolify-cli app deploy "$app_id"
    echo "Waiting for deployment..."
    sleep 30
    
    # Verify health
    if ! coolify-cli health > /dev/null; then
        echo "Health check failed!"
        exit 1
    fi
done

echo "All deployments completed!"
```

Run with:

```bash
chmod +x deploy.sh
./deploy.sh
```

### Health Monitor Script

```bash
#!/bin/bash
# health-monitor.sh - Check health every 5 minutes

while true; do
    echo "[$(date)] Checking health..."
    
    if coolify-cli health > /dev/null 2>&1; then
        echo "✓ System healthy"
    else
        echo "✗ System unhealthy - alert!"
    fi
    
    sleep 300  # 5 minutes
done
```

## Troubleshooting

### "Configuration Error"

**Problem**: CLI won't start

**Solutions**:
```bash
# Check if variables are set
echo $COOLIFY_API_KEY
echo $COOLIFY_API_URL

# Use verbose output for details
coolify-cli --verbose app list
```

### "Failed to connect to API"

**Problem**: Connection timeout

**Solutions**:
```bash
# Test API connectivity directly
curl -I https://your-coolify-instance.com

# Increase timeout
export COOLIFY_TIMEOUT=60

# Enable verbose logging
export COOLIFY_VERBOSE=true
```

### "Invalid API key"

**Problem**: 401 Unauthorized error

**Solutions**:
```bash
# Verify key format (should start with sk_ or pk_)
echo $COOLIFY_API_KEY

# Check for extra whitespace
export COOLIFY_API_KEY="sk_prod_xxx"  # Without spaces

# Regenerate key in Coolify dashboard
```

## Next Steps

1. **Read** the [API Reference](API_REFERENCE.md) for all available commands
2. **Explore** [Examples](../examples/) for real-world use cases
3. **Learn** the [Architecture](ARCHITECTURE.md) for deeper understanding
4. **Check out** [FAQ](FAQ.md) for common questions
5. **Join** the community for support and contributions

## Getting Help

- Check the [FAQ](FAQ.md) for common issues
- View [Troubleshooting](../README.md#troubleshooting) in main README
- Open an [Issue](https://github.com/Sarmkadan/coolify-cli/issues) on GitHub
- Contact maintainer at https://sarmkadan.com

## Tips & Tricks

### Alias for Convenience

Add to your shell profile (`.bashrc`, `.zshrc`):

```bash
alias c='coolify-cli'
alias ca='coolify-cli app'
alias cd='coolify-cli db'
```

Then use:
```bash
c app list      # Instead of coolify-cli app list
ca deploy 1     # Instead of coolify-cli app deploy 1
```

### Save API Key Securely

Use `direnv` to manage environment:

```bash
# Install direnv
curl -sfL https://direnv.net/install.sh | bash

# Create .envrc in project
echo 'export COOLIFY_API_KEY="sk_prod_xxx"' > .envrc
echo 'export COOLIFY_API_URL="https://coolify.example.com"' >> .envrc

# Allow direnv
direnv allow

# Variables are automatically loaded when entering directory
```

### Batch Operations

Process multiple applications:

```bash
#!/bin/bash
# Get list of IDs and deploy each
for app_id in $(coolify-cli app list --format json | jq '.[].id'); do
    echo "Deploying app $app_id..."
    coolify-cli app deploy "$app_id"
done
```

## Quick Reference

| Task | Command |
|------|---------|
| List apps | `coolify-cli app list` |
| Deploy app | `coolify-cli app deploy <ID>` |
| View logs | `coolify-cli logs <ID>` |
| Check health | `coolify-cli health` |
| List databases | `coolify-cli db list` |
| Database health | `coolify-cli db health <ID>` |
| Show version | `coolify-cli version` |

---

Ready to dive deeper? Check out the [API Reference](API_REFERENCE.md) or explore the [Examples](../examples/).
