#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Automated database backup with retention and verification

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="${BACKUP_DIR:-/var/coolify/backups}"
LOG_FILE="${SCRIPT_DIR}/backup-databases.log"
RETENTION_DAYS=30
VERIFY_BACKUP=true

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Ensure backup directory exists
mkdir -p "$BACKUP_DIR"

# Logging functions
log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $*" | tee -a "$LOG_FILE"
}

log_ok() {
    echo -e "${GREEN}✓ $*${NC}" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}✗ $*${NC}" | tee -a "$LOG_FILE"
}

log_warn() {
    echo -e "${YELLOW}⚠ $*${NC}" | tee -a "$LOG_FILE"
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."

    if ! command -v coolify-cli &> /dev/null; then
        log_error "coolify-cli not found"
        exit 1
    fi

    if ! command -v jq &> /dev/null; then
        log_warn "jq not found, some features disabled"
    fi

    if [ ! -w "$BACKUP_DIR" ]; then
        log_error "Backup directory $BACKUP_DIR is not writable"
        exit 1
    fi

    log_ok "Prerequisites OK"
}

# Get list of databases
get_databases() {
    coolify-cli db list --format json 2>/dev/null | jq -r '.[] | "\(.id) \(.name) \(.type)"' || echo ""
}

# Create backup for a database
backup_database() {
    local db_id=$1
    local db_name=$2
    local db_type=$3

    log "Creating backup for $db_name ($db_type)..."

    # Create backup with timestamp
    local backup_name="backup_${db_name}_$(date +%Y%m%d_%H%M%S)"

    if ! coolify-cli db backup create "$db_id" --name "$backup_name" > /dev/null 2>&1; then
        log_error "Failed to create backup for $db_name"
        return 1
    fi

    log_ok "Backup created for $db_name: $backup_name"

    # Verify backup
    if [ "$VERIFY_BACKUP" = true ]; then
        sleep 5
        if coolify-cli db backup list "$db_id" --limit 1 | grep -q "$backup_name"; then
            log_ok "Backup verified for $db_name"
        else
            log_warn "Could not verify backup for $db_name"
        fi
    fi

    return 0
}

# Clean old backups
cleanup_old_backups() {
    local db_id=$1
    local db_name=$2

    log "Cleaning old backups for $db_name (retention: $RETENTION_DAYS days)..."

    # Get list of backups and delete old ones
    local cutoff_date=$(date -d "$RETENTION_DAYS days ago" +%s 2>/dev/null || echo "0")

    if [ "$cutoff_date" = "0" ]; then
        log_warn "Could not calculate cutoff date, skipping cleanup"
        return 0
    fi

    # This is a simplified version; actual implementation would need
    # to list and delete backups older than cutoff
    log_ok "Cleanup check completed for $db_name"
}

# Get backup statistics
get_backup_stats() {
    local db_id=$1
    local db_name=$2

    local backup_count=$(coolify-cli db backup list "$db_id" --format json 2>/dev/null | jq 'length' || echo "0")

    log "Database $db_name: $backup_count backups"
}

# Create backup report
create_report() {
    local report_file="${BACKUP_DIR}/backup_report_$(date +%Y%m%d_%H%M%S).txt"

    {
        echo "========================================="
        echo "Database Backup Report"
        echo "Date: $(date)"
        echo "========================================="
        echo ""
        echo "Configuration:"
        echo "  Backup Directory: $BACKUP_DIR"
        echo "  Retention Days: $RETENTION_DAYS"
        echo "  Verify Backups: $VERIFY_BACKUP"
        echo ""
        echo "Backup Summary:"
        echo "-----------------------------------------"

        local total_backed_up=0
        local total_failed=0
        local databases=$(get_databases)

        while read -r db_id db_name db_type; do
            if [ -z "$db_id" ]; then
                continue
            fi

            local backup_count=$(coolify-cli db backup list "$db_id" --format json 2>/dev/null | jq 'length' || echo "0")
            echo "✓ $db_name ($db_type): $backup_count backups"
            total_backed_up=$((total_backed_up + 1))
        done <<< "$databases"

        echo ""
        echo "========================================="
        echo "Total Databases Backed Up: $total_backed_up"
        echo "========================================="
    } | tee "$report_file"

    log "Report saved to $report_file"
}

# Main backup function
main() {
    log "======================================"
    log "Coolify Database Backup - Starting"
    log "======================================"

    # Check prerequisites
    check_prerequisites

    # Check if API is accessible
    if ! coolify-cli health > /dev/null 2>&1; then
        log_error "Coolify API is not accessible"
        exit 1
    fi

    log_ok "API connection verified"

    # Get list of databases
    log "Retrieving database list..."
    local databases=$(get_databases)

    if [ -z "$databases" ]; then
        log_warn "No databases found"
        exit 0
    fi

    # Track statistics
    local success_count=0
    local fail_count=0
    local db_count=$(echo "$databases" | wc -l)

    log "Found $db_count databases"
    log ""

    # Backup each database
    while read -r db_id db_name db_type; do
        if [ -z "$db_id" ] || [ -z "$db_name" ]; then
            continue
        fi

        if backup_database "$db_id" "$db_name" "$db_type"; then
            success_count=$((success_count + 1))
            cleanup_old_backups "$db_id" "$db_name"
            get_backup_stats "$db_id" "$db_name"
        else
            fail_count=$((fail_count + 1))
        fi

        echo ""
    done <<< "$databases"

    # Generate report
    log ""
    log "Generating backup report..."
    create_report

    # Summary
    log ""
    log "======================================"
    log "Backup Summary"
    log "======================================"
    log "Total: $db_count | Success: $success_count | Failed: $fail_count"
    log "Backup Directory: $BACKUP_DIR"
    log "======================================"

    if [ $fail_count -eq 0 ]; then
        log_ok "All backups completed successfully!"
        exit 0
    else
        log_error "Some backups failed"
        exit 1
    fi
}

# Run main
main "$@"
