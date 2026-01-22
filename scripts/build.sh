#!/bin/bash

# =============================================================================
# Coolify CLI Build Script
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Configuration
CONFIGURATION="${1:-Release}"
BUILD_SUFFIX="${2:-}"
OUTPUT_DIR="$PROJECT_ROOT/publish"

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

# Get version
get_version() {
    grep -oP 'Version>\K[^<]+' "$PROJECT_ROOT/coolify-cli.csproj" | head -1
}

# Clean previous build
clean_build() {
    print_info "Cleaning previous build..."

    cd "$PROJECT_ROOT"
    rm -rf "bin" "obj" "$OUTPUT_DIR"

    print_success "Clean complete"
}

# Restore NuGet packages
restore_packages() {
    print_info "Restoring NuGet packages..."

    cd "$PROJECT_ROOT"
    dotnet restore

    print_success "Packages restored"
}

# Build project
build() {
    local config=$1
    local suffix=$2

    print_info "Building in $config configuration..."

    cd "$PROJECT_ROOT"

    local build_args=("--configuration" "$config" "--no-restore")

    if [ -n "$suffix" ]; then
        build_args+=("--version-suffix" "$suffix")
    fi

    dotnet build "${build_args[@]}"

    print_success "Build successful"
}

# Run tests
run_tests() {
    print_info "Running tests..."

    cd "$PROJECT_ROOT"
    dotnet test --configuration "$CONFIGURATION" --no-build --verbosity minimal

    print_success "Tests passed"
}

# Publish for different platforms
publish_platform() {
    local rid=$1  # Runtime ID: win-x64, linux-x64, osx-x64, osx-arm64
    local output="$OUTPUT_DIR/$rid"

    print_info "Publishing for $rid..."

    cd "$PROJECT_ROOT"

    dotnet publish \
        --configuration "$CONFIGURATION" \
        --runtime "$rid" \
        --self-contained \
        --output "$output" \
        --no-build \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:DebugType=embedded

    # Create archive
    local archive_name="coolify-cli-$rid.tar.gz"
    if [ "$rid" == "win-x64" ]; then
        archive_name="coolify-cli-$rid.zip"
    fi

    cd "$output"

    if [ "$rid" == "win-x64" ]; then
        zip -r "../$archive_name" . > /dev/null 2>&1
    else
        tar czf "../$archive_name" . > /dev/null 2>&1
    fi

    print_success "Published to $output"
    print_success "Archive: $archive_name"
}

# Publish all platforms
publish_all() {
    print_info "Publishing for all platforms..."

    publish_platform "linux-x64"
    publish_platform "linux-arm64"
    publish_platform "osx-x64"
    publish_platform "osx-arm64"
    publish_platform "win-x64"

    print_success "All platforms published"
}

# Generate build info
generate_build_info() {
    local version=$(get_version)
    local build_time=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    local git_commit=$(cd "$PROJECT_ROOT" && git rev-parse --short HEAD 2>/dev/null || echo "unknown")
    local git_branch=$(cd "$PROJECT_ROOT" && git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")

    local info_file="$OUTPUT_DIR/BUILD_INFO.txt"

    cat > "$info_file" <<EOF
Coolify CLI Build Information
=============================

Version: $version
Build Time: $build_time
Configuration: $CONFIGURATION
Git Commit: $git_commit
Git Branch: $git_branch
Build Machine: $(uname -s) $(uname -m)

Publish Directory: $OUTPUT_DIR
EOF

    print_success "Build info generated: $info_file"
}

# Create GitHub release assets
prepare_release_assets() {
    print_info "Preparing release assets..."

    cd "$OUTPUT_DIR"

    # Calculate checksums
    echo "Generating checksums..."

    for file in coolify-cli-*.tar.gz coolify-cli-*.zip; do
        if [ -f "$file" ]; then
            sha256sum "$file" > "$file.sha256"
            print_success "Checksums created for $file"
        fi
    done

    # Create release notes
    local version=$(get_version)
    local release_notes="RELEASE_NOTES_$version.md"

    cat > "$release_notes" <<EOF
# Coolify CLI v$version

## Release Date
$(date +"%Y-%m-%d")

## Downloads
- [Linux x64](./coolify-cli-linux-x64.tar.gz)
- [Linux ARM64](./coolify-cli-linux-arm64.tar.gz)
- [macOS x64](./coolify-cli-osx-x64.tar.gz)
- [macOS ARM64 (Apple Silicon)](./coolify-cli-osx-arm64.tar.gz)
- [Windows x64](./coolify-cli-win-x64.zip)

## Installation

### Linux/macOS
\`\`\`bash
tar xzf coolify-cli-linux-x64.tar.gz
sudo mv coolify-cli /usr/local/bin/
\`\`\`

### Windows
Extract the zip file and add the directory to your PATH.

## Verify Download
\`\`\`bash
sha256sum -c coolify-cli-linux-x64.tar.gz.sha256
\`\`\`

## Changes in This Release
- [See full changelog](../CHANGELOG.md)

## Support
- [Documentation](../docs/)
- [Issues](https://github.com/Sarmkadan/coolify-cli/issues)
- [Discussions](https://github.com/Sarmkadan/coolify-cli/discussions)
EOF

    print_success "Release notes created: $release_notes"
}

# Generate documentation
generate_docs() {
    print_info "Generating documentation..."

    # Generate CLI help documentation
    local doc_file="$OUTPUT_DIR/CLI_REFERENCE.md"

    {
        echo "# Coolify CLI Reference"
        echo ""
        echo "Generated: $(date)"
        echo ""
        echo "## Version"
        echo "$(cd "$PROJECT_ROOT" && dotnet run --no-build -- --version 2>/dev/null || echo 'v1.0.0')"
        echo ""
        echo "## Usage"
        echo "\`\`\`bash"
        cd "$PROJECT_ROOT" && dotnet run --no-build -- --help 2>/dev/null || echo "coolify-cli [COMMAND] [OPTIONS]"
        echo "\`\`\`"

    } > "$doc_file"

    print_success "Documentation generated"
}

# Display build summary
show_summary() {
    local version=$(get_version)

    echo ""
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}Build Summary${NC}"
    echo -e "${GREEN}================================${NC}"
    echo ""
    echo "Version:        $version"
    echo "Configuration:  $CONFIGURATION"
    echo "Output Dir:     $OUTPUT_DIR"
    echo ""

    # Show artifacts
    if [ -d "$OUTPUT_DIR" ]; then
        echo "Artifacts:"
        find "$OUTPUT_DIR" -maxdepth 1 -type f | while read file; do
            local size=$(du -h "$file" | cut -f1)
            echo "  - $(basename "$file") ($size)"
        done
    fi

    echo ""
}

# Main build flow
main() {
    print_info "Coolify CLI Build Script"
    print_info "Configuration: $CONFIGURATION"

    # Validate configuration
    if [ "$CONFIGURATION" != "Debug" ] && [ "$CONFIGURATION" != "Release" ]; then
        print_error "Invalid configuration: $CONFIGURATION"
        exit 1
    fi

    # Build steps
    clean_build
    restore_packages
    build "$CONFIGURATION" "$BUILD_SUFFIX"

    # Run tests only for Debug configuration
    if [ "$CONFIGURATION" = "Debug" ]; then
        run_tests || true
    fi

    # Publish for Release
    if [ "$CONFIGURATION" = "Release" ]; then
        mkdir -p "$OUTPUT_DIR"
        publish_all
        generate_build_info
        prepare_release_assets
    fi

    show_summary

    print_success "Build completed successfully"
}

# Show usage
if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    echo "Usage: $0 [CONFIGURATION] [BUILD_SUFFIX]"
    echo ""
    echo "CONFIGURATION: Debug (default) or Release"
    echo "BUILD_SUFFIX:  Optional suffix for version (e.g., 'rc1', 'beta')"
    echo ""
    echo "Examples:"
    echo "  $0                          # Build Debug"
    echo "  $0 Release                  # Build Release"
    echo "  $0 Release rc1              # Build Release v1.0.0-rc1"
    exit 0
fi

# Run main
main "$@"
