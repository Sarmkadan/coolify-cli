#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Analyze application logs for errors, warnings, and patterns

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${SCRIPT_DIR}/log-analysis"
LOG_FILE="${OUTPUT_DIR}/analysis.log"

# Configuration
LINES_TO_ANALYZE=1000
ERROR_THRESHOLD=10
WARN_THRESHOLD=50

mkdir -p "$OUTPUT_DIR"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Logging
log() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $*" | tee -a "$LOG_FILE"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $*" | tee -a "$LOG_FILE"
}

log_ok() {
    echo -e "${GREEN}[OK]${NC} $*" | tee -a "$LOG_FILE"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $*" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $*" | tee -a "$LOG_FILE"
}

# Get list of applications
get_applications() {
    coolify-cli app list --format json 2>/dev/null | jq -r '.[] | "\(.id) \(.name)"' || echo ""
}

# Analyze logs for a single application
analyze_app_logs() {
    local app_id=$1
    local app_name=$2

    log_info "Analyzing logs for $app_name..."

    # Get logs
    local logs=$(coolify-cli logs "$app_id" --lines $LINES_TO_ANALYZE 2>/dev/null || echo "")

    if [ -z "$logs" ]; then
        log_warn "No logs found for $app_name"
        return 1
    fi

    # Create report file
    local report_file="${OUTPUT_DIR}/${app_name}_analysis_$(date +%Y%m%d_%H%M%S).txt"

    # Analyze logs
    local error_count=$(echo "$logs" | grep -i "error" | wc -l)
    local warn_count=$(echo "$logs" | grep -i "warning\|warn" | wc -l)
    local info_count=$(echo "$logs" | grep -i "info" | wc -l)
    local debug_count=$(echo "$logs" | grep -i "debug" | wc -l)

    # Generate report
    {
        echo "========================================="
        echo "Log Analysis Report"
        echo "Application: $app_name (ID: $app_id)"
        echo "Generated: $(date)"
        echo "========================================="
        echo ""
        echo "Summary Statistics:"
        echo "  Total Errors: $error_count"
        echo "  Total Warnings: $warn_count"
        echo "  Total Info: $info_count"
        echo "  Total Debug: $debug_count"
        echo ""

        # Check thresholds
        if [ $error_count -gt $ERROR_THRESHOLD ]; then
            echo "⚠ Alert: Error count ($error_count) exceeds threshold ($ERROR_THRESHOLD)"
            echo ""
        fi

        if [ $warn_count -gt $WARN_THRESHOLD ]; then
            echo "⚠ Alert: Warning count ($warn_count) exceeds threshold ($WARN_THRESHOLD)"
            echo ""
        fi

        # Top errors
        echo "Top Errors:"
        echo "$logs" | grep -i "error" | head -5 | sed 's/^/  /'
        echo ""

        # Top warnings
        echo "Top Warnings:"
        echo "$logs" | grep -i "warning\|warn" | head -5 | sed 's/^/  /'
        echo ""

        # Recent entries
        echo "Most Recent Entries:"
        echo "$logs" | tail -10 | sed 's/^/  /'
        echo ""
        echo "========================================="
    } | tee "$report_file"

    log_ok "Analysis complete for $app_name"
    log_info "Report saved to $report_file"

    # Return status based on thresholds
    if [ $error_count -gt $ERROR_THRESHOLD ]; then
        return 1
    fi

    return 0
}

# Find error patterns
find_error_patterns() {
    local app_id=$1
    local app_name=$2

    log_info "Analyzing error patterns for $app_name..."

    local logs=$(coolify-cli logs "$app_id" --lines $LINES_TO_ANALYZE --filter ERROR 2>/dev/null || echo "")

    if [ -z "$logs" ]; then
        return 0
    fi

    local pattern_file="${OUTPUT_DIR}/${app_name}_patterns_$(date +%Y%m%d_%H%M%S).txt"

    {
        echo "Error Patterns for $app_name"
        echo "======================================"
        echo ""

        # Find common error types
        echo "Error Type Frequency:"
        echo "$logs" | grep -o "Error: [^:]*" | sort | uniq -c | sort -rn | head -10

        echo ""
        echo "Error Messages Frequency:"
        echo "$logs" | grep -i "error" | grep -o '\[[^]]*\]' | sort | uniq -c | sort -rn | head -10

        echo ""
    } | tee "$pattern_file"
}

# Generate HTML report
generate_html_report() {
    local report_date=$(date '+%Y-%m-%d %H:%M:%S')
    local html_file="${OUTPUT_DIR}/index.html"

    {
        cat <<EOF
<!DOCTYPE html>
<html>
<head>
    <title>Coolify Log Analysis Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; }
        h1 { color: #333; }
        .summary { background: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; }
        .stat-box { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 5px; }
        .stat-box.error { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }
        .stat-box.warn { background: linear-gradient(135deg, #fa709a 0%, #fee140 100%); }
        .stat-number { font-size: 32px; font-weight: bold; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f2f2f2; font-weight: bold; }
        tr:hover { background: #f5f5f5; }
        .footer { margin-top: 40px; color: #666; font-size: 12px; border-top: 1px solid #eee; padding-top: 20px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Coolify Log Analysis Report</h1>
        <p>Generated: $report_date</p>

        <div class="summary">
            <h2>Analysis Results</h2>
            <p>This report contains analysis of application logs for error patterns, warnings, and statistics.</p>
        </div>

        <div class="stats" id="stats">
            <!-- Populated by script -->
        </div>

        <h2>Applications Analyzed</h2>
        <table>
            <thead>
                <tr>
                    <th>Application</th>
                    <th>Errors</th>
                    <th>Warnings</th>
                    <th>Status</th>
                    <th>Report</th>
                </tr>
            </thead>
            <tbody id="apps">
                <!-- Populated by script -->
            </tbody>
        </table>

        <div class="footer">
            <p>Coolify CLI - Log Analysis Tool</p>
            <p>For more information, visit <a href="https://github.com/Sarmkadan/coolify-cli">github.com/Sarmkadan/coolify-cli</a></p>
        </div>
    </div>
</body>
</html>
EOF
    } > "$html_file"

    log_ok "HTML report generated: $html_file"
}

# Main analysis function
main() {
    log "======================================"
    log "Coolify Log Analysis - Starting"
    log "======================================"
    log "Analysis Date: $(date)"
    log "Lines to analyze: $LINES_TO_ANALYZE"
    log "Error Threshold: $ERROR_THRESHOLD"
    log "Warning Threshold: $WARN_THRESHOLD"
    log ""

    # Get applications
    local apps=$(get_applications)

    if [ -z "$apps" ]; then
        log_warn "No applications found"
        exit 0
    fi

    # Analyze each application
    local app_count=0
    local analyzed_count=0
    local failed_count=0

    while read -r app_id app_name; do
        if [ -z "$app_id" ] || [ -z "$app_name" ]; then
            continue
        fi

        app_count=$((app_count + 1))

        if analyze_app_logs "$app_id" "$app_name"; then
            analyzed_count=$((analyzed_count + 1))
            find_error_patterns "$app_id" "$app_name"
        else
            failed_count=$((failed_count + 1))
        fi

        echo ""
    done <<< "$apps"

    # Generate HTML report
    generate_html_report

    # Summary
    log ""
    log "======================================"
    log "Analysis Complete"
    log "======================================"
    log "Total Applications: $app_count"
    log "Successfully Analyzed: $analyzed_count"
    log "With Issues: $failed_count"
    log "Output Directory: $OUTPUT_DIR"
    log "======================================"
}

# Run main
main "$@"
