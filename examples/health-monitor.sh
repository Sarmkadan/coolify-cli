#!/bin/bash
# Continuous health monitoring with alerting

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_FILE="${SCRIPT_DIR}/health-monitor.log"
CONFIG_FILE="${SCRIPT_DIR}/health-monitor.conf"

# Default configuration
MONITOR_INTERVAL=60
CHECK_TIMEOUT=30
ALERT_EMAIL=""
ALERT_SLACK=""
CONSECUTIVE_FAILURES=3
HISTORY_SIZE=100

# Load configuration if exists
if [ -f "$CONFIG_FILE" ]; then
    source "$CONFIG_FILE"
fi

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# State tracking
declare -A failure_count
declare -a status_history

# Logging
log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $*" | tee -a "$LOG_FILE"
}

log_status() {
    echo -e "${BLUE}[INFO]${NC} $*"
}

log_ok() {
    echo -e "${GREEN}[OK]${NC} $*"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $*"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $*"
}

# Send alert
send_alert() {
    local service=$1
    local status=$2
    local message=$3

    if [ -n "$ALERT_EMAIL" ]; then
        send_email_alert "$service" "$status" "$message"
    fi

    if [ -n "$ALERT_SLACK" ]; then
        send_slack_alert "$service" "$status" "$message"
    fi
}

# Send email alert
send_email_alert() {
    local service=$1
    local status=$2
    local message=$3

    local subject="Alert: $service is $status"
    local body="Service: $service\nStatus: $status\nTime: $(date)\nMessage: $message"

    echo -e "$body" | mail -s "$subject" "$ALERT_EMAIL" 2>/dev/null || {
        log_warn "Failed to send email alert"
    }
}

# Send Slack alert
send_slack_alert() {
    local service=$1
    local status=$2
    local message=$3

    local color="danger"
    [ "$status" = "recovered" ] && color="good"

    local payload=$(cat <<EOF
{
    "text": "Service Alert",
    "attachments": [{
        "color": "$color",
        "title": "$service",
        "text": "$message",
        "ts": $(date +%s)
    }]
}
EOF
    )

    curl -X POST -H 'Content-type: application/json' \
        --data "$payload" \
        "$ALERT_SLACK" 2>/dev/null || {
        log_warn "Failed to send Slack alert"
    }
}

# Check API health
check_api_health() {
    log_status "Checking API health..."

    if timeout "$CHECK_TIMEOUT" coolify-cli health > /dev/null 2>&1; then
        log_ok "API is healthy"
        failure_count[api]=0
        return 0
    else
        failure_count[api]=$((${failure_count[api]:-0} + 1))
        log_error "API health check failed (${failure_count[api]}/$CONSECUTIVE_FAILURES)"

        if [ ${failure_count[api]} -ge $CONSECUTIVE_FAILURES ]; then
            send_alert "Coolify API" "DOWN" "API has failed $CONSECUTIVE_FAILURES consecutive health checks"
        fi

        return 1
    fi
}

# Check applications
check_applications() {
    log_status "Checking applications..."

    local app_list=$(coolify-cli app list --format json 2>/dev/null | jq -r '.[] | "\(.id) \(.name) \(.status)"' || echo "")

    if [ -z "$app_list" ]; then
        log_error "Failed to retrieve application list"
        return 1
    fi

    while read -r app_id app_name app_status; do
        if [ -z "$app_id" ]; then
            continue
        fi

        local key="app_$app_id"

        if [ "$app_status" != "running" ]; then
            failure_count[$key]=$((${failure_count[$key]:-0} + 1))

            if [ ${failure_count[$key]} -eq 1 ]; then
                log_warn "$app_name (ID: $app_id) is $app_status"
            fi

            if [ ${failure_count[$key]} -ge $CONSECUTIVE_FAILURES ]; then
                send_alert "Application: $app_name" "UNHEALTHY" "Application has been $app_status for $CONSECUTIVE_FAILURES checks"
            fi
        else
            failure_count[$key]=0
            log_ok "$app_name is running"
        fi
    done <<< "$app_list"
}

# Check databases
check_databases() {
    log_status "Checking databases..."

    local db_list=$(coolify-cli db list --format json 2>/dev/null | jq -r '.[] | "\(.id) \(.name) \(.status)"' || echo "")

    if [ -z "$db_list" ]; then
        log_error "Failed to retrieve database list"
        return 1
    fi

    while read -r db_id db_name db_status; do
        if [ -z "$db_id" ]; then
            continue
        fi

        local key="db_$db_id"

        if [ "$db_status" != "Healthy" ]; then
            failure_count[$key]=$((${failure_count[$key]:-0} + 1))

            if [ ${failure_count[$key]} -eq 1 ]; then
                log_warn "$db_name (ID: $db_id) is $db_status"
            fi

            if [ ${failure_count[$key]} -ge $CONSECUTIVE_FAILURES ]; then
                send_alert "Database: $db_name" "UNHEALTHY" "Database health check failed $CONSECUTIVE_FAILURES times"
            fi
        else
            failure_count[$key]=0
            log_ok "$db_name is healthy"
        fi
    done <<< "$db_list"
}

# Generate report
generate_report() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo ""
    echo "╔════════════════════════════════════════╗"
    echo "║  Health Monitor Report                 ║"
    echo "║  $timestamp      ║"
    echo "╚════════════════════════════════════════╝"

    # Check summary
    local total_failures=0
    for key in "${!failure_count[@]}"; do
        total_failures=$((total_failures + ${failure_count[$key]}))
    done

    if [ $total_failures -eq 0 ]; then
        echo -e "${GREEN}All systems operational${NC}"
    else
        echo -e "${YELLOW}Warning: $total_failures service(s) with issues${NC}"
    fi

    echo ""
}

# Cleanup on exit
cleanup() {
    log "Health monitoring stopped"
    exit 0
}

trap cleanup SIGTERM SIGINT

# Main monitoring loop
main() {
    log "======================================"
    log "Coolify Health Monitor - Starting"
    log "======================================"
    log "Interval: $MONITOR_INTERVAL seconds"
    log "Consecutive failures threshold: $CONSECUTIVE_FAILURES"

    if [ -n "$ALERT_EMAIL" ]; then
        log "Email alerts: $ALERT_EMAIL"
    fi

    if [ -n "$ALERT_SLACK" ]; then
        log "Slack webhook configured"
    fi

    log ""

    while true; do
        clear
        generate_report

        check_api_health
        check_applications
        check_databases

        log_status "Next check in $MONITOR_INTERVAL seconds..."
        sleep "$MONITOR_INTERVAL"
    done
}

# Run if sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
