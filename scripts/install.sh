#!/bin/bash

# =============================================================================
# Coolify CLI Installation Script
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

# Configuration
VERSION="${VERSION:-latest}"
INSTALL_DIR="${INSTALL_DIR:-/usr/local/bin}"
DOWNLOAD_BASE="https://github.com/Sarmkadan/coolify-cli/releases/download"

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

# Detect OS and architecture
detect_platform() {
    local os=$(uname -s)
    local arch=$(uname -m)

    case "$os" in
        Linux)
            case "$arch" in
                x86_64) echo "linux-x64" ;;
                aarch64) echo "linux-arm64" ;;
                *) print_error "Unsupported architecture: $arch"; exit 1 ;;
            esac
            ;;
        Darwin)
            case "$arch" in
                x86_64) echo "osx-x64" ;;
                arm64) echo "osx-arm64" ;;
                *) print_error "Unsupported architecture: $arch"; exit 1 ;;
            esac
            ;;
        MINGW*|MSYS*|CYGWIN*)
            echo "win-x64"
            ;;
        *)
            print_error "Unsupported OS: $os"
            exit 1
            ;;
    esac
}

# Check dependencies
check_dependencies() {
    print_info "Checking dependencies..."

    local deps=("curl" "tar" "sudo")
    local missing=0

    for dep in "${deps[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            print_warning "$dep not found"
            missing=$((missing + 1))
        fi
    done

    if [ $missing -gt 0 ]; then
        print_warning "Some optional dependencies missing"
    fi

    print_success "Dependencies check complete"
}

# Download and install
install_from_release() {
    local platform=$1
    local version=$2
    local install_dir=$3

    print_info "Downloading Coolify CLI ($version) for $platform..."

    # Create temp directory
    local temp_dir=$(mktemp -d)
    trap "rm -rf $temp_dir" EXIT

    # Determine file extension
    local file_ext="tar.gz"
    if [ "$platform" = "win-x64" ]; then
        file_ext="zip"
    fi

    # Download release
    local download_url="$DOWNLOAD_BASE/$version/coolify-cli-$platform.$file_ext"
    print_info "Downloading from: $download_url"

    if ! curl -fsSL "$download_url" -o "$temp_dir/coolify-cli.$file_ext"; then
        print_error "Failed to download Coolify CLI"
        return 1
    fi

    print_success "Downloaded successfully"

    # Extract
    print_info "Extracting..."

    cd "$temp_dir"

    if [ "$file_ext" = "zip" ]; then
        unzip -q coolify-cli.zip
    else
        tar xzf coolify-cli.tar.gz
    fi

    # Find executable
    local executable="coolify-cli"
    if [ "$platform" = "win-x64" ]; then
        executable="coolify-cli.exe"
    fi

    if [ ! -f "$executable" ]; then
        print_error "Executable not found in release"
        return 1
    fi

    # Make executable
    chmod +x "$executable"

    # Copy to install directory
    print_info "Installing to $install_dir..."

    if [ "$install_dir" = "/usr/local/bin" ] || [ "$install_dir" = "/usr/bin" ]; then
        sudo cp "$executable" "$install_dir/"
        sudo chmod +x "$install_dir/$executable"
    else
        mkdir -p "$install_dir"
        cp "$executable" "$install_dir/"
        chmod +x "$install_dir/$executable"
    fi

    print_success "Installed to $install_dir/$executable"

    # Verify installation
    if command -v "$executable" &> /dev/null; then
        print_success "Installation verified"
        "$executable" --version
    else
        print_warning "Executable not in PATH. Add $install_dir to your PATH"
    fi
}

# Install from source
install_from_source() {
    print_info "Building from source..."

    # Check for .NET
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found"
        echo "Install from: https://dotnet.microsoft.com/download/dotnet/10.0"
        return 1
    fi

    # Clone repository
    local repo_dir=$(mktemp -d)
    trap "rm -rf $repo_dir" EXIT

    print_info "Cloning repository..."
    if ! git clone --depth 1 https://github.com/Sarmkadan/coolify-cli.git "$repo_dir"; then
        print_error "Failed to clone repository"
        return 1
    fi

    cd "$repo_dir"

    # Build
    print_info "Building project..."
    if ! dotnet build --configuration Release; then
        print_error "Build failed"
        return 1
    fi

    # Publish
    print_info "Publishing..."
    if ! dotnet publish -c Release -o "./publish"; then
        print_error "Publish failed"
        return 1
    fi

    # Install
    print_info "Installing..."
    sudo cp "./publish/coolify-cli" "$INSTALL_DIR/"
    sudo chmod +x "$INSTALL_DIR/coolify-cli"

    print_success "Installed to $INSTALL_DIR/coolify-cli"
}

# Install with Homebrew
install_with_homebrew() {
    if ! command -v brew &> /dev/null; then
        print_error "Homebrew not found. Install from: https://brew.sh"
        return 1
    fi

    print_info "Installing via Homebrew..."

    brew tap Sarmkadan/coolify-cli
    brew install coolify-cli

    print_success "Installed via Homebrew"
}

# Setup shell completion
setup_completion() {
    print_info "Setting up shell completion..."

    local shell_type=$(echo $SHELL | xargs basename)

    case "$shell_type" in
        bash)
            local completion_dir="/etc/bash_completion.d"
            [ -d "$completion_dir" ] && sudo cp coolify-cli-completion.bash "$completion_dir/"
            ;;
        zsh)
            local completion_dir="/usr/local/share/zsh/site-functions"
            mkdir -p "$completion_dir"
            sudo cp _coolify-cli "$completion_dir/"
            ;;
    esac

    print_success "Completion setup complete"
}

# Show next steps
show_next_steps() {
    echo ""
    echo -e "${GREEN}Installation complete!${NC}"
    echo ""
    echo "Next steps:"
    echo ""
    echo "1. Set your API credentials:"
    echo "   export COOLIFY_API_KEY='your-api-key'"
    echo "   export COOLIFY_API_URL='https://your-coolify-instance.com'"
    echo ""
    echo "2. Verify installation:"
    echo "   coolify-cli --version"
    echo ""
    echo "3. Try a command:"
    echo "   coolify-cli app list"
    echo ""
    echo "4. Get help:"
    echo "   coolify-cli --help"
    echo ""
    echo "Documentation: https://github.com/Sarmkadan/coolify-cli"
    echo ""
}

# Main installation
main() {
    print_info "Coolify CLI Installer"
    echo ""

    # Detect platform
    local platform=$(detect_platform)
    print_success "Detected platform: $platform"

    # Check dependencies
    check_dependencies

    # Install options
    if [ -z "${INSTALL_METHOD:-}" ]; then
        echo ""
        echo "Installation methods:"
        echo "1. Binary release (default)"
        echo "2. Build from source"
        echo "3. Homebrew (macOS/Linux)"
        echo ""

        read -p "Select installation method (1-3): " method

        case $method in
            1) install_method="release" ;;
            2) install_method="source" ;;
            3) install_method="homebrew" ;;
            *) install_method="release" ;;
        esac
    else
        install_method="$INSTALL_METHOD"
    fi

    # Install
    case $install_method in
        release)
            install_from_release "$platform" "$VERSION" "$INSTALL_DIR"
            ;;
        source)
            install_from_source
            ;;
        homebrew)
            install_with_homebrew
            ;;
        *)
            print_error "Unknown installation method"
            exit 1
            ;;
    esac

    show_next_steps
}

# Show usage
if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --version VERSION           Version to install (default: latest)"
    echo "  --install-dir DIR          Installation directory (default: /usr/local/bin)"
    echo "  --method METHOD            Installation method: release, source, homebrew"
    echo "  --help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                          # Interactive installation"
    echo "  $0 --method release         # Install from release"
    echo "  $0 --method source          # Build from source"
    echo "  VERSION=v1.2.0 $0          # Install specific version"
    exit 0
fi

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --version)
            VERSION="$2"
            shift 2
            ;;
        --install-dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        --method)
            INSTALL_METHOD="$2"
            shift 2
            ;;
        *)
            shift
            ;;
    esac
done

# Run main
main "$@"
