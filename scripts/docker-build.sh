#!/bin/bash

# =============================================================================
# Coolify CLI Docker Build Script
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Configuration
IMAGE_NAME="${IMAGE_NAME:-sarmkadan/coolify-cli}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
REGISTRY="${REGISTRY:-docker.io}"
BUILD_PLATFORMS="${BUILD_PLATFORMS:-linux/amd64,linux/arm64}"
PUSH="${PUSH:-false}"

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

# Check dependencies
check_dependencies() {
    print_info "Checking dependencies..."

    if ! command -v docker &> /dev/null; then
        print_error "Docker not found. Please install Docker."
        exit 1
    fi

    print_success "Docker found: $(docker --version)"

    # Check if buildx is available (needed for multi-platform builds)
    if ! docker buildx version &> /dev/null; then
        print_error "Docker Buildx not available. Required for multi-platform builds."
        echo "Install from: https://docs.docker.com/build/install-buildx/"
        exit 1
    fi

    print_success "Docker Buildx available"
}

# Build Docker image
build_image() {
    local image_name=$1
    local image_tag=$2
    local platforms=$3
    local push=$4

    print_info "Building Docker image: $image_name:$image_tag"
    print_info "Platforms: $platforms"

    cd "$PROJECT_ROOT"

    local build_args=(
        --file Dockerfile
        --tag "$image_name:$image_tag"
    )

    # Build for multiple platforms
    if [ "$platforms" != "local" ]; then
        build_args+=(--platform "$platforms")

        if [ "$push" = "true" ]; then
            build_args+=(--push)
        fi

        # Use buildx for multi-platform builds
        docker buildx build "${build_args[@]}" .
    else
        # Single platform build
        docker build "${build_args[@]}" .
    fi

    print_success "Image built successfully"
}

# Tag image for registry
tag_for_registry() {
    local image_name=$1
    local image_tag=$2
    local registry=$3

    local full_name="$registry/$image_name:$image_tag"

    print_info "Tagging image for registry: $full_name"

    docker tag "$image_name:$image_tag" "$full_name"

    print_success "Image tagged"
}

# Push image to registry
push_image() {
    local image_name=$1
    local image_tag=$2
    local registry=$3

    local full_name="$registry/$image_name:$image_tag"

    print_info "Pushing image to registry..."

    docker push "$full_name"

    print_success "Image pushed: $full_name"
}

# Test image
test_image() {
    local image_name=$1
    local image_tag=$2

    print_info "Testing image: $image_name:$image_tag"

    # Test basic functionality
    docker run --rm "$image_name:$image_tag" --version

    print_success "Image test passed"
}

# Create buildx builder
create_builder() {
    print_info "Creating Docker Buildx builder..."

    docker buildx create --name coolify-builder --use

    print_success "Builder created"
}

# Remove image
remove_image() {
    local image_name=$1
    local image_tag=$2

    print_info "Removing image: $image_name:$image_tag"

    docker rmi "$image_name:$image_tag" || true

    print_success "Image removed"
}

# Generate Docker metadata
generate_metadata() {
    print_info "Generating Docker metadata..."

    local version=$(grep -oP 'Version>\K[^<]+' "$PROJECT_ROOT/coolify-cli.csproj" | head -1)
    local build_date=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    local git_commit=$(cd "$PROJECT_ROOT" && git rev-parse --short HEAD 2>/dev/null || echo "unknown")

    cat > "$PROJECT_ROOT/.docker/metadata.json" <<EOF
{
  "version": "$version",
  "build_date": "$build_date",
  "git_commit": "$git_commit",
  "image_name": "$IMAGE_NAME",
  "image_tag": "$IMAGE_TAG"
}
EOF

    print_success "Metadata generated"
}

# Run security scan on image
scan_image() {
    local image_name=$1
    local image_tag=$2

    print_info "Scanning image for vulnerabilities..."

    # Check if trivy is available
    if ! command -v trivy &> /dev/null; then
        print_info "Trivy not found. Skipping security scan."
        print_info "Install from: https://github.com/aquasecurity/trivy"
        return
    fi

    trivy image "$image_name:$image_tag"

    print_success "Security scan completed"
}

# Show build summary
show_summary() {
    local image_name=$1
    local image_tag=$2
    local registry=$3

    echo ""
    echo -e "${GREEN}================================${NC}"
    echo -e "${GREEN}Build Summary${NC}"
    echo -e "${GREEN}================================${NC}"
    echo ""
    echo "Image:      $image_name:$image_tag"
    echo "Full name:  $registry/$image_name:$image_tag"
    echo "Platforms:  $BUILD_PLATFORMS"
    echo "Push:       $PUSH"
    echo ""
}

# Main build flow
main() {
    print_info "Coolify CLI Docker Build Script"
    echo ""

    check_dependencies

    # Build image
    build_image "$IMAGE_NAME" "$IMAGE_TAG" "$BUILD_PLATFORMS" "$PUSH"

    # Test image (only for local builds)
    if [ "$BUILD_PLATFORMS" = "local" ]; then
        test_image "$IMAGE_NAME" "$IMAGE_TAG"
    fi

    # Tag for registry
    if [ -n "$REGISTRY" ]; then
        tag_for_registry "$IMAGE_NAME" "$IMAGE_TAG" "$REGISTRY"
    fi

    # Push if requested
    if [ "$PUSH" = "true" ]; then
        if [ -n "$REGISTRY" ]; then
            push_image "$IMAGE_NAME" "$IMAGE_TAG" "$REGISTRY"
        fi
    fi

    # Generate metadata
    generate_metadata

    # Security scan
    if command -v trivy &> /dev/null; then
        scan_image "$IMAGE_NAME" "$IMAGE_TAG"
    fi

    show_summary "$IMAGE_NAME" "$IMAGE_TAG" "$REGISTRY"

    print_success "Docker build completed successfully"
}

# Show usage
if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --image-name NAME          Image name (default: sarmkadan/coolify-cli)"
    echo "  --image-tag TAG            Image tag (default: latest)"
    echo "  --registry REGISTRY        Registry URL (default: docker.io)"
    echo "  --platforms PLATFORMS      Build platforms (default: linux/amd64,linux/arm64)"
    echo "  --push                     Push image to registry"
    echo "  --help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                                          # Build for local platform"
    echo "  $0 --image-tag v1.0.0                      # Build with specific tag"
    echo "  $0 --platforms linux/amd64                 # Build for single platform"
    echo "  $0 --push --image-tag latest               # Build and push"
    echo "  IMAGE_NAME=myregistry/cli $0 --push       # Use custom registry"
    exit 0
fi

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --image-name)
            IMAGE_NAME="$2"
            shift 2
            ;;
        --image-tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        --registry)
            REGISTRY="$2"
            shift 2
            ;;
        --platforms)
            BUILD_PLATFORMS="$2"
            shift 2
            ;;
        --push)
            PUSH="true"
            shift
            ;;
        *)
            shift
            ;;
    esac
done

# Run main
main "$@"
