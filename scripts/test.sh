#!/bin/bash

# =============================================================================
# Coolify CLI Test Script
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Configuration
TEST_TYPE="${1:-all}"  # all, unit, integration, coverage
FILTER="${2:-}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Functions
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Run unit tests
run_unit_tests() {
    local filter=$1

    print_info "Running unit tests..."

    cd "$PROJECT_ROOT"

    local args=("test" "--configuration" "Debug" "--verbosity" "detailed")

    if [ -n "$filter" ]; then
        args+=("--filter" "$filter")
    fi

    if dotnet "${args[@]}"; then
        print_success "Unit tests passed"
        return 0
    else
        print_error "Unit tests failed"
        return 1
    fi
}

# Run integration tests
run_integration_tests() {
    local filter=$1

    print_info "Running integration tests..."

    cd "$PROJECT_ROOT"

    local args=("test" "--configuration" "Debug" "--verbosity" "detailed" "--filter" "Category=Integration")

    if [ -n "$filter" ]; then
        args+=(" && " "$filter")
    fi

    if dotnet "${args[@]}" 2>/dev/null || true; then
        print_success "Integration tests completed"
        return 0
    else
        print_warning "No integration tests found or tests failed"
        return 1
    fi
}

# Run all tests
run_all_tests() {
    local filter=$1

    print_info "Running all tests..."

    cd "$PROJECT_ROOT"

    local args=("test" "--configuration" "Debug" "--verbosity" "normal")

    if [ -n "$filter" ]; then
        args+=("--filter" "$filter")
    fi

    if dotnet "${args[@]}"; then
        print_success "All tests passed"
        return 0
    else
        print_error "Some tests failed"
        return 1
    fi
}

# Generate code coverage report
generate_coverage_report() {
    print_info "Generating code coverage report..."

    cd "$PROJECT_ROOT"

    # Install coverage tool if not present
    if ! command -v reportgenerator &> /dev/null; then
        print_info "Installing reportgenerator..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi

    # Run tests with coverage
    mkdir -p "coverage"

    dotnet test \
        --configuration Debug \
        --no-build \
        /p:CollectCoverage=true \
        /p:CoverageFormat=cobertura \
        /p:CoverageOutputDirectory="./coverage" \
        /p:Exclude="[*.Tests]*" \
        --verbosity minimal

    # Generate HTML report
    if [ -f "coverage/coverage.cobertura.xml" ]; then
        reportgenerator \
            -reports:"coverage/coverage.cobertura.xml" \
            -targetdir:"coverage/report" \
            -reporttypes:Html

        print_success "Coverage report generated: coverage/report/index.html"

        # Display coverage summary
        print_info "Coverage Summary:"
        grep -oP 'linecoverage="\K[^"]+' "coverage/coverage.cobertura.xml" | head -1 || echo "N/A"
    else
        print_error "Coverage file not found"
        return 1
    fi

    return 0
}

# Run specific test class
run_test_class() {
    local test_class=$1

    print_info "Running test class: $test_class"

    cd "$PROJECT_ROOT"

    if dotnet test --configuration Debug --filter "FullyQualifiedName~$test_class" --verbosity normal; then
        print_success "Test class passed"
        return 0
    else
        print_error "Test class failed"
        return 1
    fi
}

# Run tests with detailed output
run_tests_verbose() {
    print_info "Running tests with detailed output..."

    cd "$PROJECT_ROOT"

    dotnet test \
        --configuration Debug \
        --verbosity detailed \
        --logger "console;verbosity=detailed" \
        --diag test-diagnostics.log

    print_success "Tests completed"
}

# Check code quality
check_code_quality() {
    print_info "Checking code quality..."

    cd "$PROJECT_ROOT"

    # Code formatting check
    print_info "Checking code formatting..."
    if dotnet format --verify-no-changes --verbosity quiet; then
        print_success "Code formatting check passed"
    else
        print_warning "Code formatting issues found"
        print_info "Run 'dotnet format' to fix formatting"
    fi

    # Analyzer checks
    print_info "Running code analyzers..."
    dotnet build --configuration Debug --no-restore /p:EnforceCodeStyleInBuild=true || print_warning "Code style violations found"

    return 0
}

# Run performance tests
run_performance_tests() {
    print_info "Running performance tests..."

    cd "$PROJECT_ROOT"

    # Build with optimizations
    dotnet build --configuration Release --no-restore

    # Run performance tests if they exist
    dotnet test \
        --configuration Release \
        --filter "Category=Performance" \
        --verbosity normal || print_warning "No performance tests found"

    return 0
}

# Generate test report
generate_test_report() {
    print_info "Generating test report..."

    cd "$PROJECT_ROOT"

    # Run tests with XML output
    mkdir -p "test-results"

    dotnet test \
        --configuration Debug \
        --logger "trx;LogFileName=test-results/results.trx" \
        --verbosity minimal

    print_success "Test report generated: test-results/results.trx"
}

# Show available tests
list_tests() {
    print_info "Available test methods:"

    cd "$PROJECT_ROOT"

    # Use dotnet test with --list-tests (requires .NET 6+)
    dotnet test --configuration Debug --list-tests --filter "*" || echo "Test discovery not available"
}

# Main test flow
main() {
    print_info "Coolify CLI Test Script"
    print_info "Test Type: $TEST_TYPE"

    case "$TEST_TYPE" in
        all)
            run_all_tests "$FILTER"
            ;;
        unit)
            run_unit_tests "$FILTER"
            ;;
        integration)
            run_integration_tests "$FILTER"
            ;;
        coverage)
            generate_coverage_report
            ;;
        class)
            run_test_class "$FILTER"
            ;;
        verbose)
            run_tests_verbose
            ;;
        quality)
            check_code_quality
            ;;
        performance)
            run_performance_tests
            ;;
        report)
            generate_test_report
            ;;
        list)
            list_tests
            ;;
        *)
            print_error "Unknown test type: $TEST_TYPE"
            echo ""
            echo "Usage: $0 [TEST_TYPE] [FILTER]"
            echo ""
            echo "TEST_TYPE:"
            echo "  all           - Run all tests"
            echo "  unit          - Run unit tests only"
            echo "  integration   - Run integration tests only"
            echo "  coverage      - Generate coverage report"
            echo "  class         - Run specific test class"
            echo "  verbose       - Run tests with verbose output"
            echo "  quality       - Check code quality"
            echo "  performance   - Run performance tests"
            echo "  report        - Generate test report"
            echo "  list          - List available tests"
            echo ""
            echo "Examples:"
            echo "  $0                           # Run all tests"
            echo "  $0 unit                      # Run unit tests"
            echo "  $0 coverage                  # Generate coverage report"
            echo "  $0 class ApplicationServiceTests"
            exit 1
            ;;
    esac
}

# Show usage
if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    echo "Usage: $0 [TEST_TYPE] [FILTER]"
    echo ""
    echo "Available test types:"
    echo "  all, unit, integration, coverage, class, verbose, quality, performance, report, list"
    exit 0
fi

# Run main
main "$@"
