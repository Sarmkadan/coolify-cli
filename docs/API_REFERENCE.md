// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete reference for all Coolify CLI commands and options.

## Command Structure

```
coolify-cli [GLOBAL_OPTIONS] COMMAND [COMMAND_OPTIONS] [ARGUMENTS]
```

## Global Options

| Option | Short | Description |
|--------|-------|-------------|
| `--verbose` | `-v` | Enable verbose logging |
| `--help` | `-h` | Show help information |
| `--version` | | Display CLI version |

## Application Management

### app list

List all applications.

**Syntax**:
```bash
coolify-cli app list [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--format` | `table` | Output format: `table`, `json`, `csv` |
| `--filter` | | Filter by status: `running`, `stopped`, `deploying` |
| `--limit` | `100` | Maximum results to return |
| `--page` | `1` | Page number for pagination |

**Examples**:
```bash
# List all applications in table format
coolify-cli app list

# Show as JSON
coolify-cli app list --format json

# Filter running applications only
coolify-cli app list --filter running

# Get specific page
coolify-cli app list --page 2 --limit 50
```

**Output**:
```
ID    Name                      Status       Deployed At
──────────────────────────────────────────────────────
1     production-api            running      2026-05-04 14:30:22
2     staging-web               stopped      2026-05-02 09:15:45
```

### app get

Get detailed information about a specific application.

**Syntax**:
```bash
coolify-cli app get <APP_ID> [OPTIONS]
```

**Arguments**:
| Argument | Required | Description |
|----------|----------|-------------|
| `<APP_ID>` | Yes | Application ID |

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--format` | `text` | Output format: `text`, `json` |
| `--include` | | Include additional data: `logs`, `metrics`, `history` |

**Examples**:
```bash
coolify-cli app get 1

coolify-cli app get 1 --format json

coolify-cli app get 1 --include logs,metrics
```

**Output**:
```
Application: production-api
ID: 1
Repository: git@github.com:company/api.git
Branch: main
Status: running
Environment: prod-eu
Created: 2026-01-15 10:30:00
Last Deployed: 2026-05-04 14:30:22
Ports: 3000, 8080
Health Check: http://localhost:3000/health
```

### app deploy

Deploy an application.

**Syntax**:
```bash
coolify-cli app deploy <APP_ID> [OPTIONS]
```

**Arguments**:
| Argument | Required | Description |
|----------|----------|-------------|
| `<APP_ID>` | Yes | Application ID to deploy |

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--strategy` | `auto` | Deployment strategy: `blue-green`, `canary`, `rolling`, `auto` |
| `--wait` | `true` | Wait for deployment to complete |
| `--timeout` | `600` | Timeout in seconds |
| `--skip-health-check` | `false` | Skip post-deployment health checks |

**Examples**:
```bash
# Deploy with default strategy
coolify-cli app deploy 1

# Use blue-green deployment
coolify-cli app deploy 1 --strategy blue-green

# Don't wait for completion
coolify-cli app deploy 1 --wait false

# Increase timeout
coolify-cli app deploy 1 --timeout 900
```

### app status

Get deployment status.

**Syntax**:
```bash
coolify-cli app status <APP_ID>
```

**Output**:
```
Application: production-api
Current Status: deploying
Progress: 65%
Started At: 2026-05-04 14:30:22
Estimated Time: 2 minutes remaining
```

### app stop

Stop a running application.

**Syntax**:
```bash
coolify-cli app stop <APP_ID>
```

### app restart

Restart an application.

**Syntax**:
```bash
coolify-cli app restart <APP_ID>
```

### app env get

List environment variables.

**Syntax**:
```bash
coolify-cli app env get <APP_ID>
```

**Output**:
```
Name                     Scope       Value
───────────────────────────────────────────────────
DATABASE_URL             application postgresql://...
API_KEY                  application sk_prod_xxx
DEBUG                    global      false
```

### app env set

Set an environment variable.

**Syntax**:
```bash
coolify-cli app env set <APP_ID> <NAME> <VALUE> [OPTIONS]
```

**Arguments**:
| Argument | Required | Description |
|----------|----------|-------------|
| `<APP_ID>` | Yes | Application ID |
| `<NAME>` | Yes | Variable name |
| `<VALUE>` | Yes | Variable value |

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--scope` | `application` | Scope: `application`, `global` |
| `--encrypted` | `false` | Encrypt value |
| `--restart` | `true` | Restart app after setting |

**Examples**:
```bash
coolify-cli app env set 1 DATABASE_URL "postgresql://..."

coolify-cli app env set 1 API_KEY "secret_key" --encrypted

coolify-cli app env set 1 DEBUG "true" --restart false
```

### app env delete

Delete an environment variable.

**Syntax**:
```bash
coolify-cli app env delete <APP_ID> <NAME>
```

### app logs

View application logs.

**Syntax**:
```bash
coolify-cli logs <APP_ID> [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--lines` | `100` | Number of lines to show |
| `--follow` | `false` | Follow logs (stream) |
| `--filter` | | Filter by level: `DEBUG`, `INFO`, `WARN`, `ERROR` |
| `--since` | | Show logs since timestamp (RFC3339) |
| `--until` | | Show logs until timestamp (RFC3339) |
| `--format` | `text` | Output format: `text`, `json` |

**Examples**:
```bash
# View last 50 lines
coolify-cli logs 1 --lines 50

# Stream logs
coolify-cli logs 1 --follow

# Only errors
coolify-cli logs 1 --filter ERROR

# Logs from last hour
coolify-cli logs 1 --since "$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)"
```

## Database Management

### db list

List all databases.

**Syntax**:
```bash
coolify-cli db list [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--format` | `table` | Output format: `table`, `json`, `csv` |
| `--type` | | Filter by type: `postgresql`, `mysql`, `redis`, `mongodb` |

**Output**:
```
ID    Name                 Type          Host                  Status
────────────────────────────────────────────────────────────────────
1     prod-postgres        postgresql    db.prod.example.com   Healthy
2     cache-redis          redis         cache.example.com     Healthy
```

### db get

Get database details.

**Syntax**:
```bash
coolify-cli db get <DB_ID>
```

### db health

Check database health.

**Syntax**:
```bash
coolify-cli db health <DB_ID> [OPTIONS]
```

**Output**:
```
Database Health Check:
Status: Healthy
Response Time: 12ms
CPU Usage: 23.45%
Memory: 512.34MB
Active Connections: 42
Error Rate: 0.12%
```

### db backup list

List database backups.

**Syntax**:
```bash
coolify-cli db backup list <DB_ID> [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--limit` | `10` | Number of backups to show |
| `--format` | `table` | Output format |

### db backup create

Create a database backup.

**Syntax**:
```bash
coolify-cli db backup create <DB_ID> [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--name` | | Custom backup name |
| `--description` | | Backup description |

### db backup restore

Restore from a backup.

**Syntax**:
```bash
coolify-cli db backup restore <DB_ID> <BACKUP_ID> [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--confirm` | `false` | Skip confirmation prompt |

### db logs

View database logs.

**Syntax**:
```bash
coolify-cli db logs <DB_ID> [OPTIONS]
```

Same options as `app logs`.

## System Commands

### health

Check system and API health.

**Syntax**:
```bash
coolify-cli health [OPTIONS]
```

**Options**:
| Option | Default | Description |
|--------|---------|-------------|
| `--check` | `all` | Specific check: `api`, `database`, `services` |
| `--detailed` | `false` | Show detailed information |

**Output**:
```
✓ Connected to Coolify API
✓ System health check passed
```

### version

Display version information.

**Syntax**:
```bash
coolify-cli version
```

**Output**:
```
Coolify CLI v1.0.0
Author: Vladyslav Zaiets
Website: https://sarmkadan.com
```

### config

Show current configuration.

**Syntax**:
```bash
coolify-cli config [OPTIONS]
```

**Output**:
```
API URL: https://coolify.example.com
Verbose: true
Timeout: 30 seconds
Cache: enabled (TTL: 300s)
```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Configuration error |
| 3 | API communication error |
| 4 | Deployment error |
| 5 | Validation error |

## Common Patterns

### Piping Output

```bash
# Get JSON and filter with jq
coolify-cli app list --format json | jq '.[] | select(.status=="running")'

# Export to CSV
coolify-cli app list --format csv > apps.csv

# Count applications
coolify-cli app list --format json | jq 'length'
```

### Batch Operations

```bash
# Deploy multiple apps
for id in 1 2 3; do
    coolify-cli app deploy "$id"
done

# Health check all databases
coolify-cli db list --format json | jq -r '.[] | .id' | while read id; do
    coolify-cli db health "$id"
done
```

### Conditional Execution

```bash
# Deploy only if healthy
if coolify-cli health > /dev/null 2>&1; then
    coolify-cli app deploy 1
else
    echo "System is not healthy!"
    exit 1
fi
```

## Error Handling

### Common Errors and Solutions

**Error**: "Configuration Error: COOLIFY_API_KEY not set"
```bash
export COOLIFY_API_KEY="sk_prod_xxx"
```

**Error**: "Failed to connect to Coolify API"
```bash
# Check API URL
echo $COOLIFY_API_URL

# Test connectivity
curl -I https://your-coolify-instance.com
```

**Error**: "Application not found"
```bash
# List available apps
coolify-cli app list

# Verify the ID exists
```

## Timeout and Retry Configuration

Configure retry behavior via environment variables:

```bash
export COOLIFY_RETRY_ATTEMPTS="5"
export COOLIFY_RETRY_DELAY_MS="2000"
export COOLIFY_TIMEOUT="60"
```

## Rate Limiting

The CLI implements automatic rate limit handling:

- Respects `X-RateLimit-*` headers
- Implements exponential backoff
- Queues requests when rate limited
- No manual retry needed

## JSON Output Schema

### Application Object

```json
{
  "id": 1,
  "name": "production-api",
  "repository": "git@github.com:company/api.git",
  "branch": "main",
  "status": "running",
  "environmentId": "prod-eu",
  "createdAt": "2026-01-15T10:30:00Z",
  "lastDeployedAt": "2026-05-04T14:30:22Z",
  "ports": [3000, 8080],
  "healthCheckUrl": "http://localhost:3000/health"
}
```

### Database Object

```json
{
  "id": 1,
  "name": "prod-postgres",
  "type": "postgresql",
  "host": "db.prod.example.com",
  "port": 5432,
  "status": "Healthy",
  "isHealthy": true,
  "lastHealthCheckAt": "2026-05-04T15:00:00Z"
}
```

---

For more examples, see the [Examples](../examples/) directory or check the [Getting Started](GETTING_STARTED.md) guide.
