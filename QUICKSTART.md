# Quick Start Guide

Get up and running with Coolify CLI in 5 minutes.

## Installation

### Option 1: Binary Release

```bash
# Download and install
curl -sSL https://github.com/Sarmkadan/coolify-cli/releases/latest/download/install.sh | sh

# Verify
coolify-cli --version
```

### Option 2: Build from Source

```bash
# Clone and build
git clone https://github.com/Sarmkadan/coolify-cli.git
cd coolify-cli
./scripts/setup.sh
dotnet build
```

### Option 3: Docker

```bash
docker run --rm sarmkadan/coolify-cli:latest --version
```

## Initial Setup

### 1. Set API Credentials

```bash
export COOLIFY_API_KEY="your-api-key-here"
export COOLIFY_API_URL="https://your-coolify-instance.com"
```

### 2. Verify Connection

```bash
coolify-cli health
```

### 3. List Applications

```bash
coolify-cli app list
```

## Common Commands

### Application Management

```bash
# List all applications
coolify-cli app list

# Get application details
coolify-cli app get <APP_ID>

# Deploy application
coolify-cli app deploy <APP_ID>

# View application status
coolify-cli app status <APP_ID>

# View application logs
coolify-cli app logs <APP_ID> --follow
```

### Database Management

```bash
# List databases
coolify-cli db list

# Check database health
coolify-cli db health <DB_ID>

# Create database backup
coolify-cli db backup create <DB_ID>

# View database logs
coolify-cli db logs <DB_ID>
```

### Environment Variables

```bash
# List environment variables
coolify-cli app env get <APP_ID>

# Set environment variable
coolify-cli app env set <APP_ID> KEY value

# Delete environment variable
coolify-cli app env delete <APP_ID> KEY
```

## Configuration

### Environment Variables

```bash
# Required
COOLIFY_API_KEY=sk_prod_xxx
COOLIFY_API_URL=https://coolify.example.com

# Optional
COOLIFY_VERBOSE=false          # Enable verbose logging
COOLIFY_TIMEOUT=30             # Request timeout in seconds
COOLIFY_CACHE_ENABLED=true     # Enable caching
COOLIFY_CACHE_TTL=300          # Cache TTL in seconds
```

### .env File

Create `.env` file in your project:

```bash
COOLIFY_API_KEY=sk_prod_xxx
COOLIFY_API_URL=https://coolify.example.com
COOLIFY_VERBOSE=true
```

Load with:

```bash
export $(cat .env | xargs)
```

## Deployment Examples

### Simple Deployment

```bash
# Deploy application
coolify-cli app deploy 1

# Wait for completion
coolify-cli app status 1 --wait
```

### Multi-App Deployment

```bash
# Deploy multiple applications
for app_id in 1 2 3; do
    coolify-cli app deploy "$app_id"
    sleep 5
done
```

### Deployment with Verification

```bash
# Deploy and verify
if coolify-cli app deploy 1; then
    echo "Deployment initiated"
    sleep 30
    coolify-cli app status 1
else
    echo "Deployment failed"
    exit 1
fi
```

## Logging

### View Recent Logs

```bash
# Last 50 lines
coolify-cli logs 1 --lines 50

# Last 100 lines
coolify-cli logs 1 --lines 100

# Filter by level
coolify-cli logs 1 --filter ERROR
```

### Stream Live Logs

```bash
# Stream logs continuously
coolify-cli logs 1 --follow

# Stop with Ctrl+C
```

## Troubleshooting

### Cannot Connect to API

```bash
# Check API URL
echo $COOLIFY_API_URL

# Test connectivity
curl -I $COOLIFY_API_URL

# Enable verbose logging
COOLIFY_VERBOSE=true coolify-cli health
```

### Authentication Errors

```bash
# Verify API key is set
echo $COOLIFY_API_KEY

# Regenerate key in Coolify dashboard
# Re-export environment variable
export COOLIFY_API_KEY="new-key-here"
```

### Timeout Issues

```bash
# Increase timeout
export COOLIFY_TIMEOUT=60

# Or for specific command
COOLIFY_TIMEOUT=120 coolify-cli app deploy 1
```

## Next Steps

### Learn More

1. **Full Documentation**: See [README.md](README.md)
2. **API Reference**: See [docs/API_REFERENCE.md](docs/API_REFERENCE.md)
3. **Architecture**: See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
4. **Examples**: See [examples/](examples/)

### Automation

1. **Bash Scripts**: See [examples/deploy-all.sh](examples/deploy-all.sh)
2. **CI/CD**: See [docs/GITHUB_ACTIONS.md](docs/GITHUB_ACTIONS.md)
3. **Cron Jobs**: Schedule deployments with cron

### Advanced

1. **Blue-Green Deployments**: See [examples/advanced-deployment.sh](examples/advanced-deployment.sh)
2. **Multi-Environment**: See [examples/multi-environment-deploy.sh](examples/multi-environment-deploy.sh)
3. **Kubernetes**: See [examples/kubernetes-integration.sh](examples/kubernetes-integration.sh)

## Getting Help

### Command Help

```bash
# Show main help
coolify-cli --help

# Show command help
coolify-cli app --help
coolify-cli db --help
```

### Support Channels

- **Documentation**: https://github.com/Sarmkadan/coolify-cli
- **Issues**: https://github.com/Sarmkadan/coolify-cli/issues
- **Discussions**: https://github.com/Sarmkadan/coolify-cli/discussions
- **Email**: support@sarmkadan.com

## Tips & Tricks

### Use JSON Output in Scripts

```bash
# Get application ID by name
APP_ID=$(coolify-cli app list --format json | jq '.[] | select(.name=="my-app") | .id')

# List running apps
coolify-cli app list --format json | jq '.[] | select(.status=="running") | .name'
```

### Batch Processing

```bash
# Deploy all applications
coolify-cli app list --format json | \
  jq -r '.[] | .id' | \
  parallel coolify-cli app deploy
```

### Create Aliases

```bash
# Add to your ~/.bashrc or ~/.zshrc
alias capp='coolify-cli app'
alias cdb='coolify-cli db'
alias clogs='coolify-cli logs'

# Usage
capp list
cdb health 1
clogs 1 --follow
```

---

**Happy deploying!** 🚀

For more information, visit: https://sarmkadan.com
