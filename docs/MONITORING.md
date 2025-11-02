# Monitoring and Observability

Guide for monitoring Coolify CLI, applications, and databases using various tools and integrations.

## Built-in Monitoring

### Health Checks

```bash
# Check API connectivity
coolify-cli health

# Verbose health check with timing
coolify-cli health --verbose

# Check specific application
coolify-cli app status <APP_ID>

# Check database health
coolify-cli db health <DB_ID>

# Periodic health monitoring (every 30 seconds)
while true; do
    coolify-cli health
    sleep 30
done
```

## Metrics Collection

### Application Metrics

```bash
# Get application status and metrics
coolify-cli app status <APP_ID> --format json

# Monitor multiple applications
for app_id in 1 2 3; do
    echo "=== App $app_id ==="
    coolify-cli app status "$app_id"
done

# Track deployment times
coolify-cli app deployment-metrics <APP_ID> --metric deployment_time

# Monitor resource usage
coolify-cli app metrics <APP_ID> --metrics cpu,memory,disk
```

### Database Metrics

```bash
# Get database health
coolify-cli db health <DB_ID>

# Database performance metrics
coolify-cli db metrics <DB_ID> --metric cpu,memory,connections

# Connection pooling status
coolify-cli db connections <DB_ID>

# Slow query logs
coolify-cli db logs <DB_ID> --filter slow_queries
```

## Prometheus Integration

### Export Metrics

```bash
# Enable Prometheus metrics endpoint
export PROMETHEUS_METRICS_ENABLED=true
export PROMETHEUS_METRICS_PORT=9090

# Access metrics
curl http://localhost:9090/metrics
```

### Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'coolify-cli'
    static_configs:
      - targets: ['localhost:9090']
```

### Useful Metrics

```promql
# Deployment duration
coolify_deployment_duration_seconds

# API response time
coolify_api_response_time_milliseconds

# Application uptime
coolify_application_uptime_seconds

# Database connection pool
coolify_database_connections_active
coolify_database_connections_idle

# Error rate
rate(coolify_errors_total[5m])
```

## Grafana Dashboards

### Create Dashboard

1. Add Prometheus data source
2. Import dashboard or create manually
3. Add panels for key metrics

### Example Panels

```json
{
  "dashboard": {
    "title": "Coolify CLI Monitoring",
    "panels": [
      {
        "title": "Deployments Last 24h",
        "targets": [
          {
            "expr": "increase(coolify_deployments_total[24h])"
          }
        ]
      },
      {
        "title": "API Response Time (p95)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, coolify_api_response_time_milliseconds)"
          }
        ]
      },
      {
        "title": "Application Uptime",
        "targets": [
          {
            "expr": "coolify_application_uptime_seconds"
          }
        ]
      }
    ]
  }
}
```

## ELK Stack Integration

### Collect Logs

```bash
# Forward logs to Logstash
coolify-cli app logs <APP_ID> --follow | \
  filebeat -e -c filebeat.yml

# Or use rsyslog
coolify-cli --verbose 2>&1 | \
  logger -t coolify-cli
```

### Logstash Configuration

```
input {
  syslog {
    port => 514
    type => coolify
  }
}

filter {
  if [type] == "coolify" {
    grok {
      match => { "message" => "%{TIMESTAMP_ISO8601:timestamp} %{LOGLEVEL:level} %{GREEDYDATA:message}" }
    }
  }
}

output {
  elasticsearch {
    hosts => ["localhost:9200"]
    index => "coolify-%{+YYYY.MM.dd}"
  }
}
```

## DataDog Integration

### Send Metrics

```bash
#!/bin/bash
# Send custom metrics to DataDog

DATADOG_API_KEY="your-api-key"

send_metric() {
    local metric_name=$1
    local value=$2
    local timestamp=$(date +%s)

    curl -X POST https://api.datadoghq.com/api/v1/series \
        -H "DD-API-KEY: $DATADOG_API_KEY" \
        -H "Content-Type: application/json" \
        -d "{
            \"series\": [
                {
                    \"metric\": \"coolify.$metric_name\",
                    \"points\": [[$timestamp, $value]],
                    \"type\": \"gauge\"
                }
            ]
        }"
}

# Send deployment metric
send_metric "deployment.duration" 125.5

# Send health check metric
send_metric "health.api_response_time" 45.2
```

## Alert Rules

### Alert Configuration

```yaml
# alerts.yaml
groups:
  - name: coolify_alerts
    rules:
      - alert: CoolifyAPIDown
        expr: up{job="coolify-cli"} == 0
        for: 2m
        annotations:
          summary: "Coolify API is down"
          description: "Coolify API has been unreachable for 2 minutes"

      - alert: HighErrorRate
        expr: rate(coolify_errors_total[5m]) > 0.05
        for: 5m
        annotations:
          summary: "High error rate detected"
          description: "Error rate exceeds 5%"

      - alert: SlowDeployment
        expr: coolify_deployment_duration_seconds > 900
        annotations:
          summary: "Deployment taking longer than 15 minutes"
          description: "Check deployment logs for issues"

      - alert: DatabaseConnectionPoolExhausted
        expr: coolify_database_connections_active / coolify_database_connections_max > 0.9
        for: 5m
        annotations:
          summary: "Database connection pool nearly exhausted"
          description: "{{ $value }}% of connections in use"
```

## Logging Configuration

### Structured Logging

```bash
# Enable JSON logging
export LOG_FORMAT=json
export LOG_LEVEL=info

# Logs are output in JSON format for easy parsing
coolify-cli app list
```

### Log Levels

```
DEBUG   - Detailed diagnostic information
INFO    - General informational messages
WARNING - Warning messages for potentially harmful situations
ERROR   - Error messages for error events
```

### Log Rotation

```bash
# Configure logrotate for /var/log/coolify-cli/
cat > /etc/logrotate.d/coolify-cli <<EOF
/var/log/coolify-cli/*.log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
    create 0644 root root
}
EOF
```

## Custom Monitoring Script

```bash
#!/bin/bash
# Monitor Coolify deployments

COOLIFY_API_KEY="your-key"
CHECK_INTERVAL=300  # 5 minutes

monitor_loop() {
    while true; do
        timestamp=$(date '+%Y-%m-%d %H:%M:%S')

        # Check API health
        if ! coolify-cli health > /dev/null 2>&1; then
            echo "[$timestamp] ERROR: API health check failed"
            # Send alert
            send_alert "API Health Check Failed"
        fi

        # Check deployments
        local deployments=$(coolify-cli app list --format json | jq '.[] | select(.status=="deploying") | .id' | wc -l)
        if [ "$deployments" -gt 0 ]; then
            echo "[$timestamp] INFO: $deployments deployments in progress"
        fi

        # Check failed deployments
        local failed=$(coolify-cli app list --format json | jq '.[] | select(.status=="failed") | .id' | wc -l)
        if [ "$failed" -gt 0 ]; then
            echo "[$timestamp] WARNING: $failed failed deployments"
            send_alert "$failed deployments failed"
        fi

        sleep "$CHECK_INTERVAL"
    done
}

send_alert() {
    local message=$1
    # Send Slack notification
    curl -X POST "$SLACK_WEBHOOK_URL" \
        -H 'Content-Type: application/json' \
        -d "{\"text\": \"Coolify Alert: $message\"}"
}

monitor_loop
```

## Performance Monitoring

### Track Deployment Performance

```bash
#!/bin/bash
# Track deployment performance metrics

for i in {1..10}; do
    start_time=$(date +%s%N)

    coolify-cli app deploy $i

    end_time=$(date +%s%N)
    duration=$((($end_time - $start_time) / 1000000))  # Convert to ms

    echo "Deployment $i: ${duration}ms"
done
```

### Monitor Resource Usage

```bash
# Monitor CLI memory usage
while true; do
    ps aux | grep coolify-cli | grep -v grep | awk '{print "Memory:", $6 "KB"}'
    sleep 5
done

# Monitor file descriptor count
watch -n 5 'ls -1 /proc/$$(pgrep coolify-cli)/fd | wc -l'
```

## Alerting Best Practices

1. **Alert on Symptoms, Not Causes**
   - Alert on error rate, not individual errors
   - Alert on response time, not CPU usage

2. **Set Appropriate Thresholds**
   - Avoid false positives
   - Use historical data to set baselines
   - Implement escalation policies

3. **Actionable Alerts**
   - Include context in alert messages
   - Provide runbooks for remediation
   - Route to appropriate teams

4. **Alert Fatigue Prevention**
   - Group related alerts
   - Implement alert deduplication
   - Review and tune regularly

## Dashboard Best Practices

1. **Key Metrics Only**
   - Deployment success rate
   - Average deployment time
   - Error rate
   - System health status

2. **Time Range Options**
   - Last hour
   - Last 24 hours
   - Last week
   - Last month

3. **Visual Clarity**
   - Use appropriate chart types
   - Color-code status
   - Show trends and baselines

## Resources

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [ELK Stack Docs](https://www.elastic.co/guide/index.html)
- [DataDog Monitoring](https://docs.datadoghq.com/)
