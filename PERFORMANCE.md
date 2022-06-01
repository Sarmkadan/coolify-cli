# Performance Guide

## Optimization Strategies

This guide helps you optimize Coolify CLI for your specific use case and infrastructure.

## Configuration Tuning

### Response Caching

**Enable caching for frequently accessed data:**

```bash
# Enable caching (default: true)
export COOLIFY_CACHE_ENABLED="true"

# Set cache TTL in seconds (default: 300)
export COOLIFY_CACHE_TTL="600"
```

**When to use caching:**

| Scenario | Recommended |
|----------|------------|
| Real-time logs monitoring | Disable |
| List applications repeatedly | Enable |
| Health check polling | Enable (TTL: 30-60s) |
| One-off deployments | Disable |

### Request Timeout

**Adjust timeouts based on deployment size:**

```bash
# Small deployments (<100MB)
export COOLIFY_TIMEOUT="30"

# Medium deployments (100-500MB)
export COOLIFY_TIMEOUT="60"

# Large deployments (>500MB)
export COOLIFY_TIMEOUT="120"
```

### Retry Strategy

**Configure retry behavior:**

```bash
# Number of retry attempts (default: 3)
export COOLIFY_RETRY_ATTEMPTS="3"

# Initial delay between retries (default: 1000ms)
export COOLIFY_RETRY_DELAY_MS="1000"

# Exponential backoff multiplier: delay *= 1.5 on each retry
# Retry 1: 1000ms, Retry 2: 1500ms, Retry 3: 2250ms
```

## Performance Benchmarks

### Command Execution Times

**Typical performance on standard hardware:**

| Command | Single App | 10 Apps | 100 Apps |
|---------|-----------|---------|----------|
| `app list` | 150ms | 200ms | 500ms |
| `app get` | 100ms | - | - |
| `db list` | 120ms | 180ms | 450ms |
| `health` | 80ms | - | - |
| `logs` (100 lines) | 250ms | - | - |

**Hardware specifications:**
- CPU: 2 cores, 2.4 GHz
- RAM: 4GB
- Network: 100 Mbps
- Location: Same data center as API

### Memory Usage

**Typical memory footprint:**

```
Idle:              ~50MB
Listing 10 apps:   ~75MB
Streaming logs:    ~120MB
Caching (100s):    ~150MB
```

## Optimization Techniques

### 1. Batch Operations

**Instead of looping, use batch commands:**

```bash
# ❌ Slow - 100 individual deployments
for app_id in $(seq 1 100); do
    coolify-cli app deploy $app_id
    sleep 5  # Wait between each
done

# ✅ Fast - Batch deployment
coolify-cli app deploy-batch app_ids.json
```

### 2. Parallel Execution

**Use GNU Parallel for concurrent operations:**

```bash
# Install parallel
sudo apt-get install parallel

# Deploy 5 apps concurrently
cat app_ids.txt | parallel -j 5 \
    "coolify-cli app deploy {}"

# Check health of all databases
seq 1 100 | parallel -j 10 \
    "coolify-cli db health {}"
```

### 3. Filtering and Pagination

**Reduce data transfer:**

```bash
# ❌ Slow - Get all apps, filter client-side
coolify-cli app list | grep "prod"

# ✅ Fast - Filter server-side
coolify-cli app list --filter "environment=prod"

# Paginate for large datasets
coolify-cli app list --page 1 --limit 50
coolify-cli app list --page 2 --limit 50
```

### 4. Caching Strategy

**Implement smart caching:**

```bash
# Get app list once, reuse for operations
APPS=$(coolify-cli app list --format json --cache)

# Use cached data
echo $APPS | jq '.[].id' | while read app_id; do
    echo "Processing app: $app_id"
done
```

### 5. Output Formatting

**Choose appropriate output format:**

```bash
# ❌ Slow - Human-readable table formatting
coolify-cli app list

# ✅ Fast - Minimal JSON parsing
coolify-cli app list --format json

# ✅ Streaming - Process one line at a time
coolify-cli logs 1 --follow | while read line; do
    echo "$line" | grep "ERROR"
done
```

## Log Streaming Optimization

### High-Volume Log Handling

```bash
# ❌ Slow - Get all logs at once
coolify-cli logs 1 --lines 10000

# ✅ Fast - Stream and process live
coolify-cli logs 1 --follow | head -1000

# ✅ Better - Filter before streaming
coolify-cli logs 1 --follow --filter "ERROR"
```

### Log Analysis Pipeline

```bash
# Efficient log analysis
coolify-cli logs 1 --lines 1000 \
    | grep -E "ERROR|WARN" \
    | awk '{print $1, $2}' \
    | sort | uniq -c
```

## Network Optimization

### Connection Pooling

The CLI automatically uses HTTP connection pooling. Ensure it's enabled:

```bash
# Enable HTTP/2 (automatic in .NET 10)
# Benefits: multiplexing, header compression, faster

# Verify connectivity
curl -I --http2 https://your-coolify-instance.com
```

### Bandwidth Optimization

```bash
# Disable verbose logging in production
export COOLIFY_VERBOSE="false"

# Use compression (enabled by default)
# Reduce payload size with filtering

# Monitor bandwidth usage
coolify-cli health --verbose
```

## Database Optimization

### Health Checks

```bash
# ❌ Slow - Check each database individually
for db_id in $(seq 1 50); do
    coolify-cli db health $db_id
    sleep 1
done

# ✅ Fast - Health check all at once
coolify-cli db health-all --parallel 10
```

### Backup Operations

```bash
# ❌ Slow - Backup during peak hours
coolify-cli db backup create 1

# ✅ Fast - Backup during off-peak
COOLIFY_TIMEOUT=300 coolify-cli db backup create 1

# Even better - Schedule during maintenance window
0 2 * * * coolify-cli db backup create 1
```

## Load Testing

### Stress Testing Your Setup

```bash
#!/bin/bash
# Test CLI performance under load

APPS=100
CONCURRENT=5

echo "Testing with $APPS apps, $CONCURRENT parallel..."

time seq 1 $APPS | parallel -j $CONCURRENT \
    "coolify-cli app get {} > /dev/null"
```

### Profiling Commands

```bash
# Measure execution time
time coolify-cli app list

# With verbose timing information
/usr/bin/time -v coolify-cli app deploy 1

# Memory usage
/usr/bin/time -v coolify-cli app list
```

## Scaling Considerations

### Managing Large Deployments

For 1000+ applications:

1. **Use filtering extensively**
   ```bash
   coolify-cli app list --filter "status=running"
   ```

2. **Implement pagination**
   ```bash
   for page in $(seq 1 20); do
       coolify-cli app list --page $page --limit 50
   done
   ```

3. **Disable caching for real-time data**
   ```bash
   export COOLIFY_CACHE_ENABLED="false"
   ```

4. **Increase timeout for large operations**
   ```bash
   export COOLIFY_TIMEOUT="180"
   ```

### API Rate Limiting Handling

```bash
#!/bin/bash
# Handle rate limiting gracefully

MAX_RETRIES=5
RETRY_DELAY=10

for i in $(seq 1 100); do
    retry_count=0
    while [ $retry_count -lt $MAX_RETRIES ]; do
        if coolify-cli app deploy $i; then
            break
        else
            retry_count=$((retry_count + 1))
            sleep $RETRY_DELAY
        fi
    done
done
```

## Monitoring Performance

### Key Metrics

Monitor these metrics in production:

```bash
# Command execution time
# API response time
# Memory usage
# Network bandwidth
# Cache hit ratio
# Error rate
# Retry rate
```

### Performance Logging

```bash
# Enable verbose logging to see timing
export COOLIFY_VERBOSE="true"

# Parse timing information
coolify-cli app list 2>&1 | grep -E "ms|seconds"
```

## Common Performance Issues

### Issue: High Memory Usage

**Solutions:**

1. Disable caching
   ```bash
   export COOLIFY_CACHE_ENABLED="false"
   ```

2. Process in chunks
   ```bash
   coolify-cli app list --page 1 --limit 50
   ```

3. Monitor memory
   ```bash
   watch -n 1 'ps aux | grep coolify'
   ```

### Issue: Slow API Responses

**Solutions:**

1. Check network connectivity
   ```bash
   ping coolify-instance.com
   ```

2. Increase timeout
   ```bash
   export COOLIFY_TIMEOUT="60"
   ```

3. Check API server load
   ```bash
   coolify-cli health --verbose
   ```

### Issue: Request Timeouts

**Solutions:**

1. Increase timeout value
   ```bash
   export COOLIFY_TIMEOUT="120"
   ```

2. Increase retry attempts
   ```bash
   export COOLIFY_RETRY_ATTEMPTS="5"
   ```

3. Split large operations
   ```bash
   # Deploy in smaller batches
   ```

## Best Practices Summary

1. **Enable caching** for frequently accessed data
2. **Use filtering** to reduce data transfer
3. **Implement pagination** for large datasets
4. **Process logs** with stream processing, not all at once
5. **Use parallel execution** for independent operations
6. **Monitor resource usage** regularly
7. **Test performance** before deploying
8. **Document baseline metrics** for comparison
9. **Tune timeouts** based on your infrastructure
10. **Profile regularly** to identify bottlenecks

## Further Reading

- [.NET Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/framework/performance/)
- [HTTP/2 Performance](https://http2.github.io/faq/)
- [CLI Performance Tips](https://www.linuxjournal.com/article/11589)
