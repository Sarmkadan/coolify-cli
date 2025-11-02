#!/bin/bash

# =============================================================================
# Multi-Environment Deployment Script
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

# Configuration files for different environments
declare -A ENVIRONMENTS=(
    [dev]="configs/dev.env"
    [staging]="configs/staging.env"
    [prod]="configs/prod.env"
)

declare -A APP_IDS=(
    [dev]="1,2,3"
    [staging]="4,5,6"
    [prod]="7,8,9"
)

LOG_DIR="logs/deployment-$(date +%Y%m%d-%H%M%S)"
mkdir -p "$LOG_DIR"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Logging
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1" | tee -a "$LOG_DIR/main.log"
}

log_success() {
    echo -e "${GREEN}[✓]${NC} $1" | tee -a "$LOG_DIR/main.log"
}

log_error() {
    echo -e "${RED}[✗]${NC} $1" | tee -a "$LOG_DIR/main.log"
}

log_warning() {
    echo -e "${YELLOW}[!]${NC} $1" | tee -a "$LOG_DIR/main.log"
}

# Load environment configuration
load_environment() {
    local env=$1

    if [ ! -f "${ENVIRONMENTS[$env]}" ]; then
        log_error "Environment config not found: ${ENVIRONMENTS[$env]}"
        return 1
    fi

    # Source the environment file
    set -a
    source "${ENVIRONMENTS[$env]}"
    set +a

    log_success "Loaded environment: $env"
}

# Deploy to single environment
deploy_environment() {
    local env=$1
    local env_log="$LOG_DIR/$env.log"

    log_info "Deploying to $env environment..."
    {
        log_info "Starting $env deployment"

        if ! load_environment "$env"; then
            log_error "Failed to load environment: $env"
            return 1
        fi

        # Get app IDs for this environment
        local app_ids="${APP_IDS[$env]}"
        IFS=',' read -ra APPS <<< "$app_ids"

        local failed=0
        local succeeded=0

        for app_id in "${APPS[@]}"; do
            app_id=$(echo "$app_id" | xargs)  # Trim whitespace

            log_info "Deploying app $app_id..."

            # Pre-flight check
            if ! coolify-cli app get "$app_id" > /dev/null 2>&1; then
                log_error "Application $app_id not found"
                failed=$((failed + 1))
                continue
            fi

            # Backup current state
            local backup_dir="backups/$env/app-$app_id-$(date +%s)"
            mkdir -p "$backup_dir"
            coolify-cli app get "$app_id" --format json > "$backup_dir/config.json"

            # Deploy
            if ! DEPLOY_ID=$(coolify-cli app deploy "$app_id" --format json 2>"$LOG_DIR/$app_id.error" | jq -r '.deployment_id'); then
                log_error "Failed to deploy app $app_id"
                failed=$((failed + 1))
                continue
            fi

            log_info "Deployment initiated: $DEPLOY_ID"

            # Wait for deployment
            local timeout=600
            local elapsed=0
            local deploy_success=false

            while [ $elapsed -lt $timeout ]; do
                local status=$(coolify-cli app deployment-status "$DEPLOY_ID" --format json 2>/dev/null | jq -r '.status' || echo "unknown")

                case "$status" in
                    success)
                        deploy_success=true
                        break
                        ;;
                    failed)
                        break
                        ;;
                    *)
                        sleep 10
                        elapsed=$((elapsed + 10))
                        ;;
                esac
            done

            if [ "$deploy_success" = true ]; then
                log_success "App $app_id deployed successfully"
                succeeded=$((succeeded + 1))
            else
                log_error "App $app_id deployment timed out or failed"
                failed=$((failed + 1))
            fi

            # Health check
            sleep 5
            if coolify-cli app status "$app_id" --format json | jq -e '.health_status == "healthy"' > /dev/null; then
                log_success "App $app_id health check passed"
            else
                log_warning "App $app_id health check failed"
            fi
        done

        log_info "$env deployment completed: $succeeded succeeded, $failed failed"

    } | tee -a "$env_log"

    return $([ $failed -eq 0 ] && echo 0 || echo 1)
}

# Validate environment
validate_environment() {
    local env=$1

    log_info "Validating $env environment..."

    if ! load_environment "$env"; then
        return 1
    fi

    # Check API connectivity
    if ! coolify-cli health > /dev/null 2>&1; then
        log_error "Cannot connect to Coolify API in $env"
        return 1
    fi

    log_success "$env environment validation passed"
}

# Promote to next environment
promote_environment() {
    local source_env=$1
    local target_env=$2

    log_info "Promoting from $source_env to $target_env..."

    # Get app IDs from source
    local source_apps="${APP_IDS[$source_env]}"
    IFS=',' read -ra SOURCE_APP_ARRAY <<< "$source_apps"

    # Verify all source deployments successful
    for app_id in "${SOURCE_APP_ARRAY[@]}"; do
        app_id=$(echo "$app_id" | xargs)
        if ! coolify-cli app status "$app_id" --format json | jq -e '.status == "running"' > /dev/null; then
            log_error "Source app $app_id is not running, cannot promote"
            return 1
        fi
    done

    log_success "All source applications verified, ready to promote to $target_env"
    return 0
}

# Rollback environment
rollback_environment() {
    local env=$1
    local backup_timestamp=$2

    log_info "Rolling back $env environment to backup: $backup_timestamp..."

    local app_ids="${APP_IDS[$env]}"
    IFS=',' read -ra APPS <<< "$app_ids"

    for app_id in "${APPS[@]}"; do
        app_id=$(echo "$app_id" | xargs)
        local backup_dir="backups/$env/app-$app_id-$backup_timestamp"

        if [ -d "$backup_dir" ]; then
            log_info "Restoring app $app_id from backup..."
            coolify-cli app restore "$app_id" --backup-dir "$backup_dir"
        fi
    done

    log_success "Rollback completed"
}

# Generate deployment report
generate_report() {
    log_info "Generating deployment report..."

    local report="$LOG_DIR/deployment-report.md"

    {
        echo "# Deployment Report"
        echo "Generated: $(date)"
        echo ""
        echo "## Deployment Summary"
        echo ""

        for env in dev staging prod; do
            echo "### $env Environment"
            echo ""

            local env_log="$LOG_DIR/$env.log"
            if [ -f "$env_log" ]; then
                local success_count=$(grep -c "deployed successfully" "$env_log" || echo "0")
                local fail_count=$(grep -c "Failed to deploy" "$env_log" || echo "0")

                echo "- Successfully deployed: $success_count applications"
                echo "- Failed deployments: $fail_count"
                echo ""
            fi
        done

        echo "## Environment Status"
        echo ""

        for env in dev staging prod; do
            echo "### $env Environment"
            if load_environment "$env"; then
                local app_ids="${APP_IDS[$env]}"
                IFS=',' read -ra APPS <<< "$app_ids"

                for app_id in "${APPS[@]}"; do
                    app_id=$(echo "$app_id" | xargs)
                    local status=$(coolify-cli app get "$app_id" --format json 2>/dev/null | jq -r '.status' || echo "unknown")
                    echo "- App $app_id: $status"
                done
            fi
            echo ""
        done

        echo "## Logs"
        echo ""
        echo "- Main log: $LOG_DIR/main.log"
        for env in dev staging prod; do
            echo "- $env log: $LOG_DIR/$env.log"
        done

    } > "$report"

    log_success "Report generated: $report"
}

# Parallel deployment with monitoring
parallel_deploy_all() {
    log_info "Starting parallel deployment to all environments..."

    local pids=()
    local envs=(dev staging prod)

    # Start deployments in parallel
    for env in "${envs[@]}"; do
        (deploy_environment "$env") &
        pids+=($!)
    done

    log_info "Waiting for all deployments to complete..."

    local failed=0
    for i in "${!pids[@]}"; do
        local pid=${pids[$i]}
        local env=${envs[$i]}

        if wait $pid; then
            log_success "$env environment deployment completed successfully"
        else
            log_error "$env environment deployment failed"
            failed=$((failed + 1))
        fi
    done

    return $([ $failed -eq 0 ] && echo 0 || echo 1)
}

# Main menu
show_menu() {
    echo ""
    echo -e "${BLUE}=== Multi-Environment Deployment ===${NC}"
    echo "1. Deploy dev environment"
    echo "2. Deploy staging environment"
    echo "3. Deploy production environment"
    echo "4. Deploy all environments (parallel)"
    echo "5. Validate environments"
    echo "6. Promote dev → staging"
    echo "7. Promote staging → production"
    echo "8. Rollback environment"
    echo "9. Generate report"
    echo "0. Exit"
    echo ""
}

# Interactive mode
main_interactive() {
    while true; do
        show_menu
        read -p "Select option: " choice

        case $choice in
            1) deploy_environment "dev" ;;
            2) deploy_environment "staging" ;;
            3) deploy_environment "prod" ;;
            4) parallel_deploy_all ;;
            5)
                for env in dev staging prod; do
                    validate_environment "$env"
                done
                ;;
            6) promote_environment "dev" "staging" ;;
            7) promote_environment "staging" "prod" ;;
            8)
                read -p "Environment to rollback (dev/staging/prod): " env
                read -p "Backup timestamp: " ts
                rollback_environment "$env" "$ts"
                ;;
            9) generate_report ;;
            0)
                log_info "Exiting..."
                exit 0
                ;;
            *)
                log_error "Invalid option"
                ;;
        esac
    done
}

# Non-interactive mode (full deployment)
main_automated() {
    log_info "Starting automated multi-environment deployment"

    # Deploy to all environments in sequence
    for env in dev staging prod; do
        if ! deploy_environment "$env"; then
            log_error "Deployment to $env failed, stopping"
            exit 1
        fi

        # Small delay between environment deployments
        sleep 5
    done

    generate_report

    log_success "Multi-environment deployment completed successfully"
}

# Determine mode and run
if [ "${1:-}" = "auto" ]; then
    main_automated
else
    main_interactive
fi
