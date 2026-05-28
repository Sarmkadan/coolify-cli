#!/bin/bash

# =============================================================================
# Environment Variable Sync Script
# Synchronize environment variables across applications and environments
# =============================================================================

set -euo pipefail

SOURCE_APP_ID="${1:?Error: SOURCE_APP_ID required}"
TARGET_APP_IDS="${2:?Error: TARGET_APP_IDS required (comma-separated)}"

LOG_FILE="env-sync-$(date +%Y%m%d-%H%M%S).log"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1" | tee -a "$LOG_FILE"; }
log_success() { echo -e "${GREEN}[✓]${NC} $1" | tee -a "$LOG_FILE"; }
log_error() { echo -e "${RED}[✗]${NC} $1" | tee -a "$LOG_FILE"; }
log_warning() { echo -e "${YELLOW}[!]${NC} $1" | tee -a "$LOG_FILE"; }

# Export environment variables from source app
export_env_vars() {
    local app_id=$1
    local export_file="env-backup-$app_id-$(date +%s).json"

    log_info "Exporting environment variables from app $app_id..."

    if ! coolify-cli app env get "$app_id" --format json > "$export_file"; then
        log_error "Failed to export environment variables from app $app_id"
        return 1
    fi

    log_success "Environment variables exported to $export_file"
    echo "$export_file"
}

# Import environment variables to target app
import_env_vars() {
    local target_app_id=$1
    local source_file=$2
    local skip_vars="${3:-}"  # Comma-separated list of variables to skip

    log_info "Importing environment variables to app $target_app_id..."

    # Parse JSON and import variables
    if [ -f "$source_file" ]; then
        # Convert JSON to key=value format
        jq -r '.[] | "\(.key)=\(.value)"' "$source_file" | while IFS='=' read -r key value; do
            # Skip certain variables if specified
            if [ -n "$skip_vars" ] && echo "$skip_vars" | grep -q "$key"; then
                log_warning "Skipping $key (in skip list)"
                continue
            fi

            # App-specific variable overrides
            case "$key" in
                APP_ENV)
                    # Override environment for target app
                    value="$target_app_id-env"
                    ;;
                DATABASE_URL|REDIS_URL)
                    # These should be customized per environment
                    log_warning "Skipping $key (requires manual configuration)"
                    continue
                    ;;
            esac

            log_info "Setting $key..."
            if ! coolify-cli app env set "$target_app_id" "$key" "$value"; then
                log_error "Failed to set $key for app $target_app_id"
                return 1
            fi
        done

        log_success "Environment variables imported to app $target_app_id"
        return 0
    else
        log_error "Source file not found: $source_file"
        return 1
    fi
}

# Sync environment variables between apps
sync_env_vars() {
    local source_app=$1
    local target_app=$2
    local skip_vars="${3:-}"

    log_info "Syncing environment variables from app $source_app to app $target_app..."

    # Export source app variables
    local export_file
    export_file=$(export_env_vars "$source_app")

    # Import to target app
    if ! import_env_vars "$target_app" "$export_file" "$skip_vars"; then
        return 1
    fi

    # Cleanup
    rm -f "$export_file"
    return 0
}

# Compare environment variables between apps
compare_env_vars() {
    local app1=$1
    local app2=$2

    log_info "Comparing environment variables between app $app1 and app $app2..."

    local file1="env-compare-$app1-$(date +%s).json"
    local file2="env-compare-$app2-$(date +%s).json"

    # Export both apps' environments
    coolify-cli app env get "$app1" --format json | jq 'sort_by(.key)' > "$file1"
    coolify-cli app env get "$app2" --format json | jq 'sort_by(.key)' > "$file2"

    # Compare
    {
        echo "=== Environment Variables Comparison ==="
        echo ""

        echo "Variables in app $app1 but not in app $app2:"
        jq -r '.[].key' "$file1" | while read var; do
            if ! jq -r '.[].key' "$file2" | grep -q "^$var$"; then
                echo "  - $var"
            fi
        done

        echo ""
        echo "Variables in app $app2 but not in app $app1:"
        jq -r '.[].key' "$file2" | while read var; do
            if ! jq -r '.[].key' "$file1" | grep -q "^$var$"; then
                echo "  - $var"
            fi
        done

        echo ""
        echo "Variables with different values:"
        jq -r '.[] | "\(.key)=\(.value)"' "$file1" | while IFS='=' read -r key val1; do
            val2=$(jq -r ".[] | select(.key==\"$key\") | .value" "$file2")
            if [ -n "$val2" ] && [ "$val1" != "$val2" ]; then
                echo "  - $key"
                echo "    App $app1: $val1"
                echo "    App $app2: $val2"
            fi
        done

    } | tee -a "$LOG_FILE"

    # Cleanup
    rm -f "$file1" "$file2"
}

# Validate environment variables
validate_env_vars() {
    local app_id=$1
    local required_vars="${2:-}"  # Comma-separated list of required variables

    log_info "Validating environment variables for app $app_id..."

    local env_file="env-validate-$app_id-$(date +%s).json"
    coolify-cli app env get "$app_id" --format json > "$env_file"

    local missing=0

    if [ -n "$required_vars" ]; then
        IFS=',' read -ra VARS <<< "$required_vars"

        for var in "${VARS[@]}"; do
            var=$(echo "$var" | xargs)  # Trim whitespace

            if ! jq -e ".[] | select(.key==\"$var\")" "$env_file" > /dev/null; then
                log_warning "Missing required variable: $var"
                missing=$((missing + 1))
            fi
        done
    fi

    # Check for potentially sensitive variables
    local sensitive_vars=("PASSWORD" "TOKEN" "KEY" "SECRET" "CREDENTIAL")
    for pattern in "${sensitive_vars[@]}"; do
        local count=$(jq -r '.[].key' "$env_file" | grep -c "$pattern" || echo "0")
        if [ "$count" -gt 0 ]; then
            log_info "Found $count sensitive variables (pattern: $pattern)"
        fi
    done

    # Check for empty variables
    local empty=$(jq -r '.[] | select(.value=="") | .key' "$env_file" | wc -l)
    if [ "$empty" -gt 0 ]; then
        log_warning "Found $empty empty variables"
    fi

    rm -f "$env_file"

    if [ $missing -gt 0 ]; then
        log_error "Validation failed: $missing required variables missing"
        return 1
    fi

    log_success "Validation passed"
    return 0
}

# Generate environment summary
generate_summary() {
    local app_ids=$1

    log_info "Generating environment summary..."

    {
        echo "=== Environment Variables Summary ==="
        echo "Generated: $(date)"
        echo ""

        IFS=',' read -ra APPS <<< "$app_ids"

        for app_id in "${APPS[@]}"; do
            app_id=$(echo "$app_id" | xargs)

            echo "### App $app_id"
            echo ""

            local env_file="env-summary-$app_id-$(date +%s).json"
            if coolify-cli app env get "$app_id" --format json > "$env_file"; then
                local var_count=$(jq '. | length' "$env_file")
                echo "Total variables: $var_count"
                echo ""

                echo "Variables:"
                jq -r '.[] | "  - \(.key)"' "$env_file"
                echo ""

                rm -f "$env_file"
            fi
        done

    } | tee -a "$LOG_FILE"
}

# Batch sync to multiple apps
batch_sync() {
    local source_app=$1
    local target_apps=$2
    local skip_vars="${3:-}"

    log_info "Starting batch sync from app $source_app to multiple apps..."

    local success=0
    local failed=0

    IFS=',' read -ra TARGETS <<< "$target_apps"

    for target_app in "${TARGETS[@]}"; do
        target_app=$(echo "$target_app" | xargs)

        if sync_env_vars "$source_app" "$target_app" "$skip_vars"; then
            success=$((success + 1))
        else
            failed=$((failed + 1))
        fi
    done

    log_info "Batch sync completed: $success succeeded, $failed failed"
    return $([ $failed -eq 0 ] && echo 0 || echo 1)
}

# Dry-run mode to preview changes
dry_run() {
    local source_app=$1
    local target_app=$2

    log_info "Running in DRY-RUN mode (no changes will be made)..."

    local export_file="env-dryrun-$(date +%s).json"
    coolify-cli app env get "$source_app" --format json > "$export_file"

    echo ""
    echo "=== DRY-RUN: Variables to be synced ==="
    jq -r '.[] | "\(.key) = \(.value)"' "$export_file"
    echo ""

    echo "=== DRY-RUN: Target app $target_app ==="
    echo "Current variables:"
    coolify-cli app env get "$target_app" --format json | jq -r '.[] | "\(.key) = \(.value)"'
    echo ""

    echo "=== DRY-RUN: Changes that would be made ==="
    echo "The above source variables would be imported to app $target_app"
    echo ""

    rm -f "$export_file"
}

# Main execution
main() {
    log_info "Environment Variable Sync Script"
    log_info "Source App: $SOURCE_APP_ID"
    log_info "Target Apps: $TARGET_APP_IDS"

    # Check if source app exists
    if ! coolify-cli app get "$SOURCE_APP_ID" > /dev/null 2>&1; then
        log_error "Source application $SOURCE_APP_ID not found"
        exit 1
    fi

    # Validate target apps
    IFS=',' read -ra TARGETS <<< "$TARGET_APP_IDS"
    for target_app in "${TARGETS[@]}"; do
        target_app=$(echo "$target_app" | xargs)
        if ! coolify-cli app get "$target_app" > /dev/null 2>&1; then
            log_error "Target application $target_app not found"
            exit 1
        fi
    done

    # Run batch sync
    if ! batch_sync "$SOURCE_APP_ID" "$TARGET_APP_IDS" "PASSWORD,API_KEY"; then
        log_error "Batch sync failed"
        exit 1
    fi

    # Generate summary
    generate_summary "$SOURCE_APP_ID,$TARGET_APP_IDS"

    log_success "Environment variable synchronization completed"
    log_info "Log file: $LOG_FILE"
}

main "$@"
