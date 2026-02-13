// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Guide for deploying applications using Coolify CLI.

## Deployment Strategies

Coolify CLI supports multiple deployment strategies optimized for different scenarios.

### 1. Blue-Green Deployment

Two identical production environments with zero-downtime switching.

**How it works**:
1. Deploy to inactive environment (green)
2. Run health checks
3. Switch traffic to green environment
4. Blue remains as rollback point

**Best for**: 
- Production critical applications
- Need instant rollback
- Can afford duplicate infrastructure

**Command**:
```bash
coolify-cli app deploy 1 --strategy blue-green
```

**Advantages**:
- Zero downtime
- Instant rollback
- Easy to test before switch
- Full traffic switchover

**Disadvantages**:
- Requires double resources
- Database migration challenges
- Initial setup complexity

### 2. Canary Deployment

Gradually roll out to subset of users.

**How it works**:
1. Deploy new version to single instance
2. Route small traffic percentage (5%)
3. Monitor metrics and error rates
4. Gradually increase traffic (10%, 25%, 50%, 100%)
5. Rollback if issues detected

**Best for**:
- High-risk changes
- Want to test in production
- Can tolerate small user impact
- Need gradual rollout

**Command**:
```bash
coolify-cli app deploy 1 --strategy canary
```

**Configuration**:
```bash
export CANARY_INITIAL_TRAFFIC="5"
export CANARY_INCREMENT="10"
export CANARY_INTERVAL="300"  # seconds between increments
export CANARY_ERROR_THRESHOLD="5"  # error rate % to trigger rollback
```

**Advantages**:
- Catch issues before full rollout
- Production validation
- Reduced risk
- Early error detection

**Disadvantages**:
- Slower rollout
- Complex setup
- Requires monitoring
- Split traffic complexity

### 3. Rolling Deployment

Gradually replace instances one at a time.

**How it works**:
1. Stop instance 1
2. Deploy new version
3. Start instance 1, wait for health
4. Repeat for instance 2, 3, ...
5. Continue until all updated

**Best for**:
- Applications with multiple instances
- Can tolerate brief reduced capacity
- Cost-sensitive deployments
- Simple orchestration

**Command**:
```bash
coolify-cli app deploy 1 --strategy rolling
```

**Configuration**:
```bash
export ROLLING_BATCH_SIZE="2"  # instances per batch
export ROLLING_WAIT_HEALTH="30"  # seconds to wait for health
```

**Advantages**:
- No additional resources
- Simple implementation
- Gradual rollout
- Easy rollback

**Disadvantages**:
- Reduced capacity during deployment
- Longer deployment time
- Version mixing
- Can't instantly rollback

## Pre-Deployment Checks

The CLI automatically performs these checks:

### 1. Configuration Validation
```bash
# Checks
✓ API connectivity
✓ Valid API key
✓ Application exists
✓ Environment variables valid
✓ Health check URL valid (if configured)
```

### 2. Health Verification
```bash
# Ensures
✓ Current version is healthy
✓ All dependent services running
✓ Database connectivity
✓ Required secrets configured
```

### 3. Resource Checks
```bash
# Validates
✓ Sufficient disk space
✓ Memory available
✓ CPU capacity
✓ Network connectivity
```

### 4. Dependency Checks
```bash
# Verifies
✓ Required services running
✓ Database migrations applicable
✓ Required secrets present
✓ Configuration loaded
```

## Deployment Process

### Step-by-Step

```
User: coolify-cli app deploy 1
    ↓
1. Validate configuration
    ├─ Check API key
    ├─ Verify app exists
    └─ Validate settings
    ↓
2. Pre-flight checks
    ├─ Health checks
    ├─ Resource availability
    └─ Dependency verification
    ↓
3. Prepare deployment
    ├─ Clone repository
    ├─ Build application
    └─ Run tests
    ↓
4. Execute deployment
    ├─ Use selected strategy
    ├─ Deploy new version
    └─ Switch traffic
    ↓
5. Post-deployment validation
    ├─ Health checks
    ├─ Smoke tests
    └─ Monitor logs
    ↓
6. Completion
    ├─ Success notification
    ├─ Store deployment record
    └─ Return to user
```

### Automatic Rollback

If deployment fails:

```
1. Detect failure
    ├─ Health check fails
    ├─ Error rate high
    └─ Deployment timeout
    ↓
2. Initiate rollback
    ├─ Switch back to previous
    ├─ Restore configuration
    └─ Clear new version
    ↓
3. Verify rollback
    ├─ Health checks
    ├─ Error rate normal
    └─ Traffic stable
    ↓
4. Alert user
    └─ Reason for rollback
    └─ Previous version restored
```

## Monitoring Deployments

### Real-time Status

```bash
# Watch deployment progress
coolify-cli app status 1

# Output:
# Application: production-api
# Current Status: deploying
# Progress: 65%
# Started At: 2026-05-04 14:30:22
# Estimated Time: 2 minutes remaining
```

### View Deployment Logs

```bash
# Get comprehensive deployment logs
coolify-cli app logs 1 --filter DEPLOY --lines 100

# Stream deployment logs live
coolify-cli logs 1 --follow --filter DEPLOY
```

### Metrics During Deployment

```bash
# Monitor health during deployment
while true; do
    coolify-cli health
    sleep 5
done

# Watch error rate
coolify-cli app get 1 --include metrics
```

## Scheduled Deployments

### Deploy During Maintenance Window

```bash
#!/bin/bash
# scheduled-deploy.sh

# Set maintenance mode
coolify-cli app set-maintenance 1 true

# Wait for current requests to finish
sleep 60

# Deploy
coolify-cli app deploy 1 --wait true

# Clear maintenance mode
coolify-cli app set-maintenance 1 false

echo "Deployment complete"
```

### Deploy Daily

Use cron to schedule deployments:

```bash
# Deploy every night at 2 AM
0 2 * * * /path/to/deploy.sh >> /var/log/deploy.log 2>&1

# In deploy.sh:
#!/bin/bash
set -e
export COOLIFY_API_KEY="sk_prod_xxx"
export COOLIFY_API_URL="https://coolify.example.com"
coolify-cli app deploy 1
```

## Multi-Environment Deployments

### Deploy to Multiple Environments

```bash
#!/bin/bash
# deploy-all-environments.sh

ENVIRONMENTS=("dev" "staging" "production")

for env in "${ENVIRONMENTS[@]}"; do
    export COOLIFY_API_URL="https://coolify-$env.example.com"
    
    echo "Deploying to $env..."
    coolify-cli app deploy 1
    
    # Verify deployment
    if ! coolify-cli health; then
        echo "Health check failed in $env!"
        exit 1
    fi
    
    # Wait before next
    sleep 30
done

echo "All deployments completed"
```

### Coordinate Multiple Services

```bash
#!/bin/bash
# Deploy API first, then web, then workers

echo "Deploying API..."
coolify-cli app deploy 1
sleep 30

echo "Deploying Web..."
coolify-cli app deploy 2
sleep 30

echo "Deploying Workers..."
coolify-cli app deploy 3

echo "All services deployed"
```

## Rollback Procedures

### Manual Rollback

```bash
# Get previous version
coolify-cli app status 1

# Rollback to previous
coolify-cli app rollback 1
```

### Rollback Specific Version

```bash
# List available versions
coolify-cli app history 1

# Rollback to specific version
coolify-cli app rollback 1 --version abc123
```

### Rollback All Services

```bash
#!/bin/bash
# Emergency rollback script

echo "Rolling back all services..."

coolify-cli app rollback 1  # API
coolify-cli app rollback 2  # Web
coolify-cli app rollback 3  # Workers

echo "Rollback complete"
```

## Database Migrations

### Before Deployment

```bash
# Create backup
coolify-cli db backup create 1

# Run migrations in dry-run
coolify-cli db migrate 1 --dry-run

# If okay, run migration
coolify-cli db migrate 1
```

### Deployment with Migration

```bash
#!/bin/bash
# Deploy with database migration

# 1. Backup database
coolify-cli db backup create 1
echo "Database backed up"

# 2. Run migration
coolify-cli db migrate 1
echo "Migration complete"

# 3. Deploy application
coolify-cli app deploy 1
echo "Deployment complete"

# 4. Verify
if ! coolify-cli health; then
    echo "Health check failed!"
    # Restore if needed
    coolify-cli db backup restore 1 backup_id
    exit 1
fi
```

## Troubleshooting Deployments

### Deployment Hangs

```bash
# Check status
coolify-cli app status 1 --verbose

# View logs for errors
coolify-cli logs 1 --lines 200 --filter ERROR

# Manual intervention
coolify-cli app stop 1
coolify-cli app deploy 1 --strategy blue-green
```

### Health Check Fails

```bash
# Get health details
coolify-cli app get 1

# Check health check URL
curl -v http://localhost:3000/health

# View application logs
coolify-cli logs 1 --lines 100

# Investigate and redeploy
coolify-cli app deploy 1
```

### Rollback Stuck

```bash
# Force kill current deployment
coolify-cli app stop 1

# Check logs
coolify-cli logs 1 --lines 50

# Manual restart
coolify-cli app restart 1

# Verify
coolify-cli health
```

### Out of Disk Space

```bash
# Check space
df -h

# Clean old images
docker image prune -a

# Clean logs
coolify-cli logs-cleanup 1

# Retry deployment
coolify-cli app deploy 1
```

## Deployment Configuration

### Application Health Check

```bash
# Set health check endpoint
coolify-cli app config set 1 health-check-url "http://localhost:3000/health"

# Set health check interval
coolify-cli app config set 1 health-check-interval "30"

# Require successful health check
coolify-cli app config set 1 require-health-check "true"
```

### Deployment Timeout

```bash
# Increase timeout
coolify-cli app deploy 1 --timeout 900

# Or set in config
coolify-cli app config set 1 deployment-timeout "900"
```

### Resource Limits

```bash
# Set memory limit
coolify-cli app config set 1 memory-limit "512m"

# Set CPU limit
coolify-cli app config set 1 cpu-limit "1"

# Set disk limit
coolify-cli app config set 1 disk-limit "10g"
```

## Best Practices

1. **Always backup before migrations**
   ```bash
   coolify-cli db backup create 1
   ```

2. **Test in staging first**
   ```bash
   coolify-cli app deploy 2  # staging
   # Verify everything works
   coolify-cli app deploy 1  # production
   ```

3. **Use health checks**
   ```bash
   coolify-cli app deploy 1 --strategy blue-green
   # Automatically validates health
   ```

4. **Monitor deployment logs**
   ```bash
   coolify-cli logs 1 --follow
   ```

5. **Have rollback plan**
   ```bash
   # Know how to rollback quickly
   coolify-cli app rollback 1
   ```

6. **Deploy during low-traffic periods**
   ```bash
   # Safer for canary and rolling deployments
   ```

7. **Communicate with team**
   - Let team know about deployments
   - Have monitoring ready
   - Have rollback team on standby

---

For more details, see [API Reference](API_REFERENCE.md) or [Examples](../examples/deploy-all.sh).
