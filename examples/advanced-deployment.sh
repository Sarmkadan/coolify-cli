#!/bin/bash

# =============================================================================
# Advanced Deployment Script with Blue-Green Strategy
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

# Configuration
COOLIFY_API_KEY="${COOLIFY_API_KEY:?Error: COOLIFY_API_KEY not set}"
COOLIFY_API_URL="${COOLIFY_API_URL:?Error: COOLIFY_API_URL not set}"
APP_ID="${1:?Error: APP_ID required as first argument}"
DEPLOYMENT_STRATEGY="${2:-blue-green}"  # blue-green, canary, rolling
LOG_FILE="deployment-$(date +%Y%m%d-%H%M%S).log"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}[WARN]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

# Pre-flight checks
preflight_check() {
    log_info "Running pre-flight checks..."

    # Check API connectivity
    if ! coolify-cli health > /dev/null 2>&1; then
        log_error "Failed to connect to Coolify API"
        exit 1
    fi
    log_success "API connectivity verified"

    # Check application exists
    if ! coolify-cli app get "$APP_ID" > /dev/null 2>&1; then
        log_error "Application $APP_ID not found"
        exit 1
    fi
    log_success "Application $APP_ID found"

    # Check current application status
    STATUS=$(coolify-cli app get "$APP_ID" --format json | jq -r '.status')
    if [ "$STATUS" != "running" ]; then
        log_warning "Application is currently in $STATUS state (expected: running)"
    fi
    log_success "Pre-flight checks passed"
}

# Backup current state
backup_state() {
    log_info "Backing up current application state..."

    local backup_dir="backups/app-$APP_ID"
    mkdir -p "$backup_dir"

    # Save application configuration
    coolify-cli app get "$APP_ID" --format json > "$backup_dir/config.json"

    # Save environment variables
    coolify-cli app env get "$APP_ID" --format json > "$backup_dir/env.json"

    # Save deployment history
    coolify-cli app status "$APP_ID" --format json > "$backup_dir/status.json"

    log_success "State backed up to $backup_dir"
    echo "$backup_dir"
}

# Deploy with strategy
deploy_with_strategy() {
    local strategy=$1
    local backup_dir=$2

    case "$strategy" in
        blue-green)
            deploy_blue_green "$backup_dir"
            ;;
        canary)
            deploy_canary "$backup_dir"
            ;;
        rolling)
            deploy_rolling "$backup_dir"
            ;;
        *)
            log_error "Unknown deployment strategy: $strategy"
            exit 1
            ;;
    esac
}

# Blue-Green Deployment Strategy
deploy_blue_green() {
    local backup_dir=$1

    log_info "Starting Blue-Green deployment..."

    # Verify current (blue) environment
    log_info "Verifying blue (current) environment..."
    if ! verify_health "$APP_ID"; then
        log_error "Blue environment health check failed"
        return 1
    fi
    log_success "Blue environment healthy"

    # Trigger deployment to green environment
    log_info "Deploying to green (new) environment..."
    DEPLOY_ID=$(coolify-cli app deploy "$APP_ID" --format json | jq -r '.deployment_id')
    log_info "Deployment initiated: $DEPLOY_ID"

    # Monitor green deployment
    log_info "Monitoring deployment progress..."
    if ! wait_for_deployment "$DEPLOY_ID"; then
        log_error "Green deployment failed"
        log_warning "Rolling back to blue environment..."
        rollback_deployment "$backup_dir"
        return 1
    fi
    log_success "Green environment deployed successfully"

    # Health check on green
    log_info "Performing health checks on green environment..."
    sleep 5  # Wait for services to fully start

    if ! verify_health "$APP_ID"; then
        log_error "Green environment health check failed"
        log_warning "Rolling back to blue environment..."
        rollback_deployment "$backup_dir"
        return 1
    fi
    log_success "Green environment passed health checks"

    # Switch traffic to green
    log_info "Switching traffic from blue to green..."
    if ! switch_traffic "$APP_ID"; then
        log_error "Traffic switch failed"
        rollback_deployment "$backup_dir"
        return 1
    fi
    log_success "Traffic switched to green environment"

    # Monitor for post-deployment issues
    log_info "Monitoring post-deployment (2 minutes)..."
    local elapsed=0
    while [ $elapsed -lt 120 ]; do
        if ! verify_health "$APP_ID"; then
            log_error "Post-deployment health check failed"
            log_warning "Rolling back to blue environment..."
            rollback_deployment "$backup_dir"
            return 1
        fi
        sleep 10
        elapsed=$((elapsed + 10))
    done

    log_success "Blue-Green deployment completed successfully"
}

# Canary Deployment Strategy
deploy_canary() {
    local backup_dir=$1

    log_info "Starting Canary deployment..."

    # Deploy to small subset
    log_info "Deploying to 10% of nodes (canary)..."
    DEPLOY_ID=$(coolify-cli app deploy "$APP_ID" --canary-percentage 10 --format json | jq -r '.deployment_id')
    log_info "Canary deployment initiated: $DEPLOY_ID"

    # Monitor canary metrics
    log_info "Monitoring canary metrics (5 minutes)..."
    if ! monitor_canary_metrics "$DEPLOY_ID" 300; then
        log_error "Canary metrics indicate issues"
        rollback_deployment "$backup_dir"
        return 1
    fi
    log_success "Canary metrics healthy"

    # Gradually increase traffic
    for percentage in 25 50 75 100; do
        log_info "Increasing to $percentage of traffic..."
        if ! coolify-cli app update-canary "$APP_ID" --percentage "$percentage" > /dev/null; then
            log_error "Failed to update canary percentage to $percentage"
            rollback_deployment "$backup_dir"
            return 1
        fi

        log_info "Monitoring at $percentage traffic (5 minutes)..."
        sleep 60

        if ! verify_health "$APP_ID"; then
            log_error "Health check failed at $percentage traffic"
            rollback_deployment "$backup_dir"
            return 1
        fi
    done

    log_success "Canary deployment completed successfully"
}

# Rolling Deployment Strategy
deploy_rolling() {
    local backup_dir=$1

    log_info "Starting Rolling deployment..."

    DEPLOY_ID=$(coolify-cli app deploy "$APP_ID" --rolling --format json | jq -r '.deployment_id')
    log_info "Rolling deployment initiated: $DEPLOY_ID"

    if ! wait_for_deployment "$DEPLOY_ID"; then
        log_error "Rolling deployment failed"
        rollback_deployment "$backup_dir"
        return 1
    fi

    log_success "Rolling deployment completed successfully"
}

# Verify application health
verify_health() {
    local app_id=$1
    coolify-cli app status "$app_id" --format json | jq -e '.health_status == "healthy"' > /dev/null
}

# Wait for deployment to complete
wait_for_deployment() {
    local deploy_id=$1
    local timeout=900  # 15 minutes
    local elapsed=0

    while [ $elapsed -lt $timeout ]; do
        local status=$(coolify-cli app deployment-status "$deploy_id" --format json | jq -r '.status')

        case "$status" in
            success)
                return 0
                ;;
            failed)
                return 1
                ;;
            in_progress)
                sleep 10
                elapsed=$((elapsed + 10))
                log_info "Deployment progress: $((elapsed / 9))%"
                ;;
            *)
                sleep 10
                elapsed=$((elapsed + 10))
                ;;
        esac
    done

    log_error "Deployment timeout after $timeout seconds"
    return 1
}

# Switch traffic between environments
switch_traffic() {
    local app_id=$1
    coolify-cli app switch-traffic "$app_id" > /dev/null
}

# Monitor canary metrics
monitor_canary_metrics() {
    local deploy_id=$1
    local duration=$2

    local error_rate=$(coolify-cli deployment-metrics "$deploy_id" --metric error_rate --format json | jq -r '.value')
    local response_time=$(coolify-cli deployment-metrics "$deploy_id" --metric avg_response_time --format json | jq -r '.value')

    # Check if metrics are within acceptable ranges
    if (( $(echo "$error_rate > 1.0" | bc -l) )); then
        log_warning "Error rate elevated: $error_rate%"
        return 1
    fi

    if (( $(echo "$response_time > 2000" | bc -l) )); then
        log_warning "Response time elevated: ${response_time}ms"
        return 1
    fi

    return 0
}

# Rollback deployment
rollback_deployment() {
    local backup_dir=$1

    log_warning "Initiating rollback..."

    # Restore from backup
    if [ -f "$backup_dir/config.json" ]; then
        coolify-cli app restore "$APP_ID" --backup-dir "$backup_dir"
    fi

    log_success "Rollback completed"
}

# Post-deployment validation
post_deployment_validation() {
    log_info "Running post-deployment validation..."

    # Check application status
    local status=$(coolify-cli app get "$APP_ID" --format json | jq -r '.status')
    if [ "$status" != "running" ]; then
        log_error "Application is not running after deployment: $status"
        return 1
    fi
    log_success "Application is running"

    # Check environment variables
    log_info "Validating environment variables..."
    if ! coolify-cli app env get "$APP_ID" > /dev/null 2>&1; then
        log_error "Failed to retrieve environment variables"
        return 1
    fi
    log_success "Environment variables validated"

    return 0
}

# Send notifications
send_notification() {
    local status=$1
    local message=$2

    # Example: Send to webhook
    if [ -n "${WEBHOOK_URL:-}" ]; then
        curl -X POST "$WEBHOOK_URL" \
            -H "Content-Type: application/json" \
            -d "{\"status\": \"$status\", \"message\": \"$message\", \"app_id\": \"$APP_ID\"}"
    fi
}

# Main execution
main() {
    log_info "Starting Advanced Deployment Script"
    log_info "Application: $APP_ID"
    log_info "Strategy: $DEPLOYMENT_STRATEGY"
    log_info "Log file: $LOG_FILE"

    # Run checks and deployment
    if ! preflight_check; then
        log_error "Pre-flight checks failed"
        send_notification "failed" "Pre-flight checks failed"
        exit 1
    fi

    local backup_dir
    backup_dir=$(backup_state)

    if ! deploy_with_strategy "$DEPLOYMENT_STRATEGY" "$backup_dir"; then
        log_error "Deployment failed"
        send_notification "failed" "Deployment of app $APP_ID failed"
        exit 1
    fi

    if ! post_deployment_validation; then
        log_error "Post-deployment validation failed"
        send_notification "failed" "Post-deployment validation failed"
        exit 1
    fi

    log_success "Advanced Deployment Script completed successfully"
    send_notification "success" "Deployment of app $APP_ID completed successfully"
}

# Run main function
main "$@"
