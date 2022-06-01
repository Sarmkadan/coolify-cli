#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Production deployment script with comprehensive safety checks, notifications, and rollback

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="${SCRIPT_DIR}/logs"
BACKUP_DIR="${SCRIPT_DIR}/backups"
LOCK_FILE="/tmp/coolify-deploy-$$.lock"

# Configuration
readonly DEPLOYMENT_TIMEOUT=900
readonly HEALTH_CHECK_RETRIES=5
readonly HEALTH_CHECK_INTERVAL=10
readonly SLACK_WEBHOOK="${SLACK_WEBHOOK:-}"
readonly EMAIL_RECIPIENTS="${EMAIL_RECIPIENTS:-}"
readonly REQUIRE_APPROVAL="${REQUIRE_APPROVAL:-true}"

# Create directories
mkdir -p "$LOG_DIR" "$BACKUP_DIR"

# Setup logging
LOG_FILE="${LOG_DIR}/deploy_$(date +%Y%m%d_%H%M%S).log"
exec 1> >(tee -a "$LOG_FILE")
exec 2>&1

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Logging
log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $*"
}

log_section() {
    echo ""
    echo -e "${CYAN}╔════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║ $1${NC}"
    echo -e "${CYAN}╚════════════════════════════════════════╝${NC}"
    echo ""
}

log_ok() {
    echo -e "${GREEN}✓ $*${NC}"
}

log_error() {
    echo -e "${RED}✗ $*${NC}"
}

log_warn() {
    echo -e "${YELLOW}⚠ $*${NC}"
}

log_info() {
    echo -e "${BLUE}ℹ $*${NC}"
}

# Cleanup
cleanup() {
    local exit_code=$?
    rm -f "$LOCK_FILE"
    if [ $exit_code -ne 0 ]; then
        log_error "Deployment failed with exit code $exit_code"
        send_notification "FAILURE" "Deployment failed - check logs at $LOG_FILE"
    fi
    exit $exit_code
}

trap cleanup EXIT

# Send Slack notification
send_slack_notification() {
    local status=$1
    local message=$2

    if [ -z "$SLACK_WEBHOOK" ]; then
        return 0
    fi

    local color="danger"
    [ "$status" = "SUCCESS" ] && color="good"
    [ "$status" = "WARNING" ] && color="warning"

    curl -X POST -H 'Content-type: application/json' \
        --data "{
            \"attachments\": [{
                \"color\": \"$color\",
                \"title\": \"Deployment $status\",
                \"text\": \"$message\",
                \"footer\": \"Coolify CLI\",
                \"ts\": $(date +%s)
            }]
        }" \
        "$SLACK_WEBHOOK" 2>/dev/null || log_warn "Failed to send Slack notification"
}

# Send email notification
send_email_notification() {
    local status=$1
    local message=$2

    if [ -z "$EMAIL_RECIPIENTS" ]; then
        return 0
    fi

    local subject="Deployment Alert: $status"
    echo -e "$message\n\nLog file: $LOG_FILE" | \
        mail -s "$subject" "$EMAIL_RECIPIENTS" 2>/dev/null || \
        log_warn "Failed to send email notification"
}

# Send notification
send_notification() {
    local status=$1
    local message=$2

    log_info "Sending notifications..."
    send_slack_notification "$status" "$message"
    send_email_notification "$status" "$message"
}

# Check prerequisites
check_prerequisites() {
    log_section "Checking Prerequisites"

    # Check if already running
    if [ -f "$LOCK_FILE" ]; then
        log_error "Another deployment is in progress"
        exit 1
    fi
    touch "$LOCK_FILE"

    # Check CLI
    if ! command -v coolify-cli &> /dev/null; then
        log_error "coolify-cli not found"
        exit 1
    fi

    # Check API key
    if [ -z "${COOLIFY_API_KEY:-}" ]; then
        log_error "COOLIFY_API_KEY not set"
        exit 1
    fi

    # Check API URL
    if [ -z "${COOLIFY_API_URL:-}" ]; then
        log_error "COOLIFY_API_URL not set"
        exit 1
    fi

    log_ok "Prerequisites verified"
}

# Pre-deployment checks
pre_deployment_checks() {
    log_section "Pre-Deployment Checks"

    # Health check
    log_info "Checking Coolify API health..."
    if ! coolify-cli health > /dev/null 2>&1; then
        log_error "API health check failed"
        exit 1
    fi
    log_ok "API is healthy"

    # Get app info
    log_info "Retrieving application information..."
    local app_info=$(coolify-cli app get "$1" 2>/dev/null || echo "")

    if [ -z "$app_info" ]; then
        log_error "Failed to get application information"
        exit 1
    fi

    echo "$app_info"
    log_ok "Application information retrieved"
}

# Create pre-deployment backup
create_backup() {
    local app_id=$1
    local backup_dir="${BACKUP_DIR}/${app_id}_$(date +%Y%m%d_%H%M%S)"

    log_info "Creating pre-deployment backup..."
    mkdir -p "$backup_dir"

    # Backup application state
    coolify-cli app get "$app_id" > "$backup_dir/app_state.json" 2>/dev/null || true

    # Backup logs
    coolify-cli logs "$app_id" --lines 500 > "$backup_dir/app_logs.txt" 2>/dev/null || true

    log_ok "Backup created at $backup_dir"
    echo "$backup_dir"
}

# Perform deployment
deploy_application() {
    local app_id=$1
    local strategy=${2:-blue-green}

    log_section "Deploying Application"

    log_info "Starting deployment (strategy: $strategy)..."
    log_info "Timeout: ${DEPLOYMENT_TIMEOUT}s"

    if ! coolify-cli app deploy "$app_id" \
        --strategy "$strategy" \
        --timeout "$DEPLOYMENT_TIMEOUT" \
        --wait true > /dev/null 2>&1; then
        log_error "Deployment command failed"
        return 1
    fi

    log_ok "Deployment initiated"
    return 0
}

# Health check after deployment
health_check_post_deployment() {
    local app_id=$1
    local max_retries=${2:-$HEALTH_CHECK_RETRIES}
    local retry=0

    log_section "Post-Deployment Health Checks"

    while [ $retry -lt $max_retries ]; do
        log_info "Health check attempt $((retry + 1))/$max_retries..."

        if coolify-cli health > /dev/null 2>&1; then
            if coolify-cli app get "$app_id" | grep -q "running"; then
                log_ok "Application is healthy and running"
                return 0
            fi
        fi

        retry=$((retry + 1))
        if [ $retry -lt $max_retries ]; then
            log_warn "Health check failed, retrying in ${HEALTH_CHECK_INTERVAL}s..."
            sleep "$HEALTH_CHECK_INTERVAL"
        fi
    done

    log_error "Application failed health checks after $max_retries attempts"
    return 1
}

# Rollback deployment
rollback_deployment() {
    local app_id=$1
    local backup_dir=$2

    log_section "Rolling Back Deployment"

    log_warn "Initiating rollback for application $app_id..."

    if ! coolify-cli app rollback "$app_id" > /dev/null 2>&1; then
        log_error "Rollback command failed"
        log_warn "Manual intervention may be required"
        log_info "Backup directory: $backup_dir"
        return 1
    fi

    log_ok "Rollback completed"

    # Verify rollback
    sleep 10
    if coolify-cli app get "$app_id" | grep -q "running"; then
        log_ok "Application is running after rollback"
        return 0
    else
        log_error "Application not running after rollback"
        return 1
    fi
}

# Smoke tests
run_smoke_tests() {
    local app_id=$1

    log_section "Running Smoke Tests"

    log_info "Checking application logs for errors..."
    local errors=$(coolify-cli logs "$app_id" --lines 100 --filter ERROR 2>/dev/null | wc -l)

    if [ "$errors" -gt 5 ]; then
        log_warn "Detected $errors errors in recent logs"
        return 1
    fi

    log_ok "Smoke tests passed"
    return 0
}

# Generate report
generate_deployment_report() {
    local app_id=$1
    local status=$2
    local start_time=$3
    local end_time=$4

    local report_file="${LOG_DIR}/deployment_report_$(date +%Y%m%d_%H%M%S).md"

    {
        echo "# Deployment Report"
        echo ""
        echo "## Summary"
        echo "- Application ID: $app_id"
        echo "- Status: $status"
        echo "- Start Time: $start_time"
        echo "- End Time: $end_time"
        echo "- Duration: $(($(date -d "$end_time" +%s) - $(date -d "$start_time" +%s)))s"
        echo ""
        echo "## Logs"
        echo "\`\`\`"
        coolify-cli logs "$app_id" --lines 50
        echo "\`\`\`"
        echo ""
        echo "## Generated: $(date)"
    } > "$report_file"

    log_ok "Report generated: $report_file"
}

# Request approval
request_approval() {
    if [ "$REQUIRE_APPROVAL" != "true" ]; then
        return 0
    fi

    log_section "Awaiting Approval"

    read -p "Proceed with deployment? (yes/no): " approval

    if [ "$approval" != "yes" ]; then
        log_warn "Deployment cancelled by user"
        return 1
    fi

    return 0
}

# Main deployment workflow
main() {
    local app_id=$1
    local strategy=${2:-blue-green}
    local start_time=$(date '+%Y-%m-%d %H:%M:%S')

    log_section "Coolify Production Deployment"
    log_info "Application: $app_id"
    log_info "Strategy: $strategy"
    log_info "Log file: $LOG_FILE"
    log_info "Start time: $start_time"

    # Prerequisites
    check_prerequisites

    # Pre-deployment checks
    pre_deployment_checks "$app_id"

    # Request approval
    if ! request_approval; then
        exit 0
    fi

    # Create backup
    local backup_dir=$(create_backup "$app_id")

    # Deploy
    if ! deploy_application "$app_id" "$strategy"; then
        log_error "Deployment failed"
        if rollback_deployment "$app_id" "$backup_dir"; then
            send_notification "FAILURE" "Deployment failed and rolled back successfully"
        else
            send_notification "CRITICAL" "Deployment failed and rollback failed - manual intervention required"
        fi
        exit 1
    fi

    # Post-deployment health checks
    if ! health_check_post_deployment "$app_id"; then
        log_error "Health checks failed after deployment"
        if rollback_deployment "$app_id" "$backup_dir"; then
            send_notification "FAILURE" "Deployment failed health checks - rolled back"
        else
            send_notification "CRITICAL" "Health checks failed and rollback failed"
        fi
        exit 1
    fi

    # Smoke tests
    if ! run_smoke_tests "$app_id"; then
        log_warn "Smoke tests detected issues"
        send_notification "WARNING" "Deployment completed but smoke tests detected issues"
    fi

    local end_time=$(date '+%Y-%m-%d %H:%M:%S')

    # Generate report
    generate_deployment_report "$app_id" "SUCCESS" "$start_time" "$end_time"

    log_section "Deployment Complete"
    log_ok "Deployment successful!"
    log_info "Duration: $(($(date -d "$end_time" +%s) - $(date -d "$start_time" +%s)))s"

    send_notification "SUCCESS" "Production deployment completed successfully for app $app_id"
}

# Script entry point
if [ $# -lt 1 ]; then
    echo "Usage: $0 <APP_ID> [STRATEGY]"
    echo ""
    echo "Example: $0 1 blue-green"
    echo ""
    echo "Strategies: blue-green (default), canary, rolling"
    exit 1
fi

main "$@"
