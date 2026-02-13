#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Deploy all applications sequentially with health verification

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_FILE="${SCRIPT_DIR}/deploy-all.log"

# Configuration
DEPLOY_TIMEOUT=600
HEALTH_CHECK_INTERVAL=5
HEALTH_CHECK_ATTEMPTS=10

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Logging functions
log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $*" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}✓ $*${NC}" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}✗ $*${NC}" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}⚠ $*${NC}" | tee -a "$LOG_FILE"
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."

    if ! command -v coolify-cli &> /dev/null; then
        log_error "coolify-cli not found in PATH"
        exit 1
    fi

    if [ -z "$COOLIFY_API_KEY" ]; then
        log_error "COOLIFY_API_KEY environment variable not set"
        exit 1
    fi

    if [ -z "$COOLIFY_API_URL" ]; then
        log_error "COOLIFY_API_URL environment variable not set"
        exit 1
    fi

    log_success "Prerequisites OK"
}

# Check system health
check_health() {
    log "Checking system health..."

    if ! coolify-cli health > /dev/null 2>&1; then
        log_error "System health check failed"
        return 1
    fi

    log_success "System health check passed"
    return 0
}

# Deploy a single application
deploy_app() {
    local app_id=$1
    local app_name=$2

    log "Deploying $app_name (ID: $app_id)..."

    # Deploy
    if ! coolify-cli app deploy "$app_id" --wait true --timeout "$DEPLOY_TIMEOUT"; then
        log_error "Deployment of $app_name failed"
        return 1
    fi

    log_success "Deployment of $app_name initiated"

    # Wait for health checks
    local attempt=0
    while [ $attempt -lt $HEALTH_CHECK_ATTEMPTS ]; do
        sleep $HEALTH_CHECK_INTERVAL

        if coolify-cli app get "$app_id" | grep -q "running"; then
            log_success "Health check passed for $app_name"
            return 0
        fi

        attempt=$((attempt + 1))
        log "Waiting for $app_name to be healthy (attempt $attempt/$HEALTH_CHECK_ATTEMPTS)"
    done

    log_error "Health check failed for $app_name after $HEALTH_CHECK_ATTEMPTS attempts"
    return 1
}

# Get list of applications to deploy
get_app_list() {
    # Get all running applications
    coolify-cli app list --format json | jq -r '.[] | "\(.id) \(.name)"' 2>/dev/null || true
}

# Main deployment function
main() {
    log "======================================"
    log "Coolify Deploy All - Starting"
    log "======================================"

    # Check prerequisites
    check_prerequisites

    # Check health
    if ! check_health; then
        log_error "Cannot proceed with deployment"
        exit 1
    fi

    # Get applications
    log "Retrieving application list..."
    local apps=$(get_app_list)

    if [ -z "$apps" ]; then
        log_warning "No applications found"
        exit 0
    fi

    # Deploy each application
    local success_count=0
    local fail_count=0
    local app_count=$(echo "$apps" | wc -l)

    log "Found $app_count applications to deploy"
    log ""

    while read -r app_id app_name; do
        if [ -z "$app_id" ] || [ -z "$app_name" ]; then
            continue
        fi

        if deploy_app "$app_id" "$app_name"; then
            success_count=$((success_count + 1))
        else
            fail_count=$((fail_count + 1))
            log_warning "Failed to deploy $app_name, continuing with next..."
        fi

        # Wait between deployments
        sleep 10
    done <<< "$apps"

    log ""
    log "======================================"
    log "Deployment Summary"
    log "======================================"
    log "Total: $app_count | Success: $success_count | Failed: $fail_count"
    log "======================================"

    if [ $fail_count -eq 0 ]; then
        log_success "All deployments completed successfully!"
        exit 0
    else
        log_error "Some deployments failed"
        exit 1
    fi
}

# Run main function
main "$@"
